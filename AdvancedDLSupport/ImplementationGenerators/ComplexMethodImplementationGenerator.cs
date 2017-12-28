using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using JetBrains.Annotations;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates method implementations for methods involving complex types.
    /// </summary>
    internal class ComplexMethodImplementationGenerator : MethodImplementationGenerator
    {
        [NotNull]
        private readonly TypeTransformerRepository _transformerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexMethodImplementationGenerator"/> class.
        /// </summary>
        /// <param name="targetModule">The module in which the method implementation should be generated.</param>
        /// <param name="targetType">The type in which the method implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="configuration">The configuration object to use.</param>
        /// <param name="transformerRepository">The repository where type transformers are stored.</param>
        public ComplexMethodImplementationGenerator
        (
            [NotNull] ModuleBuilder targetModule,
            [NotNull] TypeBuilder targetType,
            [NotNull] ILGenerator targetTypeConstructorIL,
            ImplementationConfiguration configuration,
            [NotNull] TypeTransformerRepository transformerRepository
        )
            : base(targetModule, targetType, targetTypeConstructorIL, configuration)
        {
            _transformerRepository = transformerRepository;
        }

        /// <inheritdoc />
        protected override void GenerateImplementation(MethodInfo method, string symbolName, string uniqueMemberIdentifier)
        {
            var metadataAttribute = method.GetCustomAttribute<NativeSymbolAttribute>() ??
                                    new NativeSymbolAttribute(method.Name);

            var loweredMethod = GenerateLoweredMethod(method, uniqueMemberIdentifier);

            var delegateBuilder = GenerateDelegateType
            (
                loweredMethod.LoweredMethod.ReturnType,
                loweredMethod.ParameterTypes,
                uniqueMemberIdentifier,
                metadataAttribute.CallingConvention
            );

            // Create a delegate field
            var delegateBuilderType = delegateBuilder.CreateTypeInfo();

            FieldBuilder delegateField;
            if (Configuration.UseLazyBinding)
            {
                var lazyLoadedType = typeof(Lazy<>).MakeGenericType(delegateBuilderType);
                delegateField = TargetType.DefineField($"{uniqueMemberIdentifier}_dtm", lazyLoadedType, FieldAttributes.Public);
            }
            else
            {
                delegateField = TargetType.DefineField($"{uniqueMemberIdentifier}_dtm", delegateBuilderType, FieldAttributes.Public);
            }

            GenerateDelegateInvokerBody(loweredMethod.LoweredMethod, loweredMethod.ParameterTypes, delegateBuilderType, delegateField);
            GenerateComplexMethodBody(method, loweredMethod.LoweredMethod, loweredMethod.ParameterTypes);

            AugmentHostingTypeConstructor(symbolName, delegateBuilderType, delegateField);
        }

        private void GenerateComplexMethodBody(MethodInfo method, MethodInfo loweredMethod, Type[] loweredMethodParameterTypes)
        {
            var methodBuilder = TargetType.DefineMethod
            (
                method.Name,
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                CallingConventions.Standard,
                method.ReturnType,
                method.GetParameters().Select(p => p.ParameterType).ToArray()
            );

            // Let's create a method that simply invoke the delegate
            var il = methodBuilder.GetILGenerator();

            if (Configuration.GenerateDisposalChecks)
            {
                EmitDisposalCheck(il);
            }

            il.Emit(OpCodes.Ldarg_0);

            var parameters = method.GetParameters();
            var loweredParameterTypes = loweredMethodParameterTypes;
            var repoProperty = typeof(AnonymousImplementationBase).GetProperty
            (
                nameof(AnonymousImplementationBase.TransformerRepository),
                BindingFlags.Public | BindingFlags.Instance
            );

            // Emit lowered parameters
            for (var i = 1; i <= parameters.Length; ++i)
            {
                var parameter = parameters[i - 1];
                if (ComplexTypeHelper.IsComplexType(parameter.ParameterType))
                {
                    var loweredParameterType = loweredParameterTypes[i - 1];
                    EmitValueLowering(il, parameter.ParameterType, loweredParameterType, repoProperty, i);
                }
                else
                {
                    il.Emit(OpCodes.Ldarg, i);
                }
            }

            // Call lowered method
            il.Emit(OpCodes.Call, loweredMethod);

            // Emit return value raising
            if (ComplexTypeHelper.IsComplexType(method.ReturnType))
            {
                EmitValueRaising(il, method.ReturnType, loweredMethod.ReturnType, repoProperty);
            }

            il.Emit(OpCodes.Ret);
        }

        private void EmitValueLowering(ILGenerator il, Type complexType, Type simpleType, PropertyInfo typeTransformerRepository, int parameterIndex)
        {
            var lowerValueFunc = typeof(ITypeTransformer<,>).MakeGenericType(complexType, simpleType).GetMethod(nameof(ITypeTransformer<object, object>.LowerValue));

            EmitGetComplexTransformerCall(il, complexType);

            il.Emit(OpCodes.Ldarg, parameterIndex); // Load the complex argument
            il.Emit(OpCodes.Callvirt, lowerValueFunc); // Lower it
        }

        private void EmitValueRaising(ILGenerator il, Type complexType, Type simpleType, PropertyInfo typeTransformerRepository)
        {
            var transformerType = typeof(ITypeTransformer<,>).MakeGenericType(complexType, simpleType);
            var raiseValueFunc = transformerType.GetMethod(nameof(ITypeTransformer<object, object>.RaiseValue));

            il.DeclareLocal(simpleType);
            il.Emit(OpCodes.Stloc_0); // Store the result from the lowered method call

            EmitGetComplexTransformerCall(il, complexType);

            il.Emit(OpCodes.Ldloc_0); // Load the result again
            il.Emit(OpCodes.Callvirt, raiseValueFunc); // Raise it
        }

        private void EmitGetComplexTransformerCall(ILGenerator il, Type complexType)
        {
            var getTransformerFunc = typeof(TypeTransformerRepository).GetMethod(nameof(TypeTransformerRepository.GetComplexTransformer));
            var repoProperty = typeof(AnonymousImplementationBase).GetProperty
            (
                nameof(AnonymousImplementationBase.TransformerRepository),
                BindingFlags.Public | BindingFlags.Instance
            );

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, repoProperty.GetMethod); // Get the type transformer repository

            EmitTypeOf(il, complexType);
            il.Emit(OpCodes.Callvirt, getTransformerFunc); // Get the relevant type transformer
        }

        private void EmitTypeOf(ILGenerator il, Type type)
        {
            var getTypeFromHandleFunc = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle));
            il.Emit(OpCodes.Ldtoken, type);
            il.Emit(OpCodes.Call, getTypeFromHandleFunc);
        }

        private (MethodBuilder LoweredMethod, Type[] ParameterTypes) GenerateLoweredMethod(MethodInfo method, string memberIdentifier)
        {
            var newReturnType = LowerTypeIfRequired(method.ReturnType);

            var newParameterTypes = new List<Type>();
            foreach (var parameter in method.GetParameters())
            {
                newParameterTypes.Add(LowerTypeIfRequired(parameter.ParameterType));
            }

            var loweredMethod = TargetType.DefineMethod
            (
                $"{memberIdentifier}_lowered",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                CallingConventions.Standard,
                newReturnType,
                newParameterTypes.ToArray()
            );

            return (loweredMethod, newParameterTypes.ToArray());
        }

        private Type LowerTypeIfRequired(Type type)
        {
            if (ComplexTypeHelper.IsComplexType(type))
            {
                var transformer = _transformerRepository.GetComplexTransformer(type);
                type = transformer.LowerType();
            }

            return type;
        }
    }
}
