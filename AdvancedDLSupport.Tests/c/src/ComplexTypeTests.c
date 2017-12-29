#include <stdlib.h>
#include <stdint.h>
#include <memory.h>
#include <stdbool.h>
#include "TestStruct.h"
#include "comp.h"

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

__declspec(dllexport) const TestStruct* GetAllocatedTestStruct()
{
    TestStruct* testStruct = (TestStruct*)malloc(sizeof(TestStruct));
    testStruct->A = 10;
    testStruct->B = 20;

    return testStruct;
}

__declspec(dllexport) const TestStruct* GetNullTestStruct()
{
    return NULL;
}

__declspec(dllexport) bool CheckIfStructIsNull(const TestStruct* testStruct)
{
    return testStruct == NULL;
}
