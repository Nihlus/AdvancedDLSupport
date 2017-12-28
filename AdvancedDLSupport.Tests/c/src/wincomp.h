//
// Created by jarl on 12/25/17.
//

#ifndef C_WINCOMP_H
#define C_WINCOMP_H

#if _MSC_VER
#include <stdint.h>
#endif

#if !_MSC_VER
#define __declspec(dllexport)
#define __stdcall
#endif

#endif //C_WINCOMP_H
