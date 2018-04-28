#include <stdlib.h>
#include "comp.h"

__declspec(dllexport) int32_t sym_trans_do_thing(int32_t a, int32_t b)
{
    return a * b;
}
