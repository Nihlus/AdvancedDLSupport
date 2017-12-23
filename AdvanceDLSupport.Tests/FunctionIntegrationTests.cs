using AdvancedDLSupport;
using AdvanceDLSupport.Tests.Interfaces;
using AdvanceDLSupport.Tests.Structures;
using FsCheck.Xunit;
using Xunit;

namespace AdvanceDLSupport.Tests
{
    public class FunctionIntegrationTests
    {
        private const string LibraryName = "FunctionTests";

        [Property]
        public void CanCallFunctionWithStructParameter(int value, int multiplier)
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IFunctionLibrary>(LibraryName);

            var strct =  new TestStruct { A = value };

            var expected = value * multiplier;
            var actual = library.DoStructMath(ref strct, multiplier);

            Assert.Equal(expected, actual);
        }

        [Property]
        public void CanCallFunctionWithSimpleParameter(int value, int multiplier)
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IFunctionLibrary>(LibraryName);

            var expected = value * multiplier;
            var actual = library.Multiply(value, multiplier);

            Assert.Equal(expected, actual);
        }

        [Property]
        public void CanCallFunctionWithDifferentEntryPoint(int value, int multiplier)
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IFunctionLibrary>(LibraryName);

            var strct =  new TestStruct { A = value };

            var expected = value * multiplier;
            var actual = library.Multiply(ref strct, multiplier);

            Assert.Equal(expected, actual);
        }

        [Property]
        public void CanCallFunctionWithDifferentCallingConvention(int value, int other)
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IFunctionLibrary>(LibraryName);

            var expected = value - other;
            var actual = library.STDCALLSubtract(value, other);

            Assert.Equal(expected, actual);
        }

        [Property]
        public void CanCallDuplicateFunction(int value, int other)
        {
            var library = new AnonymousImplementationBuilder().ResolveAndActivateInterface<IFunctionLibrary>(LibraryName);

            var expected = value - other;
            var actual = library.DuplicateSubtract(value, other);

            Assert.Equal(expected, actual);
        }
    }
}