//
//  GenericMethodImplementationGenerator.cs
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
    internal sealed class GenericMethodImplementationGenerator : ImplementationGeneratorBase<IntrospectiveMethodInfo>
    {
        /// <remarks>
        /// <see cref="GeneratorComplexity.Terminating"/> is intentionally not included here, since it's technically not
        /// a terminating generator. Instead, <see cref="GeneratorComplexity.DeferredImplementation"/> is used here.
        /// </remarks>
        /// <inheritdoc/>
        public override GeneratorComplexity Complexity =>
            MemberDependent | TransformsParameters | CreatesTypes | DeferredImplementation;

        private FieldInfo _genericJitEmitter;

        private MethodInfo _getImplementationMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericMethodImplementationGenerator"/> class.
        /// </summary>
        /// <param name="targetModule">The module where the implementation should be generated.</param>
        /// <param name="targetType">The type in which the implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        public GenericMethodImplementationGenerator
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
        public override IEnumerable<PipelineWorkUnit<IntrospectiveMethodInfo>> GenerateImplementation(PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            var definition = workUnit.Definition;

            if (!(definition.GetWrappedMember() is MethodBuilder builder))
            {
                throw new ArgumentNullException(nameof(workUnit), "Could not unwrap introspective method to method builder.");
            }

            var il = builder.GetILGenerator();

            // Create the argument list as an array
            var argumentArray = il.DeclareLocal(typeof(object[]));

            if (definition.ParameterTypes.Count > 0)
            {
                il.EmitConstantInt(definition.ParameterTypes.Count);
                il.EmitNewArray<object>();
            }
            else
            {
                il.EmitLoadNull();
            }

            il.EmitSetLocalVariable(argumentArray);

            // Load the parameter values into the array
            for (short i = 1; i <= definition.ParameterTypes.Count; ++i)
            {
                var parameterType = definition.ParameterTypes[i - 1];
                il.EmitLoadLocalVariable(argumentArray);
                il.EmitConstantInt(i - 1);
                il.EmitLoadArgument(i);

                if (parameterType.IsGenericParameter)
                {
                    // Check constraints for boxing needs
                    if (!parameterType.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                    {
                        il.EmitBox(parameterType);
                    }
                }
                else if (parameterType.IsValueType)
                {
                    il.EmitBox(parameterType);
                }

                il.EmitSetArrayElement<object>();
            }

            // Create the type argument list as an array
            var genericTypeArgumentsArray = il.DeclareLocal(typeof(Type[]));
            il.EmitConstantInt(definition.GenericArguments.Count);
            il.EmitNewArray<Type>();
            il.EmitSetLocalVariable(genericTypeArgumentsArray);

            for (short i = 0; i < definition.GenericArguments.Count; ++i)
            {
                il.EmitLoadLocalVariable(genericTypeArgumentsArray);
                il.EmitConstantInt(i);
                il.EmitTypeOf(definition.GenericArguments[i]);
                il.EmitSetArrayElement<Type>();
            }

            // this
            il.EmitLoadArgument(0);
            il.EmitLoadField(_genericJitEmitter);

            // methodInfo
            il.EmitCallDirect<MethodBase>(nameof(MethodBase.GetCurrentMethod));

            // convert the open MethodInfo to a closed MethodInfo
            il.EmitLoadLocalVariable(genericTypeArgumentsArray);
            il.EmitCallVirtual<MethodInfo>(nameof(MethodInfo.MakeGenericMethod), typeof(Type[]));

            // and then over to an introspective method info
            il.EmitNewObject<IntrospectiveMethodInfo>(typeof(MethodInfo));

            // entry point
            il.EmitConstantString(workUnit.SymbolName);

            // libraryPath
            il.EmitLoadArgument(0);
            il.EmitGetProperty<NativeLibraryBase>(nameof(NativeLibraryBase.LibraryPath), BindingFlags.NonPublic | BindingFlags.Instance);

            // arguments
            il.EmitLoadLocalVariable(argumentArray);

            var parameterTypes = new[]
            {
                typeof(IntrospectiveMethodInfo),
                typeof(string),
                typeof(string),
                typeof(object[])
            };

            il.EmitCallDirect<JustInTimeGenericEmitter>(nameof(JustInTimeGenericEmitter.InvokeClosedImplementation), parameterTypes);

            if (definition.ReturnType == typeof(void))
            {
                il.EmitPop();
            }
            else
            {
                il.EmitUnboxAny(definition.ReturnType);
            }

            il.EmitReturn();

            yield break;
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
            constructorIL.EmitLoadArgument(0);
            constructorIL.EmitNewObject<JustInTimeGenericEmitter>();
            constructorIL.EmitSetField(_genericJitEmitter);
        }
    }
}
