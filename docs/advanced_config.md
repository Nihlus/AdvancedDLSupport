Advanced Configuration
======================

AdvancedDLSupport supports some alternate generation options for more advanced use cases. These are enabled by passing 
an `ImplementationOptions` flag set to the `AnonymousImplementationBuilder`.

```cs
var config = ImplementationOptions.UseLazyBinding | ImplementationOptions.GenerateDisposalChecks;
var library = new NativeLibraryBuilder(config).ActivateInterface<IMyLibrary>(LibraryName);
```

At the moment, the following options are available.

### Lazy Loaded Symbols
If `ImplementationOptions::UseLazyBinding` is enabled, then no symbol pointers are loaded until you actually access the 
corresponding member. This allows more general implementations which have methods that may be missing at runtime - for 
instance, OpenGL extensions.

If you attempt to access a symbol that is not available, a `SymbolLoadingException` will be thrown.

### Disposal Checking
If `ImplementationOptions::GenerateDisposalChecks` is enabled, then all methods, property getters, and property setters 
have a disposal check injected at the start of the method. If the library object is disposed, any call to a member will 
throw an `ObjectDisposedException`.

If this option is enabled, your interface should inherit from `IDisposable`. The base class for the underlying 
implementation already implements this interface.

### Mono DllMaps
If `ImplementationOptions::EnableDllMapSupport` is enabled, then Mono [DllMaps][1] will be respected wherever possible. 
Note that the support is thus far only partial, and per-symbol library remapping (e.g, `dllentry`) is *not* supported 
yet. Library remapping via `dllmap` works just fine, though.

### Indirect calls
If `ImplementationOptions::UseIndirectCalls` is enabled, the far more performant `calli` opcode will be used instead of 
generating delegates under the hood. This warrants some more detailed information, which is available [here][2].

### Path Resolvers
You can override the algorithms used to resolve the path to the library that DLSupport will load by passing an 
`ILibraryPathResolver` to the implementation builder.

[1]: http://www.mono-project.com/docs/advanced/pinvoke/dllmap
[2]: indirect-calling.md