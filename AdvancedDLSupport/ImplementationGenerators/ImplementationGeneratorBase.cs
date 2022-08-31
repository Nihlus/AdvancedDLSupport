//
//  ImplementationGeneratorBase.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AdvancedDLSupport.Pipeline;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;
using StrictEmit;
using static AdvancedDLSupport.ImplementationOptions;
using static System.Reflection.MethodAttributes;

namespace AdvancedDLSupport.ImplementationGenerators;

/// <summary>
/// Base class for implementation generators.
/// </summary>
/// <typeparam name="T">The type of member to generate the implementation for.</typeparam>
[PublicAPI]
public abstract class ImplementationGeneratorBase<T> : IImplementationGenerator<T> where T : MemberInfo
{
    /// <inheritdoc />
    [PublicAPI]
    public ImplementationOptions Options { get; }

    /// <inheritdoc/>
    public abstract GeneratorComplexity Complexity { get; }

    /// <summary>
    /// Gets the module in which the implementation should be generated.
    /// </summary>
    [PublicAPI]
    protected ModuleBuilder TargetModule { get; }

    /// <summary>
    /// Gets the type in which the implementation should be generated.
    /// </summary>
    [PublicAPI]
    protected TypeBuilder TargetType { get; }

    /// <summary>
    /// Gets the IL generator for the constructor of the type in which the implementation should be generated.
    /// </summary>
    [PublicAPI]
    protected ILGenerator TargetTypeConstructorIL { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImplementationGeneratorBase{T}"/> class.
    /// </summary>
    /// <param name="targetModule">The module where the implementation should be generated.</param>
    /// <param name="targetType">The type in which the implementation should be generated.</param>
    /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
    /// <param name="options">The configuration object to use.</param>
    [PublicAPI]
    protected ImplementationGeneratorBase
    (
        ModuleBuilder targetModule,
        TypeBuilder targetType,
        ILGenerator targetTypeConstructorIL,
        ImplementationOptions options
    )
    {
        TargetModule = targetModule;
        TargetType = targetType;
        TargetTypeConstructorIL = targetTypeConstructorIL;
        Options = options;
    }

    /// <inheritdoc/>
    public abstract bool IsApplicable(T member);

    /// <inheritdoc />
    public abstract IEnumerable<PipelineWorkUnit<T>> GenerateImplementation(PipelineWorkUnit<T> workUnit);

    /// <summary>
    /// Generates a lazy loaded field with the specified value factory.
    /// </summary>
    /// <param name="valueFactory">The value factory to use for the lazy loaded field.</param>
    /// <param name="type">The return type of the lazy field.</param>
    [PublicAPI]
    protected void GenerateLazyLoadedObject(MethodBuilder valueFactory, Type type)
    {
        var funcType = typeof(Func<>).MakeGenericType(type);
        var lazyType = typeof(Lazy<>).MakeGenericType(type);

        var funcConstructor = funcType.GetConstructors().First();
        var lazyConstructor = lazyType.GetConstructors().First
        (
            c =>
                c.GetParameters().Any() &&
                c.GetParameters().Length == 1 &&
                c.GetParameters().First().ParameterType == funcType
        );

        // Use the lambda instead of the function directly.
        TargetTypeConstructorIL.EmitLoadFunctionPointer(valueFactory);
        TargetTypeConstructorIL.EmitNewObject(funcConstructor);
        TargetTypeConstructorIL.EmitNewObject(lazyConstructor);
    }

    /// <summary>
    /// Generates the IL required to push the value of the field to the stack, including the case where the field
    /// is lazily loaded.
    /// </summary>
    /// <param name="il">The IL generator.</param>
    /// <param name="symbolField">The field to generate the IL for.</param>
    [PublicAPI]
    protected void GenerateSymbolPush(ILGenerator il, FieldInfo symbolField)
    {
        il.EmitLoadArgument(0);
        il.EmitLoadField(symbolField);
        if (!Options.HasFlagFast(UseLazyBinding))
        {
            return;
        }

        var getMethod = typeof(Lazy<IntPtr>).GetMethod("get_Value", BindingFlags.Instance | BindingFlags.Public);
        il.EmitCallVirtual(getMethod);
    }

    /// <summary>
    /// Generates a lambda method for loading the given symbol.
    /// </summary>
    /// <param name="symbolName">The name of the symbol.</param>
    /// <returns>A method which, when called, will load and return the given symbol.</returns>
    [PublicAPI]
    protected MethodBuilder GenerateSymbolLoadingLambda(string symbolName)
    {
        var uniqueIdentifier = Guid.NewGuid().ToString().Replace('-', '_');

        var loadSymbolMethod = typeof(NativeLibraryBase).GetMethod
        (
            nameof(NativeLibraryBase.LoadSymbol),
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        // Generate lambda loader
        var lambdaBuilder = TargetType.DefineMethod
        (
            $"{symbolName}_{uniqueIdentifier}_lazy",
            Private | HideBySig | Final,
            typeof(IntPtr),
            null
        );

        var lambdaIL = lambdaBuilder.GetILGenerator();

        lambdaIL.EmitLoadArgument(0);
        lambdaIL.EmitConstantString(symbolName);
        lambdaIL.EmitCallDirect(loadSymbolMethod);
        lambdaIL.EmitReturn();

        return lambdaBuilder;
    }

    /// <summary>
    /// Generates a lambda method for loading the given function.
    /// </summary>
    /// <param name="delegateType">The type of delegate to load.</param>
    /// <param name="functionName">The name of the function.</param>
    /// <returns>A method which, when called, will load and return the given function.</returns>
    [PublicAPI]
    protected MethodBuilder GenerateFunctionLoadingLambda(Type delegateType, string functionName)
    {
        var uniqueIdentifier = Guid.NewGuid().ToString().Replace('-', '_');

        var loadFuncMethod = typeof(NativeLibraryBase).GetMethod
        (
            nameof(NativeLibraryBase.LoadFunction),
            BindingFlags.NonPublic | BindingFlags.Instance
        ) ?? throw new MissingMethodException();

        // ReSharper disable once PossibleNullReferenceException
        var loadFunc = loadFuncMethod.MakeGenericMethod(delegateType);

        // Generate lambda loader
        var lambdaBuilder = TargetType.DefineMethod
        (
            $"{delegateType.Name}_{uniqueIdentifier}_lazy",
            Private | HideBySig | Final,
            delegateType,
            null
        );

        var lambdaIL = lambdaBuilder.GetILGenerator();

        lambdaIL.EmitLoadArgument(0);
        lambdaIL.EmitConstantString(functionName);
        lambdaIL.EmitCallDirect(loadFunc);
        lambdaIL.EmitReturn();

        return lambdaBuilder;
    }
}
