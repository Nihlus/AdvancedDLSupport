Supported Constructs
====================

AdvancedDLSupport supports a number of interop constructs between C and
the equivalent C# interface. This page lists them, and gives some usage
examples.

### Functions to Methods
Functions can be marshalled into methods on the interface. Virtually any
configuration is supported.

#### Simple functions
`C`
```c
int32_t MyFunction(int32_t a, int32_t b);
```
`C#`
```cs
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
```cs
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
```cs
public interface IMyLibrary
{
    int MyFunction(ref int a, int b);
}
```

### Global Variables to Properties
You can also bind to and access global variables. These are bound to
properties.

#### Values
`C`
```c
int32_t GlobalVariableA = 5;
```
`C#`
```cs
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
```cs
public unsafe interface IMyLibrary
{
    int* GlobalVariableA { get; set; }
}
```