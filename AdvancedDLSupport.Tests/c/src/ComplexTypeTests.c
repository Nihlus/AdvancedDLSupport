#include <stdlib.h>
#include <stdint.h>
#include <memory.h>
#include <stdbool.h>
#include "TestStruct.h"

const char* GetString()
{
    return "Hello from C!";
}

const char* GetNullString()
{
    return NULL;
}

size_t StringLength(const char* value)
{
    return strlen(value);
}

bool CheckIfStringIsNull(const char* value)
{
    return value == NULL;
}

const TestStruct* GetAllocatedTestStruct()
{
    TestStruct* testStruct = (TestStruct*)malloc(sizeof(TestStruct));
    return testStruct;
}

const TestStruct* GetNullTestStruct()
{
    return NULL;
}

bool CheckIfStructIsNull(const TestStruct* testStruct)
{
    return testStruct == NULL;
}
