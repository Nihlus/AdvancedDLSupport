//
//  DllConfiguration.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;
using Mono.DllMap.Utility;

namespace Mono.DllMap;

/// <summary>
/// Represents a set of Mono DllMap entries.
/// </summary>
[PublicAPI, XmlRoot("configuration")]
public class DllConfiguration
{
    /// <summary>
    /// Gets or sets the mapping entries.
    /// </summary>
    [PublicAPI, XmlElement("dllmap")]
    public List<DllMap>? Maps { get; set; }

    /// <summary>
    /// Gets the map entries that are relevant for the current platform.
    /// </summary>
    /// <returns>The entries relevant for the current platform.</returns>
    [PublicAPI]
    public IEnumerable<DllMap> GetRelevantMaps()
    {
        var currentPlatform = DllConfigurationPlatformHelper.GetCurrentPlatform();
        var currentArch = DllConfigurationPlatformHelper.GetCurrentRuntimeArchitecture();
        var currentWordSize = DllConfigurationPlatformHelper.GetRuntimeWordSize();

        return Maps.Where
        (
            m =>
                m.OperatingSystems.HasFlagFast(currentPlatform) &&
                m.Architecture.HasFlagFast(currentArch) &&
                m.WordSize.HasFlagFast(currentWordSize)
        );
    }

    /// <summary>
    /// Parses a DllMap configuration from the given XML document.
    /// </summary>
    /// <param name="xml">The XML to parse.</param>
    /// <returns>A <see cref="DllConfiguration"/> object.</returns>
    [PublicAPI, Pure]
    public static DllConfiguration Parse(string xml)
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
    [PublicAPI, Pure]
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
    [PublicAPI, Pure]
    public static DllConfiguration Parse(TextReader tr)
    {
        var deserializer = new XmlSerializer(typeof(DllConfiguration));
        var config = (DllConfiguration)deserializer.Deserialize(tr);

        // Apply constraint inheritance
        foreach (var map in config.Maps ?? new List<DllMap>())
        {
            foreach (var symbolEntry in map.SymbolEntries ?? new List<DllEntry>())
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
    [PublicAPI, Pure, ContractAnnotation("false <= result:null; true <= result:notnull")]
    public static bool TryParse(Stream s, out DllConfiguration? result)
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
    [PublicAPI, Pure, ContractAnnotation("false <= result:null; true <= result:notnull")]
    public static bool TryParse(TextReader tr, out DllConfiguration? result)
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
    [PublicAPI, Pure, ContractAnnotation("false <= result:null; true <= result:notnull")]
    public static bool TryParse(string xml, out DllConfiguration? result)
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
