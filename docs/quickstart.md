Quickstart
==========

AdvancedDLSupport uses anonymous implementations of interfaces to create a delegate-based interop framework. Usage is
very simple - as an example, take this simple mockup C library.

`math.h`
```c
int TimesUsed;

int Multiply(int a, int b);
int Subtract(int a, int b);
```
`math.c`
```c
int Multiply(int a, int b)
{
    ++TimesUsed;
    return a * b;
}

int Subtract(int a, int b)
{
    ++TimesUsed;
    return a - b;
}
```

In order to use this with the library, we could use the traditional `[DllImport]` attribute, but AdvancedDLSupport takes 
another approach.

Instead, we declare a matching C# interface.

```c#
public interface IMath
{
    int TimesUsed { get; }

    int Multiply(int a, int b);
    int Subtract(int a, int b);
}
```

Using this interface, we can then initialize a generated type which implements our interface, and allows us to use the 
native library.

```c#
using (var mathLibrary = NativeLibraryBuilder.Default.ActivateInterface<IMath>(LibraryName))
{
    int mySubtraction = mathLibrary.Subtract(10, 5);
    int myMultiplication mathLibrary.Multiply(5, 5);
    
    int timesUsed = mathLibrary.TimesUsed;
    
    Console.WriteLine($"Subtraction: {mySubtraction}, Multiplication: {myMultiplication}, Times used: {timesUsed}");
}

// Output:
// Subtraction: 5, Multiplication: 25, Times used: 2

```

Refer to [Supported Constructs][1] for more information about ways to interact with your libraries.


[1]: supported_constructs.md
