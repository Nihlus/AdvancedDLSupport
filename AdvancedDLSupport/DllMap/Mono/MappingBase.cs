using System;
using System.Linq;
using System.Xml.Serialization;
using AdvancedDLSupport.Extensions;
using JetBrains.Annotations;

namespace AdvancedDLSupport.DllMap.Mono
{
    /// <summary>
    /// The base class for Dll mapping entries, containing system constraint information.
    /// </summary>
    internal abstract class MappingBase
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
        [XmlIgnore]
        public DllMapOS OperatingSystems => ParseDllMapAttributeList<DllMapOS>(RawOperatingSystems);

        /// <summary>
        /// Gets the supported processor architectures of the entry.
        /// </summary>
        [XmlIgnore]
        public DllMapArchitecture Architecture => ParseDllMapAttributeList<DllMapArchitecture>(RawArchitecture);

        /// <summary>
        /// Gets the supported word sizes of the entry.
        /// </summary>
        [XmlIgnore]
        public DllMapWordSize WordSize => ParseDllMapAttributeList<DllMapWordSize>(RawWordSize);

        private TEnum ParseDllMapAttributeList<TEnum>([CanBeNull] string content) where TEnum : struct
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException(nameof(TEnum), "The provided type was not an enum.");
            }

            if (content.IsNullOrWhiteSpace())
            {
                return Enum
                    .GetValues(typeof(TEnum))
                    .Cast<TEnum>()
                    .Aggregate((a, b) => (dynamic)a | (dynamic)b);
            }

            bool isInverse = false;
            var parsingString = content.Replace('-', '_');
            if (parsingString.First() == '!')
            {
                parsingString = parsingString.Skip(1).ToString();
                isInverse = true;
            }

            var parts = parsingString.Split(',');
            var systems = parts.Select
                (
                    p =>
                    (
                        CouldParse: Enum.TryParse(p, true, out TEnum x),
                        Value: x
                    )
                )
                .Where(t => t.CouldParse)
                .Select(t => t.Value).Distinct();

            if (isInverse)
            {
                systems = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().Except(systems);
            }

            return systems.Aggregate((a, b) => (dynamic)a | (dynamic)b);
        }
    }
}
