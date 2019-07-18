//
//  GenerateDLDynamicAssembliesTask.cs
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
using System.IO;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace AdvancedDLSupport.AOT.Tasks
{
    /// <summary>
    /// Represents an MSBuild task that can pre-generate implementations of <see cref="NativeLibraryBase"/>s.
    /// </summary>
    [UsedImplicitly]
    public class GenerateDLDynamicAssembliesTask : Task
    {
        /// <summary>
        /// Gets or sets the input file to pass to the task.
        /// </summary>
        [Required]
        [NotNull]
        public string InputFile { get; set; }

        /// <summary>
        /// Gets or sets the output directory in which pre-generated assemblies are outputted.
        /// </summary>
        [Required]
        [NotNull]
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AdvancedDLSupport.ImplementationOptions"/> to use.
        /// </summary>
        [NotNull]
        public string Options { get; set; } = "GenerateDisposalChecks;" +
                                              "EnableDllMapSupport;" +
                                              "EnableOptimizations;" +
                                              "SuppressSecurity";

        /// <summary>
        /// Gets the <see cref="AdvancedDLSupport.ImplementationOptions"/> from the <see cref="Options"/> property.
        /// </summary>
        [NotNull]
        public ImplementationOptions ImplementationOptions
        {
            get
            {
                var rawOpts = Options.Replace(";", ", ");
                if (ImplementationOptions.TryParse<ImplementationOptions>(rawOpts, out var opts))
                {
                    return opts;
                }

                Log.LogWarning("Failed to parse the given ImplementationOptions, will use the default ones instead.");
                return NativeLibraryBuilder.Default.Options;
            }
        }

        /// <inheritdoc />
        public override bool Execute()
        {
            var builder = new PregeneratedAssemblyBuilder(ImplementationOptions);

            if (!File.Exists(InputFile))
            {
                Log.LogError("Couldn't find the given InputFile.");
                return false;
            }

            try
            {
                var assembly = Assembly.LoadFile(InputFile);
                builder.WithSourceAssembly(assembly);
            }
            catch (BadImageFormatException bex)
            {
                Log.LogError("Failed to load input assembly due to a bitness mismatch or incompatible assembly.");
                return false;
            }

            builder.Build(OutputDirectory);

            return true;
        }
    }
}
