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
using System.Text;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using static AdvancedDLSupport.SymbolTransformationMethod;

#pragma warning disable SA1513

namespace AdvancedDLSupport
{
    /// <summary>
    /// Transforms native symbol names based on information from a <see cref="NativeSymbolsAttribute"/>.
    /// </summary>
    [PublicAPI]
    public class SymbolTransformer
    {
        /// <summary>
        /// Gets the default instance of the <see cref="SymbolTransformer"/> class.
        /// </summary>
        [NotNull, PublicAPI]
        public static readonly SymbolTransformer Default = new SymbolTransformer();

        /// <summary>
        /// Gets the transformed symbol name of the given member.
        /// </summary>
        /// <param name="containingInterface">The interface that the member belongs to.</param>
        /// <param name="memberInfo">The member.</param>
        /// <typeparam name="T">The type of the member.</typeparam>
        /// <returns>The transformed symbol.</returns>
        /// <exception cref="AmbiguousMatchException">Thrown if the member has more than one applicable name mangler.</exception>
        [PublicAPI, NotNull, Pure]
        public string GetTransformedSymbol<T>([NotNull] Type containingInterface, [NotNull] T memberInfo) where T : MemberInfo, IIntrospectiveMember
        {
            var symbolName = GetTransformedUnmangledSymbol(containingInterface, memberInfo);

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

            return symbolName;
        }

        /// <summary>
        /// Gets the transformed but unmangled symbol name of the given member.
        /// </summary>
        /// <param name="containingInterface">The interface that the member belongs to.</param>
        /// <param name="memberInfo">The member.</param>
        /// <typeparam name="T">The type of the member.</typeparam>
        /// <returns>The transformed unmangled symbol.</returns>
        /// <exception cref="AmbiguousMatchException">Thrown if the member has more than one applicable name mangler.</exception>
        internal string GetTransformedUnmangledSymbol<T>([NotNull] Type containingInterface, [NotNull] T memberInfo) where T : MemberInfo, IIntrospectiveMember
        {
            var symbolName = memberInfo.GetNativeEntrypoint();

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
        [PublicAPI, NotNull, Pure]
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
                    var pascalized = new StringBuilder(concatenated);
                    for (var i = 1; i < pascalized.Length; ++i)
                    {
                        var previousCharacter = pascalized[i - 1];
                        if (previousCharacter == '_' ||
                            previousCharacter == ' ')
                        {
                            pascalized[i] = char.ToUpper(pascalized[i]);
                        }
                    }

                    pascalized[0] = char.ToUpper(pascalized[0]);
                    return pascalized.ToString();
                }
                case Camelize:
                {
                    var camelized = new StringBuilder(concatenated);
                    for (var i = 1; i < camelized.Length; ++i)
                    {
                        var previousCharacter = camelized[i - 1];
                        if (previousCharacter == '_' ||
                            previousCharacter == ' ')
                        {
                            camelized[i] = char.ToUpper(camelized[i]);
                        }
                    }

                    camelized[0] = char.ToLower(camelized[0]);
                    return camelized.ToString();
                }
                case Underscore:
                {
                    var underscore = new StringBuilder(concatenated);
                    for (var i = 1; i < underscore.Length; ++i)
                    {
                        var previousCharacter = underscore[i - 1];
                        char? nextCharacter = null;
                        if (underscore.Length > i + 1)
                        {
                            nextCharacter = underscore[i + 1];
                        }

                        // ReSharper disable once SA1028
                        if (nextCharacter.HasValue &&
                            char.IsUpper(previousCharacter) &&
                            char.IsUpper(underscore[i]) &&
                            char.IsLower(nextCharacter.Value))
                        {
                            underscore.Insert(i, "_");
                        }
                        else if (char.IsLower(previousCharacter) && char.IsUpper(underscore[i]))
                        {
                            underscore.Insert(i, "_");
                        }
                    }

                    underscore.Replace("-", "_");

                    return underscore.ToString().ToLower();
                }
                case Kebaberize:
                case Dasherize:
                {
                    return concatenated.Replace("_", "-");
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(method), method, null);
                }
            }
        }
    }
}
