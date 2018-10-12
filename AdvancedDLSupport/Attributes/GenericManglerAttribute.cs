//
//  GenericManglerAttribute.cs
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
    /// Represents an attribute that tags a method with a name mangler type to use for that method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class GenericManglerAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the mangler type to use. This type should inherit from <see cref="IEntrypointMangler"/>.
        /// </summary>
        public Type ManglerType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericManglerAttribute"/> class.
        /// </summary>
        /// <param name="manglerType">
        /// The mangler type. This type should inherit from <see cref="IEntrypointMangler"/>.
        /// </param>
        public GenericManglerAttribute(Type manglerType)
        {
            ManglerType = manglerType;
        }

        /// <summary>
        /// Creates an instance of the bound mangler type.
        /// </summary>
        /// <returns>The mangler.</returns>
        [NotNull]
        public IEntrypointMangler CreateManglerInstance()
        {
            return (IEntrypointMangler)Activator.CreateInstance(ManglerType);
        }
    }
}
