using System;

namespace AdvancedDLSupport.Results
{
    /// <summary>
    /// Represents an attempt to resolve a path.
    /// </summary>
    public struct ResolvePathResult : IResult
    {
        /// <summary>
        /// Gets the resolved path.
        /// </summary>
        public string Path { get; }

        /// <inheritdoc />
        public string ErrorReason { get; }

        /// <inheritdoc />
        public bool IsSuccess { get; }

        /// <inheritdoc />
        public Exception Exception { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResolvePathResult"/> struct.
        /// </summary>
        /// <param name="path">The path that was resolved. Can be null.</param>
        /// <param name="errorReason">The reason why the path couldn't be resolved. Can be null.</param>
        /// <param name="isSuccess">Whether or not a path was resolved.</param>
        /// <param name="exception">The exception which caused the path resolving to fail.</param>
        private ResolvePathResult(string path, string errorReason, bool isSuccess, Exception exception)
        {
            Path = path;
            ErrorReason = errorReason;
            IsSuccess = isSuccess;
            Exception = exception;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <param name="resolvedPath">The path that was resolved.</param>
        /// <returns>A successful result.</returns>
        public static ResolvePathResult FromSuccess(string resolvedPath)
        {
            return new ResolvePathResult(resolvedPath, null, true, null);
        }

        /// <summary>
        /// Creates an unsuccessful result.
        /// </summary>
        /// <param name="errorReason">The reason why the resolution failed.</param>
        /// <returns>A failed result.</returns>
        public static ResolvePathResult FromError(string errorReason)
        {
            return new ResolvePathResult(null, errorReason, false, null);
        }

        /// <summary>
        /// Creates an unsuccessful result.
        /// </summary>
        /// <param name="exception">The exception that caused the resolution to fail.</param>
        /// <returns>A failed result.</returns>
        public static ResolvePathResult FromError(Exception exception)
        {
            return new ResolvePathResult(null, exception.Message, false, exception);
        }
    }
}
