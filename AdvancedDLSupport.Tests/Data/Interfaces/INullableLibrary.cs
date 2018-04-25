//
//  INullableLibrary.cs
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

using System.Runtime.InteropServices;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Data
{
    public interface INullableLibrary
    {
        TestStruct? GetAllocatedTestStruct();

        [NativeSymbol(nameof(GetAllocatedTestStruct))]
        [return: CallerFree]
        TestStruct? GetAllocatedTestStructAndFree();

        TestStruct? GetNullTestStruct();

        [NativeSymbol(nameof(GetNullTestStruct))]
        [return: CallerFree]
        TestStruct? GetNullTestStructAndFree();

        [return: MarshalAs(UnmanagedType.I1)]
        bool CheckIfStructIsNull(TestStruct? testStruct);

        [NativeSymbol(nameof(CheckIfStructIsNull))]
        [return: MarshalAs(UnmanagedType.I1)]
        bool CheckIfStructIsNullAndFree([CallerFree] TestStruct? testStruct);

        long GetStructPtrValue(ref TestStruct? testStruct);

        [return: MarshalAs(UnmanagedType.I1)]
        [NativeSymbol(nameof(CheckIfStructIsNull))]
        bool CheckIfRefStructIsNull(ref TestStruct? testStruct);

        int GetValueInNullableRefStruct(ref TestStruct? testStruct);

        void SetValueInNullableRefStruct(ref TestStruct? testStruct);

        string GetAFromStructAsString(ref TestStruct? testStruct);

        int GetAFromStructMultipliedByParameter(ref TestStruct? testStruct, int multiplier);
    }
}
