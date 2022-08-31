﻿//
//  MixedModeClassWithNativeSymbolInInterfaceAndClass.cs
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

// ReSharper disable ValueParameterNotUsed
namespace AdvancedDLSupport.Tests.Data.Classes;

public abstract class MixedModeClassWithNativeSymbolInInterfaceAndClass
    : NativeLibraryBase, IMixedModeLibraryWithIncorrectNativeSymbolInInterface
{
    public MixedModeClassWithNativeSymbolInInterfaceAndClass(string path, Type interfaceType, ImplementationOptions options)
        : base(path, options)
    {
    }

    [NativeSymbol(Entrypoint = "sym_Subtract")]
    public abstract int SubtractWithRemappedName(int a, int b);
}
