//
//  InheritedInterfaceTests.cs
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
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Integration
{
    public class InheritedInterfaceTests
    {
        public class IdenticalInheritedInterfaceTests : LibraryTestBase<IInterfaceWithCombinedIdenticalSignatures>
        {
            private const string LibraryName = "FunctionTests";

            public IdenticalInheritedInterfaceTests()
                : base(LibraryName)
            {
            }

            [Fact]
            public void CanCallIdenticalMethodInFirstInterface()
            {
                const int a = 5;
                const int b = 20;

                const int expected = a * b;

                var result = ((IInterfaceWithFirstIdenticalSignature)Library).Multiply(a, b);

                Assert.Equal(expected, result);
            }

            [Fact]
            public void CanCallIdenticalMethodInSecondInterface()
            {
                const int a = 5;
                const int b = 30;

                const int expected = a * b;

                var result = ((IInterfaceWithSecondIdenticalSignature)Library).Multiply(a, b);

                Assert.Equal(expected, result);
            }
        }

        public class DifferentEntrypointInheritedInterfaceTests
            : LibraryTestBase<IInterfaceWithCombinedIdenticalSignaturesWithDifferentEntrypoints>
        {
            private const string LibraryName = "FunctionTests";

            public DifferentEntrypointInheritedInterfaceTests()
                : base(LibraryName)
            {
            }

            [Fact]
            public void CanCallMethodWithSameSignatureButDifferentEntrypointInFirstInterface()
            {
                const int a = 5;
                const int b = 20;

                const int expected = a * b;

                var result = ((IInterfaceWithFirstIdenticalSignatureWithDifferentEntrypoint)Library).DoMath(a, b);

                Assert.Equal(expected, result);
            }

            [Fact]
            public void CanCallMethodWithSameSignatureButDifferentEntrypointInSecondInterface()
            {
                const int a = 5;
                const int b = 30;

                const int expected = a - b;

                var result = ((IInterfaceWithSecondIdenticalSignatureWithDifferentEntrypoint)Library).DoMath(a, b);

                Assert.Equal(expected, result);
            }
        }
    }
}
