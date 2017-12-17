using System;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Represents a failure to load a native library.
    /// </summary>
    public class LibraryLoadingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        public LibraryLoadingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public LibraryLoadingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LibraryLoadingException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="inner">The exception which caused this exception.</param>
        public LibraryLoadingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
