#include <stdlib.h>
#if _MSC_VER
#include <stdint.h>
#endif

#include "TestStruct.h"

int32_t DoStructMath(TestStruct* struc, int multiplier)
{
    return struc->A * multiplier;
}

int32_t Multiply(int value, int multiplier)
{
    return value * multiplier;
}

int32_t Subtract(int value, int other)
{
    return value - other;
}

#if _MSC_VER
int32_t __stdcall STDCALLSubtract(int value, int other)
{
    return value - other;
}
#else
int32_t STDCALLSubtract(int value, int other)
{
    return value - other;
}
#endif


