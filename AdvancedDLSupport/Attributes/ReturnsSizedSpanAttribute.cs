//
//  ReturnsSizedSpanAttribute.cs
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
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// TODO.
    /// </summary>
    [PublicAPI, AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
    public class ReturnsSizedSpanAttribute : Attribute
    {
        /// <summary>
        /// Gets the number of elements in the span.
        /// Null if <see cref="MethodName"/> should be used instead.
        /// </summary>
        [CanBeNull]
        internal int? SpanLength { get; }

        /// <summary>
        /// Gets the string name of the method used to return the number of elements in the span.
        /// Null if <see cref="SpanLength"/> should be used instead.
        /// </summary>
        [CanBeNull]
        internal string MethodName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnsSizedSpanAttribute"/> class.
        /// </summary>
        /// <param name="spanLength">The number of elements in the Span returned. </param>
        public ReturnsSizedSpanAttribute(int spanLength)
            => SpanLength = spanLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReturnsSizedSpanAttribute"/> class.
        /// </summary>
        /// <param name="methodName">The name of the method to call to get the number of elements in the Span returned. </param>
        public ReturnsSizedSpanAttribute(string methodName)
        {
            throw new NotImplementedException();
            MethodName = methodName;
        }
    }
}
