Ahead-of-Time Compilation
=========================

In certain instances, it may be desirable to save the generated types between runs of an application - either to save on
processor cycles, memory usage, or generation time. Perhaps your native interface is very large, or you have a large 
number of interfaces that take time to generate.

To provide a solution for this, ADL has an ahead-of-time (AOT) mechanism wherein it can inspect your assemblies outside
of runtime (either at compile time, or at a custom instrumentation step in your own toolchain), extract the interfaces
you've tagged as eligible for AOT compilation, and save the types that would otherwise have been generated at runtime
to their own discrete assemblies. 

Then, at runtime, the library simply loads the already generated types and uses them
in place of what would have been generated.

Using this mechanism is simple, but does introduce an extra step into your toolchain. First, any interfaces that you 
wish to make eligible for AOT compilation must be tagged with the `AOTType` attribute.

```c#
[AOTType]
public interface IMyLibrary
{
	int MyFunction(int a, int b);
}
```

This lets the AOT tool discover the type. After this, you'll need to run the AOT compiler tool against your assemblies.
It can inspect multiple assemblies at once, and will output compiled assemblies at a chosen location.

```bash
./AdvancedDLSupport.AOT.exe -i /path/to/MyAssembly.dll /and/another/Assembly.dll -o /my/output/dir
```

The tool accepts the following options:

```
AdvancedDLSupport 1.1.0
Copyright Firwood Software 2017

  -i, --input-assemblies          Required. Input assemblies to process.

  -f, --implementation-options    (Default: GenerateDisposalChecks, EnableDllMapSupport) The implementation options to use when generating.

  -o, --output-path               The output path where the generated assemblies should be stored. Defaults to the current directory.

  -v, --verbose                   (Default: false) Enable verbose logging.

  --help                          Display this help screen.

  --version                       Display version information.

```

Input assemblies are separated by spaces, and can be enclosed in double quotes if required (such as paths with spaces in 
file or folder names.)

Once you've ran the tool against your assemblies, you'll have two files - a main assembly file, and a module file that
contains the actual types. If you're interested, you can disassemble the module file to see how things work under the 
hood.

These files must then be distributed with your program. You can place them anywhere you like, but they must be readable
at runtime by the program. Finally, to make the library aware of the AOT-compiled types, you must provide it with the 
location they're in.

```c#
var myAOTPath = Path.Combine("content", "aot"); // or wherever you placed the assemblies
NativeLibraryBuilder.DiscoverCompiledTypes(myAOTPath);
```

The `DiscoverCompiledTypes` call will recursively scan the given path for files ending in the `dll` extension, and load 
the ones containing AOT-compiled types into the global cache, making the types available for future instance activation
calls.

You should only call this method *once*. It's a comparatively expensive call, and calling it multiple times will only 
cost you cycles without doing anything.

That's it! Enjoy your runtime speed gains.