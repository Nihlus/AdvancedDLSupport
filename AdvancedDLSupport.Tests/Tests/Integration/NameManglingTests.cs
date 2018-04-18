using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using FsCheck.Xunit;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class NameManglingTests : LibraryTestBase<INameManglingTests>
    {
        private const string LibraryName = "NameManglingTests";

        public NameManglingTests() : base(LibraryName)
        {
        }

        [Property]
        public void CanMangleSimpleFunction(int a, int b)
        {
            var expected = a * b;

            var actual = Library.Multiply(a, b);

            Assert.Equal(expected, actual);
        }

        [Property]
        public void CanMangleFunctionWithStructByVal(int a, int b)
        {
            var expected = a * b;
            var value = new TestStruct { A = a, B = b };

            var actual = Library.MultiplyStructByVal(value);

            Assert.Equal(expected, actual);
        }

        [Property]
        public void CanMangleFunctionWithStructByRef(int a, int b)
        {
            var expected = a * b;
            var value = new TestStruct { A = a, B = b };

            var actual = Library.MultiplyStructByRef(ref value);

            Assert.Equal(expected, actual);
        }

        [Property]
        public unsafe void CanMangleFunctionWithStructByPtr(int a, int b)
        {
            var expected = a * b;
            var value = new TestStruct { A = a, B = b };

            var actual = Library.MultiplyStructByPtr(&value);

            Assert.Equal(expected, actual);
        }
    }
}