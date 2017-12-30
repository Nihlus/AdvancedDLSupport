using System;

namespace AdvancedDLSupport
{
    /// <summary>
    /// Tags a constructor as the anonymous constructor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    internal class AnonymousConstructorAttribute : Attribute
    {
    }
}
