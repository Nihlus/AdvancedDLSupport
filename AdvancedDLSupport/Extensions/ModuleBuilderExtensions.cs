//
//  ModuleBuilderExtensions.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extensions methods for the <see cref="ModuleBuilder"/> class.
    /// </summary>
    public static class ModuleBuilderExtensions
    {
        /// <summary>
        /// Defines a delegate type in the given module with the given name and parameters.
        /// </summary>
        /// <param name="module">The module to define the delegate in.</param>
        /// <param name="name">The name of the delegate type.</param>
        /// <param name="baseMember">The base member to take parameter types from.</param>
        /// <param name="suppressSecurity">Whether or not code security should be suppressed on the delegate.</param>
        /// <returns>The delegate type.</returns>
        [NotNull]
        public static TypeBuilder DefineDelegate
        (
            [NotNull] this ModuleBuilder module,
            [NotNull] string name,
            [NotNull] IntrospectiveMethodInfo baseMember,
            bool suppressSecurity = false
        )
        {
            var metadataAttribute = baseMember.GetCustomAttribute<NativeSymbolAttribute>() ??
                                    new NativeSymbolAttribute(baseMember.Name);

            var delegateBuilder = DefineDelegateType
            (
                module,
                name,
                metadataAttribute.CallingConvention,
                suppressSecurity
            );

            foreach (var attribute in baseMember.CustomAttributes)
            {
                delegateBuilder.SetCustomAttribute(attribute.GetAttributeBuilder());
            }

            var delegateInvocationBuilder = DefineDelegateInvocationMethod
            (delegateBuilder, baseMember.ReturnType, baseMember.ParameterTypes.ToArray());

            delegateInvocationBuilder.ApplyCustomAttributesFrom(baseMember);

            return delegateBuilder;
        }

        /// <summary>
        /// Defines a delegate type in the given module with the given name and parameters.
        /// </summary>
        /// <param name="module">The module to define the delegate in.</param>
        /// <param name="name">The name of the delegate type.</param>
        /// <param name="callingConvention">The unmanaged calling convention to use.</param>
        /// <param name="returnType">The return type of the delegate.</param>
        /// <param name="parameterTypes">The parameter types of the delegate.</param>
        /// <param name="suppressSecurity">Whether or not code security should be suppressed on the delegate.</param>
        /// <returns>The delegate type.</returns>
        [NotNull]
        public static TypeBuilder DefineDelegate
        (
            [NotNull] this ModuleBuilder module,
            [NotNull] string name,
            CallingConvention callingConvention,
            [NotNull] Type returnType,
            [NotNull] Type[] parameterTypes,
            bool suppressSecurity = false
            )
        {
            var delegateBuilder = DefineDelegateType
            (
                module,
                name,
                callingConvention,
                suppressSecurity
            );

            DefineDelegateInvocationMethod(delegateBuilder, returnType, parameterTypes);

            return delegateBuilder;
        }

        /// <summary>
        /// Defines a delegate type in the given module with the given name and parameters.
        /// </summary>
        /// <param name="module">The module to define the delegate in.</param>
        /// <param name="name">The name of the delegate type.</param>
        /// <param name="callingConvention">The unmanaged calling convention to use.</param>
        /// <param name="suppressSecurity">Whether or not code security should be suppressed on the delegate.</param>
        /// <returns>The delegate type.</returns>
        [NotNull]
        private static TypeBuilder DefineDelegateType
        (
            [NotNull] ModuleBuilder module,
            [NotNull] string name,
            CallingConvention callingConvention,
            bool suppressSecurity = false)
        {
            var delegateBuilder = module.DefineType
            (
                $"{name}_delegate",
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                typeof(MulticastDelegate)
            );

            var unmanagedPtrAttributeConstructor = typeof(UnmanagedFunctionPointerAttribute).GetConstructors().First
            (
                c =>
                    c.GetParameters().Any() &&
                    c.GetParameters().Length == 1 &&
                    c.GetParameters().First().ParameterType == typeof(CallingConvention)
            );

            var setLastErrorField = typeof(UnmanagedFunctionPointerAttribute)
                .GetField(nameof(UnmanagedFunctionPointerAttribute.SetLastError));

            var functionPointerAttributeBuilder = new CustomAttributeBuilder
            (
                unmanagedPtrAttributeConstructor,
                new object[] { callingConvention },
                new[] { setLastErrorField },
                new object[] { true }
            );

            delegateBuilder.SetCustomAttribute(functionPointerAttributeBuilder);

            if (suppressSecurity)
            {
                var suppressSecurityConstructor = typeof(SuppressUnmanagedCodeSecurityAttribute).GetConstructors().First();

                var suppressSecurityAttributeBuilder = new CustomAttributeBuilder
                (
                    suppressSecurityConstructor,
                    new object[] { }
                );

                delegateBuilder.SetCustomAttribute(suppressSecurityAttributeBuilder);
            }

            var delegateCtorBuilder = delegateBuilder.DefineConstructor
            (
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(object), typeof(IntPtr) }
            );

            delegateCtorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            return delegateBuilder;
        }

        /// <summary>
        /// Defines a delegate invocation method on a delegate type.
        /// </summary>
        /// <param name="delegateBuilder">The delegate type builder.</param>
        /// <param name="returnType">The return type of the method.</param>
        /// <param name="parameterTypes">The parameter types of the method.</param>
        /// <returns>The delegate invocation method.</returns>
        [NotNull]
        private static MethodBuilder DefineDelegateInvocationMethod
        (
            [NotNull] TypeBuilder delegateBuilder,
            [NotNull] Type returnType,
            [NotNull] Type[] parameterTypes
        )
        {
            var delegateMethodBuilder = delegateBuilder.DefineMethod
            (
                "Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                returnType,
                parameterTypes
            );

            delegateMethodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
            return delegateMethodBuilder;
        }
    }
}
