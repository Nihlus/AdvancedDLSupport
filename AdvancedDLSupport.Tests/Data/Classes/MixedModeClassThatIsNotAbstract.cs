//
//  MixedModeClassThatIsNotAbstract.cs
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

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Data.Classes
{
    public class MixedModeClassThatIsNotAbstract : NativeLibraryBase, IMixedModeLibrary
    {
        public MixedModeClassThatIsNotAbstract(string path, Type interfaceType, ImplementationOptions options, TypeTransformerRepository transformerRepository)
            : base(path, options, transformerRepository)
        {
        }

        public bool RanManagedSubtract { get; private set; }

        public int ManagedAdd(int a, int b)
        {
            return a + b;
        }

        public int NativeProperty { get; set; }

        public int OtherNativeProperty { get; set; }

        public int Multiply(int value, int multiplier)
        {
            return value * multiplier;
        }

        public int Subtract(int value, int other)
        {
            RanManagedSubtract = true;
            return value - other;
        }
    }
}
