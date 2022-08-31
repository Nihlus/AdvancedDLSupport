//
//  IResult.cs
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
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Represents an attempted action, which may or may not have succeeded.
    /// </summary>
    internal interface IResult
    {
        /// <summary>
        /// Gets a human-readable reason for the error.
        /// </summary>
        string? ErrorReason { get; }

        /// <summary>
        /// Gets a value indicating whether or not the result is a successful result.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Gets the exception which caused the error (if any).
        /// </summary>
        Exception? Exception { get; }
    }
}
