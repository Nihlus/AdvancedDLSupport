Supported Constructs
====================

AdvancedDLSupport supports a number of interop constructs between C and the equivalent C# interface. This page lists
them, and gives some usage examples.

### Functions to Methods
Functions can be marshalled into methods on the interface. Virtually any configuration is supported.

#### Simple functions
`C`
```c
int32_t MyFunction(int32_t a, int32_t b);
```
`C#`
```c#
public interface IMyLibrary
{
    int MyFunction(int a, int b);
}
```

#### Struct Parameters
`C`
```c
typedef struct
{
    int32_t A;
} MyStruct;

int32_t MyStructPointerFunc(MyStruct* struc, int multiplier);
int32_t MyStructFunc(MyStruct struc, int multiplier);
```
`C#`
```c#
public struct MyStruct
{
    int A;
}

public interface IMyLibrary
{
    int MyStructPointerFunc(ref MyStruct struc, int multiplier);
    int MyStructFunc(MyStruct struc, int multiplier);
}
```

#### Pointers
`C`
```c
int32_t MyFunction(int32_t* a, int32_t b);
```
`C#`
```c#
public interface IMyLibrary
{
    int MyFunction(ref int a, int b);
}
```

### Global Variables to Properties
You can also bind to and access global variables. These are bound to properties.

#### Values
`C`
```c
int32_t GlobalVariableA = 5;
```
`C#`
```c#
public interface IMyLibrary
{
    int GlobalVariableA { get; set; }
}
```
#### Pointers

`C`
```c
int32_t* GlobalPointerVariable;
```
`C#`
```c#
public unsafe interface IMyLibrary
{
    int* GlobalVariableA { get; set; }
}
```

### Marshalling of Generic Delegates
`Action<T>` and `Func<TIn, TOut>` have been available in .NET since
version 3.5 and are the preferred option to declaring your own delegate,
but P/Invoke has to date lacked any capability for handling generics.

ADL, on the other hand, allows you to seamlessly use generic delegates
in native bindings. The following interface is completely valid, and
works without any additional user code.

The typical restrictions and gotchas related to explicit delegates in
normal P/Invoke still apply.

`C`
```c
typedef void (*Action)();
typedef void (*ActionT1)(int t1);

typedef int (*FuncT1)();
typedef int (*FuncT1T2)(int t2);

void ExecuteAction(Action action)
{
	action();
}

void ExecuteActionT1(ActionT1 action)
{
	action(5);
}

int ExecuteFuncT1(FuncT1 func)
{
	return func();
}

int ExecuteFuncT1T2(FuncT1T2 func)
{
	return func(5);
}

void NativeAction()
{
	fprintf(stdout, "Living in native land!");
}

Action GetNativeAction()
{
	return &NativeAction;
}

void NativeActionT1(int t1)
{
	fprintf(stdout, "Living in native land, with the parameter %d!", t1);
}

ActionT1 GetNativeActionT1()
{
	return &NativeActionT1;
}

int NativeFuncT1()
{
	return 5;
}

FuncT1 GetNativeFuncT1()
{
	return &NativeFuncT1;
}

int NativeFuncT1T2(int t2)
{
	return t2 * 5;
}

FuncT1T2 GetNativeFuncT1T2()
{
	return &NativeFuncT1T2;
}
```

`C#`
```c#
public interface IMyLibrary
{
    void ExecuteAction(Action action);

    void ExecuteActionT1(Action<int> action);

    int ExecuteFuncT1(Func<int> func);

    int ExecuteFuncT1T2(Func<int, int> func);

    Action GetNativeAction();

    Action<int> GetNativeActionT1();

    Func<int> GetNativeFuncT1();

    Func<int, int> GetNativeFuncT1T2();
}
```
