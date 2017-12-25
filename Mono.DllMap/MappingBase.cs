using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;
using Mono.DllMap.Utility;

namespace Mono.DllMap
{
    /// <summary>
    /// The base class for Dll mapping entries, containing system constraint information.
    /// </summary>
    [PublicAPI]
    public abstract class MappingBase
    {
        /// <summary>
        /// Gets or sets the raw string containing the supported operating systems.
        /// </summary>
        [XmlAttribute("os")]
        public string RawOperatingSystems { get; set; }

        /// <summary>
        /// Gets or sets the raw string containing the supported processor architectures.
        /// </summary>
        [XmlAttribute("cpu")]
        public string RawArchitecture { get; set; }

        /// <summary>
        /// Gets or sets the raw string containing the supported word sizes.
        /// </summary>
        [XmlAttribute("wordsize")]
        public string RawWordSize { get; set; }

        /// <summary>
        /// Gets the supported operating systems of the entry.
        /// </summary>
        [XmlIgnore, PublicAPI]
        public DllMapOS OperatingSystems
        {
            get => DllMapAttributeParser.Parse<DllMapOS>(RawOperatingSystems);
            internal set => RawOperatingSystems = string.Join(",", value.GetFlags().Select(v => v.ToString().ToLowerInvariant()));
        }

        /// <summary>
        /// Gets the supported processor architectures of the entry.
        /// </summary>
        [XmlIgnore, PublicAPI]
        public DllMapArchitecture Architecture
        {
            get => DllMapAttributeParser.Parse<DllMapArchitecture>(RawArchitecture);
            internal set => RawArchitecture = string.Join(",", value.GetFlags().Select(v => v.ToString().ToLowerInvariant()));
        }

        /// <summary>
        /// Gets the supported word sizes of the entry.
        /// </summary>
        [XmlIgnore, PublicAPI]
        public DllMapWordSize WordSize
        {
            get => DllMapAttributeParser.Parse<DllMapWordSize>(RawWordSize);
            internal set => RawWordSize = string.Join(",", value.GetFlags().Select(v => v.ToString().ToLowerInvariant()));
        }
    }
}
