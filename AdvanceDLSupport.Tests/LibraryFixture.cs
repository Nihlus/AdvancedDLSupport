using System.IO;
using System.Reflection;
using AdvancedDLSupport;
using AdvanceDLSupport.Tests.Interfaces;

namespace AdvanceDLSupport.Tests
{
	public class LibraryFixture
	{
		public ITestLibrary Library { get; }

		public LibraryFixture()
		{
			var path = Path.Combine
			(
				Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
				"libTest.so"
			);
			Library =  AnonymousImplementationBuilder.ResolveAndActivateInterface<ITestLibrary>(path);
		}
	}
}
