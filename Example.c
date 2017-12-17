#include <stdint.h>

typedef struct {
    int32_t a;
} MyStruct;

int32_t DoMath(MyStruct* struc)
{
    return struc->a;
}

