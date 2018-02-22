//
// Created by jarl on 12/25/17.
//

#ifndef C_WINCOMP_H
#define C_WINCOMP_H

#if __APPLE__ && __MACH__
    #include <stddef.h>
    #include <stdint.h>

    typedef uint16_t char16_t;
    typedef uint32_t char32_t;
#else
    #include <uchar.h>
#endif

#if _MSC_VER
    #include <Windows.h>
    #include <stdint.h>
    #include <Tchar.h>

    typedef const wchar_t* BCSTR;
    typedef const char* LPCSTR;
    typedef const wchar_t* LPWCSTR;

    #ifdef UNICODE
        #define LPTSTR(value) L ##value;
        typedef LPWCSTR LPTCSTR;
    #else
        #define LPTSTR(value) value;
        typedef LPCSTR LPTCSTR;
    #endif
#endif

#if !_MSC_VER
    #define __declspec(dllexport)
    #define __stdcall

    typedef char16_t* BSTR;
    typedef const char16_t* BCSTR;


    typedef char* LPSTR;
    typedef const char* LPCSTR;

    typedef char16_t* LPWSTR;
    typedef const char16_t* LPWCSTR;

    #ifdef UNICODE
        #define LPTSTR(value) L ##value;
        typedef LPWSTR LPTSTR;
        typedef LPWCSTR LPTCSTR;
    #else
        #define LPTSTR(value) value;
        typedef LPSTR LPTSTR;
        typedef LPCSTR LPTCSTR;
    #endif
#endif


#endif //C_WINCOMP_H
