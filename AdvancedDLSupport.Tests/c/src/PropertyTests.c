#include <stdlib.h>
#include "comp.h"

__declspec(dllexport) int32_t GlobalVariable = 5;
__declspec(dllexport) int32_t* GlobalPointerVariable = NULL;

__declspec(dllexport) void InitializeGlobalPointerVariable()
{
    GlobalPointerVariable = (int32_t*)malloc(sizeof(int32_t));
    *GlobalPointerVariable = 20;
}

__declspec(dllexport) void ResetData()
{
    if (GlobalPointerVariable != NULL)
    {
        free(GlobalPointerVariable);
    }

    GlobalVariable = 5;
    InitializeGlobalPointerVariable();
}
