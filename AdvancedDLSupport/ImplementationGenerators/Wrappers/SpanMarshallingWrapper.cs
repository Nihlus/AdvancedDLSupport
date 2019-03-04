//
//  SpanMarshallingWrapper.cs
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// TODO.
    /// </summary>
    internal class SpanMarshallingWrapper : CallWrapperBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpanMarshallingWrapper"/> class.
        /// </summary>
        /// <param name="targetModule">The module where the implementation should be generated.</param>
        /// <param name="targetType">The type in which the implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        public SpanMarshallingWrapper
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
        }

        /// <inheritdoc />
        public override IntrospectiveMethodInfo GeneratePassthroughDefinition(PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            if (!workUnit.Definition.ReturnType.IsValueType)
            {
                throw new NotSupportedException($"Method is not a {nameof(ValueType)} and cannot be marshaled as a {typeof(Span<>).Name}");
            }

            IntrospectiveMethodInfo definition = workUnit.Definition;

            Type newReturnType = definition.ReturnType.GenericTypeArguments[0].MakePointerType();

            /* TODO? Add marshaling for Span<> params */

            Type[] parametersTypes = definition.ParameterTypes.ToArray();

            MethodBuilder passthroughMethod = TargetType.DefineMethod
            (
                $"{workUnit.GetUniqueBaseMemberName()}_wrapped",
                MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                CallingConventions.Standard,
                newReturnType,
                parametersTypes
            );

            passthroughMethod.ApplyCustomAttributesFrom(definition, newReturnType, parametersTypes);

            return new IntrospectiveMethodInfo
            (
                passthroughMethod,
                newReturnType,
                parametersTypes,
                definition.MetadataType,
                definition
            );
        }

        /// <inheritdoc />
        public override void EmitEpilogue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            ConstructorInfo ctor = typeof(Span<>).MakeGenericType(workUnit.Definition.ReturnType.GenericTypeArguments[0])
                .GetConstructor(new[] { typeof(void*), typeof(int) });

            int res = ExtractInt32FromReturnsSpanAttribute(workUnit.Definition);

            il.Emit(OpCodes.Ldc_I4, res);
            il.Emit(OpCodes.Newobj, ctor);
        }

        private int ExtractInt32FromReturnsSpanAttribute(IntrospectiveMethodInfo info)
            => GetRetSpanAttr(info).SpanLength;

        private ReturnsSizedSpanAttribute GetRetSpanAttr(IntrospectiveMethodInfo info)
        {
            IReadOnlyList<CustomAttributeData> attributes = info.ReturnParameterCustomAttributes;

            foreach (CustomAttributeData customAttributeData in attributes)
            {
                if (customAttributeData.AttributeType == typeof(ReturnsSizedSpanAttribute))
                {
                    return customAttributeData.ToInstance<ReturnsSizedSpanAttribute>();
                }
            }

            throw new InvalidOperationException($"Method does not have required {nameof(ReturnsSizedSpanAttribute)}");
        }

        /// <inheritdoc />
        public override GeneratorComplexity Complexity => GeneratorComplexity.MemberDependent; // TODO correct val

        /// <inheritdoc />
        public override bool IsApplicable(IntrospectiveMethodInfo member)
            => member.ReturnType.IsGenericType // prevents exception on the line below
               && member.ReturnType.GetGenericTypeDefinition() == typeof(Span<>);
    }
}
