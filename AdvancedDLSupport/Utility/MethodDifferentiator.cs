using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Helper class for differentating methods with complex types, that is, involving types which need to be
    /// lowered before generating the native signature.
    /// </summary>
    [PublicAPI]
    public static class MethodDifferentiator
    {
        /// <summary>
        /// Determines whether or not the given method is complex,
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <returns>true if the method is complex; otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool IsComplexMethod([NotNull] MethodInfo method)
        {
            return HasComplexParameters(method) || HasComplexReturnValue(method);
        }

        /// <summary>
        /// Determines whether or not the method has complex parameters.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>true if the method has complex parameters; otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool HasComplexParameters([NotNull] MethodBase method)
        {
            var parameters = method.GetParameters();
            return parameters.Any
            (
                p =>
                    IsComplexType(p.ParameterType)
            );
        }

        /// <summary>
        /// Determines whether or not the given type is a complex type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>true if the type is complex; otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool IsComplexType([NotNull] Type type)
        {
            return
                type == typeof(string) ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// Determines whether or not the given method has a complex return type.
        /// </summary>
        /// <param name="methodInfo">The method.</param>
        /// <returns>true if the method has a complex return value; otherwise, false.</returns>
        [PublicAPI, Pure]
        public static bool HasComplexReturnValue([NotNull] MethodInfo methodInfo)
        {
            return IsComplexType(methodInfo.ReturnType);
        }
    }
}
