using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AdvancedDLSupport
{
    public struct MyStruct
    {
        public int A;
    }
    public interface IExample
    {
        int DoMath(ref MyStruct struc);
    }
    class Program
    {
        static void Main(string[] args)
        {
            IExample wrapper;
            try
            {
                wrapper = DLSupport.ResolveAndActivateInterface<IExample>("./libDemo.so");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw e;
            }
            Console.WriteLine("Wrapper loaded!");
            var field = wrapper.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField).First(I => I.Name == "DoMath_dtm");
            var val = field.GetValue(wrapper);
            if (val == null)
            {
                Console.WriteLine("DoMath_dtm is null!");
            }
            else
            {
                Console.WriteLine("DoMath_dtm is not null!");
            }
            var struc = new MyStruct{A = 22};
            Console.WriteLine("{0}", wrapper.DoMath(ref struc));
        }
    }
}
