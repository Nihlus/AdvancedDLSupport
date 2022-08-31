//
//  INameManglingTests.cs
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

using static System.Runtime.InteropServices.CallingConvention;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Data;

public interface INameManglingTests
{
    [NativeSymbol(CallingConvention = StdCall)]
    int Multiply(int a, int b);

    [NativeSymbol(CallingConvention = StdCall)]
    int MultiplyStructByVal(TestStruct strct);

    [NativeSymbol(Entrypoint = nameof(MultiplyStructByPtr), CallingConvention = StdCall)]
    int MultiplyStructByRef(ref TestStruct strct);

    [NativeSymbol(CallingConvention = StdCall)]
    unsafe int MultiplyStructByPtr(TestStruct* strct);
}
