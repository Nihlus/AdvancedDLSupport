//
//  DelegateLifetimeAttribute.cs
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
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// The delegate lifetime attribute.
    /// </summary>
    [PublicAPI, AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class DelegateLifetimeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateLifetimeAttribute"/> class.
        /// </summary>
        /// <param name="lifetime">The delegate lifetime.</param>
        public DelegateLifetimeAttribute(DelegateLifetime lifetime)
        {
            Lifetime = lifetime;
        }

        /// <summary>
        /// Gets the delegate lifetime.
        /// </summary>
        public DelegateLifetime Lifetime { get; }
    }
}
