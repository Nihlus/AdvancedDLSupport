//
//  IDynamicAssemblyProvider.cs
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

using System.Reflection.Emit;
using JetBrains.Annotations;

namespace AdvancedDLSupport.DynamicAssemblyProviders
{
    /// <summary>
    /// Provides and constructs a dynamic assembly for consumption.
    /// </summary>
    [PublicAPI]
    public interface IDynamicAssemblyProvider
    {
        /// <summary>
        /// Gets the dynamic assembly provided by this instance.
        /// </summary>
        /// <returns>The assembly.</returns>
        [PublicAPI, NotNull, Pure]
        AssemblyBuilder GetDynamicAssembly();

        /// <summary>
        /// Gets a dynamic module from the assembly by name, creating one if it doesn't exist.
        /// </summary>
        /// <param name="name">The name of the module to get.</param>
        /// <returns>The module.</returns>
        [PublicAPI, NotNull]
        ModuleBuilder GetDynamicModule([NotNull] string name);
    }
}
