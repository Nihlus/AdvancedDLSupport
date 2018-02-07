using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration
{
    public class NullableStructTests : LibraryTestBase<INullableLibrary>
    {
        public NullableStructTests(string libraryLocation) : base(libraryLocation)
        {
        }

        [Fact]
        public void CanCallFunctionWithNullableReturnValue()
        {
            var result = Library.GetAllocatedTestStruct();
            Assert.NotNull(result);
            Assert.Equal(10, result.Value.A);
            Assert.Equal(20, result.Value.B);
        }
        [Fact]
        public void CanCallFunctionWithNullableReturnValueWhereResultIsNull()
        {
            var result = Library.GetNullTestStruct();
            Assert.Null(result);
        }

        [Fact]
        public void CanCallFunctionWithNullableParameter()
        {
            Assert.False(Library.CheckIfStructIsNull(new TestStruct{ A = 10, B = 20 }));
        }

        [Fact]
        public void CanCallFunctionWithNullableParameterWhereParameterIsNull()
        {
            Assert.True(Library.CheckIfStructIsNull(null));
        }

        [Fact]
        public void CanCallFunctionWithRefNullableParameter()
        {
            TestStruct? testStruct = new TestStruct { A = 10, B = 20 };

            var result = Library.CheckIfRefStructIsNull(ref testStruct);
            Assert.False(result);
        }

        [Fact]
        public void CanCallFunctionWithRefNullableParameterWhereParameterIsNull()
        {
            TestStruct? testStruct = null;

            var result = Library.CheckIfRefStructIsNull(ref testStruct);
            Assert.True(result);
        }

        [Fact]
        public void RefNullableParameterPropagatesResultsBack()
        {
            TestStruct? testStruct = new TestStruct { A = 10, B = 20 };

            Library.SetValueInNullableRefStruct(ref testStruct);

            Assert.True(testStruct.HasValue);
            Assert.Equal(15, testStruct.Value.A);
        }

        [Fact]
        public void NativeCodeCanAccessRefNullableValues()
        {
            TestStruct? testStruct = new TestStruct { A = 10, B = 20 };

            var result = Library.GetValueInNullableRefStruct(ref testStruct);

            Assert.Equal(10, result);
        }
    }
}