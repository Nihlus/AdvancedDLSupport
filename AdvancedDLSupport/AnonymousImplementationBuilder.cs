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

            TypeCache = new ConcurrentDictionary<LibraryIdentifier, Type>(new LibraryIdentifierEqualityComparer());
            TransformerRepository = new TypeTransformerRepository()
                .WithTypeTransformer(typeof(string), new StringTransformer());
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
                var executingDir = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
                var filesInDir = string.Join(", ", Directory.EnumerateFiles(executingDir));
                throw new FileNotFoundException($"The specified library (\"{libraryPath}\") was not found in any of the loader search paths. Executing dir was {executingDir}, files were {filesInDir}", libraryPath, resolveResult.Exception);
            }

            libraryPath = resolveResult.Path;

            var key = new LibraryIdentifier(interfaceType, libraryPath, Configuration);
            if (TypeCache.TryGetValue(key, out var cachedType))
            {
                if (!(cachedType is null))
                {
                    return CreateInterfaceInstance<T>(cachedType, libraryPath, Configuration, TransformerRepository);
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
                var anonymousConstructor = typeof(AnonymousImplementationBase)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .First
                (
                    c => c.GetCustomAttribute<AnonymousConstructorAttribute>() != null
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

                ConstructMethods(typeBuilder, ctorIL, interfaceType);
                ConstructProperties(typeBuilder, ctorIL, interfaceType);

                ctorIL.Emit(OpCodes.Ret);

                try
                {
                    var finalType = typeBuilder.CreateTypeInfo();

                    var instance = CreateInterfaceInstance<T>(finalType, libraryPath, Configuration, TransformerRepository);
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
        /// Creates an instance of the final interface type.
        /// </summary>
        /// <param name="anonymousType">The constructed anonymous type.</param>
        /// <param name="library">The path to or name of the library</param>
        /// <param name="configuration">The generator configuration.</param>
        /// <param name="transformerRepository">The type transformer repository.</param>
        /// <typeparam name="T">The interface type.</typeparam>
        /// <returns>An instance of the anonymous type implementing <typeparamref name="T"/>.</returns>
        private T CreateInterfaceInstance<T>
        (
            [NotNull] Type anonymousType,
            [NotNull] string library,
            ImplementationConfiguration configuration,
            [NotNull] TypeTransformerRepository transformerRepository
        )
        {
            return (T)Activator.CreateInstance(anonymousType, library, typeof(T), Configuration, TransformerRepository);
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
            var complexMethodGenerator = new ComplexMethodImplementationGenerator(ModuleBuilder, typeBuilder, ctorIL, Configuration, TransformerRepository);

            // Let's define our methods!
            foreach (var method in interfaceType.GetMethods())
            {
                // Skip any property accessor methods
                if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                {
                    continue;
                }

                if (method.IsComplexMethod())
                {
                    complexMethodGenerator.GenerateImplementation(method);
                }
                else
                {
                    methodGenerator.GenerateImplementation(method);
                }
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
