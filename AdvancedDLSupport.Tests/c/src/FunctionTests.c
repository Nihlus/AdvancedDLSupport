#include <stdlib.h>
#include "TestStruct.h"
#include "comp.h"

__declspec(dllexport) int32_t DoStructMath(TestStruct* struc, int multiplier)
{
    return struc->A * multiplier;
}

__declspec(dllexport) int32_t Multiply(int value, int multiplier)
{
    return value * multiplier;
}

__declspec(dllexport) int32_t Subtract(int value, int other)
{
    return value - other;
}

#pragma comment(linker, "/export:STDCALLSubtract=_STDCALLSubtract@8")
__declspec(dllexport) int32_t __stdcall STDCALLSubtract(int value, int other)
{
    return value - other;
}
