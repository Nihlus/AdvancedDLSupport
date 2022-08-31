//
//  PregeneratedAssemblyBuilderTests.cs
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

using System.IO;
using AdvancedDLSupport.AOT.Tests.Data.Classes;
using AdvancedDLSupport.AOT.Tests.Data.Interfaces;
using AdvancedDLSupport.AOT.Tests.TestBases;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.AOT.Tests.Tests.Integration
{
    public class PregeneratedAssemblyBuilderTests
    {
        public class Build : PregeneratedAssemblyBuilderTestBase
        {
            [Fact]
            public void GeneratesAnOutputFileForASourceAssembly()
            {
                Builder.WithSourceAssembly(SourceAssembly);
                var result = Builder.Build(OutputDirectory);

                var outputFile = Path.Combine(OutputDirectory, result);
                Assert.True(File.Exists(outputFile));
            }

            [Fact]
            public void GeneratesAnOutputFileForASourceExplicitCombination()
            {
                Builder.WithSourceExplicitTypeCombination<AOTMixedModeClass, IAOTLibrary>();
                var result = Builder.Build(OutputDirectory);

                var outputFile = Path.Combine(OutputDirectory, result);
                Assert.True(File.Exists(outputFile));
            }
        }
    }
}
