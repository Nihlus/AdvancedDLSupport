//
//  DllConfigurationPlatformHelper.cs
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

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using static Mono.DllMap.DllMapArchitecture;
using static Mono.DllMap.DllMapOS;
using static Mono.DllMap.DllMapWordSize;

namespace Mono.DllMap.Utility;

/// <summary>
/// Helper class for determining the current platform.
/// </summary>
[PublicAPI]
public static class DllConfigurationPlatformHelper
{
    /// <summary>
    /// Gets the current platform that we're running on.
    /// </summary>
    /// <returns>The current platform.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown if the current platform couldn't be detected.</exception>
    [PublicAPI, Pure]
    public static DllMapOS GetCurrentPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Linux;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Windows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return OSX;
        }

        var operatingDesc = RuntimeInformation.OSDescription.ToUpperInvariant();
        foreach (var system in Enum.GetValues(typeof(DllMapOS)).Cast<DllMapOS>()
                     .Except(new[] { Linux, Windows, OSX }))
        {
            if (operatingDesc.Contains(system.ToString().ToUpperInvariant()))
            {
                return system;
            }
        }

        throw new PlatformNotSupportedException($"Couldn't detect platform: {RuntimeInformation.OSDescription}");
    }

    /// <summary>
    /// Gets the current process architecture of the machine.
    /// </summary>
    /// <returns>The runtime architecture.</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown if the architecture couldn't be detected.</exception>
    [PublicAPI, Pure]
    public static DllMapArchitecture GetCurrentRuntimeArchitecture()
    {
        #pragma warning disable SA1513
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.Arm:
            {
                return ARM;
            }
            case Architecture.X64:
            {
                return x86_64;
            }
            case Architecture.X86:
            {
                return x86;
            }
        }

        typeof(object).Module.GetPEKind(out _, out var machine);
        switch (machine)
        {
            case ImageFileMachine.I386:
            {
                return x86;
            }
            case ImageFileMachine.AMD64:
            {
                return x86_64;
            }
            case ImageFileMachine.ARM:
            {
                return ARM;
            }
            case ImageFileMachine.IA64:
            {
                return IA64;
            }
        }
        #pragma warning restore SA1513

        throw new PlatformNotSupportedException("Couldn't detect the current architecture.");
    }

    /// <summary>
    /// Gets the word size of the runtime.
    /// </summary>
    /// <returns>The word size.</returns>
    [PublicAPI, Pure]
    public static DllMapWordSize GetRuntimeWordSize()
    {
        var pointerSize = IntPtr.Size;

        if (pointerSize == 4)
        {
            return Word32;
        }

        return Word64;
    }
}
