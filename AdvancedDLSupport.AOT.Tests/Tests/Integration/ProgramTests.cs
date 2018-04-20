//
//  ProgramTests.cs
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
using AdvancedDLSupport.AOT.Tests.Fixtures;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.AOT.Tests.Tests.Integration
{
    public class ProgramTests : IClassFixture<InitialCleanupFixture>
    {
        [Fact]
        public void ReturnsInputAssemblyNotFoundIfOneOrMoreAssembliesDoNotExist()
        {
            var args = "--input-assemblies aaaa.dll".Split(' ');

            var result = Program.Main(args);

            Assert.Equal(ExitCodes.InputAssemblyNotFound, (ExitCodes)result);
        }

        [Fact]
        public void ReturnsFailedToLoadAssemblyIfOneOrMoreAssembliesCouldNotBeLoaded()
        {
            File.Create("empty.dll").Close();
            var args = "--input-assemblies empty.dll".Split(' ');

            var result = Program.Main(args);

            File.Delete("empty.dll");
            Assert.Equal(ExitCodes.FailedToLoadAssembly, (ExitCodes)result);
        }

        [Fact]
        public void ReturnsSuccessIfNoErrorsWereGenerated()
        {
            var args = "--input-assemblies AdvancedDLSupport.AOT.Tests.dll -o aot-test".Split(' ');

            var result = Program.Main(args);

            Assert.Equal(ExitCodes.Success, (ExitCodes)result);
        }
    }
}
