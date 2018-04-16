#include <stdlib.h>
#include <stdbool.h>
#include <stdio.h>
#include "TestStruct.h"
#include "comp.h"

__declspec(dllexport) int32_t Multiply(int32_t a, int32_t b)
{
    return a * b;
}

__declspec(dllexport) int32_t GetStructAValueByRef(TestStruct* strct)
{
    return strct->A;
}

__declspec(dllexport) int32_t GetStructAValueByValue(TestStruct strct)
{
    return strct.A;
}

__declspec(dllexport) TestStruct* GetInitializedStructByRef(int32_t a, int32_t b)
{
    TestStruct* strct = malloc(sizeof(TestStruct));
    strct->A = a;
    strct->B = b;

    return strct;
}

__declspec(dllexport) TestStruct GetInitializedStructByValue(int32_t a, int32_t b)
{
    TestStruct* strct = malloc(sizeof(TestStruct));
    strct->A = a;
    strct->B = b;

    return *strct;
}

__declspec(dllexport) TestStruct* GetNullTestStruct()
{
    return NULL;
}

__declspec(dllexport) bool IsTestStructNull(TestStruct* strct)
{
    // This is magic. If it is not present, this function always returns true if compiled under MSVC
    fprintf(stdout, "Pointer: %zx\n ", (size_t)strct);

    return strct == NULL;
}
