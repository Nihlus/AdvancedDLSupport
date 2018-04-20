#include <stdint.h>
#include "comp.h"

__declspec(dllexport) int32_t Multiply(int32_t a, int32_t b)
{
    return a * b; 
}

