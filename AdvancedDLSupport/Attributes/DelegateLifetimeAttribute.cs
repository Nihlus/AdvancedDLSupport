//
//  DelegateLifetimeAttribute.cs
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
        public DelegateLifetimeAttribute(DelegateLifetime lifetime = DelegateLifetime.Persistent)
        {
            Lifetime = lifetime;
        }

        /// <summary>
        /// Depicts the delegate lifetime.
        /// </summary>
        public enum DelegateLifetime
        {
            /// <summary>
            /// Delegate lifetime needs to be managed by user.
            /// </summary>
            None,

            /// <summary>
            /// Delegate is kept alive till unloading.
            /// </summary>
            Persistent,

            /// <summary>
            /// Delegate is alive only for this call.
            /// </summary>
            CallOnly,
        }

        /// <summary>
        /// Gets the delegate lifetime.
        /// </summary>
        public DelegateLifetime Lifetime { get; }
    }
}
