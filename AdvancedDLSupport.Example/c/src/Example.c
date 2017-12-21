#include <stdint.h>

typedef struct {
    int32_t a;
} MyStruct;

MyStruct MyStructure[3];

int32_t DoMath(MyStruct* struc)
{
    return struc->a;
}

