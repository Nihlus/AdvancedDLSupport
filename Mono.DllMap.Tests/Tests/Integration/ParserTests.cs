using System.IO;
using System.Linq;
using Mono.DllMap.Extensions;
using Xunit;
using static Mono.DllMap.DllMapOS;

namespace Mono.DllMap.Tests.Integration
{
    public class ParserTests
    {
        public class Parse
        {
            [Fact]
            public void CanParseSimpleConfig()
            {
                using (var fs = File.OpenText(Path.Combine("Data", "DllMap.config.xml")))
                {
                    var actual = DllConfiguration.Parse(fs);

                    Assert.NotNull(actual.Maps);
                    Assert.Single(actual.Maps);
                    var map = actual.Maps.First();

                    Assert.Equal("cygwin1.dll", map.SourceLibrary);
                    Assert.Equal("libc.so.6", map.TargetLibrary);

                    Assert.NotNull(map.SymbolEntries);
                    Assert.Empty(map.SymbolEntries);

                    Assert.True(map.Architecture.HasAll());
                    Assert.True(map.WordSize.HasAll());
                    Assert.True(map.OperatingSystems.HasAll());
                }
            }

            [Fact]
            public void CanParseXmlString()
            {
                var text = File.ReadAllText(Path.Combine("Data", "DllMap.config.xml"));
                var _ = DllConfiguration.Parse(text);
            }

            [Fact]
            public void CanParseXmlStream()
            {
                using (var fs = File.OpenRead(Path.Combine("Data", "DllMap.config.xml")))
                {
                    var _ = DllConfiguration.Parse(fs);
                }
            }

            [Fact]
            public void CanParseXmlTextReader()
            {
                using (var fs = File.OpenText(Path.Combine("Data", "DllMap.config.xml")))
                {
                    var _ = DllConfiguration.Parse(fs);
                }
            }

            [Fact]
            public void CanParseDllEntryConfigThatMapsToMultipleLibraries()
            {
                using (var fs = File.OpenText(Path.Combine("Data", "DllEntry_MultiLib.config.xml")))
                {
                    var actual = DllConfiguration.Parse(fs);

                    Assert.NotNull(actual.Maps);
                    Assert.Single(actual.Maps);
                    var map = actual.Maps.First();

                    Assert.Equal("SolarSystem", map.SourceLibrary);
                    Assert.Null(map.TargetLibrary);
                    Assert.True(map.Architecture.HasAll());
                    Assert.True(map.WordSize.HasAll());

                    Assert.False(map.OperatingSystems.HasAll());
                    Assert.False(map.OperatingSystems.HasFlagFast(Windows));

                    Assert.NotNull(map.SymbolEntries);
                    Assert.Equal(2, map.SymbolEntries.Count);
                    var entry1 = map.SymbolEntries[0];
                    var entry2 = map.SymbolEntries[1];

                    Assert.Equal("libearth.so", entry1.TargetLibrary);
                    Assert.Equal("libmars.so", entry2.TargetLibrary);

                    Assert.Equal("get_Animals", entry1.SourceSymbol);
                    Assert.Equal("get_Plants", entry2.SourceSymbol);

                    Assert.True(map.Architecture.HasAll());
                    Assert.True(map.WordSize.HasAll());

                    Assert.False(entry1.OperatingSystems.HasAll());
                    Assert.False(entry2.OperatingSystems.HasAll());

                    Assert.False(entry1.OperatingSystems.HasFlagFast(Windows));
                    Assert.False(entry2.OperatingSystems.HasFlagFast(Windows));
                }
            }

            [Fact]
            public void CanParseDllEntryConfigThatMapsForMultipleOperatingSystems()
            {
                using (var fs = File.OpenText(Path.Combine("Data", "DllEntry_MultiOS.config.xml")))
                {
                    var actual = DllConfiguration.Parse(fs);

                    Assert.NotNull(actual.Maps);
                    Assert.Single(actual.Maps);
                    var map = actual.Maps.First();

                    Assert.Equal("libc", map.SourceLibrary);
                    Assert.Null(map.TargetLibrary);
                    Assert.True(map.Architecture.HasAll());
                    Assert.True(map.WordSize.HasAll());
                    Assert.True(map.OperatingSystems.HasAll());

                    Assert.NotNull(map.SymbolEntries);
                    Assert.Equal(2, map.SymbolEntries.Count);
                    var entry1 = map.SymbolEntries[0];
                    var entry2 = map.SymbolEntries[1];

                    Assert.True(entry1.OperatingSystems.HasAll());
                    Assert.True(entry1.Architecture.HasAll());
                    Assert.True(entry1.WordSize.HasAll());

                    Assert.Equal("libdifferent.so", entry1.TargetLibrary);
                    Assert.Equal("somefunction", entry1.SourceSymbol);
                    Assert.Equal("differentfunction", entry1.TargetSymbol);

                    Assert.False(entry2.OperatingSystems.HasAll());
                    Assert.True(entry2.OperatingSystems.HasFlagsFast(Solaris, FreeBSD));
                    Assert.True(entry2.Architecture.HasAll());
                    Assert.True(entry2.WordSize.HasAll());

                    Assert.Equal("libanother.so", entry2.TargetLibrary);
                    Assert.Equal("somefunction", entry2.SourceSymbol);
                    Assert.Equal("differentfunction", entry2.TargetSymbol);
                }
            }
        }

        public class TryParse
        {
            [Fact]
            public void ReturnsTrueForValidXmlString()
            {
                var text = File.ReadAllText(Path.Combine("Data", "DllMap.config.xml"));
                Assert.True(DllConfiguration.TryParse(text, out var _));
            }

            [Fact]
            public void ReturnsTrueForValidXmlStream()
            {
                using (var fs = File.OpenRead(Path.Combine("Data", "DllMap.config.xml")))
                {
                    Assert.True(DllConfiguration.TryParse(fs, out var _));
                }
            }

            [Fact]
            public void ReturnsTrueForValidXmlTextReader()
            {
                using (var sr = File.OpenText(Path.Combine("Data", "DllMap.config.xml")))
                {
                    Assert.True(DllConfiguration.TryParse(sr, out var _));
                }
            }

            [Fact]
            public void OutVarIsNotNullForValidXmlString()
            {
                var text = File.ReadAllText(Path.Combine("Data", "DllMap.config.xml"));
                DllConfiguration.TryParse(text, out var val);

                Assert.NotNull(val);
            }

            [Fact]
            public void OutVarIsNotNullForValidXmlStream()
            {
                using (var fs = File.OpenRead(Path.Combine("Data", "DllMap.config.xml")))
                {
                    DllConfiguration.TryParse(fs, out var val);

                    Assert.NotNull(val);
                }
            }

            [Fact]
            public void OutVarIsNotNullForValidXmlTextReader()
            {
                using (var sr = File.OpenText(Path.Combine("Data", "DllMap.config.xml")))
                {
                    DllConfiguration.TryParse(sr, out var val);

                    Assert.NotNull(val);
                }
            }

            [Fact]
            public void ReturnsFalseForInvalidXmlString()
            {
                var text = File.ReadAllText(Path.Combine("Data", "DllMap-bad.config.xml"));
                Assert.False(DllConfiguration.TryParse(text, out var _));
            }

            [Fact]
            public void ReturnsFalseForInvalidXmlStream()
            {
                using (var fs = File.OpenRead(Path.Combine("Data", "DllMap-bad.config.xml")))
                {
                    Assert.False(DllConfiguration.TryParse(fs, out var _));
                }
            }

            [Fact]
            public void ReturnsFalseForInvalidXmlTextReader()
            {
                using (var sr = File.OpenText(Path.Combine("Data", "DllMap-bad.config.xml")))
                {
                    Assert.False(DllConfiguration.TryParse(sr, out var _));
                }
            }

            [Fact]
            public void OutVarIsNullForInvalidXmlString()
            {
                var text = File.ReadAllText(Path.Combine("Data", "DllMap-bad.config.xml"));
                DllConfiguration.TryParse(text, out var val);

                Assert.Null(val);
            }

            [Fact]
            public void OutVarIsNullForInvalidXmlStream()
            {
                using (var fs = File.OpenRead(Path.Combine("Data", "DllMap-bad.config.xml")))
                {
                    DllConfiguration.TryParse(fs, out var val);

                    Assert.Null(val);
                }
            }

            [Fact]
            public void OutVarIsNullForInvalidXmlTextReader()
            {
                using (var sr = File.OpenText(Path.Combine("Data", "DllMap-bad.config.xml")))
                {
                    DllConfiguration.TryParse(sr, out var val);

                    Assert.Null(val);
                }
            }
        }
    }
}
