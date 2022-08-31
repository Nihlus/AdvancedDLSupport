//
//  ILGeneratorExtensions.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
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
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace AdvancedDLSupport.Extensions;

/// <summary>
/// Extension methods for the <see cref="ILGenerator"/> class.
/// </summary>
internal static class ILGeneratorExtensions
{
    /// <summary>
    /// Holds the real EmitCalli overload, if it exists on this runtime.
    /// </summary>
    private static readonly Action<ILGenerator, OpCode, CallingConvention, Type, Type[]>? RealEmitCalli;

    /// <summary>
    /// Holds a delegate wrapping an action to retrieve an unmanaged signature helper.
    /// </summary>
    private static readonly Func<CallingConvention, Type, SignatureHelper>? GetMethodSignatureHelper;

    static ILGeneratorExtensions()
    {
        var generatorType = typeof(ILGenerator);

        var realEmitCalli = generatorType.GetMethod
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

        var getMethodSignatureHelper = typeof(SignatureHelper).GetMethod
        (
            nameof(SignatureHelper.GetMethodSigHelper),
            new[] { typeof(CallingConvention), typeof(Type) }
        );

        if (getMethodSignatureHelper is null)
        {
            return;
        }

        var getHelperDelegateType = typeof(Func<CallingConvention, Type, SignatureHelper>);
        GetMethodSignatureHelper = (Func<CallingConvention, Type, SignatureHelper>)Delegate.CreateDelegate
        (
            getHelperDelegateType,
            getMethodSignatureHelper
        );
    }

    /// <summary>
    /// Emits the IL required to perform an unmanaged indirect call to a method with the given signature.
    /// </summary>
    /// <param name="this">The generator to use.</param>
    /// <param name="callingConvention">The unmanaged calling convention to use.</param>
    /// <param name="returnType">The method's signature.</param>
    /// <param name="parameterTypes">The method's parameter types.</param>
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

        if (GetMethodSignatureHelper is null)
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
