//
//  AnonymousImplementationBuilder.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.Extensions;
using AdvancedDLSupport.ImplementationGenerators;
using JetBrains.Annotations;
using Mono.DllMap;
using Mono.DllMap.Extensions;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Builder class for anonymous types that bind to native libraries.
    /// </summary>
    [PublicAPI]
    public class AnonymousImplementationBuilder
    {
        /// <summary>
        /// Gets the configuration object for this builder.
        /// </summary>
        [PublicAPI]
        public ImplementationOptions Options { get; }

        private ILibraryPathResolver PathResolver { get; }

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private static readonly AssemblyBuilder AssemblyBuilder;
        private static readonly ModuleBuilder ModuleBuilder;

        private static readonly object BuilderLock = new object();

        private static readonly ConcurrentDictionary<GeneratedImplementationTypeIdentifier, Type> TypeCache;
        private static readonly TypeTransformerRepository TransformerRepository;

        static AnonymousImplementationBuilder()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DLSupportAssembly"), AssemblyBuilderAccess.Run);

            #if DEBUG
            var dbgType = typeof(DebuggableAttribute);
            var dbgConstructor = dbgType.GetConstructor(new[] { typeof(DebuggableAttribute.DebuggingModes) });
            var dbgBuilder = new CustomAttributeBuilder
            (
                dbgConstructor,
                new object[] { DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.Default }
            );

            AssemblyBuilder.SetCustomAttribute(dbgBuilder);
            #endif

            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("DLSupportModule");

            TypeCache = new ConcurrentDictionary<GeneratedImplementationTypeIdentifier, Type>(new LibraryIdentifierEqualityComparer());
            TransformerRepository = new TypeTransformerRepository()
                .WithTypeTransformer(typeof(string), new StringTransformer())
                .WithTypeTransformer(typeof(bool), new BooleanTransformer());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousImplementationBuilder"/> class.
        /// </summary>
        /// <param name="options">The configuration settings to use for the builder.</param>
        /// <param name="pathResolver">The path resolver to use.</param>
        [PublicAPI]
        public AnonymousImplementationBuilder(ImplementationOptions options = default, ILibraryPathResolver pathResolver = default )
        {
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
        /// <exception cref="FileNotFoundException">Thrown if the specified library can't be found in any of the loader paths.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the resulting instance can't be cast to the expected class. Should never occur in user code.</exception>
        [NotNull, PublicAPI]
        public TInterface ResolveAndActivateInterface<TInterface>([NotNull] string libraryPath) where TInterface : class
        {
            var anonymousInstance = ResolveAndActivateClass<AnonymousImplementationBase, TInterface>(libraryPath);
            return anonymousInstance as TInterface ?? throw new InvalidOperationException("The resulting instance was not convertible to an instance of the interface.");
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
        /// <exception cref="FileNotFoundException">Thrown if the specified library can't be found in any of the loader paths.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the resulting instance can't be cast to the expected class. Should never occur in user code.</exception>
        [NotNull, PublicAPI]
        public TClass ResolveAndActivateClass<TClass, TInterface>([NotNull] string libraryPath)
            where TClass : AnonymousImplementationBase
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
                throw new ArgumentException("The interface to activate on the class must be an interface type.", nameof(TInterface));
            }

            if (Options.HasFlagFast(ImplementationOptions.EnableDllMapSupport))
            {
                libraryPath = new DllMapResolver().MapLibraryName(interfaceType, libraryPath);
            }

            var resolveResult = PathResolver.Resolve(libraryPath);
            if (!resolveResult.IsSuccess)
            {
                throw new FileNotFoundException($"The specified library (\"{libraryPath}\") was not found in any of the loader search paths.", libraryPath, resolveResult.Exception);
            }

            libraryPath = resolveResult.Path;

            var key = new GeneratedImplementationTypeIdentifier(classType, interfaceType, libraryPath, Options);
            if (TypeCache.TryGetValue(key, out var cachedType))
            {
                if (!(cachedType is null))
                {
                    var anonymousInstance = CreateAnonymousImplementationInstance<TInterface>(cachedType, libraryPath, Options, TransformerRepository);

                    return anonymousInstance as TClass ?? throw new InvalidOperationException("The resulting instance was not convertible to an instance of the class.");
                }
            }

            lock (BuilderLock)
            {
                try
                {
                    var finalType = GenerateInterfaceImplementationType<TClass, TInterface>();

                    var anonymousInstance = CreateAnonymousImplementationInstance<TInterface>(finalType, libraryPath, Options, TransformerRepository);

                    TypeCache.TryAdd(key, finalType);

                    return anonymousInstance as TClass ?? throw new InvalidOperationException("The resulting instance was not convertible to an instance of the class.");
                }
                catch (TargetInvocationException tex)
                {
                    // Unwrap target invocation exceptions, since we can fail in the constructor
                    throw tex.InnerException ?? tex;
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
            where TBaseClass : AnonymousImplementationBase
            where TInterface : class
        {
            var baseClassType = typeof(TBaseClass);
            var interfaceType = typeof(TInterface);

            var typeName = GenerateTypeName(interfaceType);

            // Create a new type for the anonymous implementation
            var typeBuilder = ModuleBuilder.DefineType
            (
                typeName,
                TypeAttributes.AutoClass | TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                baseClassType,
                new[] { interfaceType }
            );

            // Now the constructor
            var anonymousConstructor = typeof(AnonymousImplementationBase)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .First
            (
                c => c.HasCustomAttribute<AnonymousConstructorAttribute>()
            );

            var constructorBuilder = typeBuilder.DefineConstructor
            (
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
                CallingConventions.Standard,
                anonymousConstructor.GetParameters().Select(p => p.ParameterType).ToArray()
            );

            constructorBuilder.DefineParameter(1, ParameterAttributes.In, "libraryPath");
            var ctorIL = constructorBuilder.GetILGenerator();
            for (int i = 0; i <= anonymousConstructor.GetParameters().Length; ++i)
            {
                ctorIL.Emit(OpCodes.Ldarg, i);
            }

            ctorIL.Emit(OpCodes.Call, anonymousConstructor);

            ConstructMethods(typeBuilder, baseClassType, ctorIL, interfaceType);
            ConstructProperties(typeBuilder, baseClassType, ctorIL, interfaceType);

            ctorIL.Emit(OpCodes.Ret);
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
        /// <param name="anonymousType">The constructed anonymous type.</param>
        /// <param name="library">The path to or name of the library</param>
        /// <param name="options">The generator options.</param>
        /// <param name="transformerRepository">The type transformer repository.</param>
        /// <typeparam name="T">The interface type.</typeparam>
        /// <returns>An instance of the anonymous type implementing <typeparamref name="T"/>.</returns>
        [NotNull, Pure]
        private T CreateAnonymousImplementationInstance<T>
        (
            [NotNull] Type anonymousType,
            [NotNull] string library,
            ImplementationOptions options,
            [NotNull] TypeTransformerRepository transformerRepository
        )
        {
            return (T)Activator.CreateInstance(anonymousType, library, typeof(T), options, transformerRepository);
        }

        /// <summary>
        /// Constructs the implementations for all normal methods.
        /// </summary>
        /// <param name="typeBuilder">Reference to TypeBuilder to define the methods in.</param>
        /// <param name="baseClassType">The type of the base class.</param>
        /// <param name="constructorIL">Constructor IL emitter to initialize methods by assigning symbol pointer to delegate.</param>
        /// <param name="interfaceType">Type definition of a provided interface.</param>
        private void ConstructMethods
        (
            [NotNull] TypeBuilder typeBuilder,
            Type baseClassType,
            [NotNull] ILGenerator constructorIL,
            [NotNull] Type interfaceType
        )
        {
            var methodGenerator = new MethodImplementationGenerator(ModuleBuilder, typeBuilder, constructorIL, Options);
            var complexMethodGenerator = new ComplexMethodImplementationGenerator(ModuleBuilder, typeBuilder, constructorIL, Options, TransformerRepository);

            // Let's define our methods!
            foreach (var method in interfaceType.GetMethods())
            {
                var targetMethod = method;

                // Skip any property accessor methods
                if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                {
                    continue;
                }

                // Skip methods with a managed implementation in the base class
                var baseClassMethod = baseClassType.GetMethod(method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray());
                if (!(baseClassMethod is null))
                {
                    if (!baseClassMethod.IsAbstract)
                    {
                        continue;
                    }

                    targetMethod = baseClassMethod;
                }

                if (method.IsComplexMethod())
                {
                    complexMethodGenerator.GenerateImplementation(targetMethod);
                }
                else
                {
                    methodGenerator.GenerateImplementation(targetMethod);
                }
            }
        }

        /// <summary>
        /// Constructs the implementations for all properties.
        /// </summary>
        /// <param name="typeBuilder">Reference to TypeBuilder to define the methods in.</param>
        /// <param name="baseClassType">The type of the base class.</param>
        /// <param name="constructorIL">Constructor IL emitter to initialize methods by assigning symbol pointer to delegate.</param>
        /// <param name="interfaceType">Type definition of a provided interface.</param>
        /// <exception cref="InvalidOperationException">Thrown if any property is declared as partially abstract.</exception>
        private void ConstructProperties
        (
            [NotNull] TypeBuilder typeBuilder,
            Type baseClassType,
            [NotNull] ILGenerator constructorIL,
            [NotNull] Type interfaceType
        )
        {
            var propertyGenerator = new PropertyImplementationGenerator(ModuleBuilder, typeBuilder, constructorIL, Options);

            foreach (var property in interfaceType.GetProperties())
            {
                var targetProperty = property;

                // Skip properties with a managed implementation
                var baseClassProperty = baseClassType.GetProperty(property.Name, property.PropertyType);
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
                        throw new InvalidOperationException("Properties with overriding managed implementations may not be partially managed.");
                    }

                    targetProperty = baseClassProperty;
                }

                propertyGenerator.GenerateImplementation(targetProperty);
            }
        }
    }
}
