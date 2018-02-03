//
//  DllEntry.cs
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
