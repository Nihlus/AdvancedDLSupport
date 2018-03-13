using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using FsCheck.Xunit;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class FunctionIntegrationTests : LibraryTestBase<IFunctionLibrary>
    {
        private const string LibraryName = "FunctionTests";

        public FunctionIntegrationTests() : base(LibraryName)
        {
        }

        [Fact]
        public void CanCallFunctionWithStructParameter()
        {
            const int value = 10;
            const int multiplier = 5;
            var strct =  new TestStruct { A = value };

            const int expected = value * multiplier;
            var actual = Library.DoStructMath(ref strct, multiplier);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallFunctionWithSimpleParameter()
        {
            const int value = 10;
            const int multiplier = 5;

            const int expected = value * multiplier;
            var actual = Library.Multiply(value, multiplier);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallFunctionWithDifferentEntryPoint()
        {
            const int value = 10;
            const int multiplier = 5;

            var strct =  new TestStruct { A = value };

            const int expected = value * multiplier;
            var actual = Library.Multiply(ref strct, multiplier);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallFunctionWithDifferentCallingConvention()
        {
            const int value = 10;
            const int other = 5;

            var expected = value - other;
            var actual = Library.STDCALLSubtract(value, other);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallDuplicateFunction()
        {
            const int value = 10;
            const int other = 5;

            const int expected = value - other;
            var actual = Library.DuplicateSubtract(value, other);

            Assert.Equal(expected, actual);
        }
    }
}