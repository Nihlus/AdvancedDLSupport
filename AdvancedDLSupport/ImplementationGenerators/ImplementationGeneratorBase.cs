using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Base class for implementation generators.
    /// </summary>
    /// <typeparam name="T">The type of member to generate the implementation for.</typeparam>
    internal abstract class ImplementationGeneratorBase<T> : IImplementationGenerator<T> where T : MemberInfo
    {
        /// <inheritdoc />
        public ImplementationConfiguration Configuration { get; }

        /// <summary>
        /// Gets the module in which the implementation should be generated.
        /// </summary>
        protected ModuleBuilder TargetModule { get; }

        /// <summary>
        /// Gets the type in which the implementation should be generated.
        /// </summary>
        protected TypeBuilder TargetType { get; }

        /// <summary>
        /// Gets the IL generator for the constructor of the type in which the implementation should be generated.
        /// </summary>
        protected ILGenerator TargetTypeConstructorIL { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementationGeneratorBase{T}"/> class.
        /// </summary>
        /// <param name="targetModule">The module where the implementation should be generated.</param>
        /// <param name="targetType">The type in which the implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="configuration">The configuration object to use.</param>
        public ImplementationGeneratorBase(ModuleBuilder targetModule, TypeBuilder targetType, ILGenerator targetTypeConstructorIL, ImplementationConfiguration configuration)
        {
            TargetModule = targetModule;
            TargetType = targetType;
            TargetTypeConstructorIL = targetTypeConstructorIL;
            Configuration = configuration;
        }

        /// <inheritdoc />
        public abstract void GenerateImplementation(T member);

        /// <summary>
        /// Generates the IL required to push the value of the field to the stack, including the case where the field
        /// is lazily loaded.
        /// </summary>
        /// <param name="il">The IL generator.</param>
        /// <param name="symbolField">The field to generate the IL for.</param>
        protected void GenerateSymbolPush(ILGenerator il, FieldInfo symbolField)
        {
            il.Emit(OpCodes.Ldfld, symbolField);
            if (!Configuration.UseLazyBinding)
            {
                return;
            }

            var getMethod = typeof(Lazy<IntPtr>).GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public);
            il.Emit(OpCodes.Callvirt, getMethod);
        }

        /// <summary>
        /// Generates a lambda method for loading the given symbol.
        /// </summary>
        /// <param name="symbolName">The name of the symbol.</param>
        /// <returns>A method which, when called, will load and return the given symbol.</returns>
        protected MethodBuilder GenerateSymbolLoadingLambda(string symbolName)
        {
            var uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", "_");

            var loadSymbolMethod = typeof(AnonymousImplementationBase).GetMethod
            (
                "LoadSymbol",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Generate lambda loader
            var lambdaBuilder = TargetType.DefineMethod
            (
                $"{symbolName}_{uniqueIdentifier}_lazy",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Final,
                typeof(IntPtr),
                null
            );

            var lambdaIL = lambdaBuilder.GetILGenerator();
            lambdaIL.Emit(OpCodes.Ldarg_0);
            lambdaIL.Emit(OpCodes.Ldstr, symbolName);
            lambdaIL.EmitCall(OpCodes.Call, loadSymbolMethod, null);
            lambdaIL.Emit(OpCodes.Ret);
            return lambdaBuilder;
        }

        /// <summary>
        /// Generates a lambda method for loading the given function.
        /// </summary>
        /// <param name="delegateType">The type of delegate to load.</param>
        /// <param name="functionName">The name of the function.</param>
        /// <returns>A method which, when called, will load and return the given function.</returns>
        protected MethodBuilder GenerateFunctionLoadingLambda(Type delegateType, string functionName)
        {
            var uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", "_");

            var loadFuncMethod = typeof(AnonymousImplementationBase).GetMethod
            (
                "LoadFunction",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var loadFunc = loadFuncMethod.MakeGenericMethod(delegateType);

            // Generate lambda loader
            var lambdaBuilder = TargetType.DefineMethod
            (
                $"{delegateType.Name}_{uniqueIdentifier}_lazy",
                MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Final,
                delegateType,
                null
            );

            var lambdaIL = lambdaBuilder.GetILGenerator();
            lambdaIL.Emit(OpCodes.Ldarg_0);
            lambdaIL.Emit(OpCodes.Ldstr, functionName);
            lambdaIL.EmitCall(OpCodes.Call, loadFunc, null);
            lambdaIL.Emit(OpCodes.Ret);
            return lambdaBuilder;
        }
    }
}
