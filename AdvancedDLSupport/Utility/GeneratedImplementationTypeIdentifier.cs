using System;
using System.IO;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// A key struct for ConcurrentDictionary TypeCache for all generated types provided by DLSupportConstructor.
    /// </summary>
    internal struct GeneratedImplementationTypeIdentifier : IEquatable<GeneratedImplementationTypeIdentifier>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedImplementationTypeIdentifier"/> struct.
        /// </summary>
        /// <param name="baseClassType">The base class of the library.</param>
        /// <param name="interfaceType">The interface type.</param>
        /// <param name="libraryPath">The path to the library. Will be resolved to an absolute path.</param>
        /// <param name="options">The configuration used for the library.</param>
        public GeneratedImplementationTypeIdentifier
        (
            [NotNull] Type baseClassType,
            [NotNull] Type interfaceType,
            [NotNull] string libraryPath,
            ImplementationOptions options
        )
        {
            _baseClassType = baseClassType;
            _interfaceType = interfaceType;
            _options = options;
            _absoluteLibraryPath = Path.GetFullPath(libraryPath);
        }

        /// <summary>
        /// The base class type of the library.
        /// </summary>
        private readonly Type _baseClassType;

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
        private readonly ImplementationOptions _options;

        /// <summary>
        /// Gets the type of interface this key maps to.
        /// </summary>
        /// <returns>Thge type of the interface.</returns>
        public Type GetInterfaceType() => _interfaceType;

        /// <inheritdoc />
        public bool Equals(GeneratedImplementationTypeIdentifier other)
        {
            return
                _baseClassType == other._baseClassType &&
                _interfaceType == other._interfaceType &&
                string.Equals(_absoluteLibraryPath, other._absoluteLibraryPath) &&
                _options == other._options;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is GeneratedImplementationTypeIdentifier identifier && Equals(identifier);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return
                    ((_baseClassType != null ? _interfaceType.GetHashCode() : 0) * 397) ^
                    (_interfaceType != null ? _interfaceType.GetHashCode() : 0) * 397 ^
                    (_absoluteLibraryPath != null ? _absoluteLibraryPath.GetHashCode() : 0) ^
                    ((int)_options * 397);
            }
        }
    }
}
