using System;
using System.IO;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// A key struct for ConcurrentDictionary TypeCache for all generated types provided by DLSupportConstructor.
    /// </summary>
    internal struct LibraryIdentifier : IEquatable<LibraryIdentifier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryIdentifier"/> struct.
        /// </summary>
        /// <param name="interfaceType">The interface type.</param>
        /// <param name="libraryPath">The path to the library. Will be resolved to an absolute path.</param>
        /// <param name="configuration">The configuration used for the library.</param>
        public LibraryIdentifier([NotNull] Type interfaceType, [NotNull] string libraryPath, ImplementationConfiguration configuration)
        {
            _interfaceType = interfaceType;
            _configuration = configuration;
            _absoluteLibraryPath = Path.GetFullPath(libraryPath);
        }

        /// <summary>
        /// The interface type for the library.
        /// </summary>
        private readonly Type _interfaceType;

        /// <summary>
        /// The absolute path to the library on disk.
        /// </summary>
        private readonly string _absoluteLibraryPath;

        /// <summary>
        /// The configuration used for the library at construction time.
        /// </summary>
        private readonly ImplementationConfiguration _configuration;

        /// <summary>
        /// Gets the type of interface this key maps to.
        /// </summary>
        /// <returns>Thge type of the interface.</returns>
        public Type GetInterfaceType() => _interfaceType;

        /// <inheritdoc />
        public bool Equals(LibraryIdentifier other)
        {
            return _interfaceType == other._interfaceType &&
                   string.Equals(_absoluteLibraryPath, other._absoluteLibraryPath) &&
                   _configuration.Equals(other._configuration);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is LibraryIdentifier identifier && Equals(identifier);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return
                ((_interfaceType != null ? _interfaceType.GetHashCode() : 0) * 397) ^
                (_absoluteLibraryPath != null ? _absoluteLibraryPath.GetHashCode() : 0) ^
                (_configuration.GetHashCode() * 397);
            }
        }
    }
}
