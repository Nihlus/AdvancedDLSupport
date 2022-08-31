//
//  DelegateLibraryDelegates.cs
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

#pragma warning disable SA1600, CS1591

using System.Runtime.InteropServices;

namespace AdvancedDLSupport.Tests.Data
{
    public class DelegateLibraryDelegates
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Action();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ActionInt(int param);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int IntFunc();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int IntFuncInt(int param);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ActinIntInAction(ActionInt param);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int IntFuncIntInFuncInt(IntFuncInt param1);
    }
}
