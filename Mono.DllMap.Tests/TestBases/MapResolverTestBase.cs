//
//  MapResolverTestBase.cs
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

#pragma warning disable SA1600, CS1591

namespace Mono.DllMap.Tests.TestBases
{
    public class MapResolverTestBase
    {
        protected const string OriginalLibraryName = "cygwin1.dll";
        protected const string RemappedLibraryName = "libc.so.6";
        protected const string UnmappedLibraryName = "libunmapped.so";

        protected DllMapResolver Resolver { get; }

        protected MapResolverTestBase()
        {
            Resolver = new DllMapResolver();
        }
    }
}
