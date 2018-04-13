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
