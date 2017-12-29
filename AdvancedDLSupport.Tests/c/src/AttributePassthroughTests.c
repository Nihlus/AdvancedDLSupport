#include <memory.h>
#include <stdbool.h>
#include <uchar.h>
#include "wincomp.h"

__declspec(dllexport) bool CheckIfGreaterThanZero(int value)
{
    return value > 0;
}

__declspec(dllexport) bool CheckIfStringIsNull(const char* value)
{
    return value == NULL;
}

__declspec(dllexport) const char16_t* EchoWString(const char16_t* value)
{
    return value;
}