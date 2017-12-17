namespace AdvancedDLSupport
{
    /// <summary>
    /// A key struct for ConcurrentDictionary TypeCache for all generated types provided by DLSupportConstructor.
    /// </summary>
    internal struct KeyForInterfaceTypeAndLibName
    {
        /// <summary>
        /// Interface Type name with Namespace
        /// </summary>
        public string FullInterfaceTypeName;

        /// <summary>
        /// Library Path as a composite key to support similar libraries with distinct functionality and interface.
        /// </summary>
        public string LibraryPath;
    }
}
