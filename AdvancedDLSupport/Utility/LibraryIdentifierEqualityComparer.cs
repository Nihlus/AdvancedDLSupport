using System.Collections.Generic;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Compares library identifiers for equality.
    /// </summary>
    internal class LibraryIdentifierEqualityComparer : IEqualityComparer<GeneratedImplementationTypeIdentifier>
    {
        /// <inheritdoc />
        public bool Equals(GeneratedImplementationTypeIdentifier x, GeneratedImplementationTypeIdentifier y)
        {
            return x.Equals(y);
        }

        /// <inheritdoc />
        public int GetHashCode(GeneratedImplementationTypeIdentifier obj)
        {
            return obj.GetHashCode();
        }
    }
}
