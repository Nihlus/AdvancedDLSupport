using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Holds metadata for native functions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property), PublicAPI]
    public class NativeSymbolAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the function's entrypoint.
        /// </summary>
        [PublicAPI]
        public string Entrypoint { get; set; }

        /// <summary>
        /// Gets or sets the function's calling convention.
        /// </summary>
        [PublicAPI]
        public CallingConvention CallingConvention { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeSymbolAttribute"/> class.
        /// </summary>
        [PublicAPI]
        public NativeSymbolAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeSymbolAttribute"/> class.
        /// </summary>
        /// <param name="entrypoint">The name of the function's entry point.</param>
        [PublicAPI]
        public NativeSymbolAttribute([NotNull] string entrypoint)
        {
            Entrypoint = entrypoint;
        }
    }
}
