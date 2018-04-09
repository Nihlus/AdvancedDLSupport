//
//  CommandLineArguments.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Collections.Generic;
using CommandLine;
using JetBrains.Annotations;
using static AdvancedDLSupport.ImplementationOptions;

namespace AdvancedDLSupport.AOT.Arguments
{
    /// <summary>
    /// Hosts command-line arguments given to the tool.
    /// </summary>
    [UsedImplicitly]
    public class CommandLineArguments
    {
        /// <summary>
        /// Gets or sets a list of input assemblies to process.
        /// </summary>
        [Option
        (
            'i',
            "input-assemblies",
            Required = true,
            HelpText = "Input assemblies to process."
        )]
        [PublicAPI, NotNull, ItemNotNull]
        public IEnumerable<string> InputAssemblies { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the implementation options to use when generating.
        /// </summary>
        [Option
        (
            'f',
            "implementation-options",
            Required = false,
            HelpText = "The implementation options to use when generating.",
            Default = EnableDllMapSupport | GenerateDisposalChecks
        )]
        [PublicAPI]
        public ImplementationOptions ImplementationOptions { get; set; }

        /// <summary>
        /// Gets or sets the output path where the generated assemblies should be stored.
        /// </summary>
        [Option
        (
            'o',
            "output-path",
            Required = false,
            HelpText = "The output path where the generated assemblies should be stored. Defaults to the current directory."
        )]
        [PublicAPI, NotNull]
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether or not verbose logging should be enabled.
        /// </summary>
        [Option
        (
            'v',
            "verbose",
            Required = false,
            HelpText = "Enable verbose logging.",
            Default = false
        )]
        public bool Verbose { get; set; }
    }
}
