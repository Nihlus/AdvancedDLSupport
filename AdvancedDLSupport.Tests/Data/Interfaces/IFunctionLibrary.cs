//
//  IFunctionLibrary.cs
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
    public interface IFunctionLibrary
    {
        int DoStructMath(ref TestStruct struc, int multiplier);

        int Multiply(int value, int multiplier);

        [NativeSymbol(nameof(DoStructMath))]
        int Multiply(ref TestStruct value, int multiplier);

        int Subtract(int value, int other);

        [NativeSymbol(nameof(Subtract))]
        int DuplicateSubtract(int value, int other);

        [NativeSymbol(CallingConvention = CallingConvention.StdCall)]
        int STDCALLSubtract(int value, int other);
    }
}
