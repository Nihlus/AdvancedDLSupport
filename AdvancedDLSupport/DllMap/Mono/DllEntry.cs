using System.Xml.Serialization;

namespace AdvancedDLSupport.DllMap.Mono
{
    /// <summary>
    /// Represents a subentry in a Mono DllMap for specific functions.
    /// </summary>
    [XmlRoot("dllentry")]
    internal class DllEntry : MappingBase
    {
        /// <summary>
        /// Gets or sets the target library that the entry should map to.
        /// </summary>
        [XmlAttribute("dll")]
        public string TargetLibrary { get; set; }

        /// <summary>
        /// Gets or sets the name of the source symbol that the entry maps.
        /// </summary>
        [XmlAttribute("name")]
        public string SourceSymbol { get; set; }

        /// <summary>
        /// Gets or sets the name of the target symbol that the entry should map to.
        /// </summary>
        [XmlAttribute("target")]
        public string TargetSymbol { get; set; }
    }
}
