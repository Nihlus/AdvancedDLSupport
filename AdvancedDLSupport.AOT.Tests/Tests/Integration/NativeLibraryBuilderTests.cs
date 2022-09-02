//
//  NativeLibraryBuilderTests.cs
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
using System.Collections;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using AdvancedDLSupport.AOT.Tests.Data.Interfaces;
using AdvancedDLSupport.AOT.Tests.TestBases;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.AOT.Tests.Tests.Integration;

public class NativeLibraryBuilderTests
{
    public class DiscoverCompiledTypes : NativeLibraryBuilderTestBase
    {
        private static readonly Action ClearCache;

        static DiscoverCompiledTypes()
        {
            var clearMethod = typeof(IDictionary).GetMethod(
                nameof(IDictionary.Clear),
                BindingFlags.Public | BindingFlags.Instance);
            var typeCacheField = typeof(NativeLibraryBuilder).GetField(
                "TypeCache", BindingFlags.NonPublic | BindingFlags.Static);

            if (typeCacheField == null || clearMethod == null)
            {
                Assert.True(false, "ClearCache discovery needs to be fixed!");
                return;
            }

            ClearCache = Expression.Lambda<Action>(Expression.Call(Expression.Field(null, typeCacheField), clearMethod)).Compile();
        }

        public DiscoverCompiledTypes()
        {
            ClearCache();
        }

        [Fact]
        public void CanDiscoverPrecompiledTypes()
        {
            // Pregenerate the types
            Builder.WithSourceAssembly(GetType().Assembly);
            var result = Builder.Build(OutputDirectory);

            var searchPattern = $"*{Path.GetFileNameWithoutExtension(result)}*.dll";

            searchPattern = Path.Combine(Path.GetDirectoryName(result)!, searchPattern);

            NativeLibraryBuilder.DiscoverCompiledTypes(OutputDirectory, searchPattern);
        }

        [Fact]
        public void CanDiscoverPrecompiledTypesFromStream()
        {
            // Pregenerate the types
            Builder.WithSourceAssembly(GetType().Assembly);
            var result = Builder.Build(OutputDirectory);
            var searchPattern = $"*{Path.GetFileNameWithoutExtension(result)}*.dll";
            searchPattern = Path.Combine(Path.GetDirectoryName(result)!, searchPattern);

            foreach (var asm in Directory.GetFiles(OutputDirectory, searchPattern))
            {
                NativeLibraryBuilder.DiscoverCompiledTypes(File.OpenRead(asm));
            }
        }

        [Fact]
        public void UsesPrecompiledTypesIfDiscovered()
        {
            // Pregenerate the types
            Builder.WithSourceAssembly(GetType().Assembly);
            var result = Builder.Build(OutputDirectory);
            var searchPattern = $"*{Path.GetFileNameWithoutExtension(result)}*.dll";
            searchPattern = Path.Combine(Path.GetDirectoryName(result)!, searchPattern);
            NativeLibraryBuilder.DiscoverCompiledTypes(OutputDirectory, searchPattern);

            var library = LibraryBuilder.ActivateInterface<IAOTLibrary>("AOTTests");

            var libraryAssembly = library.GetType().Assembly;

            Assert.False(libraryAssembly.GetCustomAttribute<AOTAssemblyAttribute>() is null);
        }

        [Fact]
        public void UsesPrecompiledTypesIfDiscoveredFromStream()
        {
            // Pregenerate the types
            Builder.WithSourceAssembly(GetType().Assembly);
            var result = Builder.Build(OutputDirectory);
            var searchPattern = $"*{Path.GetFileNameWithoutExtension(result)}*.dll";
            searchPattern = Path.Combine(Path.GetDirectoryName(result)!, searchPattern);
            foreach (var asm in Directory.GetFiles(OutputDirectory, searchPattern))
            {
                NativeLibraryBuilder.DiscoverCompiledTypes(File.OpenRead(asm));
            }

            var library = LibraryBuilder.ActivateInterface<IAOTLibrary>("AOTTests");

            var libraryAssembly = library.GetType().Assembly;

            Assert.False(libraryAssembly.GetCustomAttribute<AOTAssemblyAttribute>() is null);
        }
    }
}
