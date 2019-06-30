//
//  IDelegateLibrary.cs
//
//  Copyright (c) 2018 Firwood Software
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

#pragma warning disable SA1600, CS1591

namespace AdvancedDLSupport.Tests.Data
{
    public interface IDelegateLibrary
    {
        void ExecuteAction(DelegateLibraryDelegates.Action action);

        void ExecuteActionT1(DelegateLibraryDelegates.ActionInt action);

        void ExecuteActionT1Nested(DelegateLibraryDelegates.ActinIntInAction action);

        int ExecuteFuncT1(DelegateLibraryDelegates.IntFunc func);

        int ExecuteFuncT1T2(DelegateLibraryDelegates.IntFuncInt func);

        int ExecuteFuncT1T2Nested(DelegateLibraryDelegates.IntFuncIntInFuncInt func);

        DelegateLibraryDelegates.Action GetNativeAction();

        DelegateLibraryDelegates.ActionInt GetNativeActionT1();

        DelegateLibraryDelegates.ActinIntInAction GetNativeActionT1Nested();

        DelegateLibraryDelegates.IntFunc GetNativeFuncT1();

        DelegateLibraryDelegates.IntFuncInt GetNativeFuncT1T2();

        DelegateLibraryDelegates.IntFuncIntInFuncInt GetNativeFuncT1T2Nested();

        [NativeSymbol(entrypoint: "ExecuteAction")]
        void ExecuteActionDefault(DelegateLibraryDelegates.Action action);

        [NativeSymbol(entrypoint: "ExecuteAction")]
        void ExecuteActionLifetimeNone([DelegateLifetime(DelegateLifetime.UserManaged)] DelegateLibraryDelegates.Action action);

        [NativeSymbol(entrypoint: "ExecuteAction")]
        void ExecuteActionLifetimePersistent([DelegateLifetime(DelegateLifetime.Persistent)] DelegateLibraryDelegates.Action action);

        [NativeSymbol(entrypoint: "ExecuteAction")]
        void ExecuteActionLifetimeCallOnly([DelegateLifetime(DelegateLifetime.CallOnly)] DelegateLibraryDelegates.Action action);

        DelegateLibraryDelegates.Action GetNullDelegate();

        int IsNullDelegate(DelegateLibraryDelegates.Action action);
    }
}
