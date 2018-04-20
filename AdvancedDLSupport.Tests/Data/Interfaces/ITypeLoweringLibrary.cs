//
//  ITypeLoweringLibrary.cs
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
using System.Runtime.InteropServices;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Data
{
    public interface ITypeLoweringLibrary
    {
        string GetString();

        string GetNullString();

        [return: MarshalAs(UnmanagedType.BStr)]
        string EchoBStr([MarshalAs(UnmanagedType.BStr)] string value);

        [return: MarshalAs(UnmanagedType.LPTStr)]
        string GetLPTString();

        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetLPWString();

        [return: MarshalAs(UnmanagedType.I1)]
        bool CheckIfStringIsNull(string value);

        UIntPtr StringLength(string value);

        UIntPtr BStringLength([MarshalAs(UnmanagedType.BStr)] string value);

        UIntPtr LPTStringLength([MarshalAs(UnmanagedType.LPTStr)] string value);

        UIntPtr LPWStringLength([MarshalAs(UnmanagedType.LPWStr)] string value);
    }
}
