//
//  NativeLibraryBuilderTests.cs
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

using System.Reflection;
using AdvancedDLSupport.AOT.Tests.Data.Interfaces;
using AdvancedDLSupport.AOT.Tests.TestBases;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.AOT.Tests.Tests.Integration
{
    public class NativeLibraryBuilderTests
    {
        public class DiscoverCompiledTypes : NativeLibraryBuilderTestBase
        {
            [Fact]
            public void CanDiscoverPrecompiledTypes()
            {
                // Pregenerate the types
                Builder.WithSourceAssembly(GetType().Assembly);
                var result = Builder.Build(OutputDirectory);

                var searchPattern = $"*{result}*.dll";
                NativeLibraryBuilder.DiscoverCompiledTypes(OutputDirectory, searchPattern);
            }

            [Fact]
            public void UsesPrecompiledTypesIfDiscovered()
            {
                // Pregenerate the types
                Builder.WithSourceAssembly(GetType().Assembly);
                var result = Builder.Build(OutputDirectory);

                var searchPattern = $"*{result}*.dll";
                NativeLibraryBuilder.DiscoverCompiledTypes(OutputDirectory, searchPattern);

                var library = LibraryBuilder.ActivateInterface<IAOTLibrary>("AOTTests");

                var libraryAssembly = library.GetType().Assembly;

                Assert.False(libraryAssembly.GetCustomAttribute<AOTAssemblyAttribute>() is null);
            }
        }
    }
}
