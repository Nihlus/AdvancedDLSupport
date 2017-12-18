#include <stdint.h>

typedef struct
{
    int32_t a;
}
TestStruct;

int32_t DoStructMath(TestStruct* struc, int multiplier)
{
    return struc->a * multiplier;
}

int32_t Multiply(int value, int multiplier)
{
    return value * multiplier;
}

__cdecl int32_t CDeclSubtract(int value, int other)
{
    return value - other;
}