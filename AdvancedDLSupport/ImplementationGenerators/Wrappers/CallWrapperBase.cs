//
//  CallWrapperBase.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using StrictEmit;
using static System.Reflection.MethodAttributes;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Base class for call wrappers. This class implements some base functionality which allows inheritors to abstain
    /// from emitting prologues or epilogues.
    /// </summary>
    [PublicAPI]
    public abstract class CallWrapperBase : ImplementationGeneratorBase<IntrospectiveMethodInfo>, ICallWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallWrapperBase"/> class.
        /// </summary>
        /// <param name="targetModule">The module where the implementation should be generated.</param>
        /// <param name="targetType">The type in which the implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        protected CallWrapperBase
        (
            [NotNull] ModuleBuilder targetModule,
            [NotNull] TypeBuilder targetType,
            [NotNull] ILGenerator targetTypeConstructorIL,
            ImplementationOptions options
        )
            : base(targetModule, targetType, targetTypeConstructorIL, options)
        {
        }

        /// <summary>
        /// Emits any additional types that a work unit requires. By default, this does nothing.
        /// </summary>
        /// <param name="module">The module to emit the types in.</param>
        /// <param name="workUnit">The unit to generate the types from.</param>
        public virtual void EmitAdditionalTypes
        (
            [NotNull] ModuleBuilder module,
            [NotNull] PipelineWorkUnit<IntrospectiveMethodInfo> workUnit
        )
        {
        }

        /// <inheritdoc />
        public sealed override IEnumerable<PipelineWorkUnit<IntrospectiveMethodInfo>> GenerateImplementation
        (
            PipelineWorkUnit<IntrospectiveMethodInfo> workUnit
        )
        {
            var definition = workUnit.Definition;

            if (!(definition.GetWrappedMember() is MethodBuilder builder))
            {
                throw new ArgumentNullException(nameof(workUnit), "Could not unwrap introspective method to method builder.");
            }

            EmitAdditionalTypes(TargetModule, workUnit);
            var passthroughMethod = GeneratePassthroughDefinition(workUnit);

            var il = builder.GetILGenerator();

            EmitPrologue(il, workUnit);
            il.EmitCallDirect(passthroughMethod.GetWrappedMember());
            EmitEpilogue(il, workUnit);
            il.EmitReturn();

            // Pass through the method
            yield return new PipelineWorkUnit<IntrospectiveMethodInfo>(passthroughMethod, workUnit);
        }

        /// <summary>
        /// Generates the method that should be passed through for further processing.
        /// </summary>
        /// <param name="workUnit">The original definition.</param>
        /// <returns>The passthrough method.</returns>
        [NotNull]
        public virtual IntrospectiveMethodInfo GeneratePassthroughDefinition([NotNull] PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            var definition = workUnit.Definition;

            var passthroughMethod = TargetType.DefineMethod
            (
                $"{workUnit.GetUniqueBaseMemberName()}_wrapper",
                Private | Virtual | HideBySig,
                CallingConventions.Standard,
                definition.ReturnType,
                definition.ParameterTypes.ToArray()
            );

            passthroughMethod.ApplyCustomAttributesFrom(workUnit.Definition);

            return new IntrospectiveMethodInfo
            (
                passthroughMethod,
                definition.ReturnType,
                definition.ParameterTypes,
                definition.MetadataType,
                definition
            );
        }

        /// <inheritdoc />
        public virtual void EmitPrologue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            // Load the "this" reference
            il.EmitLoadArgument(0);

            for (short i = 1; i <= workUnit.Definition.ParameterTypes.Count; ++i)
            {
                il.EmitLoadArgument(i);
            }
        }

        /// <inheritdoc />
        public virtual void EmitEpilogue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
        }
    }
}
