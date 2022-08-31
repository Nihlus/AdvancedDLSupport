//
//  IndirectCallMethodImplementationGenerator.cs
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
using System.Runtime.InteropServices;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;
using StrictEmit;

using static AdvancedDLSupport.ImplementationGenerators.GeneratorComplexity;
using static AdvancedDLSupport.ImplementationOptions;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates <see cref="OpCodes.Calli"/>-based implementations for methods.
    /// </summary>
    internal sealed class IndirectCallMethodImplementationGenerator : ImplementationGeneratorBase<IntrospectiveMethodInfo>
    {
        /// <inheritdoc/>
        public override GeneratorComplexity Complexity => OptionDependent | Terminating;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndirectCallMethodImplementationGenerator"/> class.
        /// </summary>
        /// <param name="targetModule">The module in which the method implementation should be generated.</param>
        /// <param name="targetType">The type in which the method implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        /// <param name="options">The configuration object to use.</param>
        public IndirectCallMethodImplementationGenerator
        (
            [NotNull] ModuleBuilder targetModule,
            [NotNull] TypeBuilder targetType,
            [NotNull] ILGenerator targetTypeConstructorIL,
            ImplementationOptions options
        )
            : base(targetModule, targetType, targetTypeConstructorIL, options)
        {
        }

        /// <inheritdoc/>
        public override bool IsApplicable(IntrospectiveMethodInfo member)
        {
            return Options.HasFlagFast(UseIndirectCalls);
        }

        /// <inheritdoc />
        public override IEnumerable<PipelineWorkUnit<IntrospectiveMethodInfo>> GenerateImplementation(PipelineWorkUnit<IntrospectiveMethodInfo> workUnit)
        {
            var definition = workUnit.Definition;

            var backingFieldType = typeof(IntPtr);

            var backingField = Options.HasFlagFast(UseLazyBinding)
            ? TargetType.DefineField
            (
                $"{workUnit.GetUniqueBaseMemberName()}_ptr_lazy",
                typeof(Lazy<>).MakeGenericType(backingFieldType),
                FieldAttributes.Private | FieldAttributes.InitOnly
            )
            : TargetType.DefineField
            (
                $"{workUnit.GetUniqueBaseMemberName()}_ptr",
                backingFieldType,
                FieldAttributes.Private | FieldAttributes.InitOnly
            );

            AugmentHostingTypeConstructorWithNativeInitialization(workUnit.SymbolName, backingFieldType, backingField);
            GenerateNativeInvokerBody(definition, definition.GetNativeCallingConvention(), backingField);

            yield break;
        }

        /// <summary>
        /// Augments the hosting type constructor with the logic required to initialize the backing pointer field.
        /// </summary>
        /// <param name="entrypointName">The name of the entry point.</param>
        /// <param name="backingFieldType">The type of the backing field.</param>
        /// <param name="backingField">The backing pointer field.</param>
        private void AugmentHostingTypeConstructorWithNativeInitialization
        (
            [NotNull] string entrypointName,
            [NotNull] Type backingFieldType,
            [NotNull] FieldInfo backingField
        )
        {
            TargetTypeConstructorIL.EmitLoadArgument(0);
            TargetTypeConstructorIL.EmitLoadArgument(0);

            if (Options.HasFlagFast(UseLazyBinding))
            {
                var lambdaBuilder = GenerateSymbolLoadingLambda(entrypointName);
                GenerateLazyLoadedObject(lambdaBuilder, backingFieldType);
            }
            else
            {
                var loadPointerMethod = typeof(NativeLibraryBase).GetMethod
                (
                    nameof(NativeLibraryBase.LoadSymbol),
                    BindingFlags.NonPublic | BindingFlags.Instance
                );

                TargetTypeConstructorIL.EmitConstantString(entrypointName);
                TargetTypeConstructorIL.EmitCallDirect(loadPointerMethod);
            }

            TargetTypeConstructorIL.EmitSetField(backingField);
        }

        /// <summary>
        /// Generates the method body for a native calli invocation.
        /// </summary>
        /// <param name="method">The method to generate the body for.</param>
        /// <param name="callingConvention">The unmanaged calling convention to use.</param>
        /// <param name="backingField">The backing field.</param>
        private void GenerateNativeInvokerBody
        (
            [NotNull] IntrospectiveMethodInfo method,
            CallingConvention callingConvention,
            [NotNull] FieldInfo backingField
        )
        {
            if (!(method.GetWrappedMember() is MethodBuilder builder))
            {
                throw new ArgumentNullException(nameof(method), "Could not unwrap introspective method to method builder.");
            }

            // Let's create a method that simply invoke the delegate
            var methodIL = builder.GetILGenerator();

            for (short p = 1; p <= method.ParameterTypes.Count; p++)
            {
                methodIL.EmitLoadArgument(p);
            }

            GenerateSymbolPush(methodIL, backingField);

            methodIL.EmitCalli(callingConvention, method.ReturnType, method.ParameterTypes.ToArray());
            methodIL.EmitReturn();
        }
    }
}
