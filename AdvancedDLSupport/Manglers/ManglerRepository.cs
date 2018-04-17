//
//  ManglerRepository.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Repository class for name manglers.
    /// </summary>
    [PublicAPI]
    public class ManglerRepository
    {
        /// <summary>
        /// Gets the default instance of the <see cref="ManglerRepository"/> class. This instance contains all discovered
        /// mangler types.
        /// </summary>
        [PublicAPI]
        public static ManglerRepository Default { get; }

        private List<IEntrypointMangler> Manglers { get; }

        /// <summary>
        /// Initializes static members of the <see cref="ManglerRepository"/> class.
        /// </summary>
        static ManglerRepository()
        {
            Default = new ManglerRepository();
            Default.DiscoverManglers();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManglerRepository"/> class.
        /// </summary>
        private ManglerRepository()
        {
            Manglers = new List<IEntrypointMangler>();
        }

        /// <summary>
        /// Scans the currently executing assembly for classes implementing the <see cref="IEntrypointMangler"/>
        /// interface, and creates an instance of each which is added to the repository's internal store.
        /// </summary>
        internal void DiscoverManglers()
        {
            Manglers.Clear();

            var assembly = Assembly.GetExecutingAssembly();
            var manglerTypes = assembly.DefinedTypes.Where
            (
                t =>
                    t.IsClass &&
                    t.ImplementedInterfaces.Contains(typeof(IEntrypointMangler))
            );

            foreach (var manglerType in manglerTypes)
            {
                var mangler = (IEntrypointMangler)Activator.CreateInstance(manglerType);
                Manglers.Add(mangler);
            }
        }

        /// <summary>
        /// Gets the manglers that are applicable to the given member.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <typeparam name="T">A member implementing the <see cref="IIntrospectiveMember"/> interface.</typeparam>
        /// <returns>A set of applicable manglers, if any.</returns>
        [PublicAPI, NotNull, ItemNotNull]
        public IEnumerable<IEntrypointMangler> GetApplicableManglers<T>(T member) where T : MemberInfo
        {
            return Manglers.Where(m => m.IsManglerApplicable(member));
        }
    }
}
