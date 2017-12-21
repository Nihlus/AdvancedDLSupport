using System.Reflection;
using System.Reflection.Emit;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Base class for implementation generators.
    /// </summary>
    /// <typeparam name="T">The type of member to generate the implementation for.</typeparam>
    public abstract class ImplementationGeneratorBase<T> : IImplementationGenerator<T> where T : MemberInfo
    {
        /// <summary>
        /// Gets the module in which the implementation should be generated.
        /// </summary>
        protected ModuleBuilder TargetModule { get; }

        /// <summary>
        /// Gets the type in which the implementation should be generated.
        /// </summary>
        protected TypeBuilder TargetType { get; }

        /// <summary>
        /// Gets the IL generator for the constructor of the type in which the implementation should be generated.
        /// </summary>
        protected ILGenerator TargetTypeConstructorIL { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImplementationGeneratorBase{T}"/> class.
        /// </summary>
        /// <param name="targetModule">The module where the implementation should be generated.</param>
        /// <param name="targetType">The type in which the implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        public ImplementationGeneratorBase(ModuleBuilder targetModule, TypeBuilder targetType, ILGenerator targetTypeConstructorIL)
        {
            TargetModule = targetModule;
            TargetType = targetType;
            TargetTypeConstructorIL = targetTypeConstructorIL;
        }

        /// <inheritdoc />
        public abstract void GenerateImplementation(T member);
    }
}
