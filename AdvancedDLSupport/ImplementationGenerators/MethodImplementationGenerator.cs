//
//  MethodImplementationGenerator.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;
using StrictEmit;
using static AdvancedDLSupport.ImplementationOptions;
using static System.Reflection.CallingConventions;
using static System.Reflection.MethodAttributes;
using static System.Reflection.MethodImplAttributes;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates implementations for methods.
    /// </summary>
    internal sealed class MethodImplementationGenerator : ImplementationGeneratorBase<IntrospectiveMethodInfo>
    {
        [CanBeNull]
        private readonly MethodInfo _calliOverload;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodImplementationGenerator"/> class.
        /// </summary>
        /// <param name="targetModule">The module in which the method implementation should be generated.</param>
        /// <param name="targetType">The type in which the method implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        public MethodImplementationGenerator
        (
            [NotNull] ModuleBuilder targetModule,
            [NotNull] TypeBuilder targetType,
            [NotNull] ILGenerator targetTypeConstructorIL,
            ImplementationOptions options
        )
            : base(targetModule, targetType, targetTypeConstructorIL, options)
        {
            _calliOverload = typeof(ILGenerator).GetMethod
            (
                nameof(ILGenerator.EmitCalli),
                new[] { typeof(OpCode), typeof(CallingConvention), typeof(Type), typeof(Type[]) }
            );
        }

        /// <inheritdoc />
        protected override void GenerateImplementation(IntrospectiveMethodInfo method, string symbolName, string uniqueMemberIdentifier)
        {
            var definition = GenerateInvokerDefinition(method);

            GenerateImplementationForDefinition(definition, symbolName, uniqueMemberIdentifier);

            TargetType.DefineMethodOverride(definition.GetWrappedMember(), method.GetWrappedMember());
        }

        /// <inheritdoc />
        public override IntrospectiveMethodInfo GenerateImplementationForDefinition(IntrospectiveMethodInfo definition, string symbolName, string uniqueMemberIdentifier)
        {
            var metadataAttribute = definition.GetCustomAttribute<NativeSymbolAttribute>() ??
                                    new NativeSymbolAttribute(definition.Name);

            if (Options.HasFlagFast(UseIndirectCalls))
            {
                var backingFieldType = typeof(IntPtr);

                var backingField = Options.HasFlagFast(UseLazyBinding)
                ? TargetType.DefineField
                (
                    $"{uniqueMemberIdentifier}_ptr_lazy",
                    typeof(Lazy<>).MakeGenericType(backingFieldType),
                    FieldAttributes.Private | FieldAttributes.InitOnly
                )
                : TargetType.DefineField
                (
                    $"{uniqueMemberIdentifier}_ptr",
                    backingFieldType,
                    FieldAttributes.Private | FieldAttributes.InitOnly
                );

                AugmentHostingTypeConstructorWithNativeInitialization(symbolName, backingFieldType, backingField);
                GenerateNativeInvokerBody(definition, metadataAttribute.CallingConvention, backingFieldType, backingField);
            }
            else
            {
                var delegateBuilder = GenerateDelegateType(definition, uniqueMemberIdentifier, metadataAttribute.CallingConvention);

                // Create a delegate field
                var backingFieldType = delegateBuilder.CreateTypeInfo();

                var backingField = Options.HasFlagFast(UseLazyBinding)
                ? TargetType.DefineField
                (
                    $"{uniqueMemberIdentifier}_delegate_lazy",
                    typeof(Lazy<>).MakeGenericType(backingFieldType),
                    FieldAttributes.Private | FieldAttributes.InitOnly
                )
                : TargetType.DefineField
                (
                    $"{uniqueMemberIdentifier}_delegate",
                    backingFieldType,
                    FieldAttributes.Private | FieldAttributes.InitOnly
                );

                AugmentHostingTypeConstructorWithDelegateInitialization(symbolName, backingFieldType, backingField);
                GenerateDelegateInvokerBody(definition, backingFieldType, backingField);
            }

            return definition;
        }

        private void AugmentHostingTypeConstructorWithNativeInitialization
        (
            [NotNull] string entrypointName,
            [NotNull] Type backingFieldType,
            [NotNull] FieldInfo backingField
        )
        {
            TargetTypeConstructorIL.EmitLoadArgument(0);
            TargetTypeConstructorIL.EmitLoadArgument(0);

            if (Options.HasFlagFast(UseLazyBinding))
            {
                var lambdaBuilder = GenerateSymbolLoadingLambda(entrypointName);
                GenerateLazyLoadedObject(lambdaBuilder, backingFieldType);
            }
            else
            {
                var loadPointerMethod = typeof(NativeLibraryBase).GetMethod
                (
                    nameof(NativeLibraryBase.LoadSymbol),
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                TargetTypeConstructorIL.EmitConstantString(entrypointName);
                TargetTypeConstructorIL.EmitCallDirect(loadPointerMethod);
            }

            TargetTypeConstructorIL.EmitSetField(backingField);
        }

        /// <summary>
        /// Augments the hosting type constructor with the logic required to initialize the backing delegate field.
        /// </summary>
        /// <param name="entrypointName">The name of the entry point.</param>
        /// <param name="backingFieldType">The type of the backing field.</param>
        /// <param name="backingField">The backing delegate field.</param>
        private void AugmentHostingTypeConstructorWithDelegateInitialization
        (
            [NotNull] string entrypointName,
            [NotNull] Type backingFieldType,
            [NotNull] FieldInfo backingField
        )
        {
            var loadFunctionMethod = typeof(NativeLibraryBase).GetMethod
            (
                nameof(NativeLibraryBase.LoadFunction),
                BindingFlags.NonPublic | BindingFlags.Instance
            ).MakeGenericMethod(backingFieldType);

            TargetTypeConstructorIL.EmitLoadArgument(0);
            TargetTypeConstructorIL.EmitLoadArgument(0);

            if (Options.HasFlagFast(UseLazyBinding))
            {
                var lambdaBuilder = GenerateFunctionLoadingLambda(backingFieldType, entrypointName);
                GenerateLazyLoadedObject(lambdaBuilder, backingFieldType);
            }
            else
            {
                TargetTypeConstructorIL.EmitConstantString(entrypointName);
                TargetTypeConstructorIL.EmitCallDirect(loadFunctionMethod);
            }

            TargetTypeConstructorIL.EmitSetField(backingField);
        }

        /// <summary>
        /// Generates a method that invokes the method's delegate.
        /// </summary>
        /// <param name="methodDefinition">The method to invoke.</param>
        /// <returns>The generated invoker.</returns>
        [NotNull]
        private IntrospectiveMethodInfo GenerateInvokerDefinition([NotNull] IntrospectiveMethodInfo methodDefinition)
        {
            var methodBuilder = TargetType.DefineMethod
            (
                methodDefinition.Name,
                Public | Final | Virtual | HideBySig | NewSlot,
                Standard,
                methodDefinition.ReturnType,
                methodDefinition.ParameterTypes.ToArray()
            );

            return new IntrospectiveMethodInfo(methodBuilder, methodDefinition.ReturnType, methodDefinition.ParameterTypes, methodDefinition);
        }

        /// <summary>
        /// Generates the method body for a delegate invoker.
        /// </summary>
        /// <param name="method">The method to generate the body for.</param>
        /// <param name="delegateBuilderType">The type of the method delegate.</param>
        /// <param name="delegateField">The delegate field.</param>
        private void GenerateDelegateInvokerBody
        (
            [NotNull] IntrospectiveMethodInfo method,
            [NotNull] Type delegateBuilderType,
            [NotNull] FieldInfo delegateField
        )
        {
            if (!(method.GetWrappedMember() is MethodBuilder builder))
            {
                throw new ArgumentNullException(nameof(method), "Could not unwrap introspective method to method builder.");
            }

            // Let's create a method that simply invoke the delegate
            var methodIL = builder.GetILGenerator();

            if (Options.HasFlagFast(GenerateDisposalChecks))
            {
                EmitDisposalCheck(methodIL);
            }

            GenerateSymbolPush(methodIL, delegateField);

            for (int p = 1; p <= method.ParameterTypes.Count; p++)
            {
                methodIL.Emit(OpCodes.Ldarg, p);
            }

            methodIL.EmitCall(OpCodes.Call, delegateBuilderType.GetMethod("Invoke"), null);
            methodIL.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Generates the method body for a native calli invocation.
        /// </summary>
        /// <param name="method">The method to generate the body for.</param>
        /// <param name="callingConvention">The unmanaged calling convention to use.</param>
        /// <param name="backingFieldType">The type of the backing field.</param>
        /// <param name="backingField">The backing field.</param>
        private void GenerateNativeInvokerBody
        (
            [NotNull] IntrospectiveMethodInfo method,
            CallingConvention callingConvention,
            [NotNull] Type backingFieldType,
            [NotNull] FieldInfo backingField
        )
        {
            if (!(method.GetWrappedMember() is MethodBuilder builder))
            {
                throw new ArgumentNullException(nameof(method), "Could not unwrap introspective method to method builder.");
            }

            // Let's create a method that simply invoke the delegate
            var methodIL = builder.GetILGenerator();

            if (Options.HasFlagFast(GenerateDisposalChecks))
            {
                EmitDisposalCheck(methodIL);
            }

            for (short p = 1; p <= method.ParameterTypes.Count; p++)
            {
                methodIL.EmitLoadArgument(p);
            }

            GenerateSymbolPush(methodIL, backingField);

            // HACK: Workaround for missing overload of EmitCalli
            if (_calliOverload is null)
            {
                // Use the existing overload - things may break
                methodIL.EmitCalli(OpCodes.Calli, Standard, method.ReturnType, method.ParameterTypes.ToArray(), null);
            }
            else
            {
                // Use the correct overload via reflection
                _calliOverload.Invoke(methodIL, new object[] { OpCodes.Calli, callingConvention, method.ReturnType, method.ParameterTypes.ToArray() });
            }

            methodIL.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Generates a delegate type for the given method.
        /// </summary>
        /// <param name="methodInfo">The method to generate a delegate type for.</param>
        /// <param name="memberIdentifier">The member identifier to use for name generation.</param>
        /// <param name="callingConvention">The unmanaged calling convention of the delegate.</param>
        /// <returns>A delegate type.</returns>
        [NotNull]
        private TypeBuilder GenerateDelegateType
        (
            [NotNull] IntrospectiveMethodInfo methodInfo,
            [NotNull] string memberIdentifier,
            CallingConvention callingConvention
        )
        {
            // Declare a delegate type
            var delegateBuilder = TargetModule.DefineType
            (
                $"{memberIdentifier}_dt",
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
                new object[] { callingConvention },
                new[] { typeof(UnmanagedFunctionPointerAttribute).GetField(nameof(UnmanagedFunctionPointerAttribute.SetLastError)) },
                new object[] { true }
            );

            delegateBuilder.SetCustomAttribute(functionPointerAttributeBuilder);
            foreach (var attribute in methodInfo.CustomAttributes)
            {
                delegateBuilder.SetCustomAttribute(attribute.GetAttributeBuilder());
            }

            var delegateCtorBuilder = delegateBuilder.DefineConstructor
            (
                RTSpecialName | HideBySig | Public,
                Standard,
                new[] { typeof(object), typeof(IntPtr) }
            );

            delegateCtorBuilder.SetImplementationFlags(Runtime | Managed);

            var delegateMethodBuilder = delegateBuilder.DefineMethod
            (
                "Invoke",
                Public | HideBySig | NewSlot | Virtual,
                methodInfo.ReturnType,
                methodInfo.ParameterTypes.ToArray()
            );

            delegateMethodBuilder.ApplyCustomAttributesFrom(methodInfo);

            delegateMethodBuilder.SetImplementationFlags(Runtime | Managed);
            return delegateBuilder;
        }
    }
}
