Indirect Calling
================

In the CLR, there exists a lesser-known opcode known as `calli`. This opcode effectively circumvents the normal calling
safety of the CLR, and takes as an argument a pointer to an arbitrary function in machine code, and executes it.

This opcode produces native binding code which can be between 2 to 8 times as fast as traditional alternatives, such as
`DllImport` or delegate-based approaches.

ADL provides support for generating bindings using `calli`. To enable it, simply pass `UseIndirectCalls` to the builder
when instantiating it.

```c#
var options = ImplementationOptions.UseIndirectCalls;
var builder = new NativeLibraryBuilder(options);
``` 

Here are some benchmarks to demonstrate the benefits of using `calli`.

*.NET Core 2.0*

![.NET Core][benchmark-netcore]

*Mono*

![Mono][benchmark-mono] 

While using `calli` provides unprecedented speed improvements, there are some things to look out for. Primarily, `calli`
is considered unverifiable - that is, the .NET CLR restricts its use in situations where the code is running under 
partial trust. More information on this [here][calli-unverifiable]. This should only be an issue when running on the 
Windows platform.

Under normal conditions, the default security policy on Windows is to run code under full trust, and `calli` will not be
impacted.

Furthermore, when running under 32-bit .NET Core on Windows, `calli` is restricted to the `__fastcall` calling 
convention, and will ignore any hints to the contrary. This calling convention limitation is present on all .NET Core
platforms, but is only relevant on x86. Calling conventions are, by designed, ignored on other platforms in native code.
 


[calli-unverifiable]: https://blogs.msdn.microsoft.com/shawnfa/2004/06/14/calli-is-not-verifiable/
[benchmark-netcore]: https://i.imgur.com/9sjFxkB.png
[benchmark-mono]: https://i.imgur.com/isPcqZ5.png
