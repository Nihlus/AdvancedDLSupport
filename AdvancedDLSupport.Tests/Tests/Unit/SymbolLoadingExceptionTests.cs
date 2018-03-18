using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace AdvancedDLSupport.Tests.Unit
{
    public class SymbolLoadingExceptionTests
    {
        public class Constructor
        {
            [Fact]
            public void ParameterlessConstructorDoesNotAssignAnyProperties()
            {
                var actual = new SymbolLoadingException();

                Assert.NotNull(actual.Message);
                Assert.NotEqual(string.Empty, actual.Message);

                Assert.Null(actual.SymbolName);
                Assert.Null(actual.InnerException);
            }

            [Fact]
            public void MessageOnlyConstructorOnlyAssignsMessageProperty()
            {
                const string message = "Test";

                var actual = new SymbolLoadingException(message);

                Assert.NotNull(actual.Message);
                Assert.Equal(message, actual.Message);

                Assert.Null(actual.SymbolName);
                Assert.Null(actual.InnerException);
            }

            [Fact]
            public void MessageAndInnerConstructorOnlyAssignsMessageAndInnerProperties()
            {
                const string message = "Test";
                var inner = new Exception();

                var actual = new SymbolLoadingException(message, inner);

                Assert.NotNull(actual.Message);
                Assert.Equal(message, actual.Message);

                Assert.Null(actual.SymbolName);

                Assert.NotNull(actual.InnerException);
                Assert.Same(inner, actual.InnerException);
            }

            [Fact]
            public void MessageAndSymbolNameConstructorOnlyAssignsMessageAndSymbolNameProperties()
            {
                const string message = "Test";
                const string name = "Name";

                var actual = new SymbolLoadingException(message, name);

                Assert.NotNull(actual.Message);
                Assert.Equal(message, actual.Message);

                Assert.NotNull(actual.SymbolName);
                Assert.Equal(name, actual.SymbolName);

                Assert.Null(actual.InnerException);
            }

            [Fact]
            public void MessageSymbolNameAndInnerConstructorAssignsMessageSymbolNameAndInnerProperties()
            {
                const string message = "Test";
                const string name = "Name";
                var inner = new Exception();

                var actual = new SymbolLoadingException(message, name, inner);

                Assert.NotNull(actual.Message);
                Assert.Equal(message, actual.Message);

                Assert.NotNull(actual.SymbolName);
                Assert.Equal(name, actual.SymbolName);

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

                var actual = new SymbolLoadingException(message, name, inner);

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

                var actual = new SymbolLoadingException(message, name, inner);

                IFormatter formatter = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, actual);
                    ms.Position = 0;

                    var deserialized = (SymbolLoadingException)formatter.Deserialize(ms);

                    Assert.Equal(actual.Message, deserialized.Message);
                    Assert.Equal(actual.SymbolName, deserialized.SymbolName);

                    Assert.IsType<Exception>(deserialized.InnerException);
                    Assert.Equal(actual.InnerException?.Message, deserialized.InnerException.Message);
                }
            }

            [Fact]
            public void CanDeserialize()
            {
                const string message = "Test";
                const string name = "Name";
                var inner = new Exception();

                var actual = new SymbolLoadingException(message, name, inner);

                IFormatter formatter = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    formatter.Serialize(ms, actual);
                    ms.Position = 0;

                    var _ = (SymbolLoadingException)formatter.Deserialize(ms);
                }
            }
        }
    }
}