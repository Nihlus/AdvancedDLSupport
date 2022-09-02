# AdvancedDLSupport
Alternative approach to your usual P/Invoke!

[![Join the chat at https://discord.gg/fDy5Vhb](https://img.shields.io/badge/chat-on%20discord-7289DA.svg)](https://discord.gg/fDy5Vhb)

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

Read the [Docs][9], or install via NuGet and [get started][quickstart].


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
Get it on NuGet!


## Support me
[![Become a Patron][patreon-button]][patreon]
<a href='https://ko-fi.com/H2H176VD' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://az743702.vo.msecnd.net/cdn/kofi2.png?v=0' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>


## License
If the library's license doesn't fit your project or product, please [contact us][14]. Custom licensing options are 
available, and we are always open to working something out that fits you - be it modified, commercial, or otherwise.

AdvancedDLSupport's public release is licensed under the [GNU Lesser General Public License, Version 3 (LGPLv3)][12]. 
See the [LICENSE][13] for details. Without the support of the open-source movement, it would never have existed.

 
[2]: https://travis-ci.org/Firwood-Software/AdvancedDLSupport
[6]: https://ci.appveyor.com/project/Nihlus/advancedlsupport-dnwes

[9]: https://nihlus.github.io/AdvancedDLSupport

[quickstart]: docs/quickstart.md
[indirect-calls]: docs/indirect-calling.md

[12]: https://www.gnu.org/licenses/lgpl-3.0.txt
[13]: LICENSE
[14]: mailto:jarl.gullberg@gmail.com

[patreon-button]: https://c5.patreon.com/external/logo/become_a_patron_button.png (Become a Patron)
[patreon]: https://www.patreon.com/jargon
