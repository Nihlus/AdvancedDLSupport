//
//  LazyLoadingIntegrationTests.cs
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

using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

#pragma warning disable SA1600, CS1591

// ReSharper disable ArgumentsStyleLiteral
namespace AdvancedDLSupport.Tests.Integration;

public class LazyLoadingIntegrationTests : LibraryTestBase<ILazyLoadingLibrary>
{
    private const string _libraryName = "LazyLoadingTests";

    public LazyLoadingIntegrationTests()
        : base(_libraryName)
    {
    }

    protected override ImplementationOptions GetImplementationOptions()
    {
        return ImplementationOptions.UseLazyBinding;
    }

    [Fact]
    public void LoadingAnInterfaceWithAMissingFunctionThrows()
    {
        Assert.Throws<SymbolLoadingException>
        (
            () =>
                new NativeLibraryBuilder().ActivateInterface<ILazyLoadingLibrary>(_libraryName)
        );
    }

    [Fact]
    public void CallingMissingMethodInLazyLoadedInterfaceThrows()
    {
        Assert.Throws<SymbolLoadingException>
        (
            () =>
                Library.MissingMethod(0, 0)
        );
    }

    [Fact]
    public void LoadingAnInterfaceWithAMissingPropertyThrows()
    {
        Assert.Throws<SymbolLoadingException>
        (
            () =>
                new NativeLibraryBuilder().ActivateInterface<ILazyLoadingLibrary>(_libraryName)
        );
    }

    [Fact]
    public void SettingMissingPropertyInLazyLoadedInterfaceThrows()
    {
        Assert.Throws<SymbolLoadingException>
        (
            () =>
                Library.MissingProperty = 0
        );
    }

    [Fact]
    public void GettingMissingPropertyInLazyLoadedInterfaceThrows()
    {
        Assert.Throws<SymbolLoadingException>
        (
            () =>
                Library.MissingProperty
        );
    }
}
