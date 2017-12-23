using System;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Internal base class for library implementations
    /// </summary>
    public abstract class AnonymousImplementationBase : IDisposable
    {
        private static readonly IPlatformLoader PlatformLoader;

        static AnonymousImplementationBase()
        {
            PlatformLoader = PlatformLoaderBase.SelectPlatformLoader();
        }

        private readonly IntPtr _libraryHandle;

        /// <summary>
        /// Gets a value indicating whether or not the library has been disposed.
        /// </summary>
        protected bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousImplementationBase"/> class.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        protected AnonymousImplementationBase(string path)
        {
            _libraryHandle = PlatformLoader.LoadLibrary(path);
        }

        /// <summary>
        /// Forwards the symbol loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <returns>A handle to the symbol</returns>
        protected IntPtr LoadSymbol(string sym) => PlatformLoader.LoadSymbol(_libraryHandle, sym);

        /// <summary>
        /// Forwards the function loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <typeparam name="T">The delegate to load the symbol as.</typeparam>
        /// <returns>A function delegate.</returns>
        protected T LoadFunction<T>(string sym) => PlatformLoader.LoadFunction<T>(_libraryHandle, sym);

        /// <summary>
        /// Throws if the library has been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the library has been disposed.</exception>
        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name, "The library has been disposed.");
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!IsDisposed)
            {
                PlatformLoader.CloseLibrary(_libraryHandle);
                IsDisposed = true;
            }
        }
    }
}
