//
//  IntrospectiveMemberBase.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdvancedDLSupport.Reflection
{
    /// <summary>
    /// Abstract base wrapper class for introspective member informations.
    /// </summary>
    /// <typeparam name="TMemberInfo">The member info to wrap.</typeparam>
    public abstract class IntrospectiveMemberBase<TMemberInfo> : MemberInfo, IIntrospectiveMember
        where TMemberInfo : MemberInfo
    {
        /// <inheritdoc />
        public override string Name { get; }

        /// <summary>
        /// Gets the custom attributes applies to this member.
        /// </summary>
        public override IEnumerable<CustomAttributeData> CustomAttributes { get; }

        /// <inheritdoc />
        public override Type DeclaringType { get; }

        /// <inheritdoc />
        public override MemberTypes MemberType { get; }

        /// <inheritdoc />
        public override Type ReflectedType { get; }

        /// <summary>
        /// Gets the wrapped member.
        /// </summary>
        protected TMemberInfo Member { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectiveMemberBase{TMemberInfo}"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info object to wrap.</param>
        public IntrospectiveMemberBase(TMemberInfo memberInfo)
        {
            Member = memberInfo;

            Name = memberInfo.Name;
            DeclaringType = memberInfo.DeclaringType;
            MemberType = memberInfo.MemberType;
            ReflectedType = memberInfo.ReflectedType;

            CustomAttributes = memberInfo.CustomAttributes;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectiveMemberBase{TMemberInfo}"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info to wrap.</param>
        /// <param name="customAttributes">The custom attributes associated with the member.</param>
        public IntrospectiveMemberBase
        (
            TMemberInfo memberInfo,
            IEnumerable<CustomAttributeData> customAttributes = default
        )
        {
            Member = memberInfo;
            Name = memberInfo.Name;
            DeclaringType = memberInfo.DeclaringType;
            MemberType = memberInfo.MemberType;
            ReflectedType = memberInfo.ReflectedType;

            CustomAttributes = customAttributes ?? new List<CustomAttributeData>();
        }

        /// <summary>
        /// Gets the wrapped member information. No guarantees can be made about its introspective capabilities.
        /// </summary>
        /// <returns>The wrapped method.</returns>
        public TMemberInfo GetWrappedMember() => Member;

        /// <inheritdoc />
        public override object[] GetCustomAttributes(bool inherit)
        {
            // TODO: Wrap properly
            return Member.GetCustomAttributes(inherit);
        }

        /// <inheritdoc />
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            // TODO: Wrap properly
            return Member.GetCustomAttributes(attributeType, inherit);
        }

        /// <inheritdoc />
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            // TODO: Wrap properly
            return Member.IsDefined(attributeType, inherit);
        }

        /// <inheritdoc />
        public TAttribute GetCustomAttribute<TAttribute>() where TAttribute : Attribute
        {
            var matchingData = CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(TAttribute));

            if (matchingData is null)
            {
                return null;
            }

            var instance = Activator.CreateInstance(matchingData.AttributeType, matchingData.ConstructorArguments.Select(a => a.Value).ToArray());
            return instance as TAttribute;
        }

        /// <summary>
        /// Explicitly casts to and accesses the wrapped member.
        /// </summary>
        /// <param name="introspectiveInfo">The introspective info.</param>
        /// <returns>The wrapped member.</returns>
        public static explicit operator TMemberInfo(IntrospectiveMemberBase<TMemberInfo> introspectiveInfo)
        {
            return introspectiveInfo.Member;
        }
    }
}
