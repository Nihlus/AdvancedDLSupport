//
//  ITypeTransformer.cs
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

#pragma warning disable SA1402

namespace AdvancedDLSupport
{
    /// <summary>
    /// Opaque interface, used for differentiating type transformers from arbitrary objects.
    /// </summary>
    [PublicAPI]
    public interface ITypeTransformer
    {
        /// <summary>
        /// Lowers the type information of the more complex type to the less complex type.
        /// </summary>
        /// <returns>The lowered type.</returns>
        [PublicAPI, NotNull]
        Type LowerType();

        /// <summary>
        /// Raises the type information of the less complex type to the more complex type.
        /// </summary>
        /// <returns>The lowered type.</returns>
        [PublicAPI, NotNull]
        Type RaiseType();
    }

    /// <summary>
    /// Represents a transformer, which can lower or raise values between two types, altering their complexity without
    /// data loss.
    /// </summary>
    /// <typeparam name="T1">The type considered more complex than the other.</typeparam>
    /// <typeparam name="T2">The type consideres less complex than the other.</typeparam>
    [PublicAPI]
    public interface ITypeTransformer<T1, T2> : ITypeTransformer
    {
        /// <summary>
        /// Lowers a value of type <typeparamref name="T1"/> to a value of type <typeparamref name="T2"/>.
        /// </summary>
        /// <param name="value">The value to lower.</param>
        /// <param name="parameter">The parameter that the value originated from.</param>
        /// <returns>A lowered value.</returns>
        [PublicAPI, CanBeNull]
        T2 LowerValue([CanBeNull] T1 value, [NotNull] ParameterInfo parameter);

        /// <summary>
        /// Raises a value of type <typeparamref name="T2"/> to a value of type <typeparamref name="T1"/>.
        /// </summary>
        /// <param name="value">The value to raise.</param>
        /// <param name="parameter">The parameter that the value is headed to.</param>
        /// <returns>A raised value.</returns>
        [PublicAPI, CanBeNull]
        T1 RaiseValue([CanBeNull] T2 value, [NotNull] ParameterInfo parameter);
    }
}
