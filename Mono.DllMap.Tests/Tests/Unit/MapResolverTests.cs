//
//  MapResolverTests.cs
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

using System.IO;
using Mono.DllMap.Tests.TestBases;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace Mono.DllMap.Tests.Unit
{
    public class MapResolverTests
    {
        public class MapLibraryName : MapResolverTestBase
        {
            [Fact]
            public void ReturnsRemappedLibraryNameForAssemblyByGenericTypeParam()
            {
                var actual = Resolver.MapLibraryName<MapResolverTests>(OriginalLibraryName);

                Assert.Equal(RemappedLibraryName, actual);
            }

            [Fact]
            public void ReturnsRemappedLibraryNameForAssemblyByType()
            {
                var actual = Resolver.MapLibraryName(typeof(MapResolverTests), OriginalLibraryName);

                Assert.Equal(RemappedLibraryName, actual);
            }

            [Fact]
            public void ReturnsRemappedLibraryNameForAssembly()
            {
                var assembly = typeof(MapResolverTests).Assembly;
                var actual = Resolver.MapLibraryName(assembly, OriginalLibraryName);

                Assert.Equal(RemappedLibraryName, actual);
            }

            [Fact]
            public void ReturnsLibraryNameUnchangedForAssemblyByGenericTypeParamIfLibraryDoesNotHaveAMapping()
            {
                var actual = Resolver.MapLibraryName<object>(UnmappedLibraryName);

                Assert.Equal(UnmappedLibraryName, actual);
            }

            [Fact]
            public void ReturnsLibraryNameUnchangedForAssemblyByTypeIfLibraryDoesNotHaveAMapping()
            {
                var actual = Resolver.MapLibraryName(typeof(object), UnmappedLibraryName);

                Assert.Equal(UnmappedLibraryName, actual);
            }

            [Fact]
            public void ReturnsLibraryNameUnchangedForAssemblyIfLibraryDoesNotHaveAMapping()
            {
                var assembly = typeof(object).Assembly;
                var actual = Resolver.MapLibraryName(assembly, UnmappedLibraryName);

                Assert.Equal(UnmappedLibraryName, actual);
            }
        }

        public class HasDllMapFile : MapResolverTestBase
        {
            [Fact]
            public void ReturnsTrueForAssemblyWithDllMapFileByGenericTypeParam()
            {
                var actual = Resolver.HasDllMapFile<MapResolverTests>();

                Assert.True(actual);
            }

            [Fact]
            public void ReturnsTrueForAssemblyWithDllMapFileByType()
            {
                var actual = Resolver.HasDllMapFile(typeof(MapResolverTests));

                Assert.True(actual);
            }

            [Fact]
            public void ReturnsTrueForAssemblyWithDllMapFileByAssembly()
            {
                var assembly = typeof(MapResolverTests).Assembly;
                var actual = Resolver.HasDllMapFile(assembly);

                Assert.True(actual);
            }

            [Fact]
            public void ReturnsFalseForAssemblyWithoutDllMapFileByGenericTypeParam()
            {
                var actual = Resolver.HasDllMapFile<object>();

                Assert.False(actual);
            }

            [Fact]
            public void ReturnsFalseForAssemblyWithoutDllMapFileByType()
            {
                var actual = Resolver.HasDllMapFile(typeof(object));

                Assert.False(actual);
            }

            [Fact]
            public void ReturnsFalseForAssemblyWithoutDllMapFileByAssembly()
            {
                var assembly = typeof(object).Assembly;
                var actual = Resolver.HasDllMapFile(assembly);

                Assert.False(actual);
            }
        }

        public class GetDllMap : MapResolverTestBase
        {
            [Fact]
            public void ReturnsNotNullValueForAssemblyWithDllMapFileByGenericTypeParam()
            {
                var actual = Resolver.GetDllMap<MapResolverTests>();

                Assert.NotNull(actual);
            }

            [Fact]
            public void ReturnsNotNullValueForAssemblyWithDllMapFileByType()
            {
                var actual = Resolver.GetDllMap(typeof(MapResolverTests));

                Assert.NotNull(actual);
            }

            [Fact]
            public void ReturnsNotNullValueForAssemblyWithDllMapFileByAssembly()
            {
                var assembly = typeof(MapResolverTests).Assembly;
                var actual = Resolver.GetDllMap(assembly);

                Assert.NotNull(actual);
            }

            [Fact]
            public void ThrowsForAssemblyWithoutDllMapFileByGenericTypeParam()
            {
                Assert.Throws<FileNotFoundException>
                (
                    () =>
                        Resolver.GetDllMap<object>()
                );
            }

            [Fact]
            public void ThrowsForAssemblyWithoutDllMapFileByType()
            {
                Assert.Throws<FileNotFoundException>
                (
                    () =>
                        Resolver.GetDllMap(typeof(object))
                );
            }

            [Fact]
            public void ThrowsForAssemblyWithoutDllMapFileByAssembly()
            {
                var assembly = typeof(object).Assembly;
                Assert.Throws<FileNotFoundException>
                (
                    () =>
                        Resolver.GetDllMap(assembly)
                );
            }
        }
    }
}
