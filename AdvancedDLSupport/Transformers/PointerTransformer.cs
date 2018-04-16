//
//  PointerTransformer.cs
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
using System.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Lowers or raises types to or from <see cref="IntPtr"/>s.
    /// </summary>
    /// <typeparam name="T">The complex type that can be changed into a pointer.</typeparam>
    [PublicAPI]
    public abstract class PointerTransformer<T> : ITypeTransformer<T, IntPtr>
    {
        /// <inheritdoc />
        public abstract IntPtr LowerValue(T value, ParameterInfo parameter);

        /// <inheritdoc />
        public abstract T RaiseValue(IntPtr value, ParameterInfo parameter);

        /// <inheritdoc/>
        public abstract bool IsApplicable(Type complexType, ImplementationOptions options);

        /// <inheritdoc />
        public Type LowerType()
        {
            return typeof(IntPtr);
        }

        /// <inheritdoc />
        public Type RaiseType()
        {
            return typeof(T);
        }
    }
}
