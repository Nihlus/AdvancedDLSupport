//
//  GenericMethodWrapper.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Generics;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;
using StrictEmit;
using static AdvancedDLSupport.ImplementationGenerators.GeneratorComplexity;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates wrapper instructions for marshalling methods with generic type arguments.
    /// </summary>
    internal sealed class GenericMethodWrapper : CallWrapperBase
    {
        /// <inheritdoc/>
        public override GeneratorComplexity Complexity => MemberDependent | Terminating;

        private FieldInfo _genericJitEmitter;

        private MethodInfo _getImplementationMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericMethodWrapper"/> class.
        /// </summary>
        /// <param name="targetModule">The module where the implementation should be generated.</param>
        /// <param name="targetType">The type in which the implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        public GenericMethodWrapper
        (
            [NotNull] ModuleBuilder targetModule,
            [NotNull] TypeBuilder targetType,
            [NotNull] ILGenerator targetTypeConstructorIL,
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
            _genericJitEmitter = AugmentHostingType(targetType);
            AugmentHostingTypeConstructor(targetTypeConstructorIL);
        }

        /// <inheritdoc/>
        public override bool IsApplicable(IntrospectiveMethodInfo member)
        {
            // A member with open generic parameters is considered relevant
            return member.ContainsGenericParameters;
        }

        /// <inheritdoc/>
        public override void EmitPrologue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            var definition = workUnit.Definition;

            // Load the "this" reference
            il.EmitLoadArgument(0);

            for (short i = 1; i <= definition.ParameterTypes.Count; ++i)
            {
                il.EmitLoadArgument(i);
            }

            // Grab the pointer to the actual implementation
            il.EmitLoadField(_genericJitEmitter);
            il.EmitCall();
        }

        /// <inheritdoc/>
        public override void EmitEpilogue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
        }

        /// <summary>
        /// Augments the hosting type, adding a field for the micro-jitter.
        /// </summary>
        /// <param name="targetType">The target type.</param>
        /// <returns>The resulting field.</returns>
        private FieldInfo AugmentHostingType([NotNull] TypeBuilder targetType)
        {
            var field = targetType.DefineField
            (
                $"_genericJitEmitter_{Guid.NewGuid().ToString()}",
                typeof(JustInTimeGenericEmitter),
                FieldAttributes.Private | FieldAttributes.InitOnly
            );

            return field;
        }

        /// <summary>
        /// Augments the hosting type's constructor, initializing the micro-jitter field.
        /// </summary>
        /// <param name="constructorIL">The constructor's IL generator.</param>
        private void AugmentHostingTypeConstructor(ILGenerator constructorIL)
        {
            constructorIL.EmitNewObject<JustInTimeGenericEmitter>();
            constructorIL.EmitSetField(_genericJitEmitter);
        }
    }
}
