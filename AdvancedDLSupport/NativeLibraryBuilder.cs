//
//  NativeLibraryBuilder.cs
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.AOT;
using AdvancedDLSupport.DynamicAssemblyProviders;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.Loaders;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using Mono.DllMap;
using Mono.DllMap.Extensions;
using static AdvancedDLSupport.ImplementationOptions;
using static System.Reflection.CallingConventions;
using static System.Reflection.MethodAttributes;
using Assembly = System.Reflection.Assembly;

namespace AdvancedDLSupport;

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
    /// Gets a builder instance with default settings. The default settings are
    /// <see cref="ImplementationOptions.GenerateDisposalChecks"/> and
    /// <see cref="ImplementationOptions.EnableDllMapSupport"/>.
    /// </summary>
    [PublicAPI]
    public static NativeLibraryBuilder Default { get; }

    /// <summary>
    /// Gets the configuration object for this builder.
    /// </summary>
    [PublicAPI]
    public ImplementationOptions Options { get; }

    /// <summary>
    /// Gets the path resolver to use for resolving libraries.
    /// </summary>
    private ILibraryPathResolver PathResolver { get; }

    private readonly IDynamicAssemblyProvider _assemblyProvider;

    private readonly ModuleBuilder _moduleBuilder;

    private static readonly object BuilderLock = new object();

    private static readonly ConcurrentDictionary<GeneratedImplementationTypeIdentifier, Type> TypeCache;

    private ILibraryLoader? _customLibraryLoader;
    private ISymbolLoader? _customSymbolLoader;

    static NativeLibraryBuilder()
    {
        TypeCache = new ConcurrentDictionary<GeneratedImplementationTypeIdentifier, Type>
        (
            new LibraryIdentifierEqualityComparer()
        );

        Default = new NativeLibraryBuilder
        (
            GenerateDisposalChecks | EnableDllMapSupport | EnableOptimizations | SuppressSecurity
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
        ILibraryPathResolver? pathResolver = default,
        IDynamicAssemblyProvider? assemblyProvider = default
    )
    {
        _assemblyProvider = assemblyProvider ?? new TransientDynamicAssemblyProvider(DynamicAssemblyName, !options.HasFlagFast(EnableOptimizations));

        _moduleBuilder = _assemblyProvider.GetDynamicModule();

        Options = options;
        PathResolver = pathResolver ?? new DynamicLinkLibraryPathResolver();
    }

    /// <summary>
    /// Overrides the default symbol loader for this instance of <see cref="NativeLibraryBuilder"/>.
    /// </summary>
    /// <param name="factory">Factory to create the overriding symbol loader.</param>
    /// <returns>This instance, with the symbol loader overridden.</returns>
    public NativeLibraryBuilder WithSymbolLoader(Func<ISymbolLoader, ISymbolLoader> factory)
    {
        _customSymbolLoader = factory(PlatformLoaderBase.PlatformLoader);
        return this;
    }

    /// <summary>
    /// Overrides the default library loader for this instance of <see cref="NativeLibraryBuilder"/>.
    /// </summary>
    /// <param name="factory">Factory to create the overriding library loader.</param>
    /// <returns>This instance of <see cref="NativeLibraryBuilder"/>.</returns>
    public NativeLibraryBuilder WithLibraryLoader(Func<ILibraryLoader, ILibraryLoader> factory)
    {
        _customLibraryLoader = factory(PlatformLoaderBase.PlatformLoader);
        return this;
    }

    /// <summary>
    /// Scans the given directory for assemblies, attempting to discover pregenerated native binding types.
    /// </summary>
    /// <param name="searchDirectory">The directory to search.</param>
    /// <param name ="searchPattern">
    /// The pattern to search for in file names. Defaults to all files ending with .dll.
    /// </param>
    [PublicAPI]
    public static void DiscoverCompiledTypes(string searchDirectory, string searchPattern = "*.dll")
    {
        var assemblyPaths = Directory.EnumerateFiles(searchDirectory, searchPattern, SearchOption.AllDirectories);

        foreach (var assemblyPath in assemblyPaths)
        {
            var assembly = Assembly.LoadFile(assemblyPath);
            if (!assembly.HasCustomAttribute<AOTAssemblyAttribute>())
            {
                continue;
            }

            DiscoverCompiledTypes(assembly);
        }
    }

    /// <summary>
    /// Scans the assembly represented by the given stream, attempting to discover pregenerated native binding types.
    /// </summary>
    /// <param name="stream">A stream of an assembly to search.</param>
    [PublicAPI]
    public static void DiscoverCompiledTypes(Stream stream)
    {
        // unfortunately, we don't have any methods to load an assembly from Stream
        using (var byteStream = new MemoryStream())
        {
            stream.CopyTo(byteStream);
            DiscoverCompiledTypes(Assembly.Load(byteStream.ToArray()));
        }
    }

    /// <summary>
    /// Scans the given assembly, attempting to discover pregenerated native binding types.
    /// </summary>
    /// <param name="assembly">The assembly to search.</param>
    [PublicAPI]
    public static void DiscoverCompiledTypes(Assembly assembly)
    {
        var metadataType = assembly.GetExportedTypes().FirstOrDefault
        (
            t =>
                t.HasCustomAttribute<AOTMetadataAttribute>() && t.HasInterface<IAOTMetadata>()
        );

        if (metadataType is null)
        {
            throw new InvalidOperationException("The assembly did not contain a compatible metadata type.");
        }

        var typeDictionaryProperty = metadataType.GetProperty(nameof(IAOTMetadata.GeneratedTypes))!;

        var metadataInstance = Activator.CreateInstance(metadataType);

        var typeDictionary =
            (IReadOnlyDictionary<GeneratedImplementationTypeIdentifier, Type>)typeDictionaryProperty.GetValue
            (
                metadataInstance
            )!;

        foreach (var generatedType in typeDictionary)
        {
            lock (BuilderLock)
            {
                if (TypeCache.ContainsKey(generatedType.Key))
                {
                    continue;
                }

                TypeCache.TryAdd(generatedType.Key, generatedType.Value);
            }
        }
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
    [PublicAPI]
    public TInterface ActivateInterface<TInterface>(string libraryPath) where TInterface : class
    {
        // Check for remapping
        if (Options.HasFlagFast(EnableDllMapSupport))
        {
            libraryPath = new DllMapResolver().MapLibraryName(typeof(TInterface), libraryPath);
        }

        var anonymousInstance = ActivateClass<NativeLibraryBase, TInterface>(libraryPath);

        return anonymousInstance as TInterface
               ?? throw new InvalidOperationException
               (
                   "The resulting instance was not convertible to an instance of the interface."
               );
    }

    /// <summary>
    /// Resolves a mixed-mode class that implements a C function interface, making the native functions available
    /// for use.
    /// </summary>
    /// <param name="libraryPath">The name of or path to the library.</param>
    /// <typeparam name="TClass">The base class for the implementation to generate.</typeparam>
    /// <returns>An instance of the class.</returns>
    /// <exception cref="ArgumentException">Thrown if either of the type arguments are incompatible.</exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown if the specified library can't be found in any of the loader paths.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the resulting instance can't be cast to the expected class. Should never occur in user code.
    /// </exception>
    [PublicAPI]
    public TClass ActivateClass<TClass>(string libraryPath)
        where TClass : NativeLibraryBase
    {
        var classType = typeof(TClass);

        // Check for remapping
        if (Options.HasFlagFast(EnableDllMapSupport))
        {
            libraryPath = new DllMapResolver().MapLibraryName(classType, libraryPath);
        }

        return (TClass)ActivateClass(libraryPath, classType, classType.GetInterfaces());
    }

    /// <summary>
    /// Resolves a mixed-mode class that implements a C function interface, making the native functions available
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
    [PublicAPI]
    public TClass ActivateClass<TClass, TInterface>(string libraryPath)
        where TClass : NativeLibraryBase
        where TInterface : class
    {
        var classType = typeof(TClass);

        // Check for remapping
        if (Options.HasFlagFast(EnableDllMapSupport))
        {
            libraryPath = new DllMapResolver().MapLibraryName(classType, libraryPath);
        }

        return (TClass)ActivateClass(libraryPath, classType, typeof(TInterface));
    }

    /// <summary>
    /// Resolves a mixed-mode class that implements a C function interface, making the native functions available
    /// for use.
    /// </summary>
    /// <param name="libraryPath">The name of or path to the library.</param>
    /// <param name="baseClassType">The base class for the implementation to generate.</param>
    /// <param name="interfaceTypes">The interfaces to implement.</param>
    /// <returns>An instance of the class.</returns>
    /// <exception cref="ArgumentException">Thrown if either of the type arguments are incompatible.</exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown if the specified library can't be found in any of the loader paths.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the resulting instance can't be cast to the expected class. Should never occur in user code.
    /// </exception>
    [PublicAPI]
    public object ActivateClass
    (
        string libraryPath,
        Type baseClassType,
        params Type[] interfaceTypes
    )
    {
        if (!baseClassType.IsAbstract)
        {
            throw new ArgumentException("The class to activate must be abstract.", nameof(baseClassType));
        }

        if (!interfaceTypes.Any(i => i.IsInterface))
        {
            throw new ArgumentException
            (
                "The interfaces to activate on the class must be an interface type.",
                nameof(interfaceTypes)
            );
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

        libraryPath = resolveResult.Path!;

        // Check if we've already generated a type for this configuration
        var key = new GeneratedImplementationTypeIdentifier(baseClassType, interfaceTypes, Options);
        lock (BuilderLock)
        {
            if (!TypeCache.TryGetValue(key, out var generatedType))
            {
                generatedType = GenerateInterfaceImplementationType(baseClassType, interfaceTypes);
                TypeCache.TryAdd(key, generatedType);
            }

            try
            {
                var anonymousInstance = CreateAnonymousImplementationInstance
                (
                    generatedType,
                    libraryPath,
                    Options,
                    _customLibraryLoader,
                    _customSymbolLoader
                );

                return anonymousInstance;
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
    /// <param name="classType">The base class for the implementation to generate.</param>
    /// <param name="interfaceTypes">The interfaces to implement.</param>
    /// <exception cref="ArgumentException">Thrown if either of the type arguments are incompatible.</exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown if the specified library can't be found in any of the loader paths.
    /// </exception>
    /// <returns>A key-value tuple of the generated type identifier and the type.</returns>
    internal Tuple<GeneratedImplementationTypeIdentifier, Type> PregenerateImplementationType
    (
        Type classType,
        params Type[] interfaceTypes
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

        if (!interfaceTypes.Any(i => i.IsInterface))
        {
            throw new ArgumentException
            (
                "The interfaces to activate on the class must be an interface type.",
                nameof(interfaceTypes)
            );
        }

        // Check if we've already generated a type for this configuration
        var key = new GeneratedImplementationTypeIdentifier(classType, interfaceTypes, Options);
        Type? generatedType;
        lock (BuilderLock)
        {
            if (TypeCache.TryGetValue(key, out generatedType))
            {
                return new Tuple<GeneratedImplementationTypeIdentifier, Type>(key, generatedType);
            }

            generatedType = GenerateInterfaceImplementationType(classType, interfaceTypes);
            TypeCache.TryAdd(key, generatedType);
        }

        return new Tuple<GeneratedImplementationTypeIdentifier, Type>(key, generatedType);
    }

    /// <summary>
    /// Generates a type inheriting from the given class and implementing the given interface, setting it up to bind
    /// the interface functions to native C code.
    /// </summary>
    /// <param name="classType">The base class for the implementation to generate.</param>
    /// <param name="interfaceTypes">The interfaces to implement.</param>
    /// <returns>The type.</returns>
    [Pure]
    private Type GenerateInterfaceImplementationType
    (
        Type classType,
        params Type[] interfaceTypes
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

        if (!interfaceTypes.Any(i => i.IsInterface))
        {
            throw new ArgumentException
            (
                "The interface to activate on the class must be an interface type.",
                nameof(interfaceTypes)
            );
        }

        var typeName = GenerateTypeName(classType);

        // Create a new type for the anonymous implementation
        var typeBuilder = _moduleBuilder.DefineType
        (
            typeName,
            TypeAttributes.AutoClass | TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
            classType,
            interfaceTypes
        );

        // Now the constructor
        var anonymousConstructor = typeof(NativeLibraryBase)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .First
            (
                c => c.HasCustomAttribute<AnonymousConstructorAttribute>()
            );

        var constructorParams = anonymousConstructor.GetParameters();

        var constructorBuilder = typeBuilder.DefineConstructor
        (
            Public | SpecialName | RTSpecialName | HideBySig,
            Standard,
            constructorParams.Select(p => p.ParameterType).ToArray()
        );

        constructorBuilder.DefineParameter(1, ParameterAttributes.In, "libraryPath");
        var constructorIL = constructorBuilder.GetILGenerator();
        for (var i = 0; i <= constructorParams.Length; ++i)
        {
            constructorIL.Emit(OpCodes.Ldarg, i);
        }

        constructorIL.Emit(OpCodes.Call, anonymousConstructor);

        var pipeline = new ImplementationPipeline
        (
            _moduleBuilder,
            typeBuilder,
            constructorIL,
            Options
        );

        ConstructMethods(pipeline, classType, interfaceTypes);
        ConstructProperties(pipeline, classType, interfaceTypes);

        constructorIL.Emit(OpCodes.Ret);
        return typeBuilder.CreateTypeInfo()!;
    }

    /// <summary>
    /// Generates a type name for an anonymous type.
    /// </summary>
    /// <param name="type">The type to generate the name for.</param>
    /// <returns>The name.</returns>
    [Pure]
    private static string GenerateTypeName(Type type)
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
    /// <param name="library">The path to or name of the library.</param>
    /// <param name="options">The generator options.</param>
    /// <typeparam name="TInterface">The interface type.</typeparam>
    /// <returns>An instance of the anonymous type implementing <typeparamref name="TInterface"/>.</returns>
    [NotNull, Pure]
    private TInterface CreateAnonymousImplementationInstance<TInterface>
    (
        Type finalType,
        string library,
        ImplementationOptions options
    )
    {
        return (TInterface)CreateAnonymousImplementationInstance(finalType, library, options);
    }

    /// <summary>
    /// Creates an instance of the final implementation type.
    /// </summary>
    /// <param name="finalType">The constructed anonymous type.</param>
    /// <param name="library">The path to or name of the library.</param>
    /// <param name="options">The generator options.</param>
    /// <returns>An instance of the anonymous type.</returns>
    private object CreateAnonymousImplementationInstance
    (
        Type finalType,
        string? library,
        ImplementationOptions options,
        ILibraryLoader? libLoader = null,
        ISymbolLoader? symLoader = null
    )
    {
        return Activator.CreateInstance
        (
            finalType,
            library,
            options,
            libLoader,
            symLoader
        )!;
    }

    /// <summary>
    /// Constructs the implementations for all normal methods.
    /// </summary>
    /// <param name="pipeline">The implementation pipeline that consumes the methods.</param>
    /// <param name="classType">The base class of the type to generate methods for.</param>
    /// <param name="interfaceTypes">The interfaces where the methods originate.</param>
    private void ConstructMethods
    (
        ImplementationPipeline pipeline,
        Type classType,
        params Type[] interfaceTypes
    )
    {
        var constructedMethods = new List<IntrospectiveMethodInfo>();

        var symbolTransformer = SymbolTransformer.Default;
        var methods = new List<PipelineWorkUnit<IntrospectiveMethodInfo>>();
        foreach (var interfaceType in interfaceTypes)
        {
            foreach (var method in interfaceType.GetIntrospectiveMethods(true))
            {
                var targetMethod = method;

                // Skip any property accessor methods
                if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                {
                    continue;
                }

                // Skip methods that were already constructed - happens with inherited interfaces and multiple
                // identical definitions
                var existingMethod = constructedMethods.FirstOrDefault(m => m.HasSameSignatureAs(method));
                if (!(existingMethod is null))
                {
                    if (existingMethod.HasSameNativeEntrypointAs(targetMethod))
                    {
                        pipeline.TargetType.DefineMethodOverride
                        (
                            existingMethod.GetWrappedMember(),
                            targetMethod.GetWrappedMember()
                        );

                        continue;
                    }
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
                }

                // If we have an existing method at this point, this new method must be created as an explicit
                // interface implementation. Therefore, we override the method name.
                IntrospectiveMethodInfo definition;
                if (!(existingMethod is null))
                {
                    definition = pipeline.GenerateDefinitionFromSignature
                    (
                        targetMethod,
                        baseClassMethod,
                        $"{interfaceType.Name}.{targetMethod.Name}"
                    );
                }
                else
                {
                    definition = pipeline.GenerateDefinitionFromSignature(targetMethod, baseClassMethod);
                }

                methods.Add
                (
                    new PipelineWorkUnit<IntrospectiveMethodInfo>
                    (
                        definition,
                        symbolTransformer.GetTransformedSymbol(interfaceType, definition),
                        Options
                    )
                );

                constructedMethods.Add(definition);
            }
        }

        pipeline.ConsumeMethodDefinitions(methods);
    }

    /// <summary>
    /// Constructs the implementations for all properties.
    /// </summary>
    /// <param name="pipeline">The implementation pipeline that consumes the methods.</param>
    /// <param name="classType">The base class of the type to generator properties for.</param>
    /// <param name="interfaceTypes">The interfaces where the properties originate.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if any property is declared as partially abstract.
    /// </exception>
    private void ConstructProperties
    (
        ImplementationPipeline pipeline,
        Type classType,
        params Type[] interfaceTypes
    )
    {
        var constructedProperties = new List<IntrospectivePropertyInfo>();

        var symbolTransformer = SymbolTransformer.Default;
        var properties = new List<PipelineWorkUnit<IntrospectivePropertyInfo>>();
        foreach (var interfaceType in interfaceTypes)
        {
            foreach (var property in interfaceType.GetProperties().Select(p => new IntrospectivePropertyInfo(p, interfaceType)))
            {
                var targetProperty = property;

                // Skip methods that were already constructed - happens with inherited interfaces and multiple
                // identical definitions
                if (constructedProperties.Any(p => p.HasSameSignatureAs(property)))
                {
                    continue;
                }

                // Skip properties with a managed implementation
                var baseClassProperty = classType.GetProperty(property.Name, property.PropertyType);
                if (!(baseClassProperty is null))
                {
                    var isFullyManaged = !baseClassProperty.GetGetMethod()!.IsAbstract &&
                                         !baseClassProperty.GetSetMethod()!.IsAbstract;

                    if (isFullyManaged)
                    {
                        continue;
                    }

                    var isPartiallyAbstract = baseClassProperty.GetGetMethod()!.IsAbstract ^
                                              baseClassProperty.GetSetMethod()!.IsAbstract;

                    if (isPartiallyAbstract)
                    {
                        throw new InvalidOperationException
                        (
                            "Properties with overriding managed implementations may not be partially managed."
                        );
                    }

                    targetProperty = new IntrospectivePropertyInfo(baseClassProperty, interfaceType);
                }

                properties.Add
                (
                    new PipelineWorkUnit<IntrospectivePropertyInfo>
                    (
                        targetProperty,
                        symbolTransformer.GetTransformedSymbol(interfaceType, targetProperty),
                        Options
                    )
                );

                constructedProperties.Add(targetProperty);
            }
        }

        pipeline.ConsumePropertyDefinitions(properties);
    }
}
