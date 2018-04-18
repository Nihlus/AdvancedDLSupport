#include <stdlib.h>
#include "TestStruct.h"
#include "comp.h"

__declspec(dllexport) int32_t __stdcall Multiply(int32_t a, int32_t b)
{
    return a * b;
}

__declspec(dllexport) int32_t __stdcall MultiplyStructByVal(TestStruct strct)
{
    return strct.A * strct.B;
}

__declspec(dllexport) int32_t __stdcall MultiplyStructByPtr(TestStruct* strct)
{
    return strct->A * strct->B;
}
