#include <stdlib.h>
#include <memory.h>
#include <stdbool.h>
#include "wincomp.h"

__declspec(dllexport) const char* GetString()
{
    return "Hello from C!";
}

__declspec(dllexport) const char* GetNullString()
{
    return NULL;
}

__declspec(dllexport) size_t StringLength(const char* value)
{
    return strlen(value);
}

__declspec(dllexport) bool CheckIfStringIsNull(const char* value)
{
    return value == NULL;
}
