//
//  IIntrospectiveMember.cs
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

namespace AdvancedDLSupport.Reflection
{
    /// <summary>
    /// Introspective member interface.
    /// </summary>
    public interface IIntrospectiveMember
    {
        /// <summary>
        /// Gets a custom attribute of <typeparamref name="TAttribute"/>, or null if none can be found.
        /// </summary>
        /// <typeparam name="TAttribute">The type of attribute to get.</typeparam>
        /// <returns>The attribute, or null.</returns>
        TAttribute GetCustomAttribute<TAttribute>() where TAttribute : Attribute;
    }
}
