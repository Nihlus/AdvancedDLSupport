using System.Xml.Serialization;

namespace AdvancedDLSupport.DllMap.Mono
{
    /// <summary>
    /// Represents an entry in a Mono DllMap configuration.
    /// </summary>
    [XmlRoot("dllmap")]
    internal class DllMap : MappingBase
    {
        /// <summary>
        /// Gets or sets the name of the source library that the map maps.
        /// </summary>
        [XmlAttribute("dll")]
        public string SourceLibrary { get; set; }

        /// <summary>
        /// Gets or sets the name of the target library that the map maps to.
        /// </summary>
        [XmlAttribute("target")]
        public string TargetLibrary { get; set; }
    }
}
