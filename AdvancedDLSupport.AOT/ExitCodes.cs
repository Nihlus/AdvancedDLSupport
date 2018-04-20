//
//  ExitCodes.cs
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

namespace AdvancedDLSupport.AOT
{
    /// <summary>
    /// Holds exit codes for the application.
    /// </summary>
    public enum ExitCodes
    {
        /// <summary>
        /// No error, all is fine.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Could not find one or more of the input assemblies.
        /// </summary>
        InputAssemblyNotFound = 1,

        /// <summary>
        /// Failed to load a given assembly.
        /// </summary>
        FailedToLoadAssembly = 2,

        /// <summary>
        /// Input arguments could not be parsed.
        /// </summary>
        InvalidArguments = 3
    }
}
