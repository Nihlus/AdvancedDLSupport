using System;
using AdvancedDLSupport.Loaders;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Internal base class for library implementations
    /// </summary>
    [PublicAPI]
    public abstract class AnonymousImplementationBase : IDisposable
    {
        private static readonly IPlatformLoader PlatformLoader;

        static AnonymousImplementationBase()
        {
            PlatformLoader = PlatformLoaderBase.SelectPlatformLoader();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AnonymousImplementationBase"/> class.
        /// </summary>
        ~AnonymousImplementationBase()
        {
            Dispose();
        }

        [NotNull]
        private readonly string _path;

        [NotNull]
        private readonly Type _interfaceType;
        private IntPtr _libraryHandle;

        /// <summary>
        /// Gets the type transformer repository.
        /// </summary>
        protected TypeTransformerRepository TransformerRepository { get; }

        /// <summary>
        /// Gets a value indicating whether or not the library has been disposed.
        /// </summary>
        [PublicAPI]
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the library can be disposed.
        /// </summary>
        private ImplementationConfiguration Configuration { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousImplementationBase"/> class.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <param name="interfaceType">The interface type that the anonymous type implements.</param>
        /// <param name="configuration">Whether or not this library can be disposed.</param>
        /// <param name="transformerRepository">The repository containing type transformers.</param>
        [AnonymousConstructor]
        protected AnonymousImplementationBase
        (
            [NotNull] string path,
            [NotNull] Type interfaceType,
            ImplementationConfiguration configuration,
            [NotNull] TypeTransformerRepository transformerRepository
        )
        {
            Configuration = configuration;
            TransformerRepository = transformerRepository;
            _libraryHandle = PlatformLoader.LoadLibrary(path);
            _path = path;
            _interfaceType = interfaceType;
        }

        /// <summary>
        /// Forwards the symbol loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <returns>A handle to the symbol</returns>
        protected IntPtr LoadSymbol([NotNull] string sym) => PlatformLoader.LoadSymbol(_libraryHandle, sym);

        /// <summary>
        /// Forwards the function loading call to the wrapped platform loader.
        /// </summary>
        /// <param name="sym">The symbol name.</param>
        /// <typeparam name="T">The delegate to load the symbol as.</typeparam>
        /// <returns>A function delegate.</returns>
        protected T LoadFunction<T>([NotNull] string sym) => PlatformLoader.LoadFunction<T>(_libraryHandle, sym);

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
        [PublicAPI]
        public void Dispose()
        {
            if (IsDisposed || !Configuration.GenerateDisposalChecks)
            {
                return;
            }

            IsDisposed = true;

            PlatformLoader.CloseLibrary(_libraryHandle);
            _libraryHandle = IntPtr.Zero;
        }
    }
}
