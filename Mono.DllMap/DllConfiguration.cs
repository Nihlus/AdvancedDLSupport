using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace Mono.DllMap
{
    /// <summary>
    /// Represents a set of Mono DllMap entries.
    /// </summary>
    [XmlRoot("configuration"), PublicAPI]
    public class DllConfiguration
    {
        /// <summary>
        /// Gets or sets the mapping entries.
        /// </summary>
        [XmlElement("dllmap")]
        public List<DllMap> Maps { get; set; }

        /// <summary>
        /// Parses a DllMap configuration from the given XML document.
        /// </summary>
        /// <param name="xml">The XML to parse.</param>
        /// <returns>A <see cref="DllConfiguration"/> object.</returns>
        [Pure, PublicAPI]
        public static DllConfiguration Parse([NotNull] string xml)
        {
            using (var sr = new StringReader(xml))
            {
                return Parse(sr);
            }
        }

        /// <summary>
        /// Parses a DllMap configuration from the given XML document.
        /// </summary>
        /// <param name="s">The stream containing the xml.</param>
        /// <returns>A <see cref="DllConfiguration"/> object.</returns>
        [Pure, PublicAPI]
        public static DllConfiguration Parse(Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                return Parse(sr);
            }
        }

        /// <summary>
        /// Parses a DllMap configuration from the given XML document.
        /// </summary>
        /// <param name="tr">The reader containing the xml.</param>
        /// <returns>A <see cref="DllConfiguration"/> object.</returns>
        [Pure, PublicAPI]
        public static DllConfiguration Parse([NotNull] TextReader tr)
        {
            var deserializer = new XmlSerializer(typeof(DllConfiguration));
            var config = (DllConfiguration)deserializer.Deserialize(tr);

            // Apply constraint inheritance
            foreach (var map in config.Maps)
            {
                foreach (var symbolEntry in map.SymbolEntries)
                {
                    if (symbolEntry.RawArchitecture is null)
                    {
                        symbolEntry.Architecture = map.Architecture;
                    }

                    if (symbolEntry.RawOperatingSystems is null)
                    {
                        symbolEntry.OperatingSystems = map.OperatingSystems;
                    }

                    if (symbolEntry.RawWordSize is null)
                    {
                        symbolEntry.WordSize = map.WordSize;
                    }
                }
            }

            return config;
        }

        /// <summary>
        /// Attempts to parse a DllMap configuration from the given XML document.
        /// </summary>
        /// <param name="s">The XML to parse.</param>
        /// <param name="result">The resulting <see cref="DllConfiguration"/> object.</param>
        /// <returns>true if the parsing succeeded; otherwise, false.</returns>
        [Pure, PublicAPI, ContractAnnotation("false <= result:null; true <= result:notnull")]
        public static bool TryParse([NotNull] Stream s, [CanBeNull] out DllConfiguration result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to parse a DllMap configuration from the given XML document.
        /// </summary>
        /// <param name="tr">The XML to parse.</param>
        /// <param name="result">The resulting <see cref="DllConfiguration"/> object.</param>
        /// <returns>true if the parsing succeeded; otherwise, false.</returns>
        [Pure, PublicAPI, ContractAnnotation("false <= result:null; true <= result:notnull")]
        public static bool TryParse([NotNull] TextReader tr, [CanBeNull] out DllConfiguration result)
        {
            try
            {
                result = Parse(tr);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Attempts to parse a DllMap configuration from the given XML document.
        /// </summary>
        /// <param name="xml">The XML to parse.</param>
        /// <param name="result">The resulting <see cref="DllConfiguration"/> object.</param>
        /// <returns>true if the parsing succeeded; otherwise, false.</returns>
        [Pure, PublicAPI, ContractAnnotation("false <= result:null; true <= result:notnull")]
        public static bool TryParse([NotNull] string xml, [CanBeNull] out DllConfiguration result)
        {
            try
            {
                result = Parse(xml);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
