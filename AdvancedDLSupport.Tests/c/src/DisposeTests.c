#include <stdlib.h>
#include "wincomp.h"

#if _MSC_VER
#include <stdint.h>
#endif

__declspec(dllexport) int32_t Multiply(int value, int multiplier)
{
    return value * multiplier;
}
