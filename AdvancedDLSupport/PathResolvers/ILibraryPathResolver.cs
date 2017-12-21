using System;
using System.IO;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Resolves library paths.
    /// </summary>
    public interface ILibraryPathResolver
    {
        /// <summary>
        /// Resolves the absolute path to the given library.
        /// </summary>
        /// <param name="library">The name or path of the library to load.</param>
        /// <returns>The absolute path to the library.</returns>
        /// <exception cref="PlatformNotSupportedException">
        /// Thrown if the current platform doesn't have a path
        /// resolver defined.</exception>
        /// <exception cref="FileNotFoundException">Thrown if no library file can be found.</exception>
        string Resolve(string library);
    }
}
