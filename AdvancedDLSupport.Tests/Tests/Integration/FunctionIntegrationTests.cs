using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
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
            var value = 5;
            var multiplier = 15;

            var strct =  new TestStruct { A = value };

            var expected = value * multiplier;
            var actual = Library.DoStructMath(ref strct, multiplier);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallFunctionWithSimpleParameter()
        {
            var value = 5;
            var multiplier = 15;

            var expected = value * multiplier;
            var actual = Library.Multiply(value, multiplier);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallFunctionWithDifferentEntryPoint()
        {
            var value = 5;
            var multiplier = 15;

            var strct =  new TestStruct { A = value };

            var expected = value * multiplier;
            var actual = Library.Multiply(ref strct, multiplier);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallFunctionWithDifferentCallingConvention()
        {
            var value = 5;
            var other = 15;

            var expected = value - other;
            var actual = Library.STDCALLSubtract(value, other);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanCallDuplicateFunction()
        {
            var value = 5;
            var other = 15;

            var expected = value - other;
            var actual = Library.DuplicateSubtract(value, other);

            Assert.Equal(expected, actual);
        }
    }
}