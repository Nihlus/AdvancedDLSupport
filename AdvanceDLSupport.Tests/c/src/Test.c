#include <stdlib.h>
#include <stdint.h>

typedef struct
{
    int32_t a;
}
TestStruct;

int32_t GlobalVariableA = 5;
int32_t* GlobalPointerVariable;

void InitializeGlobalPointerVariable()
{
    GlobalPointerVariable = (int32_t*)malloc(sizeof(int32_t));
    *GlobalPointerVariable = 20;
}

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