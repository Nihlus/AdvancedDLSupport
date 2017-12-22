using AdvanceDLSupport.Tests.Structures;
using FsCheck.Xunit;
using Xunit;

namespace AdvanceDLSupport.Tests
{
	public class IntegrationTests : IClassFixture<LibraryFixture>
	{
		private readonly LibraryFixture _fixture;

		public IntegrationTests(LibraryFixture fixture)
		{
			_fixture = fixture;
		}

		[Fact]
		public void CanLoadLibrary()
		{
			Assert.NotNull(_fixture.Library);
		}

		[Property]
		public void CanCallFunctionWithStructParameter(int value, int multiplier)
		{
			var strct =  new TestStruct { A = value };

			var expected = value * multiplier;
			var actual = _fixture.Library.DoStructMath(ref strct, multiplier);

			Assert.Equal(expected, actual);
		}

		[Property]
		public void CanCallFunctionWithSimpleParameter(int value, int multiplier)
		{
			var expected = value * multiplier;
			var actual = _fixture.Library.Multiply(value, multiplier);

			Assert.Equal(expected, actual);
		}

		[Property]
		public void CanCallFunctionWithDifferentEntryPoint(int value, int multiplier)
		{
			var strct =  new TestStruct { A = value };

			var expected = value * multiplier;
			var actual = _fixture.Library.Multiply(ref strct, multiplier);

			Assert.Equal(expected, actual);
		}

		[Property]
		public void CanCallFunctionWithDifferentCallingConvention(int value, int other)
		{
			var expected = value - other;
			var actual = _fixture.Library.CDeclSubtract(value, other);

			Assert.Equal(expected, actual);
		}

		[Property]
		public void CanCallDuplicateFunction(int value, int other)
		{
			var expected = value - other;
			var actual = _fixture.Library.Subtract(value, other);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void CanGetGlobalVariableAsProperty()
		{
			Assert.Equal(5, _fixture.Library.GlobalVariableA);
		}

		[Fact]
		public void CanSetGlobalVariableAsProperty()
		{
			_fixture.Library.GlobalVariableA = 1;
			Assert.Equal(1, _fixture.Library.GlobalVariableA);
		}

		[Fact]
		public unsafe void CanGetGlobalPointerVariableAsProperty()
		{
			_fixture.Library.InitializeGlobalPointerVariable();
			Assert.Equal(20, *_fixture.Library.GlobalPointerVariable);
		}

		[Fact]
		public unsafe void CanSetGlobalPointerVariableAsProperty()
		{
			_fixture.Library.InitializeGlobalPointerVariable();
			*_fixture.Library.GlobalPointerVariable = 25;
			Assert.Equal(25, _fixture.Library.GlobalVariableA);
		}
	}
}