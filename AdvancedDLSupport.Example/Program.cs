using System;

#pragma warning disable SA1600, CS1591 // Elements should be documented

namespace AdvancedDLSupport.Example
{
    internal class Program
    {
        private static unsafe void Main()
        {
            IExample wrapper;
            wrapper = AnonymousImplementationBuilder.ResolveAndActivateInterface<IExample>
            (
                "./libDemo.so"
            );

            var mystruc = default(MyStruct);
            mystruc.A = 25;
            wrapper.MyStructure[0] = mystruc;

            Console.WriteLine(wrapper.MyStructure[0].A);
        }
    }
}
