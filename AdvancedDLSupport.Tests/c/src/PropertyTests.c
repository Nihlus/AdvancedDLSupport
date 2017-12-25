#include <stdlib.h>
#if _MSC_VER
#include <stdint.h>
#endif

int32_t GlobalVariable = 5;
int32_t* GlobalPointerVariable = NULL;

void InitializeGlobalPointerVariable()
{
    GlobalPointerVariable = (int32_t*)malloc(sizeof(int32_t));
    *GlobalPointerVariable = 20;
}

void ResetData()
{
    if (GlobalPointerVariable != NULL)
    {
        free(GlobalPointerVariable);
    }

    GlobalVariable = 5;
    InitializeGlobalPointerVariable();
}
