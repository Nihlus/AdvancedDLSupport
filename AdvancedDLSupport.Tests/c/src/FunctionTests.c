#include <stdlib.h>
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

__attribute__((stdcall)) int32_t STDCALLSubtract(int value, int other)
{
    return value - other;
}
