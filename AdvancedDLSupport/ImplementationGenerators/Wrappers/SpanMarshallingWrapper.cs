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
using StrictEmit;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates wrapper instructions for returning <see cref="Span{T}"/> from unmanaged code
    /// through a pointer and provided length.
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
            Type returnType = workUnit.Definition.ReturnType, newReturnType;
            var definition = workUnit.Definition;

            if (IsSpanType(returnType))
            {
                var genericType = returnType.GenericTypeArguments[0];

                if (IsOrContainsReferences(genericType))
                {
                    // Span<byte> is used because unbound generics are not allowed inside a nameof, and it still results as just 'Span'
                    throw new NotSupportedException($"Method Return Type is a class or contains references to classes and cannot be marshaled as a {nameof(Span<byte>)}. Marshalling {nameof(Span<byte>)}" +
                                                    $"requires the marshaled type T in {nameof(Span<byte>)}<T> to be a {nameof(ValueType)} without class references.");
                }

                newReturnType = genericType.MakePointerType();
            }
            else
            {
                newReturnType = returnType;
            }

            /* TODO? Add marshaling for Span<> params */

            Type[] parametersTypes = definition.ParameterTypes.ToArray();

            for (int i = 0; i < parametersTypes.Length; ++i)
            {
                var paramType = parametersTypes[i];
                if (IsSpanType(paramType))
                {
                    var genericParam = paramType.GenericTypeArguments[0];

                    if (genericParam.IsGenericType)
                    {
                        throw new NotSupportedException("Generic Type found as Span Generic Argument");
                    }

                    if (IsOrContainsReferences(genericParam))
                    {
                        throw new NotSupportedException("Reference or value type containing references found in Span<T> or ReadOnlySpan<T> generic parameter.");
                    }

                    parametersTypes[i] = genericParam.MakePointerType(); // genercParam.MakePointerType();
                }
            }

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

        /// <inheritdoc/>
        public override void EmitPrologue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            var definition = workUnit.Definition;

            var parameterTypes = definition.ParameterTypes;

            il.EmitLoadArgument(0);

            for (short i = 1; i < parameterTypes.Count; ++i)
            {
                var paramType = parameterTypes[i];

                if (IsSpanType(paramType))
                {
                    Debug.Assert(paramType.GenericTypeArguments.Length == 1, "Span Type does not have any generic parameters, CLR bug?");

                    var pinnedLocal = il.DeclareLocal(paramType.GenericTypeArguments[0].MakeByRefType(), true);

                    var getPinnableReferenceMethod = paramType.GetMethod(nameof(Span<byte>.GetPinnableReference), BindingFlags.Public | BindingFlags.Instance);

                    il.EmitLoadArgumentAddress(i);
                    il.EmitCallDirect(getPinnableReferenceMethod);
                    il.EmitDuplicate();
                    il.EmitSetLocalVariable(pinnedLocal);
                    il.EmitConvertToNativeInt();
                }
                else
                {
                    il.EmitLoadArgument(i);
                }
            }
        }

        /// <inheritdoc />
        public override void EmitEpilogue(ILGenerator il, PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            Type returnType = workUnit.Definition.ReturnType;

            if (IsSpanType(returnType))
            {
                il.EmitConstantInt(GetNativeCollectionLengthMetadata(workUnit.Definition).Length);
                il.EmitNewObject(returnType.GetConstructor(new[] { typeof(void*), typeof(int) }));
            }
        }

        private NativeCollectionLengthAttribute GetNativeCollectionLengthMetadata(IntrospectiveMethodInfo info)
        {
            IReadOnlyList<CustomAttributeData> attributes = info.ReturnParameterCustomAttributes;

            foreach (CustomAttributeData customAttributeData in attributes)
            {
                if (customAttributeData.AttributeType == typeof(NativeCollectionLengthAttribute))
                {
                    return customAttributeData.ToInstance<NativeCollectionLengthAttribute>();
                }
            }

            throw new InvalidOperationException($"Method return type does not have required {nameof(NativeCollectionLengthAttribute)}");
        }

        /// <inheritdoc />
        public override GeneratorComplexity Complexity => GeneratorComplexity.TransformsParameters | GeneratorComplexity.MemberDependent;

        /// <inheritdoc />
        public override bool IsApplicable(IntrospectiveMethodInfo member)
        {
            return IsSpanType(member.ReturnType) || member.ParameterTypes.Any(IsSpanType);
        }

        private static bool IsSpanType(Type type)
        {
            if (type.IsGenericType)
            {
                var generic = type.GetGenericTypeDefinition();
                return generic == typeof(Span<>) || generic == typeof(ReadOnlySpan<>);
            }

            return false;
        }

        private static bool IsOrContainsReferences(Type type)
        {
            if (type.IsPrimitive)
            {
                return false;
            }

            if (type.IsClass)
            {
                return true;
            }

            foreach (var field in type.GetFields())
            {
                if (IsOrContainsReferences(field.FieldType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
