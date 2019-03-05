//
//  ReturnsSpanTests.cs
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
using System.Data;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using JetBrains.Annotations;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Integration
{
    public class ReturnsSpanTests : LibraryTestBase<IReturnsSpanTests>
    {
        private const string LibraryName = "ReturnsSpanTests";

        public ReturnsSpanTests()
            : base(LibraryName)
        {
        }

        [Fact]
        public void ReturnsCorrectConstAttr()
        {
            var span = Library.GetInt32ArrayZeroToNine();

            Assert.True(span.Length == 10);

            for (var i = 0; i < 10; i++)
            {
                Assert.True(span[i] == i);
            }
        }

        [Fact]
        public void ThrowsNotSupportedTypeIsByref()
        {
            var activator = GetImplementationBuilder();

            Assert.Throws<NotSupportedException>(() => activator.ActivateInterface<IFailsReturnsSpanInvalid>(LibraryName));
        }

        [Fact]
        public void ThrowsNotSupportedTypeHasNoRetAttr()
        {
            var activator = GetImplementationBuilder();

            Assert.Throws<InvalidOperationException>(() => activator.ActivateInterface<IFailsReturnsSpanNoAttr>(LibraryName));
        }
    }
}
