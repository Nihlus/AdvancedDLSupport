using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AdvancedDLSupport.Extensions
{
    /// <summary>
    /// Extension methods for the <see cref="MethodBuilder"/> class.
    /// </summary>
    internal static class MethodBuilderExtensions
    {
        /// <summary>
        /// Holds blacklisted attributes which will not be copied to their respective types.
        /// </summary>
        private static readonly IReadOnlyDictionary<Type, IReadOnlyList<Type>> AttributeBlacklist = new Dictionary<Type, IReadOnlyList<Type>>
        {
            { typeof(IntPtr), new[] { typeof(MarshalAsAttribute) } }
        };

        /// <summary>
        /// Copies all custom attributes from the given <see cref="MethodInfo"/> instance. This method will redefine the
        /// return value and method parameters in order to apply the required custom attributes.
        /// </summary>
        /// <param name="this">The builder to copy the attributes to.</param>
        /// <param name="source">The method to copy the attributes from.</param>
        /// <param name="targetReturnParameterType">The return type of the target method.</param>
        /// <param name="targetParameterTypes">The parameter types of the target method.</param>
        [PublicAPI]
        public static void CopyCustomAttributesFrom
        (
            [NotNull] this MethodBuilder @this,
            [NotNull] MethodInfo source,
            [NotNull] Type targetReturnParameterType,
            [NotNull, ItemNotNull] IReadOnlyList<Type> targetParameterTypes
        )
        {
            // Pass through all applied attributes
            var returnValueBuilder = @this.DefineParameter(0, source.ReturnParameter.Attributes, null);
            foreach (var attribute in CustomAttributeData.GetCustomAttributes(source.ReturnParameter))
            {
                if (AttributeBlacklist.ContainsKey(targetReturnParameterType) && AttributeBlacklist[targetReturnParameterType].Contains(attribute.AttributeType))
                {
                    continue;
                }

                returnValueBuilder.SetCustomAttribute(attribute.GetAttributeBuilder());
            }

            var methodParameters = source.GetParameters();
            if (methodParameters.Any())
            {
                for (var i = 1; i <= methodParameters.Length; ++i)
                {
                    var targetParameterType = targetParameterTypes[i - 1];
                    var methodParameter = methodParameters[i - 1];

                    var parameterBuilder = @this.DefineParameter(i, methodParameter.Attributes, methodParameter.Name);
                    foreach (var attribute in CustomAttributeData.GetCustomAttributes(methodParameter))
                    {
                        if (AttributeBlacklist.ContainsKey(targetParameterType) && AttributeBlacklist[targetParameterType].Contains(attribute.AttributeType))
                        {
                            continue;
                        }

                        parameterBuilder.SetCustomAttribute(attribute.GetAttributeBuilder());
                    }
                }
            }

            foreach (var attribute in CustomAttributeData.GetCustomAttributes(source))
            {
                @this.SetCustomAttribute(attribute.GetAttributeBuilder());
            }
        }

        /// <summary>
        /// Copies all custom attributes from the given <see cref="TransientMethodInfo"/> instance. This method will redefine the
        /// return value and method parameters in order to apply the required custom attributes.
        /// </summary>
        /// <param name="this">The builder to copy the attributes to.</param>
        /// <param name="source">The method to copy the attributes from.</param>
        /// <param name="targetReturnParameterType">The return type of the target method.</param>
        /// <param name="targetParameterTypes">The parameter types of the target method.</param>
        public static void CopyCustomAttributesFrom
        (
            [NotNull] this MethodBuilder @this,
            [NotNull] TransientMethodInfo source,
            [NotNull] Type targetReturnParameterType,
            [NotNull, ItemNotNull] IReadOnlyList<Type> targetParameterTypes
        )
        {
            // Pass through all applied attributes
            var returnValueBuilder = @this.DefineParameter(0, source.ReturnParameterAttributes, null);
            foreach (var attribute in source.CustomReturnParameterAttributes)
            {
                if (AttributeBlacklist.ContainsKey(targetReturnParameterType) && AttributeBlacklist[targetReturnParameterType].Contains(attribute.AttributeType))
                {
                    continue;
                }

                returnValueBuilder.SetCustomAttribute(attribute.GetAttributeBuilder());
            }

            if (source.ParameterTypes.Any())
            {
                for (var i = 1; i <= source.ParameterTypes.Count; ++i)
                {
                    var targetParameterType = targetParameterTypes[i - 1];
                    var methodParameterCustomAttributes = source.CustomParameterAttributes[i - 1];
                    var methodParameterAttributes = source.ParameterAttributes[i - 1];
                    var methodParameterName = source.ParameterNames[i - 1];

                    var parameterBuilder = @this.DefineParameter(i, methodParameterAttributes, methodParameterName);
                    foreach (var attribute in methodParameterCustomAttributes)
                    {
                        if (AttributeBlacklist.ContainsKey(targetParameterType) && AttributeBlacklist[targetParameterType].Contains(attribute.AttributeType))
                        {
                            continue;
                        }

                        parameterBuilder.SetCustomAttribute(attribute.GetAttributeBuilder());
                    }
                }
            }

            foreach (var attribute in source.CustomAttributes)
            {
                @this.SetCustomAttribute(attribute.GetAttributeBuilder());
            }
        }
    }
}
