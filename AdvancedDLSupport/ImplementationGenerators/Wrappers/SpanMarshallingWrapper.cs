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
            IntrospectiveMethodInfo definition = workUnit.Definition;

            Debug.Assert(
                definition.ReturnType.IsGenericType &&
                definition.ReturnType.GetGenericTypeDefinition() == typeof(Span<>),
                $"{nameof(workUnit)} signature is invalid, must be of type Span<> ");

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

            il.Emit(OpCodes.Ldc_I4, ExtractInt32FromReturnsSpanAttribute(workUnit.Definition));
            il.Emit(OpCodes.Newobj, ctor);
        }

        private int ExtractInt32FromReturnsSpanAttribute(IntrospectiveMethodInfo info)
        {
            IReadOnlyList<CustomAttributeData> attributes = info.ReturnParameterCustomAttributes;
            ReturnsSizedSpanAttribute attr = null;

            foreach (CustomAttributeData customAttributeData in attributes)
            {
                if (customAttributeData.AttributeType == typeof(ReturnsSizedSpanAttribute))
                {
                    attr = customAttributeData.ToInstance<ReturnsSizedSpanAttribute>();
                }
            }

            Debug.Assert(attr != null, "Attribute was null");

            if (attr.SpanLength is null)
            {
                throw new NotImplementedException();

                Debug.Assert(attr.MethodName != null, $"Both {nameof(attr.SpanLength)} and {attr.MethodName} were null");

                MethodInfo method = info.DeclaringType.GetMethod(attr.MethodName);

                Debug.Assert(method.ReturnType == typeof(int), "Passed method doesn't return int"); // TODO proper handling
                Debug.Assert(method.GetParameters().Length == 0, "Passed method takes parameters"); // TODO proper handling

                var del = (Func<int>)method.CreateDelegate(typeof(Func<int>));
                return del();
            }
            else
            {
                return attr.SpanLength.Value;
            }
        }

        /// <inheritdoc />
        public override GeneratorComplexity Complexity => GeneratorComplexity.MemberDependent; // TODO correct val

        /// <inheritdoc />
        public override bool IsApplicable(IntrospectiveMethodInfo member)
            => member.ReturnParameterHasCustomAttribute<ReturnsSizedSpanAttribute>()
               && member.ReturnType.IsGenericType // prevents exception on the line below
               && member.ReturnType.GetGenericTypeDefinition() == typeof(Span<>);
    }
}
