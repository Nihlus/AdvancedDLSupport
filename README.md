# AdvanceDLSupport
Alternative approach to your usual P/Invoke!

Use C# interfaces to bind to native code - quick and easy usage of C API in C# code, on any platform. 
Gone are the days of broken `DllImport` and annoying workarounds for the different runtimes.

Fully compatible with Mono, .NET Framework, .NET Core, and .NET Standard. Compatible with Mono DLL mapping on all 
platforms and runtimes. Configurable and adaptible.

## Build & Test status
| Travis (Linux)          | Travis (OSX)            | AppVeyor (Windows)      | MyGet                         |
| ----------------------- | ----------------------- | ----------------------- | ----------------------------- |
| [![Build Status][1]][2] | [![Build Status][3]][4] | [![Build status][5]][6] | [![MyGet Build Status][7]][8] |

Read the [Docs][9], or get the [MyGet][10] development packages.

## Features
* Supports all the typical P/Invoke patterns and constructs
* Seamlessly mix native functions and managed code
* Use more complex types, such as `Nullable<T>` and `string` without any extra code
* Select library names at runtime
* Swappable native library search algorithms
* Import global variables 
* Optional lazy loaded symbols
* Optional Mono DllMap support

## Basic Usage

1. Declare your interface

	```cs
	public interface IMyNativeLibrary
	{
		long MyNativeGlobal { get; set; }
		int MyNativeMultiply(int a, int b);
		void MyOtherNativeFunction(MyStruct strct, ref MyStruct? maybeStruct);
	}
	```

2. Activate it
	```cs
	const string MyLibraryName = "MyLibrary";

	var activator = new AnonymousImplementationBuilder();
	var library = activator.ResolveAndActivateInterface<IMyNativeLibrary>(MyLibraryName);
	```

3. Use it

	```cs
	library.MyNativeGlobal = 10;

	var result = library.MyNativeMultiply(5, 5);

	var myStruct = new MyStruct();
	MyStruct? myOtherStruct = null;

	library.MyOtherNativeFunction(myStruct, ref myOtherStruct);
	```

See the [Quickstart][11] for more information.

## Installation

Via your favourite Nuget UI, or

`NuGet`
```
Install-Package AdvancedDLSupport -ProjectName MyProject
```

`MyGet`
```
nuget sources Add -Name AdvancedDLSupport-develop -Source https://www.myget.org/F/advancedlsupport/api/v3/index.json
```
```
Install-Package AdvancedDLSupport -ProjectName MyProject
```

## License
AdvancedDLSupport is licensed under the [GNU General Public License, Version 3 (GPLv3)][12]. See the [LICENSE][13] for
details. Without the support of the open-source movement, it would never have existed.

We also offer custom licensing for companies and individuals. Contact us for a quote.


[1]: https://travis-matrix-badges.herokuapp.com/repos/Firwood-Software/AdvanceDLSupport/branches/master/1
[2]: https://travis-ci.org/Firwood-Software/AdvanceDLSupport

[3]: https://travis-matrix-badges.herokuapp.com/repos/Firwood-Software/AdvanceDLSupport/branches/master/2 
[4]: https://travis-ci.org/Firwood-Software/AdvanceDLSupport

[5]: https://ci.appveyor.com/api/projects/status/vx6kskvtgv79uwvo?svg=true
[6]: https://ci.appveyor.com/project/Nihlus/advancedlsupport-dnwes

[7]: https://www.myget.org/BuildSource/Badge/advancedlsupport?identifier=81802e0b-f4f6-4939-93a9-9edb54b134e6
[8]: https://www.myget.org

[9]: https://firwood-software.github.io/AdvanceDLSupport
[10]: https://www.myget.org/gallery/advancedlsupport

[11]: docs/quickstart.md

[12]: https://www.gnu.org/licenses/gpl-3.0.en.html
[13]: LICENSE