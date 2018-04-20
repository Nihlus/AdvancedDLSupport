//
//  ICallWrapper.cs
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

using System.Reflection.Emit;
using AdvancedDLSupport.Pipeline;
using AdvancedDLSupport.Reflection;
using JetBrains.Annotations;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Represents a wrapper emitter that accepts a method, emits arbitrary prologue instructions, calls the method, and
    /// then emits arbitrary epilogue instructions. Typically, the input method is passed through without modifications.
    /// </summary>
    public interface ICallWrapper : IImplementationGenerator<IntrospectiveMethodInfo>
    {
        /// <summary>
        /// Determines whether or not the call wrapper is applicable for the given method definition.
        /// </summary>
        /// <param name="method">The method definition.</param>
        /// <param name="options">The relevant implementation options.</param>
        /// <returns>true if the wrapper is applicable; otherwise, false.</returns>
        bool IsApplicable([NotNull] IntrospectiveMethodInfo method, ImplementationOptions options);

        /// <summary>
        /// Emits the wrapper prologue, that is, the instructions before the method call. The evaluation stack is clean
        /// at the start of this method. Immediately following this method, a call to the wrapped method will be made.
        /// </summary>
        /// <param name="il">The generator where the instructions will be emitted.</param>
        /// <param name="workUnit">The method being worked on.</param>
        void EmitPrologue([NotNull] ILGenerator il, [NotNull] PipelineWorkUnit<IntrospectiveMethodInfo> workUnit);

        /// <summary>
        /// Emits the wrapper prologue, that is, the instructions after the method call. The return value (if any) of
        /// the wrapped method will be on top of the evaluation stack at the beginning of this method. The value on top
        /// of the evaluation stack at the end of the epilogue (if any) will be returned to the caller.
        /// </summary>
        /// <param name="il">The generator where the instructions will be emitted.</param>
        /// <param name="workUnit">The method being worked on.</param>
        void EmitEpilogue([NotNull] ILGenerator il, [NotNull] PipelineWorkUnit<IntrospectiveMethodInfo> workUnit);
    }
}
