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
        public ImplementationConfiguration Configuration { get; }

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private static readonly AssemblyBuilder AssemblyBuilder;
        private static readonly ModuleBuilder ModuleBuilder;

        private static readonly object BuilderLock = new object();

        private static readonly ConcurrentDictionary<LibraryIdentifier, Type> TypeCache;

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

            TypeCache = new ConcurrentDictionary<LibraryIdentifier, Type>(new LibraryIdentifierEqualityComparer());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousImplementationBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings to use for the builder.</param>
        [PublicAPI]
        public AnonymousImplementationBuilder(ImplementationConfiguration configuration = default)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Attempts to resolve interface to C Library via C# Interface by dynamically creating C# Class during runtime
        /// and return a new instance of the said class. This approach does not resolve any C++ implication such as name manglings.
        /// </summary>
        /// <param name="libraryPath">Path to Native Library to bind interface to.</param>
        /// <typeparam name="T">P/Invoke Interface Type to bind Native Library to.</typeparam>
        /// <returns>Returns a generated type object that binds to native library with provided interface.</returns>
        [NotNull, PublicAPI]
        public T ResolveAndActivateInterface<T>([NotNull] string libraryPath) where T : class
        {
            var interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                throw new Exception("The generic argument type must be an interface! Please review the documentation on how to use this.");
            }

            if (Configuration.EnableDllMapSupport)
            {
                libraryPath = new DllMapResolver().MapLibraryName(interfaceType, libraryPath);
            }

            var resolveResult = DynamicLinkLibraryPathResolver.ResolveAbsolutePath(libraryPath, true);
            if (!resolveResult.IsSuccess)
            {
                throw new FileNotFoundException($"The specified library (\"{libraryPath}\") was not found in any of the loader search paths.", libraryPath, resolveResult.Exception);
            }

            libraryPath = resolveResult.Path;

            var key = new LibraryIdentifier(interfaceType, libraryPath, Configuration);
            if (TypeCache.TryGetValue(key, out var cachedType))
            {
                if (!(cachedType is null))
                {
                    return (T)Activator.CreateInstance(cachedType, libraryPath, interfaceType, Configuration);
                }
            }

            lock (BuilderLock)
            {
                // Let's determine a name for our class!
                var typeName = interfaceType.Name.StartsWith("I") ? interfaceType.Name.Substring(1) : $"Generated_{interfaceType.Name}";

                if (typeName.IsNullOrWhiteSpace())
                {
                    typeName = $"Generated_{interfaceType.Name}";
                }

                var uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", "_");
                typeName = $"{typeName}_{uniqueIdentifier}";

                // Create a new type for the anonymous implementation
                var typeBuilder = ModuleBuilder.DefineType
                (
                    typeName,
                    TypeAttributes.AutoClass | TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                    typeof(AnonymousImplementationBase),
                    new[] { interfaceType }
                );

                // Now the constructor
                var constructorBuilder = typeBuilder.DefineConstructor
                (
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
                    CallingConventions.Standard,
                    new[] { typeof(string), typeof(Type), typeof(ImplementationConfiguration) }
                );

                constructorBuilder.DefineParameter(1, ParameterAttributes.In, "libraryPath");
                var ctorIL = constructorBuilder.GetILGenerator();
                ctorIL.Emit(OpCodes.Ldarg_0); // Load instance
                ctorIL.Emit(OpCodes.Ldarg_1); // Load libraryPath parameter
                ctorIL.Emit(OpCodes.Ldarg_2); // Load interface type
                ctorIL.Emit(OpCodes.Ldarg_3); // Load config parameter
                ctorIL.Emit
                (
                    OpCodes.Call,
                    typeof(AnonymousImplementationBase).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First
                    (
                        p =>
                            p.GetParameters().Length == 3 &&
                            p.GetParameters()[0].ParameterType == typeof(string) &&
                            p.GetParameters()[1].ParameterType == typeof(Type) &&
                            p.GetParameters()[2].ParameterType == typeof(ImplementationConfiguration)
                    )
                );

                ConstructMethods(typeBuilder, ctorIL, interfaceType);
                ConstructProperties(typeBuilder, ctorIL, interfaceType);

                ctorIL.Emit(OpCodes.Ret);

                try
                {
                    var finalType = typeBuilder.CreateTypeInfo();

                    var instance = (T)Activator.CreateInstance(finalType, libraryPath, interfaceType, Configuration);
                    TypeCache.TryAdd(key, finalType);

                    return instance;
                }
                catch (TargetInvocationException tex)
                {
                    // Unwrap target invocation exceptions, since we can fail in the constructor
                    throw tex.InnerException ?? tex;
                }
            }
        }

        /// <summary>
        /// Constructs the implementations for all normal methods.
        /// </summary>
        /// <param name="typeBuilder">Reference to TypeBuilder to define the methods in.</param>
        /// <param name="ctorIL">Constructor IL emitter to initialize methods by assigning symbol pointer to delegate.</param>
        /// <param name="interfaceType">Type definition of a provided interface.</param>
        private void ConstructMethods([NotNull] TypeBuilder typeBuilder, [NotNull] ILGenerator ctorIL, [NotNull] Type interfaceType)
        {
            var methodGenerator = new MethodImplementationGenerator(ModuleBuilder, typeBuilder, ctorIL, Configuration);

            // Let's define our methods!
            foreach (var method in interfaceType.GetMethods())
            {
                // Skip any property accessor methods
                if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                {
                    continue;
                }

                methodGenerator.GenerateImplementation(method);
            }
        }

        /// <summary>
        /// Constructs the implementations for all properties.
        /// </summary>
        /// <param name="typeBuilder">Reference to TypeBuilder to define the methods in.</param>
        /// <param name="ctorIL">Constructor IL emitter to initialize methods by assigning symbol pointer to delegate.</param>
        /// <param name="interfaceType">Type definition of a provided interface.</param>
        private void ConstructProperties([NotNull] TypeBuilder typeBuilder, [NotNull] ILGenerator ctorIL, [NotNull] Type interfaceType)
        {
            var propertyGenerator = new PropertyImplementationGenerator(ModuleBuilder, typeBuilder, ctorIL, Configuration);

            foreach (var property in interfaceType.GetProperties())
            {
                propertyGenerator.GenerateImplementation(property);
            }
        }
    }
}
