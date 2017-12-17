using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Builder class for anonymous types that bind to native libraries.
    /// </summary>
    public static class DLSupportConstructor
    {
        private static ModuleBuilder moduleBuilder;
        private static AssemblyBuilder assemblyBuilder;

        private static ConcurrentDictionary<KeyForInterfaceTypeAndLibName, object> TypeCache;

        static DLSupportConstructor()
        {
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DLSupportAssembly"), AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("DLSupportModule");
            TypeCache = new ConcurrentDictionary<KeyForInterfaceTypeAndLibName, object>();
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

            var key = new KeyForInterfaceTypeAndLibName
            {
                FullInterfaceTypeName = interfaceType.FullName,
                LibraryPath = libraryPath
            };
            object cachedType;
            if (TypeCache.TryGetValue(key, out cachedType))
            {
                return (T)cachedType;
            }

            lock (moduleBuilder)
            {
                if (interfaceType.GetMethods().Any(m => IsMethodParametersUnacceptable(m)))
                {
                    throw new Exception("One or more method contains a parameter/return type that is a class, P/Invoke cannot marshal class for C Library!");
                }

                // Let's determine a name for our class!
                var typeName = interfaceType.Name.StartsWith("I") ? interfaceType.Name.Substring(1) : $"Generated_{interfaceType.Name}";

                if (string.IsNullOrWhiteSpace(typeName))
                {
                    typeName = $"Generated_{interfaceType.Name}";
                }

                // Let's create a new type!
                var typeBuilder = moduleBuilder.DefineType
                (
                    MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                    CallingConventions.Standard,
                    new Type[] { typeof(object), typeof(System.IntPtr) }
                );

                // Now the constructor
                var constructorBuilder = typeBuilder.DefineConstructor
                (
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig,
                    CallingConventions.Standard,
                    new[] { typeof(string) }
                );

                constructorBuilder.DefineParameter(1, ParameterAttributes.In, "libraryPath");
                var il = constructorBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0); // Load instance
                il.Emit(OpCodes.Ldarg_1); // Load libraryPath parameter
                il.Emit(OpCodes.Call, typeof(DLSupport).GetConstructors().First
                (
                    p =>
                    p.GetParameters().Length == 1 &&
                    p.GetParameters()[0].ParameterType == typeof(string))
                );

                // Let's define our methods!
                foreach (var method in interfaceType.GetMethods())
                {
                    var parameters = method.GetParameters();

                    // Declare a delegate type!
                    var delegateBuilder = moduleBuilder.DefineType
                    (
                        $"{method.Name}_dt",
                        TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                        typeof(MulticastDelegate)
                    );

                    var delegateCtorBuilder = delegateBuilder.DefineConstructor
                    (
                        MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, CallingConventions.Standard,
                        new Type[] { typeof(object), typeof(System.IntPtr) }
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

                    // Create a delegate field!
                    var delegateField = typeBuilder.DefineField($"{method.Name}_dtm", delegateBuilderType, FieldAttributes.Public);
                    var methodBuilder = typeBuilder.DefineMethod
                    (
                        method.Name,
                        MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                        System.Reflection.CallingConventions.Standard,
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

                    // Assign Delegate from Function Pointer
                    il.Emit(OpCodes.Ldarg_0); // This is for storing field delegate, it needs the "this" reference
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, method.Name);
                    il.EmitCall(OpCodes.Call, typeof(DLSupport).GetMethod("LoadFunction").MakeGenericMethod(delegateBuilderType), null);
                    il.Emit(OpCodes.Stfld, delegateField);
                }
                foreach (var property in interfaceType.GetProperties())
                {
                    if (property.CanRead)
                    {
                    }

                    if (property.CanWrite)
                    {
                    }
                }
                il.Emit(OpCodes.Ret);
                return (T)Activator.CreateInstance(typeBuilder.CreateTypeInfo(), libraryPath);
            }
        }
    }
}
