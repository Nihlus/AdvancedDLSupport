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
        public MethodImplementationGenerator(ModuleBuilder targetModule, TypeBuilder targetType, ILGenerator targetTypeConstructorIL)
            : base(targetModule, targetType, targetTypeConstructorIL)
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
            var delegateField = TargetType.DefineField($"{method.Name}_dtm_{uniqueIdentifier}", delegateBuilderType, FieldAttributes.Public);
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

            // Assign Delegate from Function Pointer
            TargetTypeConstructorIL.Emit(OpCodes.Ldarg_0); // This is for storing field delegate, it needs the "this" reference
            TargetTypeConstructorIL.Emit(OpCodes.Ldarg_0);
            TargetTypeConstructorIL.Emit(OpCodes.Ldstr, entrypointName);
            TargetTypeConstructorIL.EmitCall(OpCodes.Call, loadFuncMethod.MakeGenericMethod(delegateBuilderType), null);
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
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Ldfld, delegateField);
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

            var attributeConstructors = typeof(UnmanagedFunctionPointerAttribute).GetConstructors();
            var functionPointerAttributeBuilder = new CustomAttributeBuilder
            (
                attributeConstructors[1],
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
