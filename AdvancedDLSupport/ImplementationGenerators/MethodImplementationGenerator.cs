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
using AdvancedDLSupport.Reflection;
using AdvancedDLSupport.Extensions;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;

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
    internal class MethodImplementationGenerator : ImplementationGeneratorBase<IntrospectiveMethodInfo>
    {
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
        }

        /// <inheritdoc />
        protected override void GenerateImplementation(IntrospectiveMethodInfo method, string symbolName, string uniqueMemberIdentifier)
        {
            var definition = GenerateDelegateInvokerDefinition(method);

            var metadataAttribute = definition.GetCustomAttribute<NativeSymbolAttribute>() ??
                                    new NativeSymbolAttribute(definition.Name);

            var delegateBuilder = GenerateDelegateType(definition, uniqueMemberIdentifier, metadataAttribute.CallingConvention);

            // Create a delegate field
            var delegateBuilderType = delegateBuilder.CreateTypeInfo();

            var delegateField = Options.HasFlagFast(UseLazyBinding) ?
                TargetType.DefineField($"{uniqueMemberIdentifier}_dt", typeof(Lazy<>).MakeGenericType(delegateBuilderType), FieldAttributes.Public) :
                TargetType.DefineField($"{uniqueMemberIdentifier}_dt", delegateBuilderType, FieldAttributes.Public);

            AugmentHostingTypeConstructor(symbolName, delegateBuilderType, delegateField);

            GenerateDelegateInvokerBody(definition, delegateBuilderType, delegateField);

            TargetType.DefineMethodOverride(definition.GetWrappedMember(), method.GetWrappedMember());
        }

        /// <summary>
        /// Augments the constructor of the hosting type with initialization logic for this method.
        /// </summary>
        /// <param name="entrypointName">The name of the native entry point.</param>
        /// <param name="delegateBuilderType">The type of the method delegate.</param>
        /// <param name="delegateField">The delegate field.</param>
        protected void AugmentHostingTypeConstructor
        (
            [NotNull] string entrypointName,
            [NotNull] Type delegateBuilderType,
            [NotNull] FieldInfo delegateField
        )
        {
            var loadFunc = typeof(AnonymousImplementationBase).GetMethod
            (
                "LoadFunction",
                BindingFlags.NonPublic | BindingFlags.Instance
            ).MakeGenericMethod(delegateBuilderType);

            TargetTypeConstructorIL.Emit(OpCodes.Ldarg_0); // This is for storing field delegate, it needs the "this" reference
            TargetTypeConstructorIL.Emit(OpCodes.Ldarg_0);

            if (Options.HasFlagFast(UseLazyBinding))
            {
                var lambdaBuilder = GenerateFunctionLoadingLambda(delegateBuilderType, entrypointName);
                GenerateLazyLoadedField(lambdaBuilder, delegateBuilderType);
            }
            else
            {
                TargetTypeConstructorIL.Emit(OpCodes.Ldstr, entrypointName);
                TargetTypeConstructorIL.EmitCall(OpCodes.Call, loadFunc, null);
            }

            TargetTypeConstructorIL.Emit(OpCodes.Stfld, delegateField);
        }

        /// <summary>
        /// Generates a method that invokes the method's delegate.
        /// </summary>
        /// <param name="method">The method to invoke.</param>
        /// <returns>The generated invoker.</returns>
        protected IntrospectiveMethodInfo GenerateDelegateInvokerDefinition([NotNull] IntrospectiveMethodInfo method)
        {
            var delegateInvoker = GenerateDelegateInvoker
            (
                method.Name,
                method.ReturnType,
                method.ParameterTypes.ToArray()
            );

            delegateInvoker.CopyCustomAttributesFrom(method, method.ReturnType, method.GetParameters().Select(p => p.ParameterType).ToList());

            return delegateInvoker;
        }

        /// <summary>
        /// Generates a method that invokes the method's delegate.
        /// </summary>
        /// <param name="methodInfo">The method.</param>
        /// <returns>The delegate invoker.</returns>
        [NotNull]
        protected IntrospectiveMethodInfo GenerateDelegateInvokerDefinition
        (
            [NotNull] string methodName,
            [NotNull] Type returnType,
            [NotNull] Type[] parameterTypes
        )
        {
            var methodBuilder = TargetType.DefineMethod
            (
                methodInfo.Name,
                Public | Final | Virtual | HideBySig | NewSlot,
                Standard,
                methodInfo.ReturnType,
                methodInfo.ParameterTypes.ToArray()
            );

            return new IntrospectiveMethodInfo(methodBuilder, returnType, parameterTypes);
        }

        /// <summary>
        /// Generates the method body for a delegate invoker.
        /// </summary>
        /// <param name="method">The method to generate the body for.</param>
        /// <param name="delegateBuilderType">The type of the method delegate.</param>
        /// <param name="delegateField">The delegate field.</param>
        protected void GenerateDelegateInvokerBody
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
        /// Generates a delegate type for the given method.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="memberIdentifier">The member identifier to use for name generation.</param>
        /// <returns>A delegate type.</returns>
        [NotNull]
        protected TypeBuilder GenerateDelegateType
        (
            [NotNull] IntrospectiveMethodInfo method,
            [NotNull] string memberIdentifier
        )
        {
            var metadataAttribute = method.GetCustomAttribute<NativeSymbolAttribute>() ??
                                    new NativeSymbolAttribute(method.Name);

            return GenerateDelegateType
            (
                method.ReturnType,
                method.ParameterTypes.ToArray(),
                memberIdentifier,
                metadataAttribute.CallingConvention
            );
        }

        /// <summary>
        /// Generates a delegate type for the given method.
        /// </summary>
        /// <param name="methodInfo">The method to generate a delegate type for.</param>
        /// <param name="memberIdentifier">The member identifier to use for name generation.</param>
        /// <param name="callingConvention">The unmanaged calling convention of the delegate.</param>
        /// <returns>A delegate type.</returns>
        [NotNull]
        protected TypeBuilder GenerateDelegateType
        (
            [NotNull] TransientMethodInfo methodInfo,
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
                new object[] { callingConvention }
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

            delegateMethodBuilder.CopyCustomAttributesFrom(methodInfo, methodInfo.ReturnType, methodInfo.ParameterTypes);

            delegateMethodBuilder.SetImplementationFlags(Runtime | Managed);
            return delegateBuilder;
        }
    }
}
