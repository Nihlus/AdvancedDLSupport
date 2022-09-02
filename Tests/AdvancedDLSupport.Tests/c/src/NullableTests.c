#include <stdlib.h>
#include <memory.h>
#include <stdbool.h>
#include <stdio.h>
#include "TestStruct.h"
#include "comp.h"

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

__declspec(dllexport) int64_t GetStructPtrValue(TestStruct* testStruct)
{
    return (int64_t)testStruct;
}

__declspec(dllexport) bool CheckIfStructIsNull(const TestStruct* testStruct)
{
    return testStruct == NULL;
}

__declspec(dllexport) int32_t GetValueInNullableRefStruct(const TestStruct* testStruct)
{
    return testStruct->A;
}

__declspec(dllexport) void SetValueInNullableRefStruct(TestStruct* testStruct)
{
    testStruct->A = 15;
}

__declspec(dllexport) const char* GetAFromStructAsString(const TestStruct* testStruct)
{
    size_t size = (size_t)snprintf(NULL, 0, "%d", testStruct->A);
    char* buffer = malloc(size + 1);

    sprintf(buffer, "%d", testStruct->A);

    return buffer;
}

__declspec(dllexport) int GetAFromStructMultipliedByParameter(const TestStruct* testStruct, int multiplier)
{
    return testStruct->A * multiplier;
}
