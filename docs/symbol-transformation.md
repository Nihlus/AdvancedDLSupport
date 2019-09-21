Symbol Transformations
======================

Sometimes, the name you want to as a member in your interface doesn't map to what the name of the actual native symbol 
is. It's commonplace in C libraries to have a prefix to functions in order to sort them by class, struct, subsystem,
etc - however, typing all that out in C# gets tedious.

To that end, ADL provides two attributes - `NativeSymbol` and `NativeSymbols`. The former can be applied to individual
methods, and the latter to entire interfaces. Let's look at how.


Given a C library with functions that looks like this,

```c
__declspec(dllexport) int32_t sym_trans_do_thing(int32_t a, int32_t b)
{
    return a * b;
}
``` 
you can do one of two things.

### NativeSymbol

Firstly, you can explicitly specify the name of the symbol you want to bind to.
```c#
public interface ISymbolTransformationTests
{
	[NativeSymbol("sym_trans_do_thing")]
    int DoThing(int a, int b);
}
```

### NativeSymbols

Alternatively, if you'd like to avoid repeating yourself over a larger number of methods, you can provide hinting 
information for the entire interface.

```c#
[NativeSymbols(Prefix = "sym_trans_", SymbolTransformationMethod = Underscore)]
public interface ISymbolTransformationTests
{
    int DoThing(int a, int b);
}
```

The prefix is appended to all symbols in the interface, and is then passed through a transformation method to finalize
the format of the symbol.

The two attributes can be used in combination, and if a method has a `NativeSymbol` attribute, its value will be used
instead of the method's name when forming the final symbol name together with the interface attribute.
 
