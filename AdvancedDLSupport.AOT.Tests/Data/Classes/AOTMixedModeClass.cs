using System;
using AdvancedDLSupport.AOT.Tests.Data.Interfaces;

namespace AdvancedDLSupport.AOT.Tests.Data.Classes
{
    public abstract class AOTMixedModeClass : NativeLibraryBase, IAOTLibrary
    {
        public AOTMixedModeClass(string path, Type interfaceType, ImplementationOptions options, TypeTransformerRepository transformerRepository)
            : base(path, interfaceType, options, transformerRepository)
        {
        }

        public abstract int Multiply(int a, int b);

        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}
