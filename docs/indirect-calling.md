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
The Mono and .NET Core tests were performed on Linux Mint 18.3, using an i7-4790K with 16GB RAM.

The full FX tests were performed on Windows 10, using an i7-7600K with 16GB RAM.

Each test case is as follows:
```
Managed                       : Managed code, no interop
DllImport                     : Traditional DllImport
Delegates                     : Delegates, with disposal checks
DelegatesWithoutDisposeChecks : Delegates, no disposal checks
calli                         : Using the calli opcode
```

*Mono*
```
BenchmarkDotNet=v0.10.14, OS=linuxmint 18.3
Intel Core i7-4790K CPU 4.00GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
  [Host] : Mono 5.10.1.42 (tarball Wed), 64bit
  Mono   : Mono 5.10.1.42 (tarball Wed), 64bit


                               Method |         Mean |      Error |     StdDev |
------------------------------------- |-------------:|-----------:|-----------:|
                           CalliByRef |     8.774 ns |  0.1943 ns |  0.1723 ns |
                       DllImportByRef |    10.844 ns |  0.0133 ns |  0.0125 ns |
                         ManagedByRef |    12.922 ns |  0.1052 ns |  0.0984 ns |
   DelegatesWithoutDisposeChecksByRef |   948.430 ns | 17.7136 ns | 17.3971 ns |
                       DelegatesByRef |   958.051 ns |  2.0486 ns |  1.7106 ns |

                         CalliByValue |    21.163 ns |  0.0866 ns |  0.0768 ns |
                     DllImportByValue |    21.991 ns |  0.2201 ns |  0.2058 ns |
                       ManagedByValue |    22.528 ns |  0.0371 ns |  0.0347 ns |
 DelegatesWithoutDisposeChecksByValue | 1,215.534 ns |  4.5346 ns |  3.7866 ns |
                     DelegatesByValue | 1,228.969 ns |  9.6864 ns |  8.0886 ns |
```

*.NET Core*
```
BenchmarkDotNet=v0.10.14, OS=linuxmint 18.3
Intel Core i7-4790K CPU 4.00GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.4
  [Host] : .NET Core 2.0.5 (CoreCLR 4.6.0.0, CoreFX 4.6.26018.01), 64bit RyuJIT
  Core   : .NET Core 2.0.5 (CoreCLR 4.6.0.0, CoreFX 4.6.26018.01), 64bit RyuJIT


                               Method |      Mean |     Error |    StdDev |
------------------------------------- |----------:|----------:|----------:|
                         ManagedByRef |  3.660 ns | 0.0045 ns | 0.0042 ns |
                           CalliByRef |  8.010 ns | 0.0373 ns | 0.0312 ns |
                       DllImportByRef | 15.204 ns | 0.1530 ns | 0.1356 ns |
   DelegatesWithoutDisposeChecksByRef | 20.411 ns | 0.4039 ns | 0.3778 ns |
                       DelegatesByRef | 22.027 ns | 0.0572 ns | 0.0478 ns |


                         ManagedByRef |  3.660 ns | 0.0045 ns | 0.0042 ns |
                         CalliByValue | 21.912 ns | 0.0149 ns | 0.0132 ns |
                     DllImportByValue | 29.796 ns | 0.0347 ns | 0.0271 ns |
                     DelegatesByValue | 36.662 ns | 0.5188 ns | 0.4853 ns |
 DelegatesWithoutDisposeChecksByValue | 35.504 ns | 0.3495 ns | 0.3269 ns |
```

*.NET FX*
```
BenchmarkDotNet=v0.10.14, OS=Windows 10.0.16299.371 (1709/FallCreatorsUpdate/Redstone3)
Intel Core i7-7600U CPU 2.80GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
Frequency=2835937 Hz, Resolution=352.6171 ns, Timer=TSC
  [Host]     : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0
  Clr        : .NET Framework 4.7.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2633.0

                               Method |     Mean |     Error |    StdDev |
------------------------------------- |---------:|----------:|----------:|
                         ManagedByRef | 26.74 ns | 0.3286 ns | 0.3074 ns |
                           CalliByRef | 27.00 ns | 0.4233 ns | 0.3960 ns |
                       DllImportByRef | 29.90 ns | 0.1497 ns | 0.1250 ns |
   DelegatesWithoutDisposeChecksByRef | 58.63 ns | 1.2115 ns | 1.0740 ns |
                       DelegatesByRef | 75.16 ns | 1.2799 ns | 1.1972 ns |

                       ManagedByValue | 19.29 ns | 0.2555 ns | 0.2265 ns |                       
                     DllImportByValue | 27.20 ns | 0.1880 ns | 0.1666 ns |
                         CalliByValue | 36.52 ns | 0.4173 ns | 0.3903 ns |
 DelegatesWithoutDisposeChecksByValue | 67.60 ns | 1.3390 ns | 1.3151 ns |
                     DelegatesByValue | 91.47 ns | 0.5847 ns | 0.5469 ns |
```


While using `calli` provides unprecedented speed improvements, there are some things to look out for. Primarily, `calli`
is considered unverifiable - that is, the .NET CLR restricts its use in situations where the code is running under 
partial trust. More information on this [here][calli-unverifiable]. This should only be an issue when running on the 
Windows platform.

Under normal conditions, the default security policy on Windows is to run code under full trust, and `calli` will not be
impacted.

In addition, one of the main contributing factors to `calli`'s speed improvements is that it skips certain types of call
marshalling that would normally be performed. As such, you may see unexpected results if you rely on them. However, 
anything that ADL implements explicit support for will work fine under `calli`.

In particular, ADL with `calli` supports nullable structs by value or by ref, string and boolean marshalling with 
`[MarshalAs]`, and normal structs by value and by ref.

Furthermore, when running under .NET Core 2.0, `calli` is restricted to the `__fastcall` calling convention, and will
ignore any hints to the contrary. The limitation in question is fixed in .NET Core 2.1 and above. Using 
`__fastcall` may cause GC issues and runtime crashes unless the unmanaged code is able to handle a managed calling
 convention, where it takes no locks and doesn't run for very long.

Mono and the .NET Framework are unaffected by this issue.


[calli-unverifiable]: https://blogs.msdn.microsoft.com/shawnfa/2004/06/14/calli-is-not-verifiable/
[benchmark-netcore]: https://i.imgur.com/9sjFxkB.png
[benchmark-mono]: https://i.imgur.com/isPcqZ5.png
