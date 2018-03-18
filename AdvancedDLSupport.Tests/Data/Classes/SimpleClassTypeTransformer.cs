using System;
using System.Reflection;

namespace AdvancedDLSupport.Tests.Data.Classes
{
    public class SimpleClassTypeTransformer : ITypeTransformer<IntPtr, SimpleClass>
    {
        public Type LowerType()
        {
            throw new NotImplementedException();
        }

        public Type RaiseType()
        {
            throw new NotImplementedException();
        }

        public SimpleClass LowerValue(IntPtr value, ParameterInfo parameter)
        {
            throw new NotImplementedException();
        }

        public IntPtr RaiseValue(SimpleClass value, ParameterInfo parameter)
        {
            throw new NotImplementedException();
        }
    }
}
