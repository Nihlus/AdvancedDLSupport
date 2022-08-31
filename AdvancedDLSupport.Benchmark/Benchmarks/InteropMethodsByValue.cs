//
//  InteropMethodsByValue.cs
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

using AdvancedDLSupport.Benchmark.Data;
using AdvancedDLSupport.Benchmark.Native;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using JetBrains.Annotations;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
#pragma warning disable CS1591, SA1600

namespace AdvancedDLSupport.Benchmark.Benchmarks
{
    [UsedImplicitly]
    [ClrJob, CoreJob, MonoJob]
    [RPlotExporter, CsvMeasurementsExporter]
    public class InteropMethodsByValue : BenchmarkBase
    {
        [Benchmark]
        public override Matrix2 DllImport()
        {
            var matrixCopy = Source;
            return DllImportTest.InvertMatrixByValue(matrixCopy);
        }

        [Benchmark]
        public override Matrix2 DllImportSuppressedSecurity()
        {
            var matrixCopy = Source;
            return DllImportTestSuppressedSecurity.InvertMatrixByValue(matrixCopy);
        }

        [Benchmark]
        public override Matrix2 Delegates()
        {
            var matrixCopy = Source;
            return ADLLibrary.InvertMatrixByValue(matrixCopy);
        }

        [Benchmark]
        public override Matrix2 DelegatesNoDispose()
        {
            var matrixCopy = Source;
            return ADLLibraryWithoutDisposeChecks.InvertMatrixByValue(matrixCopy);
        }

        [Benchmark]
        public override Matrix2 DelegatesSuppressedSecurity()
        {
            var matrixCopy = Source;
            ADLLibraryWithSuppressedSecurity.InvertMatrixByPtr(ref matrixCopy);

            return matrixCopy;
        }

        [Benchmark]
        public override Matrix2 Calli()
        {
            var matrixCopy = Source;
            return ADLLibraryWithCalli.InvertMatrixByValue(matrixCopy);
        }

        [Benchmark]
        public override Matrix2 Managed()
        {
            var matrixCopy = Source;
            return Matrix2.Invert(matrixCopy);
        }
    }
}
