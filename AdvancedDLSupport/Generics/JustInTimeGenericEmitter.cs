//
//  JustInTimeGenericEmitter.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Generics
{
    /// <summary>
    /// Acts as a micro-JIT for generic methods, allowing call-time emission of a compatible method signature.
    /// </summary>
    internal class JustInTimeGenericEmitter : IDisposable
    {
        private bool _isDisposed;

        private NativeLibraryBuilder _builder = NativeLibraryBuilder.Default;

        private Dictionary<GenericMethodSignature, MethodInfo> _closedImplementations;
        private Dictionary<GenericMethodSignature, NativeLibraryBase> _closedImplementationTypeInstances;

        /// <summary>
        /// Initializes a new instance of the <see cref="JustInTimeGenericEmitter"/> class.
        /// </summary>
        public JustInTimeGenericEmitter()
        {
            _closedImplementations = new Dictionary<GenericMethodSignature, MethodInfo>();
            _closedImplementationTypeInstances = new Dictionary<GenericMethodSignature, NativeLibraryBase>();
        }

        /// <summary>
        /// Invokes the closed implementation of the given method info, creating one if one doesn't exist.
        /// </summary>
        /// <param name="methodInfo">The method info of the runtime-called method.</param>
        /// <param name="libraryPath">The path to the library that the outer type instance was created with.</param>
        /// <param name="arguments">The arguments to the method, if any.</param>
        /// <returns>The implementations return value, if any.</returns>
        public object InvokeClosedImplementation
        (
            [NotNull] IntrospectiveMethodInfo methodInfo,
            string libraryPath,
            object[] arguments
        )
        {
            if (!HasClosedImplementation(methodInfo))
            {
                CreateClosedImplementation(methodInfo, libraryPath);
            }

            var closedImplementationTypeInstance = GetClosedImplementationTypeInstance(methodInfo);
            var closedImplementationMethod = GetClosedImplementationMethod(methodInfo);

            return closedImplementationMethod.Invoke(closedImplementationTypeInstance, arguments);
        }

        /// <summary>
        /// Determines whether or not the jitter has created a closed implementation for the given method info.
        /// </summary>
        /// <param name="methodInfo">The method.</param>
        /// <returns>true if a closed implementation exists; otherwise, false.</returns>
        private bool HasClosedImplementation([NotNull] IntrospectiveMethodInfo methodInfo)
        {
            var signature = new GenericMethodSignature(methodInfo);

            return _closedImplementations.ContainsKey(signature) &&
                   _closedImplementationTypeInstances.ContainsKey(signature);
        }

        /// <summary>
        /// Creates a closed implementation for a given method, generating a new type in the dynamic assembly to host
        /// it.
        /// </summary>
        /// <param name="methodInfo">The method.</param>
        /// <param name="libraryPath">The path of the library to bind to.</param>
        private void CreateClosedImplementation
        (
            [NotNull] IntrospectiveMethodInfo methodInfo,
            [NotNull] string libraryPath
        )
        {
            var hostType = CreateHostInterface(methodInfo);
            CreateHostMethod(hostType, methodInfo);

            var finalInterfaceType = hostType.CreateTypeInfo();

            var implementationTypeInstance = _builder.ActivateClass
            (
                libraryPath,
                typeof(NativeLibraryBase),
                finalInterfaceType
            );

            var implementationMethod = implementationTypeInstance.GetType().GetMethods().First
            (
                m =>
                    m.ReturnType == methodInfo.ReturnType && m.GetParameters()
                        .Select
                        (
                            p => p.ParameterType
                        )
                        .SequenceEqual(methodInfo.ParameterTypes)
            );

            // Store a reference to the implementation so that it doesn't get garbage collected
            _closedImplementationTypeInstances.Add(new GenericMethodSignature(methodInfo), (NativeLibraryBase)implementationTypeInstance);
            _closedImplementations.Add(new GenericMethodSignature(methodInfo), implementationMethod);
        }

        /// <summary>
        /// Gets a managed implementation of the given closed method.
        /// </summary>
        /// <param name="methodInfo">The closed signature.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the method doesn't have a closed implementation.</exception>
        /// <returns>The method.</returns>
        [NotNull]
        private MethodInfo GetClosedImplementationMethod
        (
            [NotNull] IntrospectiveMethodInfo methodInfo
        )
        {
            var signature = new GenericMethodSignature(methodInfo);

            if (_closedImplementations.TryGetValue(signature, out var result))
            {
                return result;
            }

            throw new KeyNotFoundException("The given method didn't have a closed implementation.");
        }

        /// <summary>
        /// Gets the instance that's hosting the managed implementation of the given closed method.
        /// </summary>
        /// <param name="methodInfo">The closed signature.</param>
        /// <exception cref="KeyNotFoundException">Thrown if the method doesn't have a closed implementation.</exception>
        /// <returns>The instance.</returns>
        [NotNull]
        private NativeLibraryBase GetClosedImplementationTypeInstance
        (
            [NotNull] IntrospectiveMethodInfo methodInfo
        )
        {
            var signature = new GenericMethodSignature(methodInfo);

            if (_closedImplementationTypeInstances.TryGetValue(signature, out var result))
            {
                return result;
            }

            throw new KeyNotFoundException("The given method didn't have a closed implementation.");
        }

        /// <summary>
        /// Creates a hosting interface for the given method info.
        /// </summary>
        /// <param name="methodInfo">The method.</param>
        /// <returns>The created host type.</returns>
        [NotNull]
        private TypeBuilder CreateHostInterface([NotNull] IntrospectiveMethodInfo methodInfo)
        {
            var parameterList = $"{string.Join("_", methodInfo.ParameterTypes)}";
            var hostTypeName = $"{methodInfo.ReturnType}_{methodInfo.Name}_{parameterList}_closed_implementation";
            var hostType = _builder.GetDynamicModule().DefineType
            (
                hostTypeName,
                TypeAttributes.Interface | TypeAttributes.Public
            );

            return hostType;
        }

        /// <summary>
        /// Creates a hosting method definition for the given method info.
        /// </summary>
        /// <param name="hostType">The type to create the method in.</param>
        /// <param name="methodInfo">The method.</param>
        private void CreateHostMethod([NotNull] TypeBuilder hostType, [NotNull] IntrospectiveMethodInfo methodInfo)
        {
            var hostMethod = hostType.DefineMethod
            (
                $"{methodInfo.Name}_closed_implementation",
                MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.Abstract,
                CallingConventions.Standard,
                methodInfo.ReturnType,
                methodInfo.ParameterTypes.ToArray()
            );

            hostMethod.ApplyCustomAttributesFrom(methodInfo, methodInfo.ReturnType, methodInfo.ParameterTypes);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            foreach (var nestedImplementation in _closedImplementationTypeInstances.Values)
            {
                nestedImplementation.Dispose();
            }
        }
    }
}
