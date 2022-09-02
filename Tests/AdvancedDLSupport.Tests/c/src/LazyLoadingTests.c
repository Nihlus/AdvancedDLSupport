#include <stdlib.h>
#include "comp.h"

__declspec(dllexport) int32_t Multiply(int value, int multiplier)
{
    return value * multiplier;
}
