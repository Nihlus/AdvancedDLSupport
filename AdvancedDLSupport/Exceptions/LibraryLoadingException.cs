using System;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Represents a failure to load a native library.
    /// </summary>
    [PublicAPI]
    public class LibraryLoadingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        [PublicAPI]
        public LibraryLoadingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        [PublicAPI]
        public LibraryLoadingException([NotNull] string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="inner">The exception which caused this exception.</param>
        [PublicAPI]
        public LibraryLoadingException([NotNull] string message, [NotNull] Exception inner)
            : base(message, inner)
        {
        }
    }
}
