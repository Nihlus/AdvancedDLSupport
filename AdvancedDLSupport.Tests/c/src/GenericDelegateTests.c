//
//  GenericDelegateTests.c
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) 2018 Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

#include <stdlib.h>
#include <stdio.h>
#include "comp.h"

typedef void (*Action)();
typedef void (*ActionT1)(int t1);
typedef void (*ActionT1Nested)(ActionT1 action);

typedef int (*FuncT1)();
typedef int (*FuncT1T2)(int t2);
typedef int (*FuncT1T2Nested)(FuncT1T2 func);

void NativeActionT1(int t1);

int NativeFuncT1T2(int t2);

__declspec(dllexport) void ExecuteAction(Action action)
{
	action();
}

__declspec(dllexport) void ExecuteActionT1(ActionT1 action)
{
	action(5);
}

__declspec(dllexport) void ExecuteActionT1Nested(ActionT1Nested action)
{
	fprintf(stdout, "In nested, seeing function pointer as %x", (unsigned int)&NativeActionT1);
	action(&NativeActionT1);
}

__declspec(dllexport) int ExecuteFuncT1(FuncT1 func)
{
	return func();
}

__declspec(dllexport) int ExecuteFuncT1T2(FuncT1T2 func)
{
	return func(5);
}

__declspec(dllexport) int ExecuteFuncT1T2Nested(FuncT1T2Nested func)
{
	return func(&NativeFuncT1T2);
}

__declspec(dllexport) void NativeAction()
{
	fprintf(stdout, "Living in native land!");
}

__declspec(dllexport) void NativeActionT1(int t1)
{
	fprintf(stdout, "Living in native land, seeing parameter as %d!", t1);
}

__declspec(dllexport) Action GetNativeAction()
{
	return &NativeAction;
}

__declspec(dllexport) ActionT1 GetNativeActionT1()
{
	return &NativeActionT1;
}

__declspec(dllexport) ActionT1Nested GetNativeActionT1Nested()
{
	return &ExecuteActionT1;
}

__declspec(dllexport) int NativeFuncT1()
{
	return 5;
}

__declspec(dllexport) FuncT1 GetNativeFuncT1()
{
	return &NativeFuncT1;
}

__declspec(dllexport) int NativeFuncT1T2(int t2)
{
	return t2 * 5;
}

__declspec(dllexport) FuncT1T2 GetNativeFuncT1T2()
{
	return &NativeFuncT1T2;
}

__declspec(dllexport) FuncT1T2Nested GetNativeFuncT1T2Nested()
{
	return &ExecuteFuncT1T2;
}