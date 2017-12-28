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

const char* GetString()
{
    return "Hello from C!";
}

const char* GetNullString()
{
    return NULL;
}
