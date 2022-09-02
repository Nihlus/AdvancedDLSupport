//
//  MixedModeTests.cs
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
using AdvancedDLSupport.Tests.Data.Classes;
using Xunit;

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Integration;

public class MixedModeTests : IDisposable
{
    private const string _libraryName = "MixedModeTests";

    private readonly MixedModeClass _mixedModeClass;
    private readonly NativeLibraryBuilder _builder;

    public MixedModeTests()
    {
        _builder = new NativeLibraryBuilder(ImplementationOptions.GenerateDisposalChecks);

        _mixedModeClass = _builder.ActivateClass<MixedModeClass>(_libraryName);
    }

    [Fact]
    public void ThrowsIfClassIsNotAbstract()
    {
        Assert.Throws<ArgumentException>
        (
            () =>
                _builder.ActivateClass<MixedModeClassThatIsNotAbstract>(_libraryName)
        );
    }

    [Fact]
    public void CanActivateClassWithMultipleNativeInterfaces()
    {
        _builder.ActivateClass<MixedModeClassWithMultipleNativeInterfaces>(_libraryName);
    }

    [Fact]
    public void CanActivateClassWithNativeSymbolInInterface()
    {
        var library = _builder.ActivateClass<MixedModeClassWithNativeSymbolInInterface>(_libraryName);
        Assert.Equal(5, library.SubtractWithRemappedName(10, 5));
    }

    [Fact]
    public void CanActivateClassWithNativeSymbolInClass()
    {
        var library = _builder.ActivateClass<MixedModeClassWithNativeSymbolInClass>(_libraryName);
        Assert.Equal(5, library.SubtractWithRemappedName(10, 5));
    }

    [Fact]
    public void CanActivateClassWithNativeSymbolInBothInterfaceAndClass()
    {
        var library = _builder.ActivateClass<MixedModeClassWithNativeSymbolInInterfaceAndClass>(_libraryName);
        Assert.Equal(5, library.SubtractWithRemappedName(10, 5));
    }

    [Fact]
    public void CanActivateClassWithInheritedNativeInterfaces()
    {
        _builder.ActivateClass<MixedModeClassWithInheritedInterface>(_libraryName);
    }

    [Fact]
    public void CanOverrideNativeFunctionWithManagedImplementationOnClass()
    {
        var result = _mixedModeClass.Subtract(10, 5);

        Assert.Equal(5, result);
        Assert.True(_mixedModeClass.RanManagedSubtract);
    }

    [Fact]
    public void CanCallPurelyManagedFunctionOnClass()
    {
        var result = _mixedModeClass.ManagedAdd(5, 5);

        Assert.Equal(10, result);
    }

    [Fact]
    public void CanCallNativeFunctionOnClass()
    {
        var result = _mixedModeClass.Multiply(5, 5);

        Assert.Equal(25, result);
    }

    [Fact]
    public void CanUseNativePropertyOnClass()
    {
        var originalValue = _mixedModeClass.NativeProperty;
        Assert.Equal(0, originalValue);

        var newValue = 16;
        _mixedModeClass.NativeProperty = newValue;
        Assert.Equal(newValue, _mixedModeClass.NativeProperty);
    }

    [Fact]
    public void CanOverrideNativePropertyWithManagedImplementationOnClass()
    {
        Assert.Equal(32, _mixedModeClass.OtherNativeProperty);

        _mixedModeClass.OtherNativeProperty = 255;
        Assert.True(_mixedModeClass.RanManagedSetter);
    }

    public void Dispose()
    {
        _mixedModeClass.Dispose();
    }
}
