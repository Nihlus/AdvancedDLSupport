using System;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Internal wrapper class for loaded libraries.
    /// </summary>
    public class DLSupport
    {
        private static readonly IPlatformLoader PlatformLoader;

        static DLSupport()
        {
            PlatformLoader = PlatformLoaderBase.SelectPlatformLoader();
        }

        private readonly IntPtr _libraryHandle;

        /// <summary>
        /// Initializes a new instance of the <see cref="DLSupport"/> class.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        public DLSupport(string path)
        {
            _libraryHandle = PlatformLoader.LoadLibrary(path);
        }

        /// <summary>
        /// Forwards the symbol loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <returns>A handle to the symbol</returns>
        public IntPtr LoadSymbol(string sym) => PlatformLoader.LoadSymbol(_libraryHandle, sym);

        /// <summary>
        /// Forwards the function loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <typeparam name="T">The delegate to load the symbol as.</typeparam>
        /// <returns>A function delegate.</returns>
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
