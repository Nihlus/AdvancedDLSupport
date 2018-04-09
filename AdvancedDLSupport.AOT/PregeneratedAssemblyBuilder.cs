//
//  PregeneratedAssemblyBuilder.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.Extensions;
using JetBrains.Annotations;

namespace AdvancedDLSupport.AOT
{
    /// <summary>
    /// Builder class for ahead-of-time compiled native-to-managed glue assemblies.
    /// </summary>
    [PublicAPI]
    public class PregeneratedAssemblyBuilder
    {
        [NotNull, ItemNotNull]
        private List<Assembly> SourceAssemblies { get; }

        [NotNull]
        private List<(Type ClassType, Type InterfaceType)> SourceExplicitTypeCombinations { get; }

        private ImplementationOptions Options { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PregeneratedAssemblyBuilder"/> class.
        /// </summary>
        /// <param name="options">The implementation options to use.</param>
        [PublicAPI]
        public PregeneratedAssemblyBuilder(ImplementationOptions options = 0)
        {
            SourceAssemblies = new List<Assembly>();
            SourceExplicitTypeCombinations = new List<(Type, Type)>();
        }

        /// <summary>
        /// Adds an assembly as a source assembly to generate
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
        /// <param name="interfaceType">The interface to implement.</param>
        /// <returns>The builder, with the combination.</returns>
        [PublicAPI, NotNull]
        public PregeneratedAssemblyBuilder WithSourceExplicitTypeCombination
        (
            [NotNull] Type classType,
            [NotNull] Type interfaceType
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

            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException
                (
                    "The interface to activate on the class must be an interface type.",
                    nameof(interfaceType)
                );
            }

            if (SourceExplicitTypeCombinations.Any(t => t.ClassType == classType && t.InterfaceType == interfaceType))
            {
                throw new ArgumentException
                (
                    "The source combination of explicit types must be unique to the builder instance.",
                    $"{nameof(classType)}+{nameof(interfaceType)}"
                );
            }

            SourceExplicitTypeCombinations.Add((classType, interfaceType));
            return this;
        }

        /// <summary>
        /// Builds the implementation assembly, saving it to the given path.
        /// </summary>
        /// <param name="outputPath">The path where the assembly should be saved.</param>
        public void Build(string outputPath)
        {
            // Discover automatic interfaces
            var automaticInterfaces = new List<Type>();
            foreach (var sourceAssembly in SourceAssemblies)
            {
                automaticInterfaces.AddRange(sourceAssembly.ExportedTypes.Where(t => t.HasCustomAttribute<NativeAOT>()));
            }

            // Build combination list
            var combinationList = new List<(Type ClassType, Type InterfaceType)>();
            combinationList.AddRange(automaticInterfaces.Select(t => (typeof(NativeLibraryBase), t)));

            foreach (var explicitCombination in SourceExplicitTypeCombinations)
            {
                if (combinationList.Any(t =>
                    t.ClassType == explicitCombination.ClassType &&
                    t.InterfaceType == explicitCombination.InterfaceType))
                {
                    continue;
                }

                combinationList.Add(explicitCombination);
            }

            var assemblyName = $"{Path.GetFileNameWithoutExtension(outputPath)}";
            var persistentAssemblyProvider = new PersistentDynamicAssemblyProvider(assemblyName, true);

            // And build the types
            var libraryBuilder = new NativeLibraryBuilder(Options, assemblyProvider: persistentAssemblyProvider);
            foreach (var combination in combinationList)
            {
                libraryBuilder.PregenerateImplementationType(combination.ClassType, combination.InterfaceType);
            }

            var assembly = persistentAssemblyProvider.GetDynamicAssembly();
            assembly.Save(outputPath);
        }
    }
}
