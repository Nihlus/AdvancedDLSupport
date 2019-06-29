//
//  CustomLoadingLogicTests.cs
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
using AdvancedDLSupport.Loaders;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using JetBrains.Annotations;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Integration
{
    public class CustomLoadingLogicTests : LibraryTestBase<IStringLibrary>
    {
        private const string LibraryName = "StringTests";

        private class LibraryLoadingOverride : ILibraryLoader
        {
            private readonly ILibraryLoader _defaultLoader;

            public bool LoadLibraryCalled { get; private set; }

            public LibraryLoadingOverride(ILibraryLoader @default)
            {
                _defaultLoader = @default;
            }

            public IntPtr LoadLibrary([CanBeNull] string path)
            {
                LoadLibraryCalled = true;

                return _defaultLoader.LoadLibrary(path);
            }

            public bool CloseLibrary(IntPtr library)
            {
                return _defaultLoader.CloseLibrary(library);
            }
        }

        private class SymbolLoadingOverride : ISymbolLoader
        {
            private readonly ISymbolLoader _defaultLoader;

            public bool LoadSymbolCalled { get; private set; }

            public SymbolLoadingOverride(ISymbolLoader @default)
            {
                _defaultLoader = @default;
            }

            public IntPtr LoadSymbol(IntPtr library, [NotNull] string symbolName)
            {
                LoadSymbolCalled = true;

                return _defaultLoader.LoadSymbol(library, symbolName == "GetString" ? "GetNullString" : symbolName);
            }
        }

        private LibraryLoadingOverride _libraryLogicOverride;
        private SymbolLoadingOverride _symbolLogicOverride;

        public CustomLoadingLogicTests()
            : base(LibraryName)
        {
        }

        protected override ImplementationOptions GetImplementationOptions()
        {
            return base.GetImplementationOptions() | ImplementationOptions.UseLazyBinding;
        }

        protected override NativeLibraryBuilder GetImplementationBuilder()
        {
            return base.GetImplementationBuilder().WithLibraryLoader(@default =>
            {
                _libraryLogicOverride = new LibraryLoadingOverride(@default);
                return _libraryLogicOverride;
            }).WithSymbolLoader(@default =>
            {
                _symbolLogicOverride = new SymbolLoadingOverride(@default);
                return _symbolLogicOverride;
            });
        }

        [Fact]
        public void CustomLogicCalled()
        {
            Library.GetString();
            Assert.True(_libraryLogicOverride.LoadLibraryCalled);
            Assert.True(_symbolLogicOverride.LoadSymbolCalled);
        }

        [Fact]
        public void CustomLogicSuccessful()
        {
            Assert.Equal(Library.GetString(), Library.GetNullString());
        }
    }
}
