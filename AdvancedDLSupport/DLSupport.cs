using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

#pragma warning disable SA1600, CS1591 // Elements should be documented
#pragma warning disable SA1300 // Element should begin with an uppercase letter

namespace AdvancedDLSupport
{
    public class DLSupport
    {
        private static readonly IPlatformLoader PlatformLoader;

        private static readonly ModuleBuilder ModuleBuilder;
        private static readonly AssemblyBuilder AssemblyBuilder;

        static DLSupport()
        {
            PlatformLoader = PlatformLoaderBase.SelectPlatformLoader();

            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("DLSupportAssembly"), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("DLSupportModule");
        }

        private IntPtr _libraryHandle { get; set; }

        public DLSupport(string path)
        {
            _libraryHandle = PlatformLoader.LoadLibrary(path);
        }

        public T LoadFunction<T>(string symbolName) => PlatformLoader.LoadFunction<T>(_libraryHandle, symbolName);

        /// <summary>
        /// Unsafe Dispose will free the loaded library handle, if any of the functions or variables are still in use
        /// after this library handle is freed, segmentation fault will occur and can potentially crash the runtime.
        /// Do note however that library Handle can be shared between the runtime and this class, so if that library
        /// handle is freed then both runtime and this class will be affected. In normal use cases, this shouldn't be
        /// used at all.
        /// </summary>
        public void UnsafeDispose()
        {
            PlatformLoader.CloseLibrary(_libraryHandle);
        }

        private static bool IsMethodAcceptable(MethodInfo info)
        {
            return !info.ReturnParameter.ParameterType.IsClass &&
                   !info.GetParameters().Any(p => p.ParameterType.IsClass);
        }

        /// <summary>
        /// Attempts to resolve interface to C Library via C# Interface by dynamically creating C# Class during runtime
        /// and return a new instance of the said class. This approach does not resolve any C++ implication such as name
        /// manglings.
        /// </summary>
        /// <param name="libraryPath">The path to the library.</param>
        /// <typeparam name="T">The interface type to map to.</typeparam>
        /// <returns>A generated object that implements <typeparamref name="T"/>.</returns>
        public static T ResolveAndActivateInterface<T>(string libraryPath)
        {
            var type = typeof(T);
            if (!type.IsInterface)
            {
                throw new ArgumentException
                (
                    "The generic argument type must be an interface! Please review the documentation on how to use this.", nameof(T)
                );
            }

            // Let's determine a name for our class!
            string typeName;
            if (type.Name.StartsWith("I"))
            {
                typeName = type.Name.Substring(1);
            }
            else
            {
                typeName = $"Generated_{type.Name}";
            }

            if (string.IsNullOrWhiteSpace(typeName))
            {
                typeName = $"Generated_{type.Name}";
            }

            // Let's create a new type!
            var typeBuilder = ModuleBuilder.DefineType
            (
                typeName,
                TypeAttributes.AutoClass | TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                typeof(DLSupport),
                new[] { type }
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
                    c =>
                        c.GetParameters().Length == 1 &&
                        c.GetParameters()[0].ParameterType == typeof(string))
            );

            // Let's define our methods!
            foreach (var method in type.GetMethods())
            {
                var parameters = method.GetParameters();

                // Declare a delegate type!
                var delegateBuilder = ModuleBuilder.DefineType
                (
                    $"{method.Name}_dt",
                    TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                    typeof(MulticastDelegate)
                );

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

                // Create a delegate property!
                var delegateField = typeBuilder.DefineField($"{method.Name}_dtm", delegateBuilderType, FieldAttributes.Public);
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
                for (int i = 1; i <= parameters.Length; i++)
                {
                    methodIL.Emit(OpCodes.Ldarg, i);
                }

                methodIL.EmitCall(OpCodes.Call, delegateBuilderType.GetMethod("Invoke"), null);
                methodIL.Emit(OpCodes.Ret);

                // Assign Delegate from Function Pointer
                il.Emit(OpCodes.Ldarg_0); // This is for storing field delegate, it needs the "this" reference
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldstr, method.Name);
                il.EmitCall(OpCodes.Call, typeof(DLSupport).GetMethod(nameof(LoadFunction)).MakeGenericMethod(delegateBuilderType), null);
                il.Emit(OpCodes.Stfld, delegateField);
            }

            foreach (var property in type.GetProperties())
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
