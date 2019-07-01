//
//  IntrospectiveMemberBase.cs
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
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Reflection
{
    /// <summary>
    /// Abstract base wrapper class for introspective member informations.
    /// </summary>
    /// <typeparam name="TMemberInfo">The member info to wrap.</typeparam>
    [PublicAPI]
    public abstract class IntrospectiveMemberBase<TMemberInfo> : MemberInfo, IIntrospectiveMember
        where TMemberInfo : MemberInfo
    {
        /// <inheritdoc cref="MemberInfo.Name" />
        [PublicAPI]
        public override string Name { get; }

        /// <summary>
        /// Gets the custom attributes applies to this member.
        /// </summary>
        [PublicAPI, NotNull, ItemNotNull]
        public override IEnumerable<CustomAttributeData> CustomAttributes { get; }

        /// <inheritdoc />
        [PublicAPI, NotNull]
        public override Type DeclaringType { get; }

        /// <inheritdoc />
        [PublicAPI]
        public override MemberTypes MemberType { get; }

        /// <inheritdoc />
        [PublicAPI]
        public override Type ReflectedType { get; }

        /// <summary>
        /// Gets the wrapped member.
        /// </summary>
        [PublicAPI, NotNull]
        protected TMemberInfo Member { get; }

        /// <summary>
        /// Gets the type that the member gets native metadata from (typically an interface or a mixed-mode class).
        /// </summary>
        [PublicAPI, NotNull]
        public Type MetadataType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectiveMemberBase{TMemberInfo}"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info object to wrap.</param>
        /// <param name="metadataType">The type that the member gets native metadata from.</param>
        [PublicAPI]
        protected IntrospectiveMemberBase([NotNull] TMemberInfo memberInfo, [NotNull] Type metadataType)
            : this(memberInfo, metadataType, memberInfo.CustomAttributes)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectiveMemberBase{TMemberInfo}"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info to wrap.</param>
        /// <param name="metadataType">The type that the member gets native metadata from.</param>
        /// <param name="customAttributes">The custom attributes associated with the member.</param>
        [PublicAPI]
        protected IntrospectiveMemberBase
        (
            [NotNull] TMemberInfo memberInfo,
            [NotNull] Type metadataType,
            [CanBeNull, ItemNotNull] IEnumerable<CustomAttributeData> customAttributes = default
        )
        {
            Member = memberInfo;
            Name = memberInfo.Name;
            DeclaringType = memberInfo.DeclaringType;
            MemberType = memberInfo.MemberType;
            ReflectedType = memberInfo.ReflectedType;

            MetadataType = metadataType;

            CustomAttributes = customAttributes?.ToList() ?? new List<CustomAttributeData>();
        }

        /// <summary>
        /// Determines if the member has the same native entrypoint as another given member.
        /// </summary>
        /// <param name="other">The other member.</param>
        /// <returns>true if the members have the same native entrypoint; otherwise, false.</returns>
        [PublicAPI, Pure]
        public bool HasSameNativeEntrypointAs([NotNull] IntrospectiveMethodInfo other)
        {
            return GetFullNativeEntrypoint() == other.GetFullNativeEntrypoint();
        }

        /// <inheritdoc />
        public string GetFullNativeEntrypoint()
        {
            var transformer = SymbolTransformer.Default;
            return transformer.GetTransformedSymbol(MetadataType, this);
        }

        /// <inheritdoc />
        public string GetFullUnmangledNativeEntrypoint()
        {
            var transformer = SymbolTransformer.Default;
            return transformer.GetTransformedUnmangledSymbol(MetadataType, this);
        }

        /// <inheritdoc />
        public string GetNativeEntrypoint()
        {
            var metadataAttribute = GetCustomAttribute<NativeSymbolAttribute>()
                                    ?? new NativeSymbolAttribute(Name);

            return metadataAttribute.Entrypoint;
        }

        /// <inheritdoc />
        public CallingConvention GetNativeCallingConvention()
        {
            var metadataAttribute = GetCustomAttribute<NativeSymbolAttribute>();

            if (metadataAttribute == null || metadataAttribute.CallingConvention == default)
            {
                NativeSymbolsAttribute attribute = MetadataType.GetCustomAttribute<NativeSymbolsAttribute>();

                return attribute == null ? default : attribute.DefaultCallingConvention;
            }

            return metadataAttribute.CallingConvention;
        }

        /// <summary>
        /// Gets the wrapped member information. No guarantees can be made about its introspective capabilities.
        /// </summary>
        /// <returns>The wrapped method.</returns>
        [PublicAPI, NotNull]
        public TMemberInfo GetWrappedMember() => Member;

        /// <inheritdoc />
        [PublicAPI]
        public override object[] GetCustomAttributes(bool inherit)
        {
            // TODO: Wrap properly
            return Member.GetCustomAttributes(inherit);
        }

        /// <inheritdoc />
        [PublicAPI]
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            // TODO: Wrap properly
            return Member.GetCustomAttributes(attributeType, inherit);
        }

        /// <inheritdoc />
        [PublicAPI]
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            // TODO: Wrap properly
            return Member.IsDefined(attributeType, inherit);
        }

        /// <inheritdoc />
        [PublicAPI]
        public TAttribute GetCustomAttribute<TAttribute>() where TAttribute : Attribute
        {
            var matchingData = CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(TAttribute));

            if (matchingData is null)
            {
                return null;
            }

            var type = matchingData.AttributeType;
            var instance = Activator.CreateInstance(type, matchingData.ConstructorArguments.Select(a => a.Value).ToArray());

            foreach (var namedArgument in matchingData.NamedArguments ?? new List<CustomAttributeNamedArgument>())
            {
                if (namedArgument.MemberInfo is FieldInfo field)
                {
                    field.SetValue(instance, namedArgument.TypedValue.Value);
                }

                if (namedArgument.MemberInfo is PropertyInfo property)
                {
                    property.SetValue(instance, namedArgument.TypedValue.Value);
                }
            }

            return instance as TAttribute;
        }

        /// <summary>
        /// Explicitly casts to and accesses the wrapped member.
        /// </summary>
        /// <param name="introspectiveInfo">The introspective info.</param>
        /// <returns>The wrapped member.</returns>
        [PublicAPI, CanBeNull]
        public static explicit operator TMemberInfo([CanBeNull] IntrospectiveMemberBase<TMemberInfo> introspectiveInfo)
        {
            return introspectiveInfo?.Member;
        }
    }
}
