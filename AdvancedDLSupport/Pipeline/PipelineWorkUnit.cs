//
//  PipelineWorkUnit.cs
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
using System.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Pipeline;

/// <summary>
/// Represents a unit of work passing through the pipeline.
/// </summary>
/// <typeparam name="T">The type of the unit being worked on.</typeparam>
[PublicAPI]
public class PipelineWorkUnit<T> where T : MemberInfo
{
    /// <summary>
    /// Gets the name of the native symbol that the unit of work maps to.
    /// </summary>
    [PublicAPI]
    public string SymbolName { get; }

    /// <summary>
    /// Gets the name of the original member that the unit of work stems from.
    /// </summary>
    [PublicAPI]
    public string? BaseMemberName { get; }

    /// <summary>
    /// Gets a unique identifier that can be used in generated definition names.
    /// </summary>
    [PublicAPI]
    public string UniqueIdentifier { get; }

    /// <summary>
    /// Gets the definition that the work unit wraps.
    /// </summary>
    [PublicAPI]
    public T Definition { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineWorkUnit{T}"/> class.
    /// </summary>
    /// <param name="definition">The definition to wrap.</param>
    /// <param name="symbolName">The native symbol name.</param>
    /// <param name="options">The options used when this work unit was created.</param>
    [PublicAPI]
    public PipelineWorkUnit(T definition, string symbolName, ImplementationOptions options)
    {
        Definition = definition;
        BaseMemberName = definition.Name;
        SymbolName = symbolName;
        UniqueIdentifier = ((ulong)options).ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineWorkUnit{T}"/> class.
    /// </summary>
    /// <param name="definition">The definition to wrap.</param>
    /// <param name="baseUnit">The unit of work to base this unit off of.</param>
    [PublicAPI]
    public PipelineWorkUnit(T definition, PipelineWorkUnit<T> baseUnit)
    {
        Definition = definition;
        SymbolName = baseUnit.SymbolName;
        UniqueIdentifier = baseUnit.UniqueIdentifier;
    }

    /// <summary>
    /// Gets the base member name. This name is guaranteed to be unique for a given native symbol and implementation
    /// option combination.
    /// </summary>
    /// <returns>The base member name.</returns>
    [PublicAPI]
    public string GetUniqueBaseMemberName()
    {
        return $"{BaseMemberName}_{SymbolName}_{UniqueIdentifier}_{Guid.NewGuid().ToString().ToLowerInvariant()}";
    }
}
