using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Attributes;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates implementations for methods.
    /// </summary>
    internal class MethodImplementationGenerator : ImplementationGeneratorBase<MethodInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodImplementationGenerator"/> class.
        /// </summary>
        /// <param name="targetModule">The module in which the method implementation should be generated.</param>
        /// <param name="targetType">The type in which the method implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="configuration">The configuration object to use.</param>
        public MethodImplementationGenerator(ModuleBuilder targetModule, TypeBuilder targetType, ILGenerator targetTypeConstructorIL, ImplementationConfiguration configuration)
            : base(targetModule, targetType, targetTypeConstructorIL, configuration)
        {
        }

        /// <inheritdoc />
        public override void GenerateImplementation(MethodInfo method)
        {
            var uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", "_");
            var parameters = method.GetParameters();

            var metadataAttribute = method.GetCustomAttribute<NativeFunctionAttribute>() ??
                                    new NativeFunctionAttribute(method.Name);

            var delegateBuilder = GenerateDelegateType(method, uniqueIdentifier, metadataAttribute, parameters);

            // Create a delegate field
            var delegateBuilderType = delegateBuilder.CreateTypeInfo();

            FieldBuilder delegateField;
            if (Configuration.UseLazyBinding)
            {
                var lazyLoadedType = typeof(Lazy<>).MakeGenericType(delegateBuilderType);
                delegateField = TargetType.DefineField($"{method.Name}_dtm_{uniqueIdentifier}", lazyLoadedType, FieldAttributes.Public);
            }
            else
            {
                delegateField = TargetType.DefineField($"{method.Name}_dtm_{uniqueIdentifier}", delegateBuilderType, FieldAttributes.Public);
            }

            GenerateDelegateInvoker(method, parameters, delegateField, delegateBuilderType);
            AugmentHostingTypeConstructor(method, metadataAttribute, delegateBuilderType, delegateField);
        }

        private void AugmentHostingTypeConstructor(MethodInfo method, NativeFunctionAttribute metadataAttribute, Type delegateBuilderType, FieldInfo delegateField)
        {
            var entrypointName = metadataAttribute.Entrypoint ?? method.Name;
            var loadFuncMethod = typeof(AnonymousImplementationBase).GetMethod
            (
                "LoadFunction",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            TargetTypeConstructorIL.Emit(OpCodes.Ldarg_0); // This is for storing field delegate, it needs the "this" reference
            TargetTypeConstructorIL.Emit(OpCodes.Ldarg_0);

            var loadFunc = loadFuncMethod.MakeGenericMethod(delegateBuilderType);
            if (Configuration.UseLazyBinding)
            {
                var lambdaBuilder = GenerateFunctionLoadingLambda(delegateBuilderType, entrypointName);

                var funcType = typeof(Func<>).MakeGenericType(delegateBuilderType);
                var lazyType = typeof(Lazy<>).MakeGenericType(delegateBuilderType);

                var funcConstructor = funcType.GetConstructors().First();
                var lazyConstructor = lazyType.GetConstructors().First
                (
                    c =>
                        c.GetParameters().Any() &&
                        c.GetParameters().Length == 1 &&
                        c.GetParameters().First().ParameterType == funcType
                );

                // Use the lambda instead of the function directly.
                TargetTypeConstructorIL.Emit(OpCodes.Ldftn, lambdaBuilder);
                TargetTypeConstructorIL.Emit(OpCodes.Newobj, funcConstructor);
                TargetTypeConstructorIL.Emit(OpCodes.Newobj, lazyConstructor);
            }
            else
            {
                TargetTypeConstructorIL.Emit(OpCodes.Ldstr, entrypointName);
                TargetTypeConstructorIL.EmitCall(OpCodes.Call, loadFunc, null);
            }

            TargetTypeConstructorIL.Emit(OpCodes.Stfld, delegateField);
        }

        private void GenerateDelegateInvoker(MethodInfo method, IReadOnlyCollection<ParameterInfo> parameters, FieldInfo delegateField, Type delegateBuilderType)
        {
            var methodBuilder = TargetType.DefineMethod
            (
                method.Name,
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                CallingConventions.Standard,
                method.ReturnType,
                parameters.Select(p => p.ParameterType).ToArray()
            );

            // Let's create a method that simply invoke the delegate
            var methodIL = methodBuilder.GetILGenerator();

            if (Configuration.GenerateDisposalChecks)
            {
                EmitDisposalCheck(methodIL);
            }

            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Ldfld, delegateField);

            if (Configuration.UseLazyBinding)
            {
                var getMethod = typeof(Lazy<>).MakeGenericType(delegateBuilderType).GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public);
                methodIL.Emit(OpCodes.Callvirt, getMethod);
            }

            for (int p = 1; p <= parameters.Count; p++)
            {
                methodIL.Emit(OpCodes.Ldarg, p);
            }

            methodIL.EmitCall(OpCodes.Call, delegateBuilderType.GetMethod("Invoke"), null);
            methodIL.Emit(OpCodes.Ret);
        }

        private TypeBuilder GenerateDelegateType(MethodInfo method, string uniqueIdentifier, NativeFunctionAttribute metadataAttribute, IEnumerable<ParameterInfo> parameters)
        {
            // Declare a delegate type
            var delegateBuilder = TargetModule.DefineType
            (
                $"{method.Name}_dt_{uniqueIdentifier}",
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                typeof(MulticastDelegate)
            );

            var attributeConstructor = typeof(UnmanagedFunctionPointerAttribute).GetConstructors().First
            (
                c =>
                    c.GetParameters().Any() &&
                    c.GetParameters().Length == 1 &&
                    c.GetParameters().First().ParameterType == typeof(CallingConvention)

            );

            var functionPointerAttributeBuilder = new CustomAttributeBuilder
            (
                attributeConstructor,
                new object[] { metadataAttribute.CallingConvention }
            );

            delegateBuilder.SetCustomAttribute(functionPointerAttributeBuilder);

            var delegateCtorBuilder = delegateBuilder.DefineConstructor
            (
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(object), typeof(IntPtr) }
            );

            delegateCtorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            var delegateMethodBuilder = delegateBuilder.DefineMethod
            (
                "Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                method.ReturnType,
                parameters.Select(p => p.ParameterType).ToArray()
            );

            delegateMethodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
            return delegateBuilder;
        }
    }
}
