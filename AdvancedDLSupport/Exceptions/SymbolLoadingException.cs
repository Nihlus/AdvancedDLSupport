﻿using System;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Represents a failure to load a native library.
    /// </summary>
    public class SymbolLoadingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolLoadingException"/> class.
        /// </summary>
        public SymbolLoadingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolLoadingException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        public SymbolLoadingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolLoadingException"/> class.
        /// </summary>
        /// <param name="message">The message of the exception.</param>
        /// <param name="inner">The exception which caused this exception.</param>
        public SymbolLoadingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}