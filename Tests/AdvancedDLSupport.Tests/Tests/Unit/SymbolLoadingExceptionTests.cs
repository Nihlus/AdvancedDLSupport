﻿//
//  SymbolLoadingExceptionTests.cs
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
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Unit;

public class SymbolLoadingExceptionTests
{
    public class Constructor
    {
        [Fact]
        public void ParameterlessConstructorDoesNotAssignAnyProperties()
        {
            var actual = new SymbolLoadingException();

            Assert.NotNull(actual.Message);
            Assert.NotEqual(string.Empty, actual.Message);

            Assert.Null(actual.SymbolName);
            Assert.Null(actual.InnerException);
        }

        [Fact]
        public void MessageOnlyConstructorOnlyAssignsMessageProperty()
        {
            const string message = "Test";

            var actual = new SymbolLoadingException(message);

            Assert.NotNull(actual.Message);
            Assert.Equal(message, actual.Message);

            Assert.Null(actual.SymbolName);
            Assert.Null(actual.InnerException);
        }

        [Fact]
        public void MessageAndInnerConstructorOnlyAssignsMessageAndInnerProperties()
        {
            const string message = "Test";
            var inner = new Exception();

            var actual = new SymbolLoadingException(message, inner);

            Assert.NotNull(actual.Message);
            Assert.Equal(message, actual.Message);

            Assert.Null(actual.SymbolName);

            Assert.NotNull(actual.InnerException);
            Assert.Same(inner, actual.InnerException);
        }

        [Fact]
        public void MessageAndSymbolNameConstructorOnlyAssignsMessageAndSymbolNameProperties()
        {
            const string message = "Test";
            const string name = "Name";

            var actual = new SymbolLoadingException(message, name);

            Assert.NotNull(actual.Message);
            Assert.Equal(message, actual.Message);

            Assert.NotNull(actual.SymbolName);
            Assert.Equal(name, actual.SymbolName);

            Assert.Null(actual.InnerException);
        }

        [Fact]
        public void MessageSymbolNameAndInnerConstructorAssignsMessageSymbolNameAndInnerProperties()
        {
            const string message = "Test";
            const string name = "Name";
            var inner = new Exception();

            var actual = new SymbolLoadingException(message, name, inner);

            Assert.NotNull(actual.Message);
            Assert.Equal(message, actual.Message);

            Assert.NotNull(actual.SymbolName);
            Assert.Equal(name, actual.SymbolName);

            Assert.NotNull(actual.InnerException);
            Assert.Same(inner, actual.InnerException);
        }
    }
}
