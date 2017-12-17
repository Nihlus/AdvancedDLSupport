// ReSharper disable InconsistentNaming

namespace AdvancedDLSupport
{
    /// <summary>
    /// <see cref="dl.open"/> flags. Taken from the source code of
    /// GNU libc: <a href="https://github.com/lattera/glibc/blob/master/bits/dlfcn.h"/>
    /// </summary>
    public enum SymbolFlags
    {
        /// <summary>
        /// The default flags.
        /// </summary>
        RTLD_DEFAULT = RTLD_LOCAL,

        /// <summary>
        /// Lazy function call binding.
        /// </summary>
        RTLD_LAZY = 0x00001,

        /// <summary>
        /// Immediate function call binding.
        /// </summary>
        RTLD_NOW = 0x00002,

        /// <summary>
        /// If set, makes the symbols of the loaded object and its dependencies visible
        /// as if the object was linked directly into the program.
        /// </summary>
        RTLD_GLOBAL = 0x00100,

        /// <summary>
        /// The inverse of <see cref="RTLD_GLOBAL"/>. Typically, this is the default behaviour.
        /// </summary>
        RTLD_LOCAL = 0x00000,

        /// <summary>
        /// Do not delete the object when closed.
        /// </summary>
        RTLD_NODELETE = 0x01000,
    }
}
