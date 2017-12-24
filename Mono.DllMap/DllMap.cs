using System.Collections.Generic;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace Mono.DllMap
{
    /// <summary>
    /// Represents an entry in a Mono DllMap configuration.
    /// </summary>
    [XmlRoot("dllmap"), PublicAPI]
    public class DllMap : MappingBase
    {
        /// <summary>
        /// Gets or sets the name of the source library that the map maps.
        /// </summary>
        [XmlAttribute("dll"), PublicAPI]
        public string SourceLibrary { get; set; }

        /// <summary>
        /// Gets or sets the name of the target library that the map maps to.
        /// </summary>
        [XmlAttribute("target"), PublicAPI]
        public string TargetLibrary { get; set; }

        /// <summary>
        /// Gets or sets the list of symbol remapping entries in the mapping entry.
        /// </summary>
        [XmlElement("dllentry"), PublicAPI]
        public List<DllEntry> SymbolEntries { get; set; }
    }
}
