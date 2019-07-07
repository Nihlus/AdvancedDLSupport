//
//  ILGeneratorExtensions.cs
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
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="ILGenerator"/> class.
    /// </summary>
    internal static class ILGeneratorExtensions
    {
        /// <summary>
        /// Holds the real EmitCalli overload, if it exists on this runtime.
        /// </summary>
        [CanBeNull]
        private static readonly Action<ILGenerator, OpCode, CallingConvention, Type, Type[]> RealEmitCalli;

        /// <summary>
        /// Holds a delegate wrapping the internal UpdateStackSize method.
        /// </summary>
        [CanBeNull]
        private static readonly Action<ILGenerator, OpCode, int> UpdateStackSize;

        /// <summary>
        /// Holds a delegate wrapping the internal EnsureCapacity method.
        /// </summary>
        [CanBeNull]
        private static readonly Action<ILGenerator, int> EnsureCapacity;

        /// <summary>
        /// Holds a delegate wrapping the internal PutInteger4 method.
        /// </summary>
        [CanBeNull]
        private static readonly Action<ILGenerator, int> PutInteger4;

        /// <summary>
        /// Holds a delegate wrapping the internal GetTokenForSig method.
        /// </summary>
        private static readonly Func<ILGenerator, byte[], int> GetTokenForSig;

        /// <summary>
        /// Holds a delegate wrapping an action to retrieve an unmanaged signature helper.
        /// </summary>
        private static readonly Func<CallingConvention, Type, SignatureHelper> GetMethodSignatureHelper;

        static ILGeneratorExtensions()
        {
            var ilGeneratorType = typeof(ILGenerator);

            var realEmitCalli = ilGeneratorType.GetMethod
            (
                nameof(ILGenerator.EmitCalli),
                new[] { typeof(OpCode), typeof(CallingConvention), typeof(Type), typeof(Type[]) }
            );

            if (!(realEmitCalli is null))
            {
                var delegateType = typeof(Action<ILGenerator, OpCode, CallingConvention, Type, Type[]>);
                RealEmitCalli = (Action<ILGenerator, OpCode, CallingConvention, Type, Type[]>)Delegate.CreateDelegate
                (
                    delegateType,
                    realEmitCalli
                );

                return;
            }

            var updateStackSize = ilGeneratorType.GetMethod
            (
                "UpdateStackSize",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(OpCode), typeof(int) },
                null
            );

            var ensureCapacity = ilGeneratorType.GetMethod
            (
                "EnsureCapacity",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(int) },
                null
            );

            var putInteger4 = ilGeneratorType.GetMethod
            (
                "PutInteger4",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(int) },
                null
            );

            var getMethodSignatureHelper = typeof(SignatureHelper).GetMethod
            (
                nameof(SignatureHelper.GetMethodSigHelper),
                new[] { typeof(CallingConvention), typeof(Type) }
            );

            var getTokenForSig = ilGeneratorType.GetMethod
            (
                "GetTokenForSig",
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(byte[]) },
                null
            );

            var lacksAnyRequiredMethod = updateStackSize is null ||
                                         ensureCapacity is null ||
                                         putInteger4 is null ||
                                         getMethodSignatureHelper is null ||
                                         getTokenForSig is null;

            if (lacksAnyRequiredMethod)
            {
                return;
            }

            var updateStackSizeDelegateType = typeof(Action<ILGenerator, OpCode, int>);
            UpdateStackSize = (Action<ILGenerator, OpCode, int>)Delegate.CreateDelegate
            (
                updateStackSizeDelegateType,
                updateStackSize
            );

            var simpleIntDelegateType = typeof(Action<ILGenerator, int>);
            EnsureCapacity = (Action<ILGenerator, int>)Delegate.CreateDelegate(simpleIntDelegateType, ensureCapacity);
            PutInteger4 = (Action<ILGenerator, int>)Delegate.CreateDelegate(simpleIntDelegateType, putInteger4);

            var getHelperDelegateType = typeof(Func<CallingConvention, Type, SignatureHelper>);
            GetMethodSignatureHelper = (Func<CallingConvention, Type, SignatureHelper>)Delegate.CreateDelegate
            (
                getHelperDelegateType,
                getMethodSignatureHelper
            );

            var getTokenForSigDelegateType = typeof(Func<ILGenerator, byte[], int>);
            GetTokenForSig = (Func<ILGenerator, byte[], int>)Delegate.CreateDelegate
            (
                getTokenForSigDelegateType,
                getTokenForSig
            );
        }

        /// <summary>
        /// Emits the IL required to perform an unmanaged indirect call to a method with the given signature.
        /// </summary>
        /// <param name="this">The generator to use.</param>
        /// <param name="callingConvention">The unmanaged calling convention to use.</param>
        /// <param name="returnType">The method's signature.</param>
        /// <param name="parameterTypes">The method's parameter types.</param>
        /// <remarks>This method is based on the .NET Core 2.1 implementation of EmitCalli.</remarks>
        public static void EmitCalli
        (
            this ILGenerator @this,
            CallingConvention callingConvention,
            Type returnType,
            Type[] parameterTypes
        )
        {
            if (!(RealEmitCalli is null))
            {
                RealEmitCalli(@this, OpCodes.Calli, callingConvention, returnType, parameterTypes);
                return;
            }

            if (EnsureCapacity is null || PutInteger4 is null || UpdateStackSize is null || GetMethodSignatureHelper is null)
            {
                throw new PlatformNotSupportedException("Calli is not supported on this runtime.");
            }

            var sig = GetMethodSignatureHelper(callingConvention, returnType);

            if (!(parameterTypes is null))
            {
                foreach (var parameterType in parameterTypes)
                {
                    sig.AddArgument(parameterType);
                }
            }

            @this.Emit(OpCodes.Calli, sig);
        }
    }
}
