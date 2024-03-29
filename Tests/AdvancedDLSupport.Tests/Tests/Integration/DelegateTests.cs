//
//  DelegateTests.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
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

using System;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Tests.Data;
using AdvancedDLSupport.Tests.Data.Delegates;
using AdvancedDLSupport.Tests.TestBases;
using Xunit;

namespace AdvancedDLSupport.Tests.Integration;

public class DelegateTests
{
    private const string _libraryName = "GenericDelegateTests";

    public class FromManagedToNative : LibraryTestBase<IDelegateLibrary>
    {
        public FromManagedToNative()
            : base(_libraryName)
        {
        }

        [Fact]
        public void NativeCanPassNullDelegate()
        {
            Assert.Equal(1, Library.IsNullDelegate(null));
        }

        [Fact]
        public void NativeCanCallAction()
        {
            bool ranAction = false;
            Library.ExecuteAction(() => ranAction = true);

            Assert.True(ranAction);
        }

        [Fact]
        public void NativeCanCallActionWithParameter()
        {
            bool ranAction = false;
            int result = 0;
            Library.ExecuteActionT1(x =>
            {
                ranAction = true;
                result = x;
            });

            Assert.True(ranAction);
            Assert.Equal(5, result);
        }

        [Fact]
        public void NativeCanCallActionWithParameterFromManaged()
        {
            bool ranAction = false;
            int result = 0;
            int input = 13;
            Library.ExecuteActionT1WithParameter(
                x =>
                {
                    ranAction = true;
                    result = x;
                },
                input);

            Assert.True(ranAction);
            Assert.Equal(input, result);
        }

        [Fact]
        public void NativeCanCallFunc()
        {
            var result = Library.ExecuteFuncT1(() => 5);

            Assert.Equal(5, result);
        }

        [Fact]
        public void NativeCanCallFuncWithParameter()
        {
            var result = Library.ExecuteFuncT1T2(x => 5 * x);

            Assert.Equal(25, result);
        }

        [Fact]
        public void NativeCanCallFuncWithParameterFromManaged()
        {
            int input = 13;
            var result = Library.ExecuteFuncT1T2WithParameter(
                x => 5 * x,
                input);

            Assert.Equal(5 * input, result);
        }

        [Fact(Skip = "Not working due to CLR limitations.")]
        public void NativeCanCallNestedAction()
        {
            bool ranAction = false;
            Library.ExecuteActionT1Nested
            (
                action =>
                {
                    ranAction = true;
                    action(5);
                }
            );

            Assert.True(ranAction);
        }

        [Fact(Skip = "Not working due to CLR limitations.")]
        public void NativeCanCallNestedFunc()
        {
            var result = Library.ExecuteFuncT1T2Nested
            (
                func => func(5)
            );

            Assert.Equal(25, result);
        }
    }

    public class FromNativeToManaged : LibraryTestBase<IDelegateLibrary>
    {
        public FromNativeToManaged()
            : base(_libraryName)
        {
        }

        [Fact]
        public void ManagedCanGetNullDelegate()
        {
            Assert.Null(Library.GetNullDelegate());
        }

        [Fact]
        public void ManagedCanCallAction()
        {
            var action = Library.GetNativeAction();
            action();
        }

        [Fact]
        public void ManagedCanCallActionWithParameter()
        {
            var action = Library.GetNativeActionT1();
            action(5);
        }

        [Fact]
        public void ManagedCanCallFunc()
        {
            var func = Library.GetNativeFuncT1();
            var result = func();

            Assert.Equal(5, result);
        }

        [Fact]
        public void ManagedCanCallFuncWithParameter()
        {
            var func = Library.GetNativeFuncT1T2();
            var result = func(5);

            Assert.Equal(25, result);
        }

        [Fact(Skip = "Not working due to CLR limitations.")]
        public void ManagedCanCallNestedAction()
        {
            var action = Library.GetNativeActionT1Nested();

            int result = 0;
            action(i => result = i);

            Assert.Equal(5, result);
        }

        [Fact(Skip = "Not working due to CLR limitations.")]
        public void ManagedCanCallNestedFunc()
        {
            var func = Library.GetNativeFuncT1T2Nested();

            int result = 0;
            func(i => result = i * 5);

            Assert.Equal(25, result);
        }
    }

    public class DelegateLifetime : LibraryTestBase<IDelegateLibrary>
    {
        private static WeakReference<DelegateLibraryDelegates.Action> CreateAction
        (
            bool keepAlive,
            Action<DelegateLibraryDelegates.Action> executeAction
        )
        {
            bool ranAction = false;
            DelegateLibraryDelegates.Action action = () => ranAction = true;

            executeAction(action);

            Assert.True(ranAction);
            return new WeakReference<DelegateLibraryDelegates.Action>(action);
        }

        public DelegateLifetime()
            : base(_libraryName)
        {
        }

        [Fact]
        public void IsDelegateKeptAliveByDefault()
        {
            var weakRef = CreateAction(false, x => Library.ExecuteActionDefault(x));

            GC.Collect();

            Assert.True(weakRef.TryGetTarget(out var _));
        }

        [Fact]
        public void PersistentLifetimeDelegateGetsNotCollected()
        {
            var weakRef = CreateAction(false, x => Library.ExecuteActionLifetimePersistent(x));

            GC.Collect();

            Assert.True(weakRef.TryGetTarget(out var _));
        }

        [Fact]
        public void NoneLifetimeDelegateGetsCollected()
        {
            if (RuntimeInformation.FrameworkDescription.Contains("Mono"))
            {
                return; // Do not test on mono, as it does not collect these (at least not immediately)
            }

            var weakRef = CreateAction(false, x => Library.ExecuteActionLifetimeNone(x));

            GC.Collect();

            Assert.False(weakRef.TryGetTarget(out var _));
        }

        [Fact]
        public void CallOnlyLifetimeDelegateGetsCollected()
        {
            if (RuntimeInformation.FrameworkDescription.Contains("Mono"))
            {
                return; // Do not test on mono, as it does not collect these (at least not immediately)
            }

            var weakRef = CreateAction(false, x => Library.ExecuteActionLifetimeCallOnly(x));
            GC.Collect();

            Assert.False(weakRef.TryGetTarget(out var _));
        }
    }
}
