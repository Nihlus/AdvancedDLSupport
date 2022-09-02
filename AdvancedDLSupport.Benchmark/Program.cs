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
using AdvancedDLSupport.Benchmark.Benchmarks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Running;

namespace AdvancedDLSupport.Benchmark;

/// <summary>
/// The main program class.
/// </summary>
internal static class Program
{
    /// <summary>
    /// The name of the native library.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    internal const string LibraryName = "TestLibrary";

    /// <summary>
    /// The main entry point.
    /// </summary>
    internal static void Main()
    {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .With
            (
                new SimpleFilter
                (
                    b =>
                    {
                        var isClrJob = b.Job.Env.Runtime.Name == "Clr";
                        var isRunningOnMono = !(Type.GetType("Mono.Runtime") is null);

                        if (!isClrJob)
                        {
                            return true;
                        }

                        return !isRunningOnMono;
                    }
                )
            );

        BenchmarkRunner.Run<InteropMethodsByRef>(config);
        BenchmarkRunner.Run<InteropMethodsByValue>(config);

        /*var logger = ConsoleLogger.Default;
        MarkdownExporter.Console.ExportToLog(refSummary, logger);
        MarkdownExporter.Console.ExportToLog(valueSummary, logger);

        ConclusionHelper.Print(logger, config.GetCompositeAnalyser().Analyse(refSummary).ToList());
        ConclusionHelper.Print(logger, config.GetCompositeAnalyser().Analyse(valueSummary).ToList());*/
    }
}
