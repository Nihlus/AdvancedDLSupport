//
//  IBooleanMarshallingTests.cs
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
using static System.Runtime.InteropServices.UnmanagedType;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Data
{
    public interface IBooleanMarshallingTests
    {
        void DoSomethingWithBoolean(bool boolean);

        [NativeSymbol(Entrypoint = "IsByteTrue")]
        int IsDefaultTrue(bool boolean);

        int IsSByteTrue([MarshalAs(I1)] bool boolean);

        int IsShortTrue([MarshalAs(I2)] bool boolean);

        int IsIntTrue([MarshalAs(I4)] bool boolean);

        int IsLongTrue([MarshalAs(I8)] bool boolean);

        int IsByteTrue([MarshalAs(U1)] bool boolean);

        int IsUShortTrue([MarshalAs(U2)] bool boolean);

        int IsUIntTrue([MarshalAs(U4)] bool boolean);

        int IsULongTrue([MarshalAs(U8)] bool boolean);

        int IsBOOLTrue([MarshalAs(Bool)] bool boolean);

        int IsVariantBoolTrue([MarshalAs(VariantBool)] bool boolean);

        [NativeSymbol(Entrypoint = "GetTrueByte")]
        bool GetTrueDefault();

        [return: MarshalAs(I1)]
        bool GetTrueSByte();

        [return: MarshalAs(I2)]
        bool GetTrueShort();

        [return: MarshalAs(I4)]
        bool GetTrueInt();

        [return: MarshalAs(I8)]
        bool GetTrueLong();

        [return: MarshalAs(U1)]
        bool GetTrueByte();

        [return: MarshalAs(U2)]
        bool GetTrueUShort();

        [return: MarshalAs(U4)]
        bool GetTrueUInt();

        [return: MarshalAs(U8)]
        bool GetTrueULong();

        [return: MarshalAs(Bool)]
        bool GetTrueBOOL();

        [return: MarshalAs(VariantBool)]
        bool GetTrueVariantBool();
    }
}
