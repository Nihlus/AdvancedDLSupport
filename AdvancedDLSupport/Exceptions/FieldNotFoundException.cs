//
//  FieldNotFoundException.cs
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

namespace AdvancedDLSupport;

/// <summary>
/// Represents a failure to find a required field.
/// </summary>
[PublicAPI, Serializable]
public class FieldNotFoundException : Exception
{
    /// <summary>
    /// Gets the name of the field that was not found.
    /// </summary>
    [PublicAPI]
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
    public FieldNotFoundException(string fieldName)
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
    public FieldNotFoundException(string fieldName, Exception? inner)
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
    public FieldNotFoundException(string message, string fieldName)
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
    public FieldNotFoundException(string message, string fieldName, Exception inner)
        : base(message, inner)
    {
        FieldName = fieldName;
    }
}
