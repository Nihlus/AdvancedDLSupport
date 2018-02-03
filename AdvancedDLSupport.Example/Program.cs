//
//  Program.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;

// ReSharper disable UnusedVariable
#pragma warning disable SA1600, CS1591 // Elements should be documented

namespace AdvancedDLSupport.Example
{
    internal class Program
    {
        private static unsafe void Main()
        {
            var wrapper = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IExample>
            (
                "Demo"
            );
            wrapper.InitializeMyStructure();
            *wrapper.MyStructure = new MyStruct(24);
            Console.WriteLine(wrapper.MyStructure->A);
            wrapper.MyStructure->A = 25;

            Console.WriteLine(wrapper.MyStructure->A);

            var testStruct = wrapper.GetAllocatedTestStruct();

            Console.WriteLine(wrapper.GetNullString() is null);
            Console.WriteLine(wrapper.GetString());
            Console.ReadLine();
        }
    }
}
