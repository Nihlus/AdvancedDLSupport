//
//  IPropertyLibrary.cs
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

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Data
{
    public unsafe interface IPropertyLibrary : IDisposable
    {
        void InitializeGlobalPointerVariable();

        void ResetData();

        [NativeSymbol(nameof(GlobalVariable))]
        int GlobalVariableSetOnly { set; }

        [NativeSymbol(nameof(GlobalVariable))]
        int GlobalVariableGetOnly { get; }

        int GlobalVariable { get; set; }

        [NativeSymbol(nameof(GlobalPointerVariable))]
        int* GlobalPointerVariableSetOnly { set; }

        [NativeSymbol(nameof(GlobalPointerVariable))]
        int* GlobalPointerVariableGetOnly { get; }

        int* GlobalPointerVariable { get; set; }
    }
}
