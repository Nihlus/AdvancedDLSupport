using System;
using AdvancedDLSupport;
using AdvanceDLSupport.Tests.Interfaces;
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
			var actual = _fixture.Library.Multiply(ref strct, multiplier);

			Assert.Equal(expected, actual);
		}

		[Property]
		public void CanCallFunctionWithSimpleParameter(int value, int multiplier)
		{
			var expected = value * multiplier;
			var actual = _fixture.Library.Multiply(value, multiplier);

			Assert.Equal(expected, actual);
		}
	}
}