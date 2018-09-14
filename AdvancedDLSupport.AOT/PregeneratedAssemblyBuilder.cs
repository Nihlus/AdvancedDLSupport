//
//  PregeneratedAssemblyBuilder.cs
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.Extensions;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;
using NLog;
using StrictEmit;

using static AdvancedDLSupport.ImplementationOptions;
using static System.Reflection.MethodAttributes;

namespace AdvancedDLSupport.AOT
{
    /// <summary>
    /// Builder class for ahead-of-time compiled native-to-managed glue assemblies.
    /// </summary>
    [PublicAPI]
    public class PregeneratedAssemblyBuilder
    {
        private static ILogger _log = LogManager.GetCurrentClassLogger();

        private static object _fileCopyLock = new object();

        [NotNull, ItemNotNull]
        private List<Assembly> SourceAssemblies { get; }

        [NotNull]
        private List<(Type ClassType, IReadOnlyList<Type> InterfaceTypes)> SourceExplicitTypeCombinations { get; }

        private ImplementationOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="PregeneratedAssemblyBuilder"/> class.
        /// </summary>
        /// <param name="options">The implementation options to use.</param>
        [PublicAPI]
        public PregeneratedAssemblyBuilder(ImplementationOptions options = 0)
        {
            _options = options;

            SourceAssemblies = new List<Assembly>();
            SourceExplicitTypeCombinations = new List<(Type, IReadOnlyList<Type>)>();
        }

        /// <summary>
        /// Adds an assembly as a source assembly to generate.
        /// </summary>
        /// <param name="assembly">The source assembly.</param>
        /// <returns>The builder, with the assembly.</returns>
        [PublicAPI, NotNull]
        public PregeneratedAssemblyBuilder WithSourceAssembly([NotNull] Assembly assembly)
        {
            if (SourceAssemblies.Contains(assembly))
            {
                throw new ArgumentException("Source assembly must be unique to the builder instance.", nameof(assembly));
            }

            SourceAssemblies.Add(assembly);
            return this;
        }

        /// <summary>
        /// Adds an explicit type combination that isn't automatically discovered from the assembly set.
        /// </summary>
        /// <typeparam name="TBaseClass">The base class of the type.</typeparam>
        /// <typeparam name="TInterface">The interface to implement.</typeparam>
        /// <returns>The builder, with the combination.</returns>
        [PublicAPI, NotNull]
        public PregeneratedAssemblyBuilder WithSourceExplicitTypeCombination<TBaseClass, TInterface>()
            where TBaseClass : NativeLibraryBase
            where TInterface : class
        {
            return WithSourceExplicitTypeCombination(typeof(TBaseClass), typeof(TInterface));
        }

        /// <summary>
        /// Adds an explicit type combination that isn't automatically discovered from the assembly set.
        /// </summary>
        /// <param name="classType">The base class of the type.</param>
        /// <param name="interfaceTypes">The interfaces to implement.</param>
        /// <returns>The builder, with the combination.</returns>
        [PublicAPI, NotNull]
        public PregeneratedAssemblyBuilder WithSourceExplicitTypeCombination
        (
            [NotNull] Type classType,
            [NotNull] params Type[] interfaceTypes
        )
        {
            if (!classType.IsAbstract)
            {
                throw new ArgumentException
                (
                    "The class to activate must be abstract.",
                    nameof(classType)
                );
            }

            if (!(classType.IsSubclassOf(typeof(NativeLibraryBase)) || classType == typeof(NativeLibraryBase)))
            {
                throw new ArgumentException
                (
                    $"The base class must be or derive from {nameof(NativeLibraryBase)}.",
                    nameof(classType)
                );
            }

            if (!interfaceTypes.Any(i => i.IsInterface))
            {
                throw new ArgumentException
                (
                    "The interface to activate on the class must be an interface type.",
                    nameof(interfaceTypes)
                );
            }

            foreach (var combination in SourceExplicitTypeCombinations)
            {
                var areInterfaceListsEqual = combination.InterfaceTypes
                    .OrderBy(i => i.Name)
                    .SequenceEqual(interfaceTypes.OrderBy(i => i.Name));

                var areClassesEqual = combination.ClassType == classType;

                if (areClassesEqual && areInterfaceListsEqual)
                {
                    throw new ArgumentException
                    (
                        "The source combination of explicit types must be unique to the builder instance.",
                        $"{nameof(classType)}+{nameof(interfaceTypes)}"
                    );
                }
            }

            SourceExplicitTypeCombinations.Add((classType, interfaceTypes));
            return this;
        }

        /// <summary>
        /// Builds the implementation assembly, saving it to the given path.
        /// </summary>
        /// <param name="outputPath">The path where the assembly should be saved.</param>
        /// <returns>The name of the output assembly.</returns>
        [PublicAPI, NotNull]
        public string Build([NotNull] string outputPath)
        {
            outputPath = outputPath.IsNullOrWhiteSpace()
                ? Directory.GetCurrentDirectory()
                : Path.GetFullPath(outputPath);

            // Discover automatic interfaces
            var automaticInterfaces = new List<Type>();
            foreach (var sourceAssembly in SourceAssemblies)
            {
                _log.Info($"Scanning {sourceAssembly.GetName().Name}...");
                foreach (var automaticInterface in sourceAssembly.ExportedTypes.Where(t => t.HasCustomAttribute<AOTTypeAttribute>()))
                {
                    automaticInterfaces.Add(automaticInterface);
                    _log.Info($"Discovered {automaticInterface.Name}.");
                }
            }

            // Build combination list
            var combinationList = new List<(Type ClassType, IReadOnlyList<Type> InterfaceTypes)>();
            combinationList.AddRange
            (
                automaticInterfaces.Select(t => (typeof(NativeLibraryBase), (IReadOnlyList<Type>)new List<Type> { t }))
            );

            foreach (var explicitCombination in SourceExplicitTypeCombinations)
            {
                if (combinationList.Any(t =>
                    t.ClassType == explicitCombination.ClassType &&
                    t.InterfaceTypes
                        .OrderBy(i => i.Name)
                        .SequenceEqual(explicitCombination.InterfaceTypes.OrderBy(i => i.Name))))
                {
                    continue;
                }

                combinationList.Add(explicitCombination);
            }

            var persistentAssemblyProvider = new PersistentDynamicAssemblyProvider(outputPath, !_options.HasFlagFast(EnableOptimizations));
            // And build the types
            var generatedTypeDictionary = new Dictionary<GeneratedImplementationTypeIdentifier, Type>();

            var libraryBuilder = new NativeLibraryBuilder(_options, assemblyProvider: persistentAssemblyProvider);
            foreach (var combination in combinationList)
            {
                var generatedCombination = libraryBuilder.PregenerateImplementationType(combination.ClassType, combination.InterfaceTypes.ToArray());

                generatedTypeDictionary.Add(generatedCombination.Item1, generatedCombination.Item2);
            }

            var assembly = persistentAssemblyProvider.GetDynamicAssembly();

            // Apply the AOT attribute
            var aotAssemblyConstructor = typeof(AOTAssemblyAttribute).GetConstructors().First(c => !c.GetParameters().Any());
            var aotAssemblyAttributeBuilder = new CustomAttributeBuilder(aotAssemblyConstructor, new object[] { });
            assembly.SetCustomAttribute(aotAssemblyAttributeBuilder);

            // Create the metadata class
            CreateMetadataType(persistentAssemblyProvider.GetDynamicModule(), generatedTypeDictionary);

            if (!outputPath.IsNullOrWhiteSpace())
            {
                Directory.CreateDirectory(outputPath);
            }

            var outputFileName = $"{PersistentDynamicAssemblyProvider.DynamicAssemblyName}_{persistentAssemblyProvider.UniqueIdentifier}.dll";

            assembly.Save(outputFileName);

            return outputFileName;
        }

        /// <summary>
        /// Creates a metadata type in the assembly that contains a listing of the pregenerated types and their
        /// respective lookup keys.
        /// </summary>
        /// <param name="module">The module to emit the type in.</param>
        /// <param name="typeDictionary">The generated types.</param>
        private void CreateMetadataType
        (
            [NotNull] ModuleBuilder module,
            [NotNull] IReadOnlyDictionary<GeneratedImplementationTypeIdentifier, Type> typeDictionary
        )
        {
            var type = module.DefineType
            (
                $"AOTMetadata_{Guid.NewGuid().ToString().ToLowerInvariant()}",
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                typeof(object),
                new[] { typeof(IAOTMetadata) }
            );

            // Tag the type with the metadata attribute
            var aotMetadataAttributeConstructor = typeof(AOTMetadataAttribute).GetConstructors().First(c => !c.GetParameters().Any());
            var aotMetadataAttributeBuilder = new CustomAttributeBuilder(aotMetadataAttributeConstructor, new object[] { });
            type.SetCustomAttribute(aotMetadataAttributeBuilder);

            var backingField = type.DefineField
            (
                $"{nameof(IAOTMetadata.GeneratedTypes)}_backing",
                typeof(IReadOnlyDictionary<GeneratedImplementationTypeIdentifier, Type>),
                FieldAttributes.Private | FieldAttributes.Static
            );

            var constructor = type.DefineConstructor
            (
                Public,
                CallingConventions.Standard,
                new Type[] { }
            );

            var dictionaryType = typeof(Dictionary<GeneratedImplementationTypeIdentifier, Type>);
            var dictionaryConstructor = dictionaryType.GetConstructors().First
            (
                c => !c.GetParameters().Any()
            );

            var dictionaryAdd = dictionaryType.GetMethod(nameof(Dictionary<object, object>.Add));

            var constructorIL = constructor.GetILGenerator();

            constructorIL.EmitNewObject(dictionaryConstructor);
            constructorIL.EmitSetStaticField(backingField);

            foreach (var entry in typeDictionary)
            {
                constructorIL.EmitLoadStaticField(backingField);

                EmitCreateKeyInstance(constructorIL, entry.Key);
                constructorIL.EmitTypeOf(entry.Value);

                constructorIL.EmitCallVirtual(dictionaryAdd);
            }

            constructorIL.EmitReturn();

            var property = type.DefineProperty
            (
                nameof(IAOTMetadata.GeneratedTypes),
                PropertyAttributes.HasDefault,
                typeof(IReadOnlyDictionary<GeneratedImplementationTypeIdentifier, Type>),
                null
            );

            var getter = type.DefineMethod
            (
                $"get_{nameof(IAOTMetadata.GeneratedTypes)}",
                Public | Final | Virtual | SpecialName | HideBySig | NewSlot,
                typeof(IReadOnlyDictionary<GeneratedImplementationTypeIdentifier, Type>),
                null
            );

            var getterIL = getter.GetILGenerator();
            getterIL.EmitLoadStaticField(backingField);
            getterIL.EmitReturn();

            property.SetGetMethod(getter);

            type.CreateType();
        }

        /// <summary>
        /// Emits a set of IL instructions that creates a new instance of the given <paramref name ="entryKey"/>.
        /// </summary>
        /// <param name="constructorIL">The generator where the IL is to be emitted.</param>
        /// <param name="entryKey">The instance to emit.</param>
        private void EmitCreateKeyInstance([NotNull] ILGenerator constructorIL, GeneratedImplementationTypeIdentifier entryKey)
        {
            var constructor = entryKey.GetType().GetConstructor
            (
                new[] { typeof(Type), typeof(IReadOnlyList<Type>), typeof(ImplementationOptions) }
            );

            // Build the interface type array
            var typeArrayVariable = constructorIL.DeclareLocal(typeof(Type[]));

            constructorIL.EmitConstantInt(entryKey.InterfaceTypes.Count);
            constructorIL.EmitNewArray<Type>();
            constructorIL.EmitSetLocalVariable(typeArrayVariable);

            for (var i = 0; i < entryKey.InterfaceTypes.Count; ++i)
            {
                constructorIL.EmitLoadLocalVariable(typeArrayVariable);

                constructorIL.EmitConstantInt(i);
                constructorIL.EmitTypeOf(entryKey.InterfaceTypes[i]);
                constructorIL.EmitSetArrayElement<Type>();
            }

            // Create the key
            constructorIL.EmitTypeOf(entryKey.BaseClassType);
            constructorIL.EmitLoadLocalVariable(typeArrayVariable);
            constructorIL.EmitConstantInt((int)entryKey.Options);

            constructorIL.EmitNewObject(constructor);
        }
    }
}
