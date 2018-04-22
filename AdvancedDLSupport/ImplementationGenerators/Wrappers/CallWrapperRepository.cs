//
//  CallWrapperRepository.cs
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

using System.Collections.Generic;
using System.Linq;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Holds available call wrappers.
    /// </summary>
    [PublicAPI]
    public class CallWrapperRepository
    {
        private readonly IList<ICallWrapper> _callWrappers;

        /// <summary>
        /// Adds the given call wrapper to the repository. If the repository already contains a wrapper of the same
        /// type, the call is ignored.
        /// </summary>
        /// <param name="callWrapper">The wrapper to add.</param>
        /// <returns>The repository, with the wrapper.</returns>
        [PublicAPI, NotNull]
        public CallWrapperRepository WithCallWrapper(ICallWrapper callWrapper)
        {
            if (_callWrappers.All(c => c.GetType() != callWrapper.GetType()))
            {
                _callWrappers.Add(callWrapper);
            }

            return this;
        }

        /// <summary>
        /// Determines if the given method definition has an applicable wrapper in the repository.
        /// </summary>
        /// <param name="definition">The method definition.</param>
        /// <param name="options">The implementation options.</param>
        /// <returns>true if the method has an applicable wrapper; otherwise, false.</returns>
        [PublicAPI]
        public bool HasApplicableWrapper
        (
            [NotNull] IntrospectiveMethodInfo definition,
            ImplementationOptions options
        )
        {
            return _callWrappers.Any(c => c.IsApplicable(definition, options));
        }

        /// <summary>
        /// Gets the applicable wrappers in the repository for the given method definition.
        /// </summary>
        /// <param name="definition">The method definition.</param>
        /// <param name="options">The implementation options.</param>
        /// <returns>The applicable wrappers.</returns>
        [PublicAPI, NotNull, ItemNotNull]
        public IEnumerable<ICallWrapper> GetApplicableWrappers
        (
            [NotNull] IntrospectiveMethodInfo definition,
            ImplementationOptions options
        )
        {
            return _callWrappers.Where(c => c.IsApplicable(definition, options));
        }
    }
}
