using System.Collections.Generic;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Compares library identifiers for equality.
    /// </summary>
    internal class LibraryIdentifierEqualityComparer : IEqualityComparer<LibraryIdentifier>
    {
        /// <inheritdoc />
        public bool Equals(LibraryIdentifier x, LibraryIdentifier y)
        {
            return x.Equals(y);
        }

        /// <inheritdoc />
        public int GetHashCode(LibraryIdentifier obj)
        {
            return obj.GetHashCode();
        }
    }
}
