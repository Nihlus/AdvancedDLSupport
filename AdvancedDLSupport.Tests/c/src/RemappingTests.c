#include <stdlib.h>
#include "wincomp.h"

__declspec(dllexport) int32_t Multiply(int value, int multiplier)
{
    return value * multiplier;
}
