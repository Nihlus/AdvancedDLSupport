using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace AdvancedDLSupport.Tests.Unit
{
    public class LibraryLoadingExceptionTests
    {
        public class Constructor
        {
            [Fact]
            public void ParameterlessConstructorDoesNotAssignAnyProperties()
            {
                var actual = new LibraryLoadingException();

                Assert.NotNull(actual.Message);
                Assert.NotEqual(string.Empty, actual.Message);

                Assert.Null(actual.LibraryName);
                Assert.Null(actual.InnerException);
            }

            [Fact]
            public void MessageOnlyConstructorOnlyAssignsMessageProperty()
            {
                const string message = "Test";

                var actual = new LibraryLoadingException(message);

                Assert.NotNull(actual.Message);
                Assert.Equal(message, actual.Message);

                Assert.Null(actual.LibraryName);
                Assert.Null(actual.InnerException);
            }

            [Fact]
            public void MessageAndInnerConstructorOnlyAssignsMessageAndInnerProperties()
            {
                const string message = "Test";
                var inner = new Exception();

                var actual = new LibraryLoadingException(message, inner);

                Assert.NotNull(actual.Message);
                Assert.Equal(message, actual.Message);

                Assert.Null(actual.LibraryName);

                Assert.NotNull(actual.InnerException);
                Assert.Same(inner, actual.InnerException);
            }

            [Fact]
            public void MessageAndLibraryNameConstructorOnlyAssignsMessageAndLibraryNameProperties()
            {
                const string message = "Test";
                const string name = "Name";

                var actual = new LibraryLoadingException(message, name);

                Assert.NotNull(actual.Message);
                Assert.Equal(message, actual.Message);

                Assert.NotNull(actual.LibraryName);
                Assert.Equal(name, actual.LibraryName);

                Assert.Null(actual.InnerException);
            }

            [Fact]
            public void MessageLibraryNameAndInnerConstructorAssignsMessageLibraryNameAndInnerProperties()
            {
                const string message = "Test";
                const string name = "Name";
                var inner = new Exception();

                var actual = new LibraryLoadingException(message, name, inner);

                Assert.NotNull(actual.Message);
                Assert.Equal(message, actual.Message);

                Assert.NotNull(actual.LibraryName);
                Assert.Equal(name, actual.LibraryName);

                Assert.NotNull(actual.InnerException);
                Assert.Same(inner, actual.InnerException);
            }
        }

        public class Serialization
        {
            [Fact]
            public void CanSerialize()
            {
                const string message = "Test";
                const string name = "Name";
                var inner = new Exception();

                var actual = new LibraryLoadingException(message, name, inner);

                IFormatter formatter = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, actual);
                }
            }

            [Fact]
            public void IncludesAllProperties()
            {
                const string message = "Test";
                const string name = "Name";
                var inner = new Exception();

                var actual = new LibraryLoadingException(message, name, inner);

                IFormatter formatter = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, actual);
                    ms.Position = 0;

                    var deserialized = (LibraryLoadingException)formatter.Deserialize(ms);

                    Assert.Equal(actual.Message, deserialized.Message);
                    Assert.Equal(actual.LibraryName, deserialized.LibraryName);

                    Assert.IsType<Exception>(deserialized.InnerException);
                    Assert.Equal(actual.InnerException.Message, deserialized.InnerException.Message);
                }
            }

            [Fact]
            public void CanDeserialize()
            {
                const string message = "Test";
                const string name = "Name";
                var inner = new Exception();

                var actual = new LibraryLoadingException(message, name, inner);

                IFormatter formatter = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, actual);
                    ms.Position = 0;

                    var _ = (LibraryLoadingException)formatter.Deserialize(ms);
                }
            }
        }
    }
}