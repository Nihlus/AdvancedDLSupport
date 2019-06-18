using System;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Loaders
{
    /// <summary>
    /// Represents a class which can load Libraries on a specific platform.
    /// </summary>
    public interface ILibraryLoader
    {
        /// <summary>
        /// Load the given library. A null path signifies intent to load the main executable instead of an external
        /// library.
        /// </summary>
        /// <param name="path">The path to the library.</param>
        /// <returns>A handle to the library. This value carries no intrinsic meaning.</returns>
        /// <exception cref="LibraryLoadingException">Thrown if the library could not be loaded.</exception>
        [PublicAPI]
        IntPtr LoadLibrary([CanBeNull] string path);

        /// <summary>
        /// Closes the open handle to the given library.
        /// </summary>
        /// <param name="library">The handle to the library to close.</param>
        /// <returns>true if the library was closed successfully; otherwise, false.</returns>
        [PublicAPI]
        bool CloseLibrary(IntPtr library);
    }
}
