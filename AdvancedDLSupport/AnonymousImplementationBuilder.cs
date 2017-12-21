using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.ImplementationGenerators;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Builder class for anonymous types that bind to native libraries.
    /// </summary>
    public static class AnonymousImplementationBuilder
    {
        private static readonly ModuleBuilder ModuleBuilder;
        private static readonly AssemblyBuilder AssemblyBuilder;

        private static readonly object BuilderLock = new object();

        private static readonly ConcurrentDictionary<LibraryIdentifier, object> TypeCache;

        static AnonymousImplementationBuilder()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DLSupportAssembly"), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("DLSupportModule");
            TypeCache = new ConcurrentDictionary<LibraryIdentifier, object>();
        }

        /// <summary>
        /// Attempts to resolve interface to C Library via C# Interface by dynamically creating C# Class during runtime
        /// and return a new instance of the said class. This approach does not resolve any C++ implication such as name manglings.
        /// </summary>
        /// <param name="libraryPath">Path to Native Library to bind interface to.</param>
        /// <typeparam name="T">P/Invoke Interface Type to bind Native Library to.</typeparam>
        /// <returns>Returns a generated type object that binds to native library with provided interface.</returns>
        public static T ResolveAndActivateInterface<T>(string libraryPath) where T : class
        {
            var interfaceType = typeof(T);
            if (!interfaceType.IsInterface)
            {
                throw new Exception("The generic argument type must be an interface! Please review the documentation on how to use this.");
            }

            var key = new LibraryIdentifier(interfaceType, libraryPath);
            if (TypeCache.TryGetValue(key, out var cachedType))
            {
                return (T)cachedType;
            }

            lock (BuilderLock)
            {
                // Let's determine a name for our class!
                var typeName = interfaceType.Name.StartsWith("I") ? interfaceType.Name.Substring(1) : $"Generated_{interfaceType.Name}";

                if (string.IsNullOrWhiteSpace(typeName))
                {
                    typeName = $"Generated_{interfaceType.Name}";
                }

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
                    new[] { typeof(string) }
                );

                constructorBuilder.DefineParameter(1, ParameterAttributes.In, "libraryPath");
                var ctorIL = constructorBuilder.GetILGenerator();
                ctorIL.Emit(OpCodes.Ldarg_0); // Load instance
                ctorIL.Emit(OpCodes.Ldarg_1); // Load libraryPath parameter
                ctorIL.Emit(OpCodes.Call, typeof(AnonymousImplementationBase).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First
                (
                    p =>
                    p.GetParameters().Length == 1 &&
                    p.GetParameters()[0].ParameterType == typeof(string))
                );

                ConstructMethods(ref typeBuilder, ref ctorIL, interfaceType);
                ConstructProperties(ref typeBuilder, ref ctorIL, interfaceType);

                ctorIL.Emit(OpCodes.Ret);
                return (T)Activator.CreateInstance(typeBuilder.CreateTypeInfo(), libraryPath);
            }
        }

        /// <summary>
        /// Define all non-property accessor methods. The accessor methods are handled by both ConstructNonArrayProperties and
        /// ConstructArrayProperties methods.
        /// </summary>
        /// <param name="typeBuilder">Reference to TypeBuilder to define the methods in.</param>
        /// <param name="ctorIL">Constructor IL emitter to initialize methods by assigning symbol pointer to delegate.</param>
        /// <param name="interfaceType">Type definition of a provided interface.</param>
        private static void ConstructMethods(ref TypeBuilder typeBuilder, ref ILGenerator ctorIL, Type interfaceType)
        {
            var methodGenerator = new MethodImplementationGenerator(ModuleBuilder, typeBuilder, ctorIL);

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
        /// Define all non-array properties, array property is a special case that have to be written differently.
        /// </summary>
        /// <param name="typeBuilder">Reference to TypeBuilder to define the methods in.</param>
        /// <param name="ctorIL">Constructor IL emitter to initialize methods by assigning symbol pointer to delegate.</param>
        /// <param name="interfaceType">Type definition of a provided interface.</param>
        private static void ConstructProperties(ref TypeBuilder typeBuilder, ref ILGenerator ctorIL, Type interfaceType)
        {
            var propertyGenerator = new PropertyImplementationGenerator(ModuleBuilder, typeBuilder, ctorIL);

            foreach (var property in interfaceType.GetProperties())
            {
                propertyGenerator.GenerateImplementation(property);
            }
        }
    }
}
