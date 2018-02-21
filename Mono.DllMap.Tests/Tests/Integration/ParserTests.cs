using System.IO;
using System.Linq;
using Mono.DllMap.Extensions;
using Xunit;
using static Mono.DllMap.DllMapOS;

namespace Mono.DllMap.Tests.Integration
{
    public class ParserTests
    {
        [Fact]
        void CanParseSimpleConfig()
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
        void CanParseXmlString()
        {
            var text = File.ReadAllText(Path.Combine("Data", "DllMap.config.xml"));
            var _ = DllConfiguration.Parse(text);
        }

        [Fact]
        void CanParseXmlStream()
        {
            using (var fs = File.OpenRead(Path.Combine("Data", "DllMap.config.xml")))
            {
                var _ = DllConfiguration.Parse(fs);
            }
        }

        [Fact]
        void CanParseXmlTextReader()
        {
            using (var fs = File.OpenText(Path.Combine("Data", "DllMap.config.xml")))
            {
                var _ = DllConfiguration.Parse(fs);
            }
        }

        [Fact]
        void CanParseDllEntryConfigThatMapsToMultipleLibraries()
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
        void CanParseDllEntryConfigThatMapsForMultipleOperatingSystems()
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
}
