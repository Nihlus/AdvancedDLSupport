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
        private const string ReplacementPattern = "$1_$2";
        private const string Underscore = "_";
        private const string Hyphen = "-";

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
        public string Transform
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
                case SymbolTransformationMethod.Underscore:
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

        [NotNull]
        private string CamelizeValue([NotNull] string input)
        {
            var word = PascalizeValue(input);
            return word.Length > 0 ? word.Substring(0, 1).ToLower() + word.Substring(1) : word;
        }

        private static readonly Regex PascalizeReplaceOne = new Regex("(?:^|_)(.)", RegexOptions.Compiled);
        private static readonly MatchEvaluator PascalizeMatchEvaluator = (match) => match.Groups[1].Value.ToUpper();

        [NotNull]
        private string PascalizeValue([NotNull] string input)
        {
            return PascalizeReplaceOne.Replace(input, PascalizeMatchEvaluator);
        }

        private static readonly Regex UnderscoreReplaceOne = new Regex(@"([\p{Lu}]+)([\p{Lu}][\p{Ll}])", RegexOptions.Compiled);
        private static readonly Regex UnderscoreReplaceTwo = new Regex(@"([\p{Ll}\d])([\p{Lu}])", RegexOptions.Compiled);
        private static readonly Regex UnderscoreReplaceThree = new Regex(@"[-\s]", RegexOptions.Compiled);

        [NotNull]
        private string UnderscoreValue([NotNull] string input)
        {
            return UnderscoreReplaceThree.Replace
            (
                UnderscoreReplaceTwo.Replace
                (
                    UnderscoreReplaceOne.Replace
                    (
                        input,
                        ReplacementPattern
                    ),
                    ReplacementPattern
                ),
                Underscore
            ).ToLower();
        }

        /// <summary>
        /// Replaces all underscores with dashes.
        /// </summary>
        /// <param name="underscoredWord">A string to replace underscores with dashes.</param>
        /// <returns>A string of which the underscores are replaced with dashes.</returns>
        [NotNull]
        private string DasherizeValue([NotNull] string underscoredWord)
        {
            return underscoredWord.Replace(Underscore, Hyphen);
        }

        /// <summary>
        /// Keberizes the value.
        /// </summary>
        /// <param name="input">The string to kebaberize.</param>
        /// <returns>A Kebaberized string.</returns>
        [NotNull]
        private string KebaberizeValue([NotNull] string input)
        {
            return DasherizeValue(UnderscoreValue(input));
        }
    }
}
