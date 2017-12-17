using System;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Linq;

#pragma warning disable SA1600, CS1591 // Elements should be documented
#pragma warning disable SA1300 // Element should begin with an uppercase letter

namespace AdvancedDLSupport
{

    public class DLSupport
    {
        private static readonly IPlatformLoader PlatformLoader;

        static DLSupport()
        {
            PlatformLoader = PlatformLoaderBase.SelectPlatformLoader();
        }
        private IntPtr _libraryHandle {get;set;}
        public DLSupport(string path)
        {
            _libraryHandle = PlatformLoader.LoadLibrary(path);

            
        }
        public IntPtr LoadSymbol(string sym) => PlatformLoader.LoadSymbol(_libraryHandle, sym);

        public T LoadFunction<T>(string sym) => PlatformLoader.LoadFunction<T>(_libraryHandle, sym);

        /// <summary>
        /// Unsafe Dispose will free the loaded library handle, if any of the functions or variables are still in use after this library handle is freed,
        /// segmentation fault will occur and can potentially crash the Runtime. Do note however that Library Handle can be shared between the Runtime and this class,
        /// so if that library handle is freed then both Runtime and this class will be affected.
        /// In normal use case, this shouldn't be used at all.
        /// </summary>
        public void UnsafeDispose() => PlatformLoader.CloseLibrary(_libraryHandle);

    }

}