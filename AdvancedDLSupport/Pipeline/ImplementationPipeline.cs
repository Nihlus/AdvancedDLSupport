//
//  ImplementationPipeline.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
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
using Mono.DllMap.Extensions;
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

        private readonly RefPermutationImplementationGenerator _refPermutationGenerator;
        private readonly LoweredMethodImplementationGenerator _loweredMethodGenerator;

        private readonly DelegateMethodImplementationGenerator _delegateMethodGenerator;
        private readonly IndirectCallMethodImplementationGenerator _indirectCallMethodGenerator;

        private readonly PropertyImplementationGenerator _propertyGenerator;

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

            _refPermutationGenerator = new RefPermutationImplementationGenerator
            (
                targetModule,
                _targetType,
                constructorIL,
                _options,
                _transformerRepository
            );

            _loweredMethodGenerator = new LoweredMethodImplementationGenerator
            (
                targetModule,
                _targetType,
                constructorIL,
                _options,
                _transformerRepository
            );

            _delegateMethodGenerator = new DelegateMethodImplementationGenerator
            (
                targetModule,
                _targetType,
                constructorIL,
                _options
            );

            _indirectCallMethodGenerator = new IndirectCallMethodImplementationGenerator
            (
                targetModule,
                _targetType,
                constructorIL,
                _options
            );

            _propertyGenerator = new PropertyImplementationGenerator
            (
                targetModule,
                _targetType,
                constructorIL,
                _options
            );
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
            // Add the initial pool of methods
            var definitionQueue = new Queue<PipelineWorkUnit<IntrospectiveMethodInfo>>(methods);

            while (definitionQueue.Any())
            {
                var workUnit = definitionQueue.Dequeue();
                var method = workUnit.Definition;

                IEnumerable<PipelineWorkUnit<IntrospectiveMethodInfo>> generatedDefinitions;

                if (method.RequiresRefPermutations())
                {
                    generatedDefinitions = _refPermutationGenerator.GenerateImplementation(workUnit);
                }
                else
                {
                    var requiresLowering =
                        _transformerRepository.HasApplicableTransformer(method.ReturnType, _options) ||
                        method.ParameterTypes.Any(pt => _transformerRepository.HasApplicableTransformer(pt, _options));

                    if (requiresLowering)
                    {
                        generatedDefinitions = _loweredMethodGenerator.GenerateImplementation(workUnit);
                    }
                    else if (_options.HasFlagFast(ImplementationOptions.UseIndirectCalls))
                    {
                        // This is a terminating processing branch - no new definitions should be generated.
                        generatedDefinitions = _indirectCallMethodGenerator.GenerateImplementation(workUnit);
                    }
                    else
                    {
                        // This is a terminating processing branch - no new definitions should be generated.
                        generatedDefinitions = _delegateMethodGenerator.GenerateImplementation(workUnit);
                    }
                }

                foreach (var generatedDefinition in generatedDefinitions)
                {
                    definitionQueue.Enqueue(generatedDefinition);
                }
            }
        }

        /// <summary>
        /// Consumes a set of property definitions, passing them through the pipeline.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void ConsumePropertyDefinitions([NotNull] IEnumerable<PipelineWorkUnit<IntrospectivePropertyInfo>> properties)
        {
            var definitionQueue = new Queue<PipelineWorkUnit<IntrospectivePropertyInfo>>(properties);

            while (definitionQueue.Any())
            {
                var workUnit = definitionQueue.Dequeue();

                var generatedDefinitions = _propertyGenerator.GenerateImplementation(workUnit);

                foreach (var generatedDefinition in generatedDefinitions)
                {
                    definitionQueue.Enqueue(generatedDefinition);
                }
            }
        }
    }
}
