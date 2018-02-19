//
//  DllMap.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System.Collections.Generic;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace Mono.DllMap
{
    /// <summary>
    /// Represents an entry in a Mono DllMap configuration.
    /// </summary>
    [PublicAPI, XmlRoot("dllmap")]
    public class DllMap : MappingBase
    {
        /// <summary>
        /// Gets or sets the name of the source library that the map maps.
        /// </summary>
        [PublicAPI, XmlAttribute("dll")]
        public string SourceLibrary { get; set; }

        /// <summary>
        /// Gets or sets the name of the target library that the map maps to.
        /// </summary>
        [PublicAPI, XmlAttribute("target")]
        public string TargetLibrary { get; set; }

        /// <summary>
        /// Gets or sets the list of symbol remapping entries in the mapping entry.
        /// </summary>
        [PublicAPI, XmlElement("dllentry")]
        public List<DllEntry> SymbolEntries { get; set; }
    }
}
