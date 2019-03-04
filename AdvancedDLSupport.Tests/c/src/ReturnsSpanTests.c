#include "comp.h"
#include "TestStruct.h"
#include "../../../../../../../../Program Files (x86)/Microsoft Visual Studio/2017/Community/VC/Tools/MSVC/14.16.27023/include/stdint.h"

__declspec(dllexport) int32_t*__stdcall ReturnsInt32ArrayZeroToNine()
{
	int32_t* arr = malloc(sizeof(int32_t) * 10);

	for (int i = 0; i < 10; i++)
		arr[i] = i;

	return arr;
}