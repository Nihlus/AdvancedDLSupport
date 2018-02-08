#include <stdlib.h>
#include "comp.h"

__declspec(dllexport) int32_t NativeProperty = 0;
__declspec(dllexport) int32_t OtherNativeProperty = 0;

__declspec(dllexport) void SetNativePropertyValue()
{
    NativeProperty = 128;
}

__declspec(dllexport) int32_t Multiply(int value, int multiplier)
{
    return value * multiplier;
}

__declspec(dllexport) int32_t Subtract(int value, int other)
{
    return value - other;
}