//
//  ResolvePathResult.cs
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
    /// Represents an attempt to resolve a path.
    /// </summary>
    [PublicAPI]
    public struct ResolvePathResult : IResult
    {
        /// <summary>
        /// Gets the resolved path.
        /// </summary>
        [PublicAPI]
        public string Path { get; }

        /// <inheritdoc />
        [PublicAPI]
        public string ErrorReason { get; }

        /// <inheritdoc />
        [PublicAPI]
        public bool IsSuccess { get; }

        /// <inheritdoc />
        [PublicAPI]
        public Exception Exception { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvePathResult"/> struct.
        /// </summary>
        /// <param name="path">The path that was resolved. Can be null.</param>
        /// <param name="errorReason">The reason why the path couldn't be resolved. Can be null.</param>
        /// <param name="isSuccess">Whether or not a path was resolved.</param>
        /// <param name="exception">The exception which caused the path resolving to fail.</param>
        private ResolvePathResult
        (
            [CanBeNull] string path,
            [CanBeNull] string errorReason,
            bool isSuccess,
            [CanBeNull] Exception exception
        )
        {
            Path = path;
            ErrorReason = errorReason;
            IsSuccess = isSuccess;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="resolvedPath">The path that was resolved.</param>
        /// <returns>A successful result.</returns>
        [PublicAPI]
        public static ResolvePathResult FromSuccess([CanBeNull] string resolvedPath)
        {
            return new ResolvePathResult(resolvedPath, null, true, null);
        }

        /// <summary>
        /// Creates an unsuccessful result.
        /// </summary>
        /// <param name="errorReason">The reason why the resolution failed.</param>
        /// <returns>A failed result.</returns>
        [PublicAPI]
        public static ResolvePathResult FromError([NotNull] string errorReason)
        {
            return new ResolvePathResult(null, errorReason, false, null);
        }

        /// <summary>
        /// Creates an unsuccessful result.
        /// </summary>
        /// <param name="exception">The exception that caused the resolution to fail.</param>
        /// <returns>A failed result.</returns>
        [PublicAPI]
        public static ResolvePathResult FromError([NotNull] Exception exception)
        {
            return new ResolvePathResult(null, exception.Message, false, exception);
        }
    }
}
