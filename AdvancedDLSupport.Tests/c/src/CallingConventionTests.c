#include <stdint.h>

__declspec(dllexport) int32_t __stdcall MultiplyBy5_STD(int32_t num)
{
	return num * 5;
}

__declspec(dllexport) int32_t __cdecl MultiplyBy5(int32_t num)
{
	return num * 5;
}