using System;
using System.Runtime.InteropServices;

namespace AdvancedDLSupport.Attributes
{
    /// <summary>
    /// Holds metadata for native functions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NativeFunctionAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the name of the function's entrypoint.
        /// </summary>
        public string Entrypoint { get; set; }

        /// <summary>
        /// Gets or sets the function's calling convention.
        /// </summary>
        public CallingConvention CallingConvention { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeFunctionAttribute"/> class.
        /// </summary>
        public NativeFunctionAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeFunctionAttribute"/> class.
        /// </summary>
        /// <param name="entrypoint">The name of the function's entry point.</param>
        public NativeFunctionAttribute(string entrypoint)
        {
            Entrypoint = entrypoint;
        }
    }
}
