//
//  SymbolTransformer.cs
//
//  Copyright (c) 2018 Firwood Software
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

using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using static AdvancedDLSupport.SymbolTransformationMethod;

#pragma warning disable SA1513

namespace AdvancedDLSupport
{
    /// <summary>
    /// Transforms native symbol names based on information from a <see cref="NativeSymbolsAttribute"/>.
    /// </summary>
    public class SymbolTransformer
    {
        /// <summary>
        /// Gets the default instance of the <see cref="SymbolTransformer"/> class.
        /// </summary>
        public static readonly SymbolTransformer Default = new SymbolTransformer();

        /// <summary>
        /// Gets the transformed symbol name of the given member.
        /// </summary>
        /// <param name="containingInterface">The interface that the member belongs to.</param>
        /// <param name="memberInfo">The member.</param>
        /// <typeparam name="T">The type of the member.</typeparam>
        /// <returns>The transformed symbol.</returns>
        /// <exception cref="AmbiguousMatchException">Thrown if the member has more than on applicable name mangler.</exception>
        public string GetTransformedSymbol<T>([NotNull] Type containingInterface, [NotNull] T memberInfo) where T : MemberInfo, IIntrospectiveMember
        {
            var nativeSymbolAttribute = memberInfo.GetCustomAttribute<NativeSymbolAttribute>()
                                        ?? new NativeSymbolAttribute(memberInfo.Name);

            var symbolName = nativeSymbolAttribute.Entrypoint;
            var applicableManglers = ManglerRepository.Default.GetApplicableManglers(memberInfo).ToList();
            if (applicableManglers.Count > 1)
            {
                throw new AmbiguousMatchException
                (
                    "Multiple name manglers were deemed applicable to the member. Provide hinting information in the native symbol attribute."
                );
            }

            if (applicableManglers.Any())
            {
                var applicableMangler = applicableManglers.First();
                symbolName = applicableMangler.Mangle(memberInfo);
            }

            var nativeSymbolsAttribute = containingInterface.GetCustomAttribute<NativeSymbolsAttribute>();

            if (nativeSymbolsAttribute is null)
            {
                return symbolName;
            }

            return Transform(symbolName, nativeSymbolsAttribute.Prefix, nativeSymbolsAttribute.SymbolTransformationMethod);
        }

        /// <summary>
        /// Transforms the given symbol name.
        /// </summary>
        /// <param name="symbol">The symbol to transform.</param>
        /// <param name="prefix">The prefix to be added to the symbol. Defaults to nothing.</param>
        /// <param name="method">The transformer to apply to the symbol after concatenation.</param>
        /// <returns>The transformed symbol name.</returns>
        [Pure]
        private string Transform
        (
            [NotNull] string symbol,
            [CanBeNull] string prefix = null,
            SymbolTransformationMethod method = None
        )
        {
            prefix = prefix ?? string.Empty;

            var concatenated = $"{prefix}{symbol}";

            switch (method)
            {
                case None:
                {
                    return concatenated;
                }
                case Pascalize:
                {
                    return PascalizeValue(concatenated);
                }
                case Camelize:
                {
                    return CamelizeValue(concatenated);
                }
                case Underscore:
                {
                    return UnderscoreValue(concatenated);
                }
                case Dasherize:
                {
                    return DasherizeValue(concatenated);
                }
                case Kebaberize:
                {
                    return KebaberizeValue(concatenated);
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
                }
            }
        }

        private string CamelizeValue(string input)
        {
            var word = PascalizeValue(input);
            return word.Length > 0 ? word.Substring(0, 1).ToLower() + word.Substring(1) : word;
        }

        private string PascalizeValue(string input)
        {
            return Regex.Replace(input, "(?:^|_)(.)", match => match.Groups[1].Value.ToUpper());
        }

        private string UnderscoreValue(string input)
        {
            return Regex.Replace(
                Regex.Replace(
                    Regex.Replace(
                        input,
                        @"([\p{Lu}]+)([\p{Lu}][\p{Ll}])",
                        "$1_$2"),
                    @"([\p{Ll}\d])([\p{Lu}])",
                    "$1_$2"), @"[-\s]",
                "_")
                .ToLower();
        }

        private string DasherizeValue(string underscoredWord)
        {
            return underscoredWord.Replace('_', '-');
        }

        private string KebaberizeValue(string input)
        {
            return DasherizeValue(UnderscoreValue(input));
        }
    }
}
