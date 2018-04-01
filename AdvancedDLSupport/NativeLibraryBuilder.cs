//
//  NativeLibraryBuilder.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.DynamicAssemblyProviders;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.ImplementationGenerators;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using Mono.DllMap;
using Mono.DllMap.Extensions;
using static AdvancedDLSupport.ImplementationOptions;
using static System.Reflection.CallingConventions;
using static System.Reflection.MethodAttributes;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Builder class for anonymous types that bind to native libraries.
    /// </summary>
    [PublicAPI]
    public class NativeLibraryBuilder
    {
        /// <summary>
        /// Gets the name of the dynamic assembly.
        /// </summary>
        internal const string DynamicAssemblyName = "DLSupportDynamicAssembly";

        /// <summary>
        /// Gets the name of the dynamic module.
        /// </summary>
        internal const string DynamicModuleName = "DLSupportDynamicModule";

        /// <summary>
        /// Gets a builder instance with default settings. The default settings are
        /// <see cref="ImplementationOptions.GenerateDisposalChecks"/> and
        /// <see cref="ImplementationOptions.EnableDllMapSupport"/>.
        /// </summary>
        [PublicAPI, NotNull]
        public static NativeLibraryBuilder Default { get; }

        /// <summary>
        /// Gets the configuration object for this builder.
        /// </summary>
        [PublicAPI]
        public ImplementationOptions Options { get; }

        /// <summary>
        /// Gets the path resolver to use for resolving libraries.
        /// </summary>
        [NotNull]
        private ILibraryPathResolver PathResolver { get; }

        private readonly IDynamicAssemblyProvider _assemblyProvider;
        private readonly ModuleBuilder _moduleBuilder;

        private static readonly object BuilderLock = new object();

        private static readonly ConcurrentDictionary<GeneratedImplementationTypeIdentifier, Type> TypeCache;
        private static readonly TypeTransformerRepository TransformerRepository;

        static NativeLibraryBuilder()
        {
            TypeCache = new ConcurrentDictionary<GeneratedImplementationTypeIdentifier, Type>
            (
                new LibraryIdentifierEqualityComparer()
            );

            TransformerRepository = new TypeTransformerRepository();

            Default = new NativeLibraryBuilder
            (
                GenerateDisposalChecks | EnableDllMapSupport
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeLibraryBuilder"/> class.
        /// </summary>
        /// <param name="options">The configuration settings to use for the builder.</param>
        /// <param name="pathResolver">The path resolver to use.</param>
        /// <param name="assemblyProvider">Optional. The dynamic assembly provider to use. Defaults to a transient provider..</param>
        [PublicAPI]
        public NativeLibraryBuilder
        (
            ImplementationOptions options = default,
            [CanBeNull] ILibraryPathResolver pathResolver = default,
            [CanBeNull] IDynamicAssemblyProvider assemblyProvider = default
        )
        {
            #if DEBUG
            _assemblyProvider = assemblyProvider ?? new TransientDynamicAssemblyProvider(DynamicAssemblyName, true);
            #else
            _assemblyProvider = assemblyProvider ?? new TransientDynamicAssemblyProvider(DynamicAssemblyName, false);
            #endif

            _moduleBuilder = _assemblyProvider.GetDynamicModule(DynamicModuleName);

            Options = options;
            PathResolver = pathResolver ?? new DynamicLinkLibraryPathResolver();
        }

        /// <summary>
        /// Resolves a C function interface to an anonymous class that implements it, making the native functions
        /// available for use.
        /// </summary>
        /// <param name="libraryPath">The name of or path to the library.</param>
        /// <typeparam name="TInterface">The interface type.</typeparam>
        /// <returns>A generated class that implements the given interface.</returns>
        /// <exception cref="ArgumentException">Thrown if either of the type arguments are incompatible.</exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the specified library can't be found in any of the loader paths.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the resulting instance can't be cast to the expected class. Should never occur in user code.
        /// </exception>
        [NotNull, PublicAPI]
        public TInterface ActivateInterface<TInterface>([NotNull] string libraryPath) where TInterface : class
        {
            var anonymousInstance = ActivateClass<NativeLibraryBase, TInterface>(libraryPath);

            return anonymousInstance as TInterface
            ?? throw new InvalidOperationException
            (
                "The resulting instance was not convertible to an instance of the interface."
            );
        }

        /// <summary>
        /// Resolves a mixed-mode class that implementing a C function interface, making the native functions available
        /// for use.
        /// </summary>
        /// <param name="libraryPath">The name of or path to the library.</param>
        /// <typeparam name="TClass">The base class for the implementation to generate.</typeparam>
        /// <typeparam name="TInterface">The interface to implement.</typeparam>
        /// <returns>An instance of the class.</returns>
        /// <exception cref="ArgumentException">Thrown if either of the type arguments are incompatible.</exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the specified library can't be found in any of the loader paths.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the resulting instance can't be cast to the expected class. Should never occur in user code.
        /// </exception>
        [NotNull, PublicAPI]
        public TClass ActivateClass<TClass, TInterface>([NotNull] string libraryPath)
            where TClass : NativeLibraryBase
            where TInterface : class
        {
            var classType = typeof(TClass);
            if (!classType.IsAbstract)
            {
                throw new ArgumentException("The class to activate must be abstract.", nameof(TClass));
            }

            var interfaceType = typeof(TInterface);
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException
                (
                    "The interface to activate on the class must be an interface type.",
                    nameof(TInterface)
                );
            }

            // Check for remapping
            if (Options.HasFlagFast(EnableDllMapSupport))
            {
                libraryPath = new DllMapResolver().MapLibraryName(interfaceType, libraryPath);
            }

            // Attempt to resolve a name or path for the given library
            var resolveResult = PathResolver.Resolve(libraryPath);
            if (!resolveResult.IsSuccess)
            {
                throw new FileNotFoundException
                (
                    $"The specified library (\"{libraryPath}\") was not found in any of the loader search paths.",
                    libraryPath,
                    resolveResult.Exception
                );
            }

            libraryPath = resolveResult.Path;

            // Check if we've already generated a type for this configuration
            var key = new GeneratedImplementationTypeIdentifier(classType, interfaceType, libraryPath, Options);
            lock (BuilderLock)
            {
                if (!TypeCache.TryGetValue(key, out var generatedType))
                {
                    generatedType = GenerateInterfaceImplementationType<TClass, TInterface>();
                    TypeCache.TryAdd(key, generatedType);
                }

                try
                {
                    var anonymousInstance = CreateAnonymousImplementationInstance<TInterface>
                    (
                        generatedType,
                        libraryPath,
                        Options,
                        TransformerRepository
                    );

                    return anonymousInstance as TClass
                    ?? throw new InvalidOperationException
                    (
                        "The resulting instance was not convertible to an instance of the class."
                    );
                }
                catch (TargetInvocationException tex)
                {
                    if (tex.InnerException is null)
                    {
                        throw;
                    }

                    // Unwrap target invocation exceptions, since we can fail in the constructor
                    throw tex.InnerException;
                }
            }
        }

        /// <summary>
        /// Generates the implementation type for a given class and interface combination, caching it for later use.
        /// </summary>
        /// <param name="libraryPath">The name of or path to the library.</param>
        /// <typeparam name="TBaseClass">The base class for the implementation to generate.</typeparam>
        /// <typeparam name="TInterface">The interface to implement.</typeparam>
        /// <exception cref="ArgumentException">Thrown if either of the type arguments are incompatible.</exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the specified library can't be found in any of the loader paths.
        /// </exception>
        internal void PregenerateImplementationType<TBaseClass, TInterface>(string libraryPath)
            where TBaseClass : NativeLibraryBase
            where TInterface : class
        {
            var classType = typeof(TBaseClass);
            if (!classType.IsAbstract)
            {
                throw new ArgumentException
                (
                    "The class to activate must be abstract.",
                    nameof(TBaseClass)
                );
            }

            var interfaceType = typeof(TInterface);
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException
                (
                    "The interface to activate on the class must be an interface type.",
                    nameof(TInterface)
                );
            }

            // Check for remapping
            if (Options.HasFlagFast(EnableDllMapSupport))
            {
                libraryPath = new DllMapResolver().MapLibraryName(interfaceType, libraryPath);
            }

            // Attempt to resolve a name or path for the given library
            var resolveResult = PathResolver.Resolve(libraryPath);
            if (!resolveResult.IsSuccess)
            {
                throw new FileNotFoundException
                (
                    $"The specified library (\"{libraryPath}\") was not found in any of the loader search paths.",
                    libraryPath,
                    resolveResult.Exception
                );
            }

            libraryPath = resolveResult.Path;

            // Check if we've already generated a type for this configuration
            var key = new GeneratedImplementationTypeIdentifier(classType, interfaceType, libraryPath, Options);
            lock (BuilderLock)
            {
                if (!TypeCache.TryGetValue(key, out var generatedType))
                {
                    generatedType = GenerateInterfaceImplementationType<TBaseClass, TInterface>();
                    TypeCache.TryAdd(key, generatedType);
                }
            }
        }

        /// <summary>
        /// Generates the implementation type for a given class and interface combination, caching it for later use.
        /// </summary>
        /// <param name="libraryPath">The name of or path to the library.</param>
        /// <param name="classType">The base class for the implementation to generate.</param>
        /// <param name="interfaceType">The interface to implement.</param>
        /// <exception cref="ArgumentException">Thrown if either of the type arguments are incompatible.</exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the specified library can't be found in any of the loader paths.
        /// </exception>
        internal void PregenerateImplementationType
        (
            string libraryPath,
            [NotNull] Type classType,
            [NotNull] Type interfaceType
        )
        {
            if (!classType.IsAbstract)
            {
                throw new ArgumentException
                (
                    "The class to activate must be abstract.",
                    nameof(classType)
                );
            }

            if (!(classType.IsSubclassOf(typeof(NativeLibraryBase)) || classType == typeof(NativeLibraryBase)))
            {
                throw new ArgumentException
                (
                    $"The base class must be or derive from {nameof(NativeLibraryBase)}.",
                    nameof(classType)
                );
            }

            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException
                (
                    "The interface to activate on the class must be an interface type.",
                    nameof(interfaceType)
                );
            }

            // Check for remapping
            if (Options.HasFlagFast(EnableDllMapSupport))
            {
                libraryPath = new DllMapResolver().MapLibraryName(interfaceType, libraryPath);
            }

            // Check if we've already generated a type for this configuration
            var key = new GeneratedImplementationTypeIdentifier(classType, interfaceType, libraryPath, Options);
            lock (BuilderLock)
            {
                if (!TypeCache.TryGetValue(key, out var generatedType))
                {
                    generatedType = GenerateInterfaceImplementationType(classType, interfaceType);
                    TypeCache.TryAdd(key, generatedType);
                }
            }
        }

        /// <summary>
        /// Generates a type inheriting from the given class and implementing the given interface, setting it up to bind
        /// the interface functions to native C code.
        /// </summary>
        /// <typeparam name="TBaseClass">The base class of the type to generate.</typeparam>
        /// <typeparam name="TInterface">The interface that the type should implement.</typeparam>
        /// <returns>The type.</returns>
        [NotNull, Pure]
        private Type GenerateInterfaceImplementationType<TBaseClass, TInterface>()
            where TBaseClass : NativeLibraryBase
            where TInterface : class
        {
            var baseClassType = typeof(TBaseClass);
            var interfaceType = typeof(TInterface);

            return GenerateInterfaceImplementationType(baseClassType, interfaceType);
        }

        /// <summary>
        /// Generates a type inheriting from the given class and implementing the given interface, setting it up to bind
        /// the interface functions to native C code.
        /// </summary>
        /// <param name="classType">The base class for the implementation to generate.</param>
        /// <param name="interfaceType">The interface to implement.</param>
        /// <returns>The type.</returns>
        [NotNull, Pure]
        private Type GenerateInterfaceImplementationType([NotNull] Type classType, [NotNull] Type interfaceType)
        {
            if (!classType.IsAbstract)
            {
                throw new ArgumentException
                (
                    "The class to activate must be abstract.",
                    nameof(classType)
                );
            }

            if (!(classType.IsSubclassOf(typeof(NativeLibraryBase)) || classType == typeof(NativeLibraryBase)))
            {
                throw new ArgumentException
                (
                    $"The base class must be or derive from {nameof(NativeLibraryBase)}.",
                    nameof(classType)
                );
            }

            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException
                (
                    "The interface to activate on the class must be an interface type.",
                    nameof(interfaceType)
                );
            }

            var typeName = GenerateTypeName(interfaceType);

            // Create a new type for the anonymous implementation
            var typeBuilder = _moduleBuilder.DefineType
            (
                typeName,
                TypeAttributes.AutoClass | TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                classType,
                new[] { interfaceType }
            );

            // Now the constructor
            var anonymousConstructor = typeof(NativeLibraryBase)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .First
            (
                c => c.HasCustomAttribute<AnonymousConstructorAttribute>()
            );

            var constructorBuilder = typeBuilder.DefineConstructor
            (
                Public | SpecialName | RTSpecialName | HideBySig,
                Standard,
                anonymousConstructor.GetParameters().Select(p => p.ParameterType).ToArray()
            );

            constructorBuilder.DefineParameter(1, ParameterAttributes.In, "libraryPath");
            var constructorIL = constructorBuilder.GetILGenerator();
            for (int i = 0; i <= anonymousConstructor.GetParameters().Length; ++i)
            {
                constructorIL.Emit(OpCodes.Ldarg, i);
            }

            constructorIL.Emit(OpCodes.Call, anonymousConstructor);

            var pipeline = new ImplementationPipeline
            (
                _moduleBuilder,
                typeBuilder,
                constructorIL,
                Options,
                TransformerRepository
            );

            ConstructMethods(pipeline, classType, interfaceType);
            ConstructProperties(pipeline, classType, interfaceType);

            constructorIL.Emit(OpCodes.Ret);
            return typeBuilder.CreateTypeInfo();
        }

        /// <summary>
        /// Generates a type name for an anonymous type.
        /// </summary>
        /// <param name="type">The type to generate the name for.</param>
        /// <returns>The name.</returns>
        [NotNull, Pure]
        private static string GenerateTypeName([NotNull] Type type)
        {
            var typeName = type.Name.StartsWith("I")
                ? type.Name.Substring(1)
                : $"Generated_{type.Name}";

            if (typeName.IsNullOrWhiteSpace())
            {
                typeName = $"Generated_{type.Name}";
            }

            var uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", "_");
            typeName = $"{typeName}_{uniqueIdentifier}";
            return typeName;
        }

        /// <summary>
        /// Creates an instance of the final implementation type.
        /// </summary>
        /// <param name="finalType">The constructed anonymous type.</param>
        /// <param name="library">The path to or name of the library</param>
        /// <param name="options">The generator options.</param>
        /// <param name="transformerRepository">The type transformer repository.</param>
        /// <typeparam name="TInterface">The interface type.</typeparam>
        /// <returns>An instance of the anonymous type implementing <typeparamref name="TInterface"/>.</returns>
        [NotNull, Pure]
        private TInterface CreateAnonymousImplementationInstance<TInterface>
        (
            [NotNull] Type finalType,
            [NotNull] string library,
            ImplementationOptions options,
            [NotNull] TypeTransformerRepository transformerRepository
        )
        {
            return (TInterface)Activator.CreateInstance
            (
                finalType,
                library,
                typeof(TInterface),
                options,
                transformerRepository
            );
        }

        /// <summary>
        /// Constructs the implementations for all normal methods.
        /// </summary>
        /// <param name="pipeline">The implementation pipeline that consumes the methods.</param>
        /// <param name="classType">The base class of the type to generate methods for.</param>
        /// <param name="interfaceType">The interface where the methods originate.</param>
        private void ConstructMethods
        (
            [NotNull] ImplementationPipeline pipeline,
            [NotNull] Type classType,
            [NotNull] Type interfaceType
        )
        {
            var methods = new List<PipelineWorkUnit<IntrospectiveMethodInfo>>();
            foreach (var method in interfaceType.GetIntrospectiveMethods(true))
            {
                var targetMethod = method;

                // Skip any property accessor methods
                if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                {
                    continue;
                }

                // Skip methods with a managed implementation in the base class
                var baseClassMethod = classType.GetIntrospectiveMethod
                (
                    method.Name,
                    method.ParameterTypes.ToArray()
                );

                if (!(baseClassMethod is null))
                {
                    if (!baseClassMethod.IsAbstract)
                    {
                        continue;
                    }

                    targetMethod = baseClassMethod;
                }

                var definition = pipeline.GenerateDefinitionFromSignature(targetMethod);
                methods.Add
                (
                    new PipelineWorkUnit<IntrospectiveMethodInfo>
                    (
                        definition,
                        targetMethod.GetSymbolName(),
                        Options
                    )
                );
            }

            pipeline.ConsumeMethodDefinitions(methods);
        }

        /// <summary>
        /// Constructs the implementations for all properties.
        /// </summary>
        /// <param name="pipeline">The implementation pipeline that consumes the methods.</param>
        /// <param name="classType">The base class of the type to generator properties for.</param>
        /// <param name="interfaceType">The interface where the properties originate.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if any property is declared as partially abstract.
        /// </exception>
        private void ConstructProperties
        (
            [NotNull] ImplementationPipeline pipeline,
            [NotNull] Type classType,
            [NotNull] Type interfaceType
        )
        {
            var properties = new List<PipelineWorkUnit<IntrospectivePropertyInfo>>();
            foreach (var property in interfaceType.GetProperties())
            {
                var targetProperty = property;

                // Skip properties with a managed implementation
                var baseClassProperty = classType.GetProperty(property.Name, property.PropertyType);
                if (!(baseClassProperty is null))
                {
                    var isFullyManaged = !baseClassProperty.GetGetMethod().IsAbstract &&
                                         !baseClassProperty.GetSetMethod().IsAbstract;

                    if (isFullyManaged)
                    {
                        continue;
                    }

                    var isPartiallyAbstract = baseClassProperty.GetGetMethod().IsAbstract ^
                                              baseClassProperty.GetSetMethod().IsAbstract;

                    if (isPartiallyAbstract)
                    {
                        throw new InvalidOperationException
                        (
                            "Properties with overriding managed implementations may not be partially managed."
                        );
                    }

                    targetProperty = baseClassProperty;
                }

                var introspectiveProperty = new IntrospectivePropertyInfo(targetProperty);
                properties.Add
                (
                    new PipelineWorkUnit<IntrospectivePropertyInfo>
                    (
                        introspectiveProperty,
                        introspectiveProperty.GetSymbolName(),
                        Options
                    )
                );
            }

            pipeline.ConsumePropertyDefinitions(properties);
        }
    }
}
