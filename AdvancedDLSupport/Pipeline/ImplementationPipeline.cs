//
//  ImplementationPipeline.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.ImplementationGenerators;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using static System.Reflection.MethodAttributes;

namespace AdvancedDLSupport.Pipeline
{
    /// <summary>
    /// Represents a pipeline which consumes definitions, and processes them to generate a dynamic type.
    /// </summary>
    [PublicAPI]
    public class ImplementationPipeline
    {
        [NotNull]
        private readonly ModuleBuilder _targetModule;

        [NotNull]
        private readonly ILGenerator _constructorIL;

        private readonly ImplementationOptions _options;

        [NotNull]
        private readonly ImplementationGeneratorSorter _generatorSorter;

        [NotNull, ItemNotNull]
        private IReadOnlyList<IImplementationGenerator<IntrospectiveMethodInfo>> _methodGeneratorPipeline;

        [NotNull, ItemNotNull]
        private IReadOnlyList<IImplementationGenerator<IntrospectivePropertyInfo>> _propertyGeneratorPipeline;

        /// <summary>
        /// Gets the target type of the pipeline.
        /// </summary>
        [NotNull]
        internal TypeBuilder TargetType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementationPipeline"/> class.
        /// </summary>
        /// <param name="targetModule">The module to generates any additional types in.</param>
        /// <param name="targetType">The target type to generate implementations in.</param>
        /// <param name="constructorIL">The <see cref="ILGenerator"/> of the target type's constructor.</param>
        /// <param name="options">The implementation options to use.</param>
        public ImplementationPipeline
        (
            [NotNull] ModuleBuilder targetModule,
            [NotNull] TypeBuilder targetType,
            [NotNull] ILGenerator constructorIL,
            ImplementationOptions options
        )
        {
            _targetModule = targetModule;
            TargetType = targetType;
            _constructorIL = constructorIL;
            _options = options;

            _generatorSorter = new ImplementationGeneratorSorter();

            _methodGeneratorPipeline = _generatorSorter.SortGenerators(GetBaselineMethodGenerators()).ToList();
            _propertyGeneratorPipeline = _generatorSorter.SortGenerators(GetBaselinePropertyGenerators()).ToList();
        }

        /// <summary>
        /// Injects a set of method implementation generation stages into the pipeline.
        /// </summary>
        /// <param name="stages">The stages to inject.</param>
        [PublicAPI]
        public void InjectMethodStages([NotNull] params IImplementationGenerator<IntrospectiveMethodInfo>[] stages)
        {
            _methodGeneratorPipeline = _generatorSorter.SortGenerators(GetBaselineMethodGenerators().Concat(stages)).ToList();
        }

        /// <summary>
        /// Gets the baseline set of method implementation generators.
        /// </summary>
        /// <returns>The baseline set.</returns>
        [NotNull, ItemNotNull, Pure]
        private IEnumerable<IImplementationGenerator<IntrospectiveMethodInfo>> GetBaselineMethodGenerators()
        {
            yield return new RefPermutationImplementationGenerator
            (
                _targetModule,
                TargetType,
                _constructorIL,
                _options
            );

            yield return new DelegateMethodImplementationGenerator
            (
                _targetModule,
                TargetType,
                _constructorIL,
                _options
            );

            yield return new IndirectCallMethodImplementationGenerator
            (
                _targetModule,
                TargetType,
                _constructorIL,
                _options
            );

            yield return new BooleanMarshallingWrapper
            (
                _targetModule,
                TargetType,
                _constructorIL,
                _options
            );

            yield return new DisposalCallWrapper
            (
                _targetModule,
                TargetType,
                _constructorIL,
                _options
            );

            yield return new StringMarshallingWrapper
            (
                _targetModule,
                TargetType,
                _constructorIL,
                _options
            );

            yield return new ValueNullableMarshallingWrapper
            (
                _targetModule,
                TargetType,
                _constructorIL,
                _options
            );

            yield return new GenericDelegateWrapper
            (
                _targetModule,
                TargetType,
                _constructorIL,
                _options
            );

            yield return new DelegateWrapper
            (
                _targetModule,
                TargetType,
                _constructorIL,
                _options
            );

            yield return new SpanMarshallingWrapper
            (
                _targetModule,
                TargetType,
                _constructorIL,
                _options
            );
        }

        /// <summary>
        /// Injects a set of property implementation stages into the pipeline.
        /// </summary>
        /// <param name="stages">The stages to inject.</param>
        [PublicAPI]
        public void InjectPropertyStage([NotNull] params IImplementationGenerator<IntrospectivePropertyInfo>[] stages)
        {
            _propertyGeneratorPipeline = _generatorSorter.SortGenerators(GetBaselinePropertyGenerators().Concat(stages)).ToList();
        }

        /// <summary>
        /// Gets the baseline set of property implementation generators.
        /// </summary>
        /// <returns>The baseline set.</returns>
        [NotNull, ItemNotNull, Pure]
        private IEnumerable<IImplementationGenerator<IntrospectivePropertyInfo>> GetBaselinePropertyGenerators()
        {
            yield return new PropertyImplementationGenerator
            (
                _targetModule,
                TargetType,
                _constructorIL,
                _options
            );
        }

        /// <summary>
        /// Generates the definition of the complex method.
        /// </summary>
        /// <param name="interfaceDefinition">The interface definition to base it on.</param>
        /// <param name="abstractImplementation">The abstract implementation, if any.</param>
        /// <param name="nameOverride">
        /// The name to use for the method. If null, the interface member name is used.
        /// </param>
        /// <returns>An introspective method info for the definition.</returns>
        [NotNull]
        internal IntrospectiveMethodInfo GenerateDefinitionFromSignature
        (
            [NotNull] IntrospectiveMethodInfo interfaceDefinition,
            [CanBeNull] IntrospectiveMethodInfo abstractImplementation,
            [CanBeNull] string nameOverride = null
        )
        {
            var methodBuilder = TargetType.DefineMethod
            (
                nameOverride ?? interfaceDefinition.Name,
                Public | Final | Virtual | HideBySig | NewSlot,
                CallingConventions.Standard,
                interfaceDefinition.ReturnType,
                interfaceDefinition.ReturnParameterRequiredModifiers,
                interfaceDefinition.ReturnParameterOptionalModifiers,
                interfaceDefinition.ParameterTypes.ToArray(),
                interfaceDefinition.ParameterRequiredModifiers.ToArray(),
                interfaceDefinition.ParameterOptionalModifiers.ToArray()
            );

            // In the following blocks, which set of attributes to pass through is selected. The logic is as follows:
            // If either the interface or abstract implementation have attributes, select the one which does
            // If both have attributes, select the abstract implementation
            // If neither have attributes, select the interface definition
            if (!(abstractImplementation is null))
            {
                if (abstractImplementation.CustomAttributes.Any())
                {
                    methodBuilder.ApplyCustomAttributesFrom(abstractImplementation);
                }
                else
                {
                    methodBuilder.ApplyCustomAttributesFrom(interfaceDefinition);
                }

                TargetType.DefineMethodOverride(methodBuilder, abstractImplementation.GetWrappedMember());
            }
            else
            {
                methodBuilder.ApplyCustomAttributesFrom(interfaceDefinition);
                TargetType.DefineMethodOverride(methodBuilder, interfaceDefinition.GetWrappedMember());
            }

            var attributePassthroughDefinition = interfaceDefinition;
            if (!(abstractImplementation is null) && abstractImplementation.CustomAttributes.Any())
            {
                attributePassthroughDefinition = abstractImplementation;
            }

            return new IntrospectiveMethodInfo
            (
                methodBuilder,
                interfaceDefinition.ReturnType,
                interfaceDefinition.ParameterTypes,
                interfaceDefinition.MetadataType,
                attributePassthroughDefinition
            );
        }

        /// <summary>
        /// Consumes a set of method definitions, passing them through the pipeline.
        /// </summary>
        /// <param name="methods">The definitions.</param>
        [PublicAPI]
        public void ConsumeMethodDefinitions([NotNull, ItemNotNull] IEnumerable<PipelineWorkUnit<IntrospectiveMethodInfo>> methods)
        {
            ConsumeDefinitions(methods, _methodGeneratorPipeline);
        }

        /// <summary>
        /// Consumes a set of property definitions, passing them through the pipeline.
        /// </summary>
        /// <param name="properties">The properties.</param>
        [PublicAPI]
        public void ConsumePropertyDefinitions([NotNull] IEnumerable<PipelineWorkUnit<IntrospectivePropertyInfo>> properties)
        {
            ConsumeDefinitions(properties, _propertyGeneratorPipeline);
        }

        /// <summary>
        /// Consumes a set of definitions, passing them through the given pipeline. Each stage is guaranteed to run only
        /// once for any given branch of the input definitions. The generation process follows a recursive depth-first
        /// reductive algorithm.
        /// </summary>
        /// <param name="definitions">The definitions to process.</param>
        /// <param name="pipeline">A sorted list of generators, acting as the process pipeline.</param>
        /// <typeparam name="T">The type of definition to process.</typeparam>
        private void ConsumeDefinitions<T>
        (
            [NotNull] IEnumerable<PipelineWorkUnit<T>> definitions,
            [NotNull] IReadOnlyList<IImplementationGenerator<T>> pipeline
        )
            where T : MemberInfo
        {
            var definitionQueue = new Queue<PipelineWorkUnit<T>>(definitions);

            while (definitionQueue.Any())
            {
                var workUnit = definitionQueue.Dequeue();
                var definition = workUnit.Definition;

                // Find the entry stage of the pipeline
                var stage = pipeline.First(s => s.IsApplicable(definition));

                // GetTransformedSymbol the definitions through the stage
                var generatedDefinitions = stage.GenerateImplementation(workUnit).ToList();

                if (!generatedDefinitions.Any())
                {
                    continue;
                }

                // Run the new definitions through the remaining stages of the pipeline
                ConsumeDefinitions(generatedDefinitions, pipeline.Except(new[] { stage }).ToList());
            }
        }
    }
}
