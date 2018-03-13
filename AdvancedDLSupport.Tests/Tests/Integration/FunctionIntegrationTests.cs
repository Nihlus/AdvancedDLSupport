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

        [Property]
        public void CanCallFunctionWithStructParameter(int value, int multiplier)
        {
            var strct =  new TestStruct { A = value };

            var expected = value * multiplier;
            var actual = Library.DoStructMath(ref strct, multiplier);

            Assert.Equal(expected, actual);
        }

        [Property]
        public void CanCallFunctionWithSimpleParameter(int value, int multiplier)
        {
            var expected = value * multiplier;
            var actual = Library.Multiply(value, multiplier);

            Assert.Equal(expected, actual);
        }

        [Property]
        public void CanCallFunctionWithDifferentEntryPoint(int value, int multiplier)
        {
            var strct =  new TestStruct { A = value };

            var expected = value * multiplier;
            var actual = Library.Multiply(ref strct, multiplier);

            Assert.Equal(expected, actual);
        }

        [Property]
        public void CanCallFunctionWithDifferentCallingConvention(int value, int other)
        {
            var expected = value - other;
            var actual = Library.STDCALLSubtract(value, other);

            Assert.Equal(expected, actual);
        }

        [Property]
        public void CanCallDuplicateFunction(int value, int other)
        {
            var expected = value - other;
            var actual = Library.DuplicateSubtract(value, other);

            Assert.Equal(expected, actual);
        }
    }
}