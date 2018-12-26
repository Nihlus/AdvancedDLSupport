//
//  FieldNotFoundException.cs
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
using System.Runtime.Serialization;
using System.Security.Permissions;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Represents a failure to find a required field.
    /// </summary>
    [PublicAPI, Serializable]
    public class FieldNotFoundException : Exception
    {
        /// <summary>
        /// Gets the name of the field that was not found.
        /// </summary>
        [PublicAPI, NotNull]
        public string FieldName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldNotFoundException"/> class.
        /// </summary>
        [PublicAPI]
        public FieldNotFoundException()
        {
            FieldName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldNotFoundException"/> class.
        /// </summary>
        /// <param name="fieldName">The message of the exception.</param>
        [PublicAPI]
        public FieldNotFoundException([NotNull] string fieldName)
            : base($"Could not find the field \"{fieldName}\".")
        {
            FieldName = fieldName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldNotFoundException"/> class.
        /// </summary>
        /// <param name="fieldName">The name of the field that was not found.</param>
        /// <param name="inner">The exception which caused this exception.</param>
        [PublicAPI]
        public FieldNotFoundException([NotNull] string fieldName, [CanBeNull] Exception inner)
            : base($"Could not find the field \"{fieldName}\".", inner)
        {
            FieldName = fieldName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="fieldName">The name of the field that was not found.</param>
        [PublicAPI]
        public FieldNotFoundException([NotNull] string message, [NotNull] string fieldName)
            : base(message)
        {
            FieldName = fieldName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="fieldName">The name of the field that was not found.</param>
        /// <param name="inner">The exception which caused this exception.</param>
        [PublicAPI]
        public FieldNotFoundException([NotNull] string message, [NotNull] string fieldName, [NotNull] Exception inner)
            : base(message, inner)
        {
            FieldName = fieldName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The serialized information.</param>
        /// <param name="context">The streaming context.</param>
        protected FieldNotFoundException([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            FieldName = info.GetString(nameof(FieldName)) ?? string.Empty;
        }

        /// <inheritdoc />
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(FieldName), FieldName);
            base.GetObjectData(info, context);
        }
    }
}
