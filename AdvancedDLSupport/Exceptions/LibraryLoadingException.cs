//
//  LibraryLoadingException.cs
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
    /// Represents a failure to load a native library.
    /// </summary>
    [PublicAPI, Serializable]
    public class LibraryLoadingException : Exception
    {
        /// <summary>
        /// Gets the name of the library that failed to load.
        /// </summary>
        [PublicAPI]
        public string LibraryName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        [PublicAPI]
        public LibraryLoadingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        [PublicAPI]
        public LibraryLoadingException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="inner">The exception which caused this exception.</param>
        [PublicAPI]
        public LibraryLoadingException(string? message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="libraryName">The name of the library that failed to load.</param>
        [PublicAPI]
        public LibraryLoadingException(string message, string? libraryName)
            : base(message)
        {
            LibraryName = libraryName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="libraryName">The name of the library that failed to load.</param>
        /// <param name="inner">The exception which caused this exception.</param>
        [PublicAPI]
        public LibraryLoadingException(string message, string? libraryName, Exception inner)
            : base(message, inner)
        {
            LibraryName = libraryName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        /// <param name="info">The serialized information.</param>
        /// <param name="context">The streaming context.</param>
        protected LibraryLoadingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            LibraryName = info.GetString(nameof(LibraryName));
        }

        /// <inheritdoc />
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(LibraryName), LibraryName);
            base.GetObjectData(info, context);
        }
    }
}
