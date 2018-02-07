//
//  PropertyImplementationGenerator.cs
//
//  Copyright (c) 2018 Firwood Software
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;
using Mono.DllMap.Extensions;

using static AdvancedDLSupport.ImplementationOptions;
using static System.Reflection.MethodAttributes;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates implementations for properties.
    /// </summary>
    internal class PropertyImplementationGenerator : ImplementationGeneratorBase<IntrospectivePropertyInfo>
    {
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
            [NotNull] ModuleBuilder targetModule,
            [NotNull] TypeBuilder targetType,
            [NotNull] ILGenerator targetTypeConstructorIL,
            ImplementationOptions options
        )
            : base(targetModule, targetType, targetTypeConstructorIL, options)
        {
        }

        /// <inheritdoc />
        protected override void GenerateImplementation(IntrospectivePropertyInfo property, string symbolName, string uniqueMemberIdentifier)
        {
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
                uniqueMemberIdentifier,
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

            PropertyInitializationInConstructor(symbolName, propertyFieldBuilder); // This is ok for all 3 types of properties.
        }

        private void PropertyInitializationInConstructor
        (
            [NotNull] string symbolName,
            [NotNull] FieldInfo propertyFieldBuilder
        )
        {
            var loadSymbolMethod = typeof(AnonymousImplementationBase).GetMethod
            (
                "LoadSymbol",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            TargetTypeConstructorIL.Emit(OpCodes.Ldarg_0);
            TargetTypeConstructorIL.Emit(OpCodes.Ldarg_0);

            if (Options.HasFlagFast(UseLazyBinding))
            {
                var lambdaBuilder = GenerateSymbolLoadingLambda(symbolName);
                GenerateLazyLoadedField(lambdaBuilder, typeof(IntPtr));
            }
            else
            {
                TargetTypeConstructorIL.Emit(OpCodes.Ldstr, symbolName);
                TargetTypeConstructorIL.EmitCall(OpCodes.Call, loadSymbolMethod, null);
            }

            TargetTypeConstructorIL.Emit(OpCodes.Stfld, propertyFieldBuilder);
        }

        private void GeneratePropertySetter
        (
            [NotNull] IntrospectivePropertyInfo property,
            [NotNull] FieldInfo propertyFieldBuilder,
            [NotNull] PropertyBuilder propertyBuilder
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
                throw new NotSupportedException(
                    string.Format
                    (
                        "{0} Type is not supported. Only ValueType property or Pointer Property is supported.",
                        property.PropertyType.FullName
                    )
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

                GenerateSymbolPush(setterIL, propertyFieldBuilder); // Push Symbol address to stack
                setterIL.Emit(OpCodes.Ldc_I4, 0);                   // Push 0 offset to stack

                setterIL.Emit(OpCodes.Ldarg_1);                     // Push value to stack
                setterIL.EmitCall(OpCodes.Call, explicitConvertToIntPtrFunc, null); // Explicit Convert Pointer to IntPtr object
            }
            else
            {
                setterIL.Emit(OpCodes.Ldarg_1);
                GenerateSymbolPush(setterIL, propertyFieldBuilder);
                setterIL.Emit(OpCodes.Ldc_I4, 0); // false for deleting structure that is already stored in pointer
            }

            setterIL.EmitCall
            (
                OpCodes.Call,
                underlyingMethod,
                null
            );

            setterIL.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setterMethod);
            TargetType.DefineMethodOverride(setterMethod, actualSetMethod);
        }

        private void GeneratePropertyGetter
        (
            [NotNull] IntrospectivePropertyInfo property,
            [NotNull] FieldInfo propertyFieldBuilder,
            [NotNull] PropertyBuilder propertyBuilder
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
                throw new NotSupportedException(
                    string.Format
                    (
                        "{0} Type is not supported. Only ValueType property or Pointer Property is supported.",
                        property.PropertyType.FullName
                    )
                );
            }

            var getterIL = getterMethod.GetILGenerator();

            if (Options.HasFlagFast(GenerateDisposalChecks))
            {
                EmitDisposalCheck(getterIL);
            }

            GenerateSymbolPush(getterIL, propertyFieldBuilder);

            getterIL.EmitCall
            (
                OpCodes.Call,
                underlyingMethod,
                null
            );

            getterIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getterMethod);
            TargetType.DefineMethodOverride(getterMethod, actualGetMethod);
        }
    }
}
