//
//  DelegateLifetime.cs
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

namespace AdvancedDLSupport
{
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
}
