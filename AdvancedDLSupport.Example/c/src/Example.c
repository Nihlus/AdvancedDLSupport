#include <stdint.h>

typedef struct {
    int32_t a;
} MyStruct;

MyStruct MyStructure;

int32_t DoMath(MyStruct* struc)
{
    MyStructure.a = struc->a;
    return struc->a;
}

