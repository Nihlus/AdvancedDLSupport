using System;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Holds configuration options for library binding generations.
    /// </summary>
    [PublicAPI]
    public struct ImplementationConfiguration : IEquatable<ImplementationConfiguration>
    {
        /// <summary>
        /// Gets a value indicating whether or not to use lazy binding for symbols.
        /// </summary>
        [PublicAPI]
        public bool UseLazyBinding { get; }

        /// <summary>
        /// Gets a value indicating whether or not to generate per-call checks that the library has not been
        /// disposed.
        /// </summary>
        [PublicAPI]
        public bool GenerateDisposalChecks { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementationConfiguration"/> struct.
        /// </summary>
        /// <param name="useLazyBinding">Whether or not to use lazy binding.</param>
        /// <param name="generateDisposalChecks">Whether or not to generate disposal checks.</param>
        [PublicAPI]
        public ImplementationConfiguration(bool useLazyBinding = false, bool generateDisposalChecks = false)
        {
            UseLazyBinding = useLazyBinding;
            GenerateDisposalChecks = generateDisposalChecks;
        }

        /// <inheritdoc />
        [Pure, PublicAPI]
        public bool Equals(ImplementationConfiguration other)
        {
            return UseLazyBinding == other.UseLazyBinding &&
                   GenerateDisposalChecks == other.GenerateDisposalChecks;
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
                return (UseLazyBinding.GetHashCode() * 397) ^ GenerateDisposalChecks.GetHashCode();
            }
        }
    }
}
