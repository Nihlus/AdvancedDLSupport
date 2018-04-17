# AdvanceDLSupport
Alternative approach to your usual P/Invoke!

Use C# interfaces to bind to native code - quick and easy usage of C API in C# code, on any platform. 
Gone are the days of broken `DllImport` and annoying workarounds for the different runtimes.

Fully compatible with Mono, .NET Framework, .NET Core, and .NET Standard. Compatible with Mono DLL mapping on all 
platforms and runtimes. Configurable and adaptible.


## Why use ADL?
1) Modern API - no more static classes, no more `extern`. Use your native API as if it were first-class objects.
2) Flexibility - Inject your native API into your classes, change the library scanning logic, mix your managed and
   native code.
3) [Speed][indirect-calls] - ADL is blazing fast, and gives your native interop an edge. See performance increases that
   are at least 2 and up to 8 times faster than other, existing solutions.
4) [Easy to use][quickstart] - Not only is ADL simple to set up and get working, it's a breeze to maintain, and reduces 
   clutter in your codebase.


## Build & Test status
|                   | Travis (Linux - Mono & .NET Core)          | Travis (OSX - Mono & .NET Core)        | AppVeyor (Windows - .NET & .NET Core)    | MyGet (Development)     |
| ----------------- |------------------------------------------- | -------------------------------------- | ---------------------------------------- | ------------------------|
| x64 (Debug)       | [![Build Status][linux-x64-debug]][2]      | [![Build Status][mac-x64-debug]][2]    | [![Build status][win-x64-debug]][6]      |                         |
| x64 (Release      | [![Build Status][linux-x64-release]][2]    | [![Build Status][mac-x64-release]][2]  | [![Build status][win-x64-release]][6]    |                         |
| x86 (Debug)       | [![Build Status][build-not-found]][2]      | [![Build Status][build-not-found]][2]  | [![Build status][win-x86-debug]][6]      |                         |
| x86 (Release)     | [![Build Status][build-not-found]][2]      | [![Build Status][build-not-found]][2]  | [![Build status][win-x86-release]][6]    |                         |
| Any CPU (Debug)   | [![Build Status][linux-anycpu-debug]][2]   | [![Build Status][mac-anycpu-debug]][2] | [![Build status][win-anycpu-debug]][6]   | [![Build Status][7]][8] |                        |
| Any CPU (Release) | [![Build Status][linux-anycpu-release]][2] | [![Build Status][mac-anycpu-debug]][2] | [![Build status][win-anycpu-release]][6] |                         |


Total project coverage: [![Codecov.io][codecov-coverage]][codecov]

Read the [Docs][9], or get the [MyGet][10] development packages and [get started][quickstart].


## Features
* Supports all the typical P/Invoke patterns and constructs
* Seamlessly mix native functions and managed code
* Use more complex types, such as `Nullable<T>` and `string` without any extra code
* Select library architectures at runtime
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

	var activator = new NativeLibraryBuilder();
	var library = activator.ActivateInterface<IMyNativeLibrary>(MyLibraryName);
	```

3. Use it

	```cs
	library.MyNativeGlobal = 10;

	var result = library.MyNativeMultiply(5, 5);

	var myStruct = new MyStruct();
	MyStruct? myOtherStruct = null;

	library.MyOtherNativeFunction(myStruct, ref myOtherStruct);
	```

See the [Quickstart][quickstart] for more information.

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
If the library's license doesn't fit your project or product, please [contact us][14]. Custom licensing options are 
available, and we are always open to working something out that fits you - be it modified, commercial, or otherwise.

AdvancedDLSupport's public release is licensed under the [GNU Lesser General Public License, Version 3 (LGPLv3)][12]. 
See the [LICENSE][13] for details. Without the support of the open-source movement, it would never have existed.


[linux-x64-debug]: https://travis-matrix-badges.herokuapp.com/repos/Firwood-Software/AdvanceDLSupport/branches/master/1
[linux-x64-release]: https://travis-matrix-badges.herokuapp.com/repos/Firwood-Software/AdvanceDLSupport/branches/master/2
[linux-anycpu-debug]: https://travis-matrix-badges.herokuapp.com/repos/Firwood-Software/AdvanceDLSupport/branches/master/3
[linux-anycpu-release]: https://travis-matrix-badges.herokuapp.com/repos/Firwood-Software/AdvanceDLSupport/branches/master/4
[mac-x64-debug]: https://travis-matrix-badges.herokuapp.com/repos/Firwood-Software/AdvanceDLSupport/branches/master/5
[mac-x64-release]: https://travis-matrix-badges.herokuapp.com/repos/Firwood-Software/AdvanceDLSupport/branches/master/6
[mac-anycpu-debug]: https://travis-matrix-badges.herokuapp.com/repos/Firwood-Software/AdvanceDLSupport/branches/master/7
[mac-anycpu-release]: https://travis-matrix-badges.herokuapp.com/repos/Firwood-Software/AdvanceDLSupport/branches/master/8
 
[2]: https://travis-ci.org/Firwood-Software/AdvanceDLSupport

[win-x86-debug]: https://appveyor-matrix-badges.herokuapp.com/repos/Nihlus/advancedlsupport-dnwes/branch/master/1
[win-x64-debug]: https://appveyor-matrix-badges.herokuapp.com/repos/Nihlus/advancedlsupport-dnwes/branch/master/2
[win-anycpu-debug]: https://appveyor-matrix-badges.herokuapp.com/repos/Nihlus/advancedlsupport-dnwes/branch/master/3
[win-x86-release]: https://appveyor-matrix-badges.herokuapp.com/repos/Nihlus/advancedlsupport-dnwes/branch/master/4
[win-x64-release]: https://appveyor-matrix-badges.herokuapp.com/repos/Nihlus/advancedlsupport-dnwes/branch/master/5
[win-anycpu-release]: https://appveyor-matrix-badges.herokuapp.com/repos/Nihlus/advancedlsupport-dnwes/branch/master/6

[6]: https://ci.appveyor.com/project/Nihlus/advancedlsupport-dnwes

[build-not-found]: https://img.shields.io/badge/build-not%20found-lightgrey.svg

[codecov-coverage]: https://codecov.io/gh/Firwood-Software/AdvanceDLSupport/branch/master/graph/badge.svg
[codecov]: https://codecov.io/gh/Firwood-Software/AdvanceDLSupport

[7]: https://www.myget.org/BuildSource/Badge/advancedlsupport?identifier=81802e0b-f4f6-4939-93a9-9edb54b134e6
[8]: https://www.myget.org

[9]: https://firwood-software.github.io/AdvanceDLSupport
[10]: https://www.myget.org/gallery/advancedlsupport

[quickstart]: docs/quickstart.md
[indirect-calls]: docs/indirect-calling.md

[12]: https://www.gnu.org/licenses/lgpl-3.0.txt
[13]: LICENSE
[14]: mailto:jarl.gullberg@gmail.com
