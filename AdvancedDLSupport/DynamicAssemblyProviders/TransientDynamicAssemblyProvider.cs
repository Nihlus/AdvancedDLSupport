//
//  TransientDynamicAssemblyProvider.cs
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
using JetBrains.Annotations;

namespace AdvancedDLSupport.DynamicAssemblyProviders
{
    /// <summary>
    /// Provides transient dynamic assemblies.
    /// </summary>
    [PublicAPI]
    public class TransientDynamicAssemblyProvider : IDynamicAssemblyProvider
    {
        /// <summary>
        /// Gets a value indicating whether or not the assembly is debuggable.
        /// </summary>
        [PublicAPI]
        public bool IsDebuggable { get; }

        private AssemblyBuilder _dynamicAssembly;

        private ModuleBuilder _dynamicModule;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientDynamicAssemblyProvider"/> class.
        /// </summary>
        /// <param name="assemblyName">
        /// The name of the dynamic assembly. This name will be suffixed with a unique identifier.
        /// </param>
        /// <param name="debuggable">
        /// Whether or not the assembly should be marked as debuggable. This disables any compiler optimizations.
        /// </param>
        [PublicAPI]
        public TransientDynamicAssemblyProvider(string assemblyName, bool debuggable)
        {
            IsDebuggable = debuggable;

            _dynamicAssembly = AssemblyBuilder.DefineDynamicAssembly
            (
                new AssemblyName(assemblyName), AssemblyBuilderAccess.Run
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

        /// <inheritdoc/>
        public AssemblyBuilder GetDynamicAssembly()
        {
            return _dynamicAssembly;
        }

        /// <inheritdoc/>
        public ModuleBuilder GetDynamicModule()
        {
            return _dynamicModule ?? (_dynamicModule = _dynamicAssembly.DefineDynamicModule("DLSupportDynamicModules"));
        }
    }
}
