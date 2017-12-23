using System;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Holds configuration options for library binding generations.
    /// </summary>
    public struct ImplementationConfiguration : IEquatable<ImplementationConfiguration>
    {
        /// <summary>
        /// Gets a value indicating whether or not to use lazy binding for symbols.
        /// </summary>
        public bool UseLazyBinding { get; }

        /// <summary>
        /// Gets a value indicating whether or not to generate per-call checks that the library has not been
        /// disposed.
        /// </summary>
        public bool GenerateDisposalChecks { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementationConfiguration"/> struct.
        /// </summary>
        /// <param name="useLazyBinding">Whether or not to use lazy binding.</param>
        /// <param name="generateDisposalChecks">Whether or not to generate disposal checks.</param>
        public ImplementationConfiguration(bool useLazyBinding, bool generateDisposalChecks)
        {
            UseLazyBinding = useLazyBinding;
            GenerateDisposalChecks = generateDisposalChecks;
        }

        /// <inheritdoc />
        public bool Equals(ImplementationConfiguration other)
        {
            return UseLazyBinding == other.UseLazyBinding && GenerateDisposalChecks == other.GenerateDisposalChecks;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is ImplementationConfiguration configuration && Equals(configuration);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (UseLazyBinding.GetHashCode() * 397) ^ GenerateDisposalChecks.GetHashCode();
            }
        }
    }
}
