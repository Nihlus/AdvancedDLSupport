#include "comp.h"
#include "TestStruct.h"

int32_t globalArray[10];
int isInitialized = 0;

void InitGlobals()
{
	if (isInitialized == 0)
	{
		for (int i = 0; i < 10; ++i)
		{
			globalArray[i] = i;
		}

		isInitialized = 1;
	}
}

//Rewritten so there will not be a memory leak
__declspec(dllexport) int32_t* GetInt32ArrayZeroToNine()
{
	InitGlobals();

	return globalArray;
}

__declspec(dllexport) void WriteToInt32Array(int32_t* arr, int arrLen)
{
	InitGlobals();

	int len = arrLen < 10 ? arrLen : 10;

	for (int i = 0; i < len; ++i)
	{
		arr[i] = globalArray[i];
	}
}

