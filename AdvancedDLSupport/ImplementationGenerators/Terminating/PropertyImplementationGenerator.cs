//
//  PropertyImplementationGenerator.cs
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
using System.Runtime.InteropServices;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;
using StrictEmit;
using static AdvancedDLSupport.ImplementationGenerators.GeneratorComplexity;
using static AdvancedDLSupport.ImplementationOptions;
using static System.Reflection.MethodAttributes;

namespace AdvancedDLSupport.ImplementationGenerators;

/// <summary>
/// Generates implementations for properties.
/// </summary>
internal sealed class PropertyImplementationGenerator : ImplementationGeneratorBase<IntrospectivePropertyInfo>
{
    /// <inheritdoc/>
    public override GeneratorComplexity Complexity => Terminating;

    private const MethodAttributes PropertyMethodAttributes =
        PrivateScope |
        Public |
        Virtual |
        HideBySig |
        VtableLayoutMask |
        SpecialName;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyImplementationGenerator"/> class.
    /// </summary>
    /// <param name="targetModule">The module in which the property implementation should be generated.</param>
    /// <param name="targetType">The type in which the property implementation should be generated.</param>
    /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
    /// <param name="options">The configuration object to use.</param>
    public PropertyImplementationGenerator
    (
        ModuleBuilder targetModule,
        TypeBuilder targetType,
        ILGenerator targetTypeConstructorIL,
        ImplementationOptions options
    )
        : base(targetModule, targetType, targetTypeConstructorIL, options)
    {
    }

    /// <inheritdoc/>
    public override bool IsApplicable(IntrospectivePropertyInfo member)
    {
        return true;
    }

    /// <inheritdoc />
    public override IEnumerable<PipelineWorkUnit<IntrospectivePropertyInfo>> GenerateImplementation(PipelineWorkUnit<IntrospectivePropertyInfo> workUnit)
    {
        var property = workUnit.Definition;

        var propertyBuilder = TargetType.DefineProperty
        (
            property.Name,
            PropertyAttributes.None,
            CallingConventions.HasThis,
            property.PropertyType,
            property.IndexParameterTypes.ToArray()
        );

        // Note, the field is going to have to be a pointer, because it is pointing to global variable
        var fieldType = Options.HasFlagFast(UseLazyBinding) ? typeof(Lazy<IntPtr>) : typeof(IntPtr);
        var propertyFieldBuilder = TargetType.DefineField
        (
            $"{workUnit.GetUniqueBaseMemberName()}_backing",
            fieldType,
            FieldAttributes.Private
        );

        if (property.CanRead)
        {
            GeneratePropertyGetter(property, propertyFieldBuilder, propertyBuilder);
        }

        if (property.CanWrite)
        {
            GeneratePropertySetter(property, propertyFieldBuilder, propertyBuilder);
        }

        AugmentHostingTypeConstructor(workUnit.SymbolName, propertyFieldBuilder);

        yield break;
    }

    private void AugmentHostingTypeConstructor
    (
        string symbolName,
        FieldInfo propertyFieldBuilder
    )
    {
        var loadSymbolMethod = typeof(NativeLibraryBase).GetMethod
        (
            nameof(NativeLibraryBase.LoadSymbol),
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        TargetTypeConstructorIL.EmitLoadArgument(0);
        TargetTypeConstructorIL.EmitLoadArgument(0);

        if (Options.HasFlagFast(UseLazyBinding))
        {
            var lambdaBuilder = GenerateSymbolLoadingLambda(symbolName);
            GenerateLazyLoadedObject(lambdaBuilder, typeof(IntPtr));
        }
        else
        {
            TargetTypeConstructorIL.EmitConstantString(symbolName);
            TargetTypeConstructorIL.EmitCallDirect(loadSymbolMethod);
        }

        TargetTypeConstructorIL.EmitSetField(propertyFieldBuilder);
    }

    private void GeneratePropertySetter
    (
        IntrospectivePropertyInfo property,
        FieldInfo propertyFieldBuilder,
        PropertyBuilder propertyBuilder
    )
    {
        var wrappedProperty = property.GetWrappedMember();
        var actualSetMethod = wrappedProperty.GetSetMethod();
        var setterMethod = TargetType.DefineMethod
        (
            actualSetMethod.Name,
            PropertyMethodAttributes,
            actualSetMethod.CallingConvention,
            typeof(void),
            actualSetMethod.GetParameters().Select(p => p.ParameterType).ToArray()
        );

        MethodInfo underlyingMethod;
        if (property.PropertyType.IsPointer)
        {
            underlyingMethod = typeof(Marshal).GetMethods().First
            (
                m =>
                    m.Name == nameof(Marshal.WriteIntPtr) &&
                    m.GetParameters().Length == 3
            );
        }
        else if (property.PropertyType.IsValueType)
        {
            underlyingMethod = typeof(Marshal).GetMethods().First
                (
                    m =>
                        m.Name == nameof(Marshal.StructureToPtr) &&
                        m.GetParameters().Length == 3 &&
                        m.IsGenericMethod
                )
                .MakeGenericMethod(property.PropertyType);
        }
        else
        {
            throw new NotSupportedException
            (
                $"The type \"{property.PropertyType.FullName}\" is not supported. Only value types or pointers are supported."
            );
        }

        var setterIL = setterMethod.GetILGenerator();

        if (Options.HasFlagFast(GenerateDisposalChecks))
        {
            EmitDisposalCheck(setterIL);
        }

        if (property.PropertyType.IsPointer)
        {
            var explicitConvertToIntPtrFunc = typeof(IntPtr).GetMethods().First
            (
                m =>
                    m.Name == "op_Explicit"
            );

            GenerateSymbolPush(setterIL, propertyFieldBuilder);
            setterIL.EmitConstantInt(0);

            setterIL.EmitLoadArgument(1);
            setterIL.EmitCallDirect(explicitConvertToIntPtrFunc);
        }
        else
        {
            setterIL.EmitLoadArgument(1);
            GenerateSymbolPush(setterIL, propertyFieldBuilder);
            setterIL.EmitConstantInt(0);
        }

        setterIL.EmitCallDirect(underlyingMethod);
        setterIL.EmitReturn();

        propertyBuilder.SetSetMethod(setterMethod);
        TargetType.DefineMethodOverride(setterMethod, actualSetMethod);
    }

    private void GeneratePropertyGetter
    (
        IntrospectivePropertyInfo property,
        FieldInfo propertyFieldBuilder,
        PropertyBuilder propertyBuilder
    )
    {
        var wrappedProperty = property.GetWrappedMember();
        var actualGetMethod = wrappedProperty.GetGetMethod();
        var getterMethod = TargetType.DefineMethod
        (
            actualGetMethod.Name,
            PropertyMethodAttributes,
            actualGetMethod.CallingConvention,
            actualGetMethod.ReturnType,
            Type.EmptyTypes
        );

        MethodInfo underlyingMethod;
        if (property.PropertyType.IsPointer)
        {
            underlyingMethod = typeof(Marshal).GetMethods().First
            (
                m =>
                    m.Name == nameof(Marshal.ReadIntPtr) &&
                    m.GetParameters().Length == 1
            );
        }
        else if (property.PropertyType.IsValueType)
        {
            underlyingMethod = typeof(Marshal).GetMethods().First
                (
                    m =>
                        m.Name == nameof(Marshal.PtrToStructure) &&
                        m.GetParameters().Length == 1 &&
                        m.IsGenericMethod
                )
                .MakeGenericMethod(property.PropertyType);
        }
        else
        {
            throw new NotSupportedException
            (
                $"The type \"{property.PropertyType.FullName}\" is not supported. Only value types or pointers are supported."
            );
        }

        var getterIL = getterMethod.GetILGenerator();

        if (Options.HasFlagFast(GenerateDisposalChecks))
        {
            EmitDisposalCheck(getterIL);
        }

        GenerateSymbolPush(getterIL, propertyFieldBuilder);

        getterIL.EmitCallDirect(underlyingMethod);
        getterIL.EmitReturn();

        propertyBuilder.SetGetMethod(getterMethod);
        TargetType.DefineMethodOverride(getterMethod, actualGetMethod);
    }

    /// <summary>
    /// Emits a call to <see cref="NativeLibraryBase.ThrowIfDisposed"/>.
    /// </summary>
    /// <param name="il">The IL generator.</param>
    [PublicAPI]
    private void EmitDisposalCheck(ILGenerator il)
    {
        var throwMethod = typeof(NativeLibraryBase).GetMethod("ThrowIfDisposed", BindingFlags.NonPublic | BindingFlags.Instance);

        il.EmitLoadArgument(0);
        il.EmitCallDirect(throwMethod);
    }
}
