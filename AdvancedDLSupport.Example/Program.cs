using System;
using System.Linq;
using System.Reflection;

#pragma warning disable SA1600, CS1591 // Elements should be documented

namespace AdvancedDLSupport.Example
{
    internal class Program
    {
        private static void Main()
        {
            IExample wrapper;
            wrapper = DLSupportConstructor.ResolveAndActivateInterface<IExample>
            (
                "./libDemo.so"
            );

            var field = wrapper.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField)
            .First
            (
                f => f.Name.StartsWith("DoMath_dtm")
            );

            var val = field.GetValue(wrapper);
            var struc = new MyStruct { A = 22 };
            Console.WriteLine("{0}", wrapper.DoMath(ref struc));
            Console.WriteLine("Premodified Struct: {0}", wrapper.MyStructure.A);
            wrapper.MyStructure = new MyStruct { A = 25 };
            Console.WriteLine("Modified struct: {0}", wrapper.MyStructure.A);
        }
    }
}
