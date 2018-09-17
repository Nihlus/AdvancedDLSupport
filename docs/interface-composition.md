Interface Composition
=====================

Sometimes, you might want to split your native library across multiple interfaces, but still maintain some shared 
functions between them. Different areas of a library with common error handling, or perhaps some math functions.

To facilitate this, ADL supports interface inheritance in its pipeline. For example:

```c#
public interface IMyLibraryErrorHandling
{
	string GetErrorString(int errorCode);
}

public interface IMyLibraryThingDoer : IMyLibraryErrorHandling
{
	int DoTheThing(); 
}

// ...

var thingDoer = NativeLibraryBase.Default.ActivateInterface<IMyLibraryThingDoer>(LibraryName);

var result = thingDoer.DoTheThing();
var errorMessage = thingDoer.GetErrorString(result);
```

This type of inheritance is supported to an arbitrary inheritance depth and composition.

Something to be aware of is that C# does not normally transfer attributes applied in an interface to the implementing 
class. ADL does its own transfer of attributes from the interface to the class, and as such, you can apply attributes in
the interface and still have them affect a mixed-mode class. 

This does come with a gotcha - if the implementation in the mixed-mode class has *any* attributes at all, those 
attributes will take precedence over the ones in the interface (which will be ignored).
