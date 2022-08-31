//
//  MethodNotFoundException.cs
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
using System.Runtime.Serialization;
using System.Security.Permissions;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Represents a failure to find a required method.
    /// </summary>
    [PublicAPI, Serializable]
    public class MethodNotFoundException : Exception
    {
        /// <summary>
        /// Gets the name of the method that was not found.
        /// </summary>
        [PublicAPI, NotNull]
        public string MethodName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotFoundException"/> class.
        /// </summary>
        [PublicAPI]
        public MethodNotFoundException()
        {
            MethodName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotFoundException"/> class.
        /// </summary>
        /// <param name="methodName">The name of the method that was not found.</param>
        [PublicAPI]
        public MethodNotFoundException([NotNull] string methodName)
            : base($"Could not find the field \"{methodName}\".")
        {
            MethodName = methodName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotFoundException"/> class.
        /// </summary>
        /// <param name="methodName">The name of the method that was not found.</param>
        /// <param name="inner">The exception which caused this exception.</param>
        [PublicAPI]
        public MethodNotFoundException([NotNull] string methodName, [CanBeNull] Exception inner)
            : base($"Could not find the field \"{methodName}\".", inner)
        {
            MethodName = methodName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="methodName">The name of the method that was not found.</param>
        [PublicAPI]
        public MethodNotFoundException([NotNull] string message, [NotNull] string methodName)
            : base(message)
        {
            MethodName = methodName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="methodName">The name of the method that was not found.</param>
        /// <param name="inner">The exception which caused this exception.</param>
        [PublicAPI]
        public MethodNotFoundException([NotNull] string message, [NotNull] string methodName, [NotNull] Exception inner)
            : base(message, inner)
        {
            MethodName = methodName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The serialized information.</param>
        /// <param name="context">The streaming context.</param>
        protected MethodNotFoundException([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            MethodName = info.GetString(nameof(MethodName)) ?? string.Empty;
        }

        /// <inheritdoc />
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(MethodName), MethodName);
            base.GetObjectData(info, context);
        }
    }
}
