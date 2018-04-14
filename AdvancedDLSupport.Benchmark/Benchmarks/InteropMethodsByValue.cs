//
//  InteropMethodsByValue.cs
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

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
#pragma warning disable CS1591, SA1600

namespace AdvancedDLSupport.Benchmark.Benchmarks
{
    [ClrJob, CoreJob, MonoJob]
    [RPlotExporter, CsvMeasurementsExporter]
    public class InteropMethodsByValue
    {
        private static readonly Matrix2 Source = new Matrix2 { Row0 = { X = 4, Y = 7 }, Row1 = { X = 2, Y = 6 } };
        private static readonly Matrix2 Result = new Matrix2 { Row0 = { X = 0.6f, Y = -0.7f }, Row1 = { X = -0.2f, Y = 0.4f } };

        private static ITest _adlLibrary;
        private static ITest _adlLibraryWithoutDisposeChecks;
        private static ITest _adlLibraryWithCalli;

        /// <summary>
        /// Initializes the local data neccesary to run tests.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            _adlLibrary = NativeLibraryBuilder.Default.ActivateInterface<ITest>(Program.LibraryName);
            _adlLibraryWithoutDisposeChecks = new NativeLibraryBuilder().ActivateInterface<ITest>(Program.LibraryName);
            _adlLibraryWithCalli = new NativeLibraryBuilder(ImplementationOptions.UseIndirectCalls).ActivateInterface<ITest>(Program.LibraryName);
        }

        [Benchmark]
        public static Matrix2 DllImport()
        {
            var matrixCopy = Source;
            return DllImportTest.InvertMatrixByValue(matrixCopy);
        }

        [Benchmark]
        public static Matrix2 Delegates()
        {
            var matrixCopy = Source;
            return _adlLibrary.InvertMatrixByValue(matrixCopy);
        }

        [Benchmark]
        public static Matrix2 DelegatesNoDispose()
        {
            var matrixCopy = Source;
            return _adlLibraryWithoutDisposeChecks.InvertMatrixByValue(matrixCopy);
        }

        [Benchmark]
        public static Matrix2 Calli()
        {
            var matrixCopy = Source;
            return _adlLibraryWithCalli.InvertMatrixByValue(matrixCopy);
        }

        [Benchmark]
        public static Matrix2 Managed()
        {
            var matrixCopy = Source;
            return Matrix2.Invert(matrixCopy);
        }
    }
}
