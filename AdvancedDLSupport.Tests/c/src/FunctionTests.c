#include <stdlib.h>
#if _MSC_VER
#include <stdint.h>
#endif

#include "TestStruct.h"
#include "wincomp.h"

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

__declspec(dllexport) int32_t __stdcall STDCALLSubtract(int value, int other)
{
    return value - other;
}
