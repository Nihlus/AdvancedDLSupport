using System.Collections.Generic;
using System.Xml.Serialization;

namespace AdvancedDLSupport.DllMap.Mono
{
    /// <summary>
    /// Represents a set of Mono DllMap entries.
    /// </summary>
    [XmlRoot("configuration")]
    internal class DllConfiguration
    {
        /// <summary>
        /// Gets or sets the mapping entries.
        /// </summary>
        [XmlElement("dllmap")]
        public List<DllMap> Maps { get; set; }
    }
}
