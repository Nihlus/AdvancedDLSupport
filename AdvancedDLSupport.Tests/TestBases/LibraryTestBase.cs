//
//  LibraryTestBase.cs
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

using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.TestBases
{
    public abstract class LibraryTestBase<T> : IDisposable where T : class
    {
        protected ImplementationOptions Config { get; }

        [NotNull]
        protected T Library { get; }

        [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor", Justification = "Used to set implementation options in derived classes")]
        protected LibraryTestBase([NotNull] string libraryLocation)
        {
            Config = GetImplementationOptions();
            Library = GetImplementationBuilder().ActivateInterface<T>(libraryLocation);
        }

        protected virtual ImplementationOptions GetImplementationOptions()
        {
            return ImplementationOptions.GenerateDisposalChecks;
        }

        [NotNull]
        protected virtual NativeLibraryBuilder GetImplementationBuilder()
        {
            return new NativeLibraryBuilder(Config);
        }

        public void Dispose()
        {
            var libraryBase = Library as NativeLibraryBase;
            libraryBase?.Dispose();
        }
    }
}
