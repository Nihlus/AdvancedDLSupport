//
//  PersistentDynamicAssemblyProvider.cs
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
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.DynamicAssemblyProviders;
using JetBrains.Annotations;

namespace AdvancedDLSupport.AOT;

/// <summary>
/// Provides a persistent dynamic assembly for use with the native binding generator.
/// </summary>
public class PersistentDynamicAssemblyProvider : IDynamicAssemblyProvider
{
    private readonly AssemblyBuilder _dynamicAssembly;

    private bool _isDisposed;
    private ModuleBuilder? _dynamicModule;

    /// <summary>
    /// Gets a value indicating whether or not the assembly is debuggable.
    /// </summary>
    [PublicAPI]
    public bool IsDebuggable { get; }

    /// <summary>
    /// Gets the name of the dynamic assembly.
    /// </summary>
    public static string DynamicAssemblyName => "DLSupportDynamicAssembly";

    /// <summary>
    /// Gets the output filename of the assembly.
    /// </summary>
    public string OutputFilename { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PersistentDynamicAssemblyProvider"/> class.
    /// </summary>
    /// <param name="debuggable">
    /// Whether or not the assembly should be marked as debuggable. This disables any compiler optimizations.
    /// </param>
    /// <param name="outputDirectory">The directory where the dynamic assembly should be saved.</param>
    [PublicAPI]
    public PersistentDynamicAssemblyProvider(string outputDirectory, bool debuggable)
    {
        IsDebuggable = debuggable;

        OutputFilename = $"{DynamicAssemblyName}_{Guid.NewGuid().ToString().ToLowerInvariant()}.dll";

        _dynamicAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly
        (
            new AssemblyName(DynamicAssemblyName),
            AssemblyBuilderAccess.RunAndSave,
            outputDirectory
        );

        if (!debuggable)
        {
            return;
        }

        var dbgType = typeof(DebuggableAttribute);
        var dbgConstructor = dbgType.GetConstructor(new[] { typeof(DebuggableAttribute.DebuggingModes) });
        var dbgModes = new object[]
        {
            DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.Default
        };

        if (dbgConstructor is null)
        {
            throw new InvalidOperationException($"Could not find the {nameof(DebuggableAttribute)} constructor.");
        }

        var dbgBuilder = new CustomAttributeBuilder(dbgConstructor, dbgModes);

        _dynamicAssembly.SetCustomAttribute(dbgBuilder);
    }

    /// <inheritdoc />
    public AssemblyBuilder GetDynamicAssembly()
    {
        return _dynamicAssembly;
    }

    /// <inheritdoc/>
    public ModuleBuilder GetDynamicModule()
    {
        return _dynamicModule ??= _dynamicAssembly.DefineDynamicModule
        (
            "DLSupportDynamicModule",
            OutputFilename,
            IsDebuggable
        );
    }
}
