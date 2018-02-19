Mixed-Mode Classes
==========

Mixed-mode classes are useful constructs for object-oriented C programming, as well as instances where you may want to
have native and managed code coexisting in the same class definition.

In short, it allows you to define normal classes that implement native interfaces, and activate instances of them like 
you'd do with normal native interfaces.

As an example, take this class (taken from the unit test suite):

```cs
using System;

namespace AdvancedDLSupport.Tests.Data.Classes
{
	public abstract class MixedModeClass : NativeLibraryBase, IMixedModeLibrary
	{
		public MixedModeClass(string path, Type interfaceType, ImplementationConfiguration configuration, TypeTransformerRepository transformerRepository)
			: base(path, interfaceType, configuration, transformerRepository)
		{
		}

		public bool RanManagedSubtract { get; private set; }

		public bool RanManagedSetter { get; private set; }

		public int ManagedAdd(int a, int b)
		{
			return a + b;
		}

		public int OtherNativeProperty
		{
			get => 32;

			set => this.RanManagedSetter = true;
		}

		public abstract int NativeProperty { get; set; }

		public abstract int Multiply(int value, int multiplier);

		public int Subtract(int value, int other) 
		{
			RanManagedSubtract = true;
			return value - other;
		}
	}
}
```

with the corresponding interface

```cs
namespace AdvancedDLSupport.Tests.Data
{
	public interface IMixedModeLibrary
	{
		int NativeProperty { get; set; }
		int OtherNativeProperty { get; set; }

		int Multiply(int a, int b);
		int Subtract(int a, int b);
	}
}
```

As you can see, managed functions can coexist with unmanaged ones, and managed code can override implementations of the
unmanaged functions (as seen in Subtract and OtherNativeProperty).

In order to create a mixed-mode class, simply declare an `abstract` class that inherits from 
`NativeLibraryBase`, and implements the interface you want to activate. Any interface members can have 
explicit managed implementations, or remain `abstract` to be routed to their corresponding native implementations.

There are a few limitations:

* Mixed-mode class must inherit from AnonymousImplementationBase
* Mixed-mode classes must be abstract
* Properties may only be fully managed or fully unmanaged - no mixing of getters and setters

Once you have your class definition, instances of it can be created in much the same way as you would interface 
instances:

```cs
NativeLibraryBuilder::ActivateClass<MixedModeClass, IMixedModeLibrary>(LibraryName);
```