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


#if __APPLE__ && __MACH__
#include <stddef.h>
#include <stdint.h>

typedef uint16_t char16_t;
typedef uint32_t char32_t;
#else
#include <uchar.h>
#endif

#endif //C_WINCOMP_H
