using System.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Interface for classes that generate anonymous implementations for members.
    /// </summary>
    /// <typeparam name="T">The type of member that the class will generate for.</typeparam>
    internal interface IImplementationGenerator<in T> where T : MemberInfo
    {
        /// <summary>
        /// Gets the implementation configuration object to use.
        /// </summary>
        ImplementationConfiguration Configuration { get; }

        /// <summary>
        /// Generates the implementation for the given member.
        /// </summary>
        /// <param name="member">The member to generate the implementation for.</param>
        void GenerateImplementation([NotNull] T member);
    }
}
