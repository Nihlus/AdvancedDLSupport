using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Attributes;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Builder class for anonymous types that bind to native libraries.
    /// </summary>
    public static class AnonymousImplementationBuilder
    {
        private static readonly ModuleBuilder ModuleBuilder;
        private static readonly AssemblyBuilder AssemblyBuilder;

        private static readonly ConcurrentDictionary<LibraryIdentifier, object> TypeCache;

        static AnonymousImplementationBuilder()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DLSupportAssembly"), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("DLSupportModule");
            TypeCache = new ConcurrentDictionary<LibraryIdentifier, object>();
        }

        private static bool IsMethodParametersUnacceptable(MethodInfo info)
        {
            if (info.ReturnParameter.ParameterType.IsClass)
            {
                if (info.ReturnParameter.ParameterType != typeof(Delegate))
                {
                    return true;
                }
            }

            return !info.GetParameters().Any(p => p.ParameterType.IsValueType || p.ParameterType.IsByRef || p.ParameterType == typeof(Delegate));
        }

        /// <summary>
        /// Define all non-array properties, array property is a special case that have to be written differently.
        /// </summary>
        /// <param name="typeBuilder">Reference to TypeBuilder to define the methods in.</param>
        /// <param name="ctorIL">Constructor IL emitter to initialize methods by assigning symbol pointer to delegate.</param>
        /// <param name="interfaceType">Type definition of a provided interface.</param>
        private static void ConstructProperties(ref TypeBuilder typeBuilder, ref ILGenerator ctorIL, Type interfaceType)
        {
            foreach (var property in interfaceType.GetProperties())
            {
                if (property.PropertyType.IsArray)
                {
                    GenerateArrayPropertyImplementation(typeBuilder, ctorIL, property);
                }
                else
                {
                    GeneratePropertyImplementation(typeBuilder, ctorIL, property);
                }
            }
        }

        private static void GenerateArrayPropertyImplementation(TypeBuilder typeBuilder, ILGenerator ctorIL, PropertyInfo property)
        {
            throw new NotImplementedException();
        }

        private static void GeneratePropertyImplementation(TypeBuilder typeBuilder, ILGenerator constructorIL, PropertyInfo property)
        {
            var uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", "_");

            // Note, the field is going to have to be a pointer, because it is pointing to global variable
            var propertyFieldBuilder = typeBuilder.DefineField
            (
                $"{property.Name}_{uniqueIdentifier}",
                typeof(IntPtr),
                FieldAttributes.Private
            );

            var propertyBuilder = typeBuilder.DefineProperty
            (
                property.Name,
                PropertyAttributes.None,
                CallingConventions.HasThis,
                property.PropertyType,
                property.GetIndexParameters().Select(p => p.ParameterType).ToArray()
            );

            if (property.CanRead)
            {
                var actualGetMethod = property.GetGetMethod();
                var getterMethod = typeBuilder.DefineMethod
                (
                    actualGetMethod.Name,
                    MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask | MethodAttributes.SpecialName,
                    actualGetMethod.CallingConvention,
                    actualGetMethod.ReturnType,
                    Type.EmptyTypes
                );

                var getterIL = getterMethod.GetILGenerator();
                getterIL.Emit(OpCodes.Ldarg_0);
                getterIL.Emit(OpCodes.Ldfld, propertyFieldBuilder);
                getterIL.EmitCall(
                    OpCodes.Call,
                    typeof(Marshal).GetMethods().First
                    (
                        m =>
                        m.GetParameters().Length == 1 &&
                        m.IsGenericMethod &&
                        m.Name == "PtrToStructure"
                    ).MakeGenericMethod(property.PropertyType),
                    null
                );
                getterIL.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getterMethod);
            }

            if (property.CanWrite)
            {
                var actualSetMethod = property.GetSetMethod();
                var setterMethod = typeBuilder.DefineMethod
                (
                    actualSetMethod.Name,
                    MethodAttributes.PrivateScope | MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.VtableLayoutMask | MethodAttributes.SpecialName,
                    actualSetMethod.CallingConvention,
                    typeof(void),
                    actualSetMethod.GetParameters().Select(p => p.ParameterType).ToArray()
                );

                var setterIL = setterMethod.GetILGenerator();
                setterIL.Emit(OpCodes.Ldarg_1);
                setterIL.Emit(OpCodes.Ldarg_0);
                setterIL.Emit(OpCodes.Ldfld, propertyFieldBuilder);
                setterIL.Emit(OpCodes.Ldc_I4, 0); // false for deleting structure that is already stored in pointer
                setterIL.EmitCall
                (
                    OpCodes.Call,
                    typeof(Marshal).GetMethods().First
                    (
                        m => m.Name == "StructureToPtr" &&
                        m.GetParameters().Length == 3 &&
                        m.IsGenericMethod
                    ).MakeGenericMethod(property.PropertyType),
                    null
                );

                setterIL.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setterMethod);
            }

            var loadSymbolMethod = typeof(AnonymousImplementationBase).GetMethod
            (
                "LoadSymbol",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldarg_0);
            constructorIL.Emit(OpCodes.Ldstr, property.Name);
            constructorIL.EmitCall(OpCodes.Call, loadSymbolMethod, null);
            constructorIL.Emit(OpCodes.Stfld, propertyFieldBuilder);
        }

        private static void GenerateMethodImplementation(TypeBuilder typeBuilder, ILGenerator constructurIL, MethodInfo method)
        {
            var uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", "_");
            var parameters = method.GetParameters();

            var metadataAttribute = method.GetCustomAttribute<NativeFunctionAttribute>() ??
                                    new NativeFunctionAttribute(method.Name);

            // Declare a delegate type!
            var delegateBuilder = ModuleBuilder.DefineType
            (
                $"{method.Name}_dt_{uniqueIdentifier}",
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                typeof(MulticastDelegate)
            );

            var attributeConstructors = typeof(UnmanagedFunctionPointerAttribute).GetConstructors();
            var functionPointerAttributeBuilder = new CustomAttributeBuilder
            (
                attributeConstructors[1],
                new object[] { metadataAttribute.CallingConvention }
            );

            delegateBuilder.SetCustomAttribute(functionPointerAttributeBuilder);

            var delegateCtorBuilder = delegateBuilder.DefineConstructor
            (
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(object), typeof(IntPtr) }
            );

            delegateCtorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            var delegateMethodBuilder = delegateBuilder.DefineMethod
            (
                "Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                method.ReturnType,
                parameters.Select(p => p.ParameterType).ToArray()
            );

            delegateMethodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            var delegateBuilderType = delegateBuilder.CreateTypeInfo();

            // Create a delegate field
            var delegateField = typeBuilder.DefineField($"{method.Name}_dtm_{uniqueIdentifier}", delegateBuilderType, FieldAttributes.Public);
            var methodBuilder = typeBuilder.DefineMethod
            (
                method.Name,
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                CallingConventions.Standard,
                method.ReturnType,
                parameters.Select(p => p.ParameterType).ToArray()
            );

            // Let's create a method that simply invoke the delegate
            var methodIL = methodBuilder.GetILGenerator();
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.Emit(OpCodes.Ldfld, delegateField);
            for (int p = 1; p <= parameters.Length; p++)
            {
                methodIL.Emit(OpCodes.Ldarg, p);
            }

            methodIL.EmitCall(OpCodes.Call, delegateBuilderType.GetMethod("Invoke"), null);
            methodIL.Emit(OpCodes.Ret);

            var entrypointName = metadataAttribute.Entrypoint ?? method.Name;

            var loadFuncMethod = typeof(AnonymousImplementationBase).GetMethod
            (
                "LoadFunction",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Assign Delegate from Function Pointer
            constructurIL.Emit(OpCodes.Ldarg_0); // This is for storing field delegate, it needs the "this" reference
            constructurIL.Emit(OpCodes.Ldarg_0);
            constructurIL.Emit(OpCodes.Ldstr, entrypointName);
            constructurIL.EmitCall(OpCodes.Call, loadFuncMethod.MakeGenericMethod(delegateBuilderType), null);
            constructurIL.Emit(OpCodes.Stfld, delegateField);
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
            // Let's define our methods!
            foreach (var method in interfaceType.GetMethods())
            {
                // Skip any property accessor methods, those will be manually implemented.
                if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
                {
                    continue;
                }

                GenerateMethodImplementation(typeBuilder, ctorIL, method);
            }
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

            lock (ModuleBuilder)
            {
                if (interfaceType.GetMethods().Any(IsMethodParametersUnacceptable))
                {
                    // throw new Exception("One or more method contains a parameter/return type that is a class, P/Invoke cannot marshal class for C Library!");
                }

                // Let's determine a name for our class!
                var typeName = interfaceType.Name.StartsWith("I") ? interfaceType.Name.Substring(1) : $"Generated_{interfaceType.Name}";

                if (string.IsNullOrWhiteSpace(typeName))
                {
                    typeName = $"Generated_{interfaceType.Name}";
                }

                // Let's create a new type!
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
    }
}
