//
//  IDynamicAssemblyProvider.cs
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
        /// Gets the dynamic module from the assembly, creating one if it doesn't exist.
        /// </summary>
        /// <returns>The module.</returns>
        [PublicAPI, NotNull]
        ModuleBuilder GetDynamicModule();
    }
}
