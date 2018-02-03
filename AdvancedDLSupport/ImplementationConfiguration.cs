using System;
using System.IO;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Holds configuration options for library binding generations.
    /// </summary>
    [PublicAPI]
    public class ImplementationConfiguration : IEquatable<ImplementationConfiguration>
    {
        /// <summary>
        /// Gets a value indicating whether or not to use lazy binding for symbols.
        /// </summary>
        [PublicAPI]
        public bool UseLazyBinding { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not to generate per-call checks that the library has not been
        /// disposed.
        /// </summary>
        [PublicAPI]
        public bool GenerateDisposalChecks { get; set; }

        /// <summary>
        /// Gets a value indicating whether support for Mono's DllMap system should be enabled.
        /// </summary>
        [PublicAPI]
        public bool EnableDllMapSupport { get; set; }

        /// <summary>
        /// Gets the path resolver to use.
        /// </summary>
        [PublicAPI, CanBeNull]
        public ILibraryPathResolver PathResolver { get; set; }

        /// <inheritdoc />
        [Pure, PublicAPI]
        public bool Equals(ImplementationConfiguration other)
        {
            return
                UseLazyBinding == other.UseLazyBinding &&
                GenerateDisposalChecks == other.GenerateDisposalChecks &&
                EnableDllMapSupport == other.EnableDllMapSupport &&
                PathResolver == other.PathResolver;
        }

        /// <inheritdoc />
        [Pure, PublicAPI]
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is ImplementationConfiguration configuration && Equals(configuration);
        }

        /// <inheritdoc />
        [Pure, PublicAPI]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = UseLazyBinding.GetHashCode();
                hashCode = (hashCode * 397) ^ GenerateDisposalChecks.GetHashCode();
                hashCode = (hashCode * 397) ^ EnableDllMapSupport.GetHashCode();
                hashCode = (hashCode * 397) ^ (PathResolver is null ? 0 : PathResolver.GetHashCode());
                return hashCode;
            }
        }
    }
}
