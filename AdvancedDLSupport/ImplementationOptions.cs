using System;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Holds generated implementation flag options.
    /// </summary>
    [Flags]
    public enum ImplementationOptions
    {
        /// <summary>
        /// Generate the bindings with lazy loaded symbol resolution.
        /// </summary>
        UseLazyBinding = 1 << 0,

        /// <summary>
        /// Generate disposal checks for all binder methods.
        /// </summary>
        GenerateDisposalChecks = 1 << 1,

        /// <summary>
        /// Enable Mono dllmap support for library scanning.
        /// </summary>
        EnableDllMapSupport = 1 << 2
    }
}
