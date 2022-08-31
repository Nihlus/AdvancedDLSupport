//
//  NativeLibraryBuilderIntegrationTests.cs
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
using System.IO;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.Data.Classes;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Integration
{
    public class NativeLibraryBuilderIntegrationTests
    {
        public class ActivateInterface : LibraryTestBase<IBaseLibrary>
        {
            private const string LibraryName = "BaseTests";

            public ActivateInterface()
                : base(LibraryName)
            {
            }

            [Fact]
            public void CanLoadLibrary()
            {
                Assert.NotNull(Library);
            }

            [Fact]
            public void LoadingLibraryWithMismatchedBitnessThrows()
            {
                var incorrectBitness = RuntimeInformation.ProcessArchitecture == Architecture.X64 ? "x32" : "x64";

                var libraryName = $"{LibraryName}-{incorrectBitness}";
                var libraryPath = Path.Combine("lib", incorrectBitness, libraryName);

                Assert.Throws<LibraryLoadingException>
                (
                    () => NativeLibraryBuilder.Default.ActivateInterface<IBaseLibrary>(libraryPath)
                );
            }

            [Fact]
            public void LoadingSameInterfaceAndSameFileTwiceProducesDifferentReferences()
            {
                var secondLoad = new NativeLibraryBuilder().ActivateInterface<IBaseLibrary>(LibraryName);

                Assert.NotSame(Library, secondLoad);
            }

            [Fact]
            public void LoadingSameInterfaceWithSameOptionsAndSameFileTwiceUsesSameGeneratedType()
            {
                var secondLoad = new NativeLibraryBuilder(ImplementationOptions.GenerateDisposalChecks)
                    .ActivateInterface<IBaseLibrary>(LibraryName);

                Assert.IsType(Library.GetType(), secondLoad);
            }

            [Fact]
            public void LoadingSameInterfaceAndSameFileButWithDifferentOptionsDoesNotUseSameGeneratedType()
            {
                var options = ImplementationOptions.UseLazyBinding;

                var secondLoad = new NativeLibraryBuilder(options).ActivateInterface<IBaseLibrary>(LibraryName);

                Assert.IsNotType(Library.GetType(), secondLoad);
            }

            [Fact]
            public void LoadingSameInterfaceAndSameFileButWithDifferentOptionsProducesDifferentReferences()
            {
                var options = ImplementationOptions.UseLazyBinding;

                var secondLoad = new NativeLibraryBuilder(options).ActivateInterface<IBaseLibrary>(LibraryName);

                Assert.NotSame(Library, secondLoad);
            }

            [Fact]
            public void ActivatingANonInterfaceThrowsArgumentException()
            {
                Assert.Throws<ArgumentException>
                (
                    () => NativeLibraryBuilder.Default.ActivateInterface<SimpleClass>(LibraryName)
                );
            }

            [Fact]
            public void ActivatingAnInterfaceWithAnInvalidPathThrowsFileNotFoundException()
            {
                Assert.Throws<FileNotFoundException>
                (
                    () => NativeLibraryBuilder.Default.ActivateInterface<IBaseLibrary>("not a path")
                );
            }
        }
    }
}
