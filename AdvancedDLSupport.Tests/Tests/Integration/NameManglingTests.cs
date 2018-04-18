using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class NameManglingTests : LibraryTestBase<INameManglingTests>
    {
        private const string LibraryName = "NameManglingTests";

        public NameManglingTests() : base(LibraryName)
        {
        }

        [Fact]
        public void CanMangleSimpleFunction()
        {
            var a = 5;
            var b = 15;

            var expected = a * b;

            var actual = Library.Multiply(a, b);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanMangleFunctionWithStructByVal()
        {
            var a = 5;
            var b = 15;

            var expected = a * b;
            var value = new TestStruct { A = a, B = b };

            var actual = Library.MultiplyStructByVal(value);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanMangleFunctionWithStructByRef()
        {
            var a = 5;
            var b = 15;

            var expected = a * b;
            var value = new TestStruct { A = a, B = b };

            var actual = Library.MultiplyStructByRef(ref value);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public unsafe void CanMangleFunctionWithStructByPtr()
        {
            var a = 5;
            var b = 15;

            var expected = a * b;
            var value = new TestStruct { A = a, B = b };

            var actual = Library.MultiplyStructByPtr(&value);

            Assert.Equal(expected, actual);
        }
    }
}