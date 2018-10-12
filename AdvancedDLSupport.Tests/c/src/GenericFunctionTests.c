//
// Created by jarl on 10/9/18.
//

#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>
#include "comp.h"
#include "TestStruct.h"

size_t GetSizef(float value)
{
    return sizeof(value);
}

size_t GetSized(double value)
{
    return sizeof(value);
}

size_t GetSizeb(uint8_t value)
{
    return sizeof(value);
}

size_t GetSizes(int16_t value)
{
    return sizeof(value);
}

size_t GetSizei(int32_t value)
{
    return sizeof(value);
}

size_t GetSizest(TestStruct value)
{
    return sizeof(value);
}

float GetValuef()
{
    return 1.0;
}

double GetValued()
{
    return 2.0;
}

uint8_t GetValueb()
{
    return 4;
}

int16_t GetValues()
{
    return 8;
}

int32_t GetValuei()
{
    return 16;
}

TestStruct GetValuest()
{
    TestStruct testStruct = { 32, 64 };

    return testStruct;
}
