//
//  DisposalCallWrapper.cs
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

using System.Reflection.Emit;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;

using static AdvancedDLSupport.ImplementationGenerators.GeneratorComplexity;
using static AdvancedDLSupport.ImplementationOptions;
using static System.Reflection.BindingFlags;

namespace AdvancedDLSupport.ImplementationGenerators;

/// <summary>
/// Generates a disposal check before the method call.
/// </summary>
internal sealed class DisposalCallWrapper : CallWrapperBase
{
    /// <inheritdoc />
    public override GeneratorComplexity Complexity => OptionDependent;

    /// <summary>
    /// Initializes a new instance of the <see cref="DisposalCallWrapper"/> class.
    /// </summary>
    /// <param name="targetModule">The module where the implementation should be generated.</param>
    /// <param name="targetType">The type in which the implementation should be generated.</param>
    /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
    /// <param name="options">The configuration object to use.</param>
    public DisposalCallWrapper
    (
        ModuleBuilder targetModule,
        TypeBuilder targetType,
        ILGenerator targetTypeConstructorIL,
        ImplementationOptions options
    )
        : base
        (
            targetModule,
            targetType,
            targetTypeConstructorIL,
            options
        )
    {
    }

    /// <inheritdoc />
    public override bool IsApplicable(IntrospectiveMethodInfo member)
    {
        return Options.HasFlagFast(GenerateDisposalChecks);
    }

    /// <inheritdoc />
    public override void EmitPrologue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
    {
        var throwMethod = typeof(NativeLibraryBase).GetMethod("ThrowIfDisposed", NonPublic | Instance);

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, throwMethod);

        // Emit the parameters as usual
        base.EmitPrologue(il, workUnit);
    }
}
