#include <stdlib.h>
#include <stdint.h>
#include "comp.h"

__declspec(dllexport) void DoSomethingWithBoolean(uint8_t boolean)
{
    if (boolean == 0)
    {
        int a = 0;
    }

    int b = 1;
}

__declspec(dllexport) int32_t IsSByteTrue(int8_t boolean)
{
    if (boolean > 0)
    {
        return 1;
    }

    return 0;
}

__declspec(dllexport) int32_t IsShortTrue(int16_t boolean)
{
    if (boolean > 0)
    {
        return 1;
    }

    return 0;
}

__declspec(dllexport) int32_t IsIntTrue(int32_t boolean)
{
    if (boolean > 0)
    {
        return 1;
    }

    return 0;
}

__declspec(dllexport) int32_t IsLongTrue(int64_t boolean)
{
    if (boolean > 0)
    {
        return 1;
    }

    return 0;
}

__declspec(dllexport) int32_t IsByteTrue(uint8_t boolean)
{
    if (boolean > 0)
    {
        return 1;
    }

    return 0;
}

__declspec(dllexport) int32_t IsUShortTrue(uint16_t boolean)
{
    if (boolean > 0)
    {
        return 1;
    }

    return 0;
}

__declspec(dllexport) int32_t IsUIntTrue(uint32_t boolean)
{
    if (boolean > 0)
    {
        return 1;
    }

    return 0;
}

__declspec(dllexport) int32_t IsULongTrue(uint64_t boolean)
{
    if (boolean > 0)
    {
        return 1;
    }

    return 0;
}

__declspec(dllexport) int32_t IsBOOLTrue(uint32_t boolean)
{
    if (boolean != 0)
    {
        return 1;
    }

    return 0;
}

__declspec(dllexport) int32_t IsVariantBoolTrue(int16_t boolean)
{
    if (boolean == -1)
    {
        return 1;
    }

    return 0;
}

__declspec(dllexport) int8_t GetTrueSByte()
{
    return 1;
}

__declspec(dllexport) int16_t GetTrueShort()
{
    return 1;
}

__declspec(dllexport) int32_t GetTrueInt()
{
    return 1;
}

__declspec(dllexport) int64_t GetTrueLong()
{
    return 1;
}

__declspec(dllexport) uint8_t GetTrueByte()
{
    return 1;
}

__declspec(dllexport) uint16_t GetTrueUShort()
{
    return 1;
}

__declspec(dllexport) uint32_t GetTrueUInt()
{
    return 1;
}

__declspec(dllexport) uint64_t GetTrueULong()
{
    return 1;
}

__declspec(dllexport) uint32_t GetTrueBOOL()
{
    return 1;
}

__declspec(dllexport) int16_t GetTrueVariantBool()
{
    return -1;
}