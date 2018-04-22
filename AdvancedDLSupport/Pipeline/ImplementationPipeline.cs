//
//  ImplementationPipeline.cs
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
    public class ImplementationPipeline
    {
        private readonly TypeBuilder _targetType;
        private readonly ImplementationOptions _options;
        private readonly TypeTransformerRepository _transformerRepository;

        private readonly IReadOnlyList<IImplementationGenerator<IntrospectiveMethodInfo>> _methodGeneratorPipeline;
        private readonly IReadOnlyList<IImplementationGenerator<IntrospectivePropertyInfo>> _propertyGeneratorPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementationPipeline"/> class.
        /// </summary>
        /// <param name="targetModule">The module to generates any additional types in.</param>
        /// <param name="targetType">The target type to generate implementations in.</param>
        /// <param name="constructorIL">The <see cref="ILGenerator"/> of the target type's constructor.</param>
        /// <param name="options">The implementation options to use.</param>
        /// <param name="transformerRepository">The repository containing the type transformers.</param>
        public ImplementationPipeline
        (
            [NotNull] ModuleBuilder targetModule,
            [NotNull] TypeBuilder targetType,
            [NotNull] ILGenerator constructorIL,
            ImplementationOptions options,
            [NotNull] TypeTransformerRepository transformerRepository
        )
        {
            _targetType = targetType;
            _options = options;
            _transformerRepository = transformerRepository;

            var generatorSorter = new ComplexitySorter();

            var refPermutationGenerator = new RefPermutationImplementationGenerator
            (
                targetModule,
                _targetType,
                constructorIL,
                _options,
                _transformerRepository
            );

            var loweredMethodGenerator = new LoweredMethodImplementationGenerator
            (
                targetModule,
                _targetType,
                constructorIL,
                _options,
                _transformerRepository
            );

            var delegateMethodGenerator = new DelegateMethodImplementationGenerator
            (
                targetModule,
                _targetType,
                constructorIL,
                _options
            );

            var indirectCallMethodGenerator = new IndirectCallMethodImplementationGenerator
            (
                targetModule,
                _targetType,
                constructorIL,
                _options
            );

            var booleanMarshallingWrapper = new BooleanMarshallingWrapper
            (
                targetModule,
                _targetType,
                constructorIL,
                _options
            );

            var methodGenerators = new IImplementationGenerator<IntrospectiveMethodInfo>[]
            {
                booleanMarshallingWrapper,
                refPermutationGenerator,
                loweredMethodGenerator,
                indirectCallMethodGenerator,
                delegateMethodGenerator
            };

            _methodGeneratorPipeline = generatorSorter.SortGenerators(methodGenerators).ToList();

            var propertyGenerator = new PropertyImplementationGenerator
            (
                targetModule,
                _targetType,
                constructorIL,
                _options
            );

            var propertyGenerators = new IImplementationGenerator<IntrospectivePropertyInfo>[]
            {
                propertyGenerator
            };

            _propertyGeneratorPipeline = generatorSorter.SortGenerators(propertyGenerators).ToList();
        }

        /// <summary>
        /// Generates the definition of the complex method.
        /// </summary>
        /// <param name="interfaceDefinition">The interface definition to base it on.</param>
        /// <returns>An introspective method info for the definition.</returns>
        [NotNull]
        public IntrospectiveMethodInfo GenerateDefinitionFromSignature([NotNull] IntrospectiveMethodInfo interfaceDefinition)
        {
            var methodBuilder = _targetType.DefineMethod
            (
                interfaceDefinition.Name,
                Public | Final | Virtual | HideBySig | NewSlot,
                CallingConventions.Standard,
                interfaceDefinition.ReturnType,
                interfaceDefinition.ParameterTypes.ToArray()
            );

            methodBuilder.ApplyCustomAttributesFrom(interfaceDefinition);

            _targetType.DefineMethodOverride(methodBuilder, interfaceDefinition.GetWrappedMember());

            return new IntrospectiveMethodInfo(methodBuilder, interfaceDefinition.ReturnType, interfaceDefinition.ParameterTypes, interfaceDefinition);
        }

        /// <summary>
        /// Consumes a set of method definitions, passing them through the pipeline.
        /// </summary>
        /// <param name="methods">The definitions.</param>
        public void ConsumeMethodDefinitions([NotNull, ItemNotNull] IEnumerable<PipelineWorkUnit<IntrospectiveMethodInfo>> methods)
        {
            ConsumeDefinitions(methods, _methodGeneratorPipeline);
        }

        /// <summary>
        /// Consumes a set of property definitions, passing them through the pipeline.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void ConsumePropertyDefinitions([NotNull] IEnumerable<PipelineWorkUnit<IntrospectivePropertyInfo>> properties)
        {
            ConsumeDefinitions(properties, _propertyGeneratorPipeline);
        }

        /// <summary>
        /// Consumes a set of definitions, passing them through the given pipeline.
        /// </summary>
        /// <param name="definitions">The definitions to process.</param>
        /// <param name="pipeline">A sorted list of generators, acting as the process pipeline</param>
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

                IEnumerable<PipelineWorkUnit<T>> generatedDefinitions = new List<PipelineWorkUnit<T>>();

                // Go through each stage in the pipeline, pushing the work unit through a stage if the stage is
                // applicable. If any additional work units are generated, terminate this unit and enqueue the new
                // units for further processing.
                foreach (var stage in pipeline)
                {
                    if (!stage.IsApplicable(definition))
                    {
                        continue;
                    }

                    generatedDefinitions = stage.GenerateImplementation(workUnit).ToList();

                    if (generatedDefinitions.Any())
                    {
                        break;
                    }
                }

                foreach (var generatedDefinition in generatedDefinitions)
                {
                    definitionQueue.Enqueue(generatedDefinition);
                }
            }
        }
    }
}
