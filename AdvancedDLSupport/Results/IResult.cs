using System;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Represents an attempted action, which may or may not have succeeded.
    /// </summary>
    internal interface IResult
    {
        /// <summary>
        /// Gets a human-readable reason for the error.
        /// </summary>
        [CanBeNull]
        string ErrorReason { get; }

        /// <summary>
        /// Gets a value indicating whether or not the result is a successful result.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Gets the exception which caused the error (if any).
        /// </summary>
        [CanBeNull]
        Exception Exception { get; }
    }
}
