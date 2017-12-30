#include <stdlib.h>
#if _MSC_VER
#include <stdint.h>
#endif

typedef struct {
    int32_t a;
} MyStruct;

MyStruct* MyStructure;

void InitializeMyStructure()
{
    MyStructure = (MyStruct*) malloc(sizeof(MyStruct));
}

int32_t DoMath(MyStruct* struc)
{
    return struc->a;
}

const MyStruct* GetAllocatedTestStruct()
{
    MyStruct* testStruct = (MyStruct*)malloc(sizeof(MyStruct));
    testStruct->a = 10;
    return testStruct;
}

const char* GetString()
{
    return "Hello from C!";
}

const char* GetNullString()
{
    return NULL;
}
