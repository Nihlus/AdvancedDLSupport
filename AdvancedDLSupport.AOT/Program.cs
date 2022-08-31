//
//  Program.cs
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
using System.IO;
using System.Linq;
using System.Reflection;
using AdvancedDLSupport.AOT.Arguments;
using AdvancedDLSupport.Extensions;
using CommandLine;
using NLog;

namespace AdvancedDLSupport.AOT;

/// <summary>
/// The main entrypoint class of the program.
/// </summary>
public static class Program
{
    private static ILogger _log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Gets the command-line arguments to the program.
    /// </summary>
    internal static CommandLineArguments? Arguments { get; private set; }

    /// <summary>
    /// The main entry point.
    /// </summary>
    /// <param name="args">The raw arguments passed to the program.</param>
    /// <returns>The exit code of the application.</returns>
    public static int Main(string[] args)
    {
        Parser.Default.ParseArguments<CommandLineArguments>(args)
            .WithParsed(o => Arguments = o)
            .WithNotParsed(e => Arguments = null);

        if (Arguments is null)
        {
            return (int)ExitCodes.InvalidArguments;
        }

        var builder = new PregeneratedAssemblyBuilder(Arguments.ImplementationOptions);

        // Ensure all input paths are fully resolved, and that we don't try to process empty inputs
        Arguments.InputAssemblies = Arguments.InputAssemblies.Select(Path.GetFullPath).Where(i => !i.IsNullOrWhiteSpace());

        // Default to the current directory as the output directory
        if (Arguments.OutputPath.IsNullOrWhiteSpace())
        {
            Arguments.OutputPath = Directory.GetCurrentDirectory();
        }

        foreach (var inputAssembly in Arguments.InputAssemblies)
        {
            if (!File.Exists(inputAssembly))
            {
                _log.Error(new FileNotFoundException("Could not find the given input assembly.", inputAssembly));
                return (int)ExitCodes.InputAssemblyNotFound;
            }

            try
            {
                var assembly = Assembly.LoadFile(inputAssembly);
                builder.WithSourceAssembly(assembly);

                _log.Info($"Loaded input assembly \"{assembly.GetName().Name}\".");
            }
            catch (BadImageFormatException bex)
            {
                _log.Error(bex, "Failed to load input assembly due to a bitness mismatch or incompatible assembly.");
                return (int)ExitCodes.FailedToLoadAssembly;
            }
        }

        builder.Build(Arguments.OutputPath);

        return (int)ExitCodes.Success;
    }
}
