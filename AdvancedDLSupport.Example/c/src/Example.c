#include <stdlib.h>
#if _MSC_VER
#include <stdint.h>
#endif

typedef struct {
    int32_t a;
} MyStruct;

__declspec(dllexport) MyStruct* MyStructure;

__declspec(dllexport) void InitializeMyStructure()
{
    MyStructure = (MyStruct*) malloc(sizeof(MyStruct));
}

__declspec(dllexport) int32_t DoMath(MyStruct* struc)
{
    return struc->a;
}

__declspec(dllexport) const MyStruct* GetAllocatedTestStruct()
{
    MyStruct* testStruct = (MyStruct*)malloc(sizeof(MyStruct));
    testStruct->a = 10;
    return testStruct;
}

__declspec(dllexport) const char* GetString()
{
    return "Hello from C!";
}

__declspec(dllexport) const char* GetNullString()
{
    return NULL;
}
