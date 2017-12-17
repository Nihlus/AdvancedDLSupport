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
            try
            {
                wrapper = DLSupportConstructor.ResolveAndActivateInterface<IExample>("./libDemo.so");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            Console.WriteLine("Wrapper loaded!");
            var field = wrapper.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField)
            .First
            (
                f => f.Name == "DoMath_dtm"
            );

            var val = field.GetValue(wrapper);
            if (val == null)
            {
                Console.WriteLine("DoMath_dtm is null!");
            }
            else
            {
                Console.WriteLine("DoMath_dtm is not null!");
            }

            var struc = new MyStruct { A = 22 };
            Console.WriteLine("{0}", wrapper.DoMath(ref struc));
        }
    }
}
