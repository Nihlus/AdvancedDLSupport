//
//  GeneratedImplementationTypeIdentifier.cs
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
    /// A key struct for ConcurrentDictionary TypeCache for all generated types provided by DLSupportConstructor.
    /// </summary>
    internal struct GeneratedImplementationTypeIdentifier : IEquatable<GeneratedImplementationTypeIdentifier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedImplementationTypeIdentifier"/> struct.
        /// </summary>
        /// <param name="baseClassType">The base class of the library.</param>
        /// <param name="interfaceType">The interface type.</param>
        /// <param name="options">The configuration used for the library.</param>
        public GeneratedImplementationTypeIdentifier
        (
            [NotNull] Type baseClassType,
            [NotNull] Type interfaceType,
            ImplementationOptions options
        )
        {
            _baseClassType = baseClassType;
            _interfaceType = interfaceType;
            _options = options;
        }

        /// <summary>
        /// The base class type of the library.
        /// </summary>
        private readonly Type _baseClassType;

        /// <summary>
        /// The interface type for the library.
        /// </summary>
        private readonly Type _interfaceType;

        /// <summary>
        /// The configuration used for the library at construction time.
        /// </summary>
        private readonly ImplementationOptions _options;

        /// <inheritdoc />
        public bool Equals(GeneratedImplementationTypeIdentifier other)
        {
            return
                _baseClassType == other._baseClassType &&
                _interfaceType == other._interfaceType &&
                _options == other._options;
        }

        /// <inheritdoc />
        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is GeneratedImplementationTypeIdentifier identifier && Equals(identifier);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return
                    ((_baseClassType != null ? _interfaceType.GetHashCode() : 0) * 397) ^
                    ((_interfaceType != null ? _interfaceType.GetHashCode() : 0) * 397) ^
                    ((int)_options * 397);
            }
        }
    }
}
