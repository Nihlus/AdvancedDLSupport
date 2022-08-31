//
//  IIndirectCallLibrary.cs
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

using System.Runtime.InteropServices;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Data
{
    public interface IIndirectCallLibrary
    {
        int Multiply(int a, int b);

        int GetStructAValueByRef(ref TestStruct strct);

        [NativeSymbol(nameof(GetStructAValueByRef))]
        int GetStructAValueByIn(in TestStruct strct);

        int GetStructAValueByValue(TestStruct strct);

        ref TestStruct GetInitializedStructByRef(int a, int b);

        TestStruct GetInitializedStructByValue(int a, int b);

        TestStruct? GetNullTestStruct();

        [return: MarshalAs(UnmanagedType.U1)]
        bool IsTestStructNull(TestStruct? strct);
    }
}
