//
//  PropertyIntegrationTests.cs
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
using System.Runtime.InteropServices;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

#pragma warning disable SA1600, CS1591

// ReSharper disable ArgumentsStyleLiteral
namespace AdvancedDLSupport.Tests.Integration;

public class PropertyIntegrationTests
{
    private const string LibraryName = "PropertyTests";
    private const string TestCollectionName = "VolatilePropertyData";

    [Collection(TestCollectionName)]
    public class Getter : LibraryTestBase<IPropertyLibrary>
    {
        public Getter()
            : base(LibraryName)
        {
            Library.ResetData();
        }

        [Fact]
        public void CanGetGlobalVariableAsProperty()
        {
            Assert.Equal(5, Library.GlobalVariable);
        }

        [Fact]
        public void CanGetGlobalVariableAsGetOnlyProperty()
        {
            Assert.Equal(5, Library.GlobalVariableGetOnly);
        }

        [Fact]
        public unsafe void CanGetGlobalPointerVariableAsProperty()
        {
            Assert.Equal(20, *Library.GlobalPointerVariable);
        }

        [Fact]
        public unsafe void CanGetGlobalPointerVariableAsGetOnlyProperty()
        {
            Assert.Equal(20, *Library.GlobalPointerVariableGetOnly);
        }
    }

    [Collection(TestCollectionName)]
    public class Setter : LibraryTestBase<IPropertyLibrary>
    {
        public Setter()
            : base(LibraryName)
        {
            Library.ResetData();
        }

        [Fact]
        public void CanSetGlobalVariableAsProperty()
        {
            Library.GlobalVariable = 1;
            Assert.Equal(1, Library.GlobalVariable);
        }

        [Fact]
        public void CanSetGlobalVariableAsSetOnlyProperty()
        {
            Library.GlobalVariableSetOnly = 1;
            Assert.Equal(1, Library.GlobalVariable);
        }

        [Fact]
        public unsafe void CanSetGlobalPointerVariableAsProperty()
        {
            *Library.GlobalPointerVariable = 25;
            Assert.Equal(25, *Library.GlobalPointerVariable);
        }

        [Fact]
        public unsafe void CanSetGlobalPointerVariableAsSetOnlyProperty()
        {
            Marshal.StructureToPtr(25, new IntPtr(Library.GlobalPointerVariableGetOnly), false);
            Assert.Equal(25, *Library.GlobalPointerVariable);
        }
    }

    public class FailureCases
    {
        [Fact]
        public void ThrowsNotSupportedExceptionIfPropertyHasClassType()
        {
            var builder = NativeLibraryBuilder.Default;

            Assert.Throws<NotSupportedException>
            (
                () =>
                    builder.ActivateInterface<IPropertyWithClassTypeLibrary>(LibraryName)
            );
        }
    }
}
