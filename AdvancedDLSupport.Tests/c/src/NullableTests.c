#include <stdlib.h>
#include <memory.h>
#include <stdbool.h>
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
