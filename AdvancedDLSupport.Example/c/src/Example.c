#include <stdlib.h>
#include <stdint.h>

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

