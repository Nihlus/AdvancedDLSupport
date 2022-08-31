//
//  BenchmarkBase.cs
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
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Benchmark.Data;
using AdvancedDLSupport.Benchmark.Native;
using BenchmarkDotNet.Attributes;
using static AdvancedDLSupport.ImplementationOptions;

namespace AdvancedDLSupport.Benchmark.Benchmarks;

/// <summary>
/// Acts as the base for library benchmarks.
/// </summary>
public abstract class BenchmarkBase
{
    /// <summary>
    /// Gets a source matrix that can be inverted.
    /// </summary>
    protected static readonly Matrix2 Source = new Matrix2 { Row0 = { X = 4, Y = 7 }, Row1 = { X = 2, Y = 6 } };

    /// <summary>
    /// Gets a delegate-based implementation.
    /// </summary>
    protected static ITest ADLLibrary { get; private set; }

    /// <summary>
    /// Gets a delegate-based implementation without disposal checks.
    /// </summary>
    protected static ITest ADLLibraryWithoutDisposeChecks { get; private set; }

    /// <summary>
    /// Gets a delegate-based implementation with suppressed unmanaged code security.
    /// </summary>
    protected static ITest ADLLibraryWithSuppressedSecurity { get; private set; }

    /// <summary>
    /// Gets a calli-based implementation.
    /// </summary>
    protected static ITest ADLLibraryWithCalli { get; private set; }

    /// <summary>
    /// Initializes the local data neccesary to run tests.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        ADLLibrary = new NativeLibraryBuilder(GenerateDisposalChecks).ActivateInterface<ITest>(Program.LibraryName);
        ADLLibraryWithoutDisposeChecks = new NativeLibraryBuilder().ActivateInterface<ITest>(Program.LibraryName);
        ADLLibraryWithSuppressedSecurity = new NativeLibraryBuilder(SuppressSecurity).ActivateInterface<ITest>(Program.LibraryName);
        ADLLibraryWithCalli = new NativeLibraryBuilder(UseIndirectCalls).ActivateInterface<ITest>(Program.LibraryName);
    }

    /// <summary>
    /// Benchmarks a matrix inversion using traditional <see cref="DllImportAttribute"/>s.
    /// </summary>
    /// <returns>An inverted matrix.</returns>
    [Benchmark]
    public abstract Matrix2 DllImport();

    /// <summary>
    /// Benchmarks a matrix inversion using traditional <see cref="DllImportAttribute"/>s with suppressed unmanaged
    /// code security.
    /// </summary>
    /// <returns>An inverted matrix.</returns>
    [Benchmark]
    public abstract Matrix2 DllImportSuppressedSecurity();

    /// <summary>
    /// Benchmarks a matrix inversion using <see cref="MulticastDelegate"/>s.
    /// </summary>
    /// <returns>An inverted matrix.</returns>
    [Benchmark]
    public abstract Matrix2 Delegates();

    /// <summary>
    /// Benchmarks a matrix inversion using <see cref="MulticastDelegate"/>s, omitting disposal checks.
    /// </summary>
    /// <returns>An inverted matrix.</returns>
    [Benchmark]
    public abstract Matrix2 DelegatesNoDispose();

    /// <summary>
    /// Benchmarks a matrix inversion using <see cref="MulticastDelegate"/>s, suppressing unmanaged code security.
    /// </summary>
    /// <returns>An inverted matrix.</returns>
    [Benchmark]
    public abstract Matrix2 DelegatesSuppressedSecurity();

    /// <summary>
    /// Benchmarks a matrix inversion using <see cref="OpCodes.Calli"/>.
    /// </summary>
    /// <returns>An inverted matrix.</returns>
    [Benchmark]
    public abstract Matrix2 Calli();

    /// <summary>
    /// Benchmarks a matrix inversion using a managed implementation.
    /// </summary>
    /// <returns>An inverted matrix.</returns>
    [Benchmark]
    public abstract Matrix2 Managed();
}
