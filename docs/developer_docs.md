The Developer Documentation
===========================

The purpose of this library is to simplify and automate native library wrapping via Platform Invocation.
C# inherently does not support accessing the properties from within the CLR, 
it require library to be dynamically loaded through other means which was originally the libDL.
This is original reason why Advanced DL SUpport is named as such.

The use of Common Intermediate Language is emphasized on this project to optimize as much as possible and to emit the 
least amount of code to support such features required of the wrapper, so strong background knowledge of CIL is required 
before contributing.

### The Basic

Assume we have the following code in C:

```c
#include <stdint.h>

int32_t Add(int32_t a, int32_t b)
{
	return a + b;
}
```

Assume we have the complementary interface code in C#:
```cs
public interface IMyLibrary
{
	int Add(int a, int b);
}
```

The final code would be produced by Advance DL Support as an anonymous instance for interface above:

```cs
public class AnonymousClass : IMyLibrary {

	public delegate int Add_dt(int a, int b);

	public Add_dt Add_dtm;

	public int Add(int a, int b) => Add_dtm(a, b);
}
```

This is what we refer to as a Delegate Approach, because this approach require significantly more code to be written to 
support a single instance of dynamic linked library.