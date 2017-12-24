using System.Xml.Serialization;
using JetBrains.Annotations;

namespace Mono.DllMap
{
    /// <summary>
    /// Represents a subentry in a Mono DllMap for specific functions.
    /// </summary>
    [XmlRoot("dllentry"), PublicAPI]
    public class DllEntry : MappingBase
    {
        /// <summary>
        /// Gets or sets the target library that the entry should map to.
        /// </summary>
        [XmlAttribute("dll"), PublicAPI]
        public string TargetLibrary { get; set; }

        /// <summary>
        /// Gets or sets the name of the source symbol that the entry maps.
        /// </summary>
        [XmlAttribute("name"), PublicAPI]
        public string SourceSymbol { get; set; }

        /// <summary>
        /// Gets or sets the name of the target symbol that the entry should map to.
        /// </summary>
        [XmlAttribute("target"), PublicAPI]
        public string TargetSymbol { get; set; }
    }
}
