using System;

#pragma warning disable SA1600, CS1591 // Elements should be documented

namespace AdvancedDLSupport.Example
{
    internal class Program
    {
        private static unsafe void Main()
        {
            var wrapper = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IExample>
            (
                "./libDemo.so"
            );
            wrapper.InitializeMyStructure();
            *wrapper.MyStructure = new MyStruct(24);
            Console.WriteLine(wrapper.MyStructure->A);
            wrapper.MyStructure->A = 25;

            Console.WriteLine(wrapper.MyStructure->A);
        }
    }
}
