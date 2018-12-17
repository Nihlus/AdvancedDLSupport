//
//  SymbolTransformerTests.cs
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
using System.Text;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Tests.Unit
{
    public class SymbolTransformerTests
    {
        public SymbolTransformer Transformer { get; set; } = new SymbolTransformer();

        [Fact]
        public void None()
        {
            const string prefix = "x";
            const string before = "test";
            const string after = "xtest";

            Assert.Equal(after, Transformer.Transform(before, prefix));
        }

        [Fact]
        public void Pascalizes()
        {
            const string before = "anExampleTitle";
            const string after = "AnExampleTitle";

            Assert.Equal(after, Transformer.Transform(before, null, SymbolTransformationMethod.Pascalize));
        }

        [Fact]
        public void Camelizes()
        {
            const string before = "WeeTitleStuff";
            const string after = "weeTitleStuff";

            Assert.Equal(after, Transformer.Transform(before, null, SymbolTransformationMethod.Camelize));
        }

        [Fact]
        public void Underscores()
        {
            const string before = "someTestTitle";
            const string after = "some_test_title";

            Assert.Equal(after, Transformer.Transform(before, null, SymbolTransformationMethod.Underscore));
        }

        [Fact]
        public void Dashes()
        {
            const string before = "A_test-Title";
            const string after = "A-test-Title";

            Assert.Equal(after, Transformer.Transform(before, null, SymbolTransformationMethod.Dasherize));
        }

        [Fact]
        public void Kaberizes()
        {
            const string before = "A_test-Title";
            const string after = "A-test-Title";

            Assert.Equal(after, Transformer.Transform(before, null, SymbolTransformationMethod.Dasherize));
        }
    }
}
