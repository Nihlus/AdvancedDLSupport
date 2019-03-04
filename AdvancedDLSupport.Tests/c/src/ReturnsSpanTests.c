#include "comp.h"
#include "TestStruct.h"

__declspec(dllexport) int32_t*__stdcall ReturnsInt32ArrayZeroToNine()
{
	int32_t* arr = malloc(sizeof(int32_t) * 10);

	for (int i = 0; i < 10; i++)
		arr[i] = i;

	return arr;
}

__declspec(dllexport) void* __stdcall Dummy() { return NULL; }