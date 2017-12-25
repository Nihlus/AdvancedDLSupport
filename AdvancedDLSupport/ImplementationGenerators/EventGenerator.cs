using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using AdvancedDLSupport.Attributes;

namespace AdvancedDLSupport.ImplementationGenerators
{
    /// <summary>
    /// Generates implementations for events.
    /// </summary>
    internal class EventImplementationGenerator : ImplementationGeneratorBase<EventInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodImplementationGenerator"/> class.
        /// </summary>
        /// <param name="targetModule">The module in which the method implementation should be generated.</param>
        /// <param name="targetType">The type in which the method implementation should be generated.</param>
        /// <param name="targetTypeConstructorIL">The IL generator for the target type's constructor.</param>
        public EventImplementationGenerator(ModuleBuilder targetModule, TypeBuilder targetType, ILGenerator targetTypeConstructorIL)
            : base(targetModule, targetType, targetTypeConstructorIL)
        {
        }

        /// <inheritdoc />
        public override void GenerateImplementation(EventInfo eventInfo)
        {
            var uniqueIdentifier = Guid.NewGuid().ToString().Replace("-", "_");

            var metadataAttribute = eventInfo.GetCustomAttribute<NativeFunctionAttribute>() ??
                                    new NativeFunctionAttribute(eventInfo.Name);

            // Create an event
            var eventDefinition = TargetType.DefineEvent(eventInfo.Name, EventAttributes.None, eventInfo.EventHandlerType);
            
            // Create an event invoker method
            GenerateEventInvoker(eventInfo, uniqueIdentifier, eventDefinition);
        }

        private void AugmentHostingTypeConstructor(MethodInfo method, NativeFunctionAttribute metadataAttribute, Type delegateBuilderType, FieldInfo delegateField)
        {
            var entrypointName = metadataAttribute.Entrypoint ?? method.Name;
            var loadFuncMethod = typeof(AnonymousImplementationBase).GetMethod
            (
                "LoadFunction",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            // Assign Delegate from Function Pointer
            TargetTypeConstructorIL.Emit(OpCodes.Ldarg_0); // This is for storing field delegate, it needs the "this" reference
            TargetTypeConstructorIL.Emit(OpCodes.Ldarg_0);
            TargetTypeConstructorIL.Emit(OpCodes.Ldstr, entrypointName);
            TargetTypeConstructorIL.EmitCall(OpCodes.Call, loadFuncMethod.MakeGenericMethod(delegateBuilderType), null);
            TargetTypeConstructorIL.Emit(OpCodes.Stfld, delegateField);
        }

        private void GenerateEventInvoker(EventInfo eventInfo, string uniqueIdentifier, EventBuilder eventDefinition)
        {
            var methodBuilder = eventInfo.EventHandlerType == typeof(EventHandler) ?
            TargetType.DefineMethod // EventHandler<obj, int>
            (
                $"{eventInfo.Name}_invoker_{uniqueIdentifier}",
                MethodAttributes.Private,
                CallingConventions.Standard,
                typeof(void),
                eventInfo.EventHandlerType.GenericTypeArguments
            )
            :
            TargetType.DefineMethod // Delegate(int)
            (
                $"{eventInfo.Name}_invoker_{uniqueIdentifier}",
                MethodAttributes.Private,
                CallingConventions.Standard,
                typeof(void),
                eventInfo.EventHandlerType
            );

            // Let's implement a method that simply invoke an event
            var methodIL = methodBuilder.GetILGenerator();
            methodIL.Emit(OpCodes.Ldarg_0);
            methodIL.EmitCall
            (
                OpCodes.Call,
                eventInfo.GetOtherMethods().First
                (
                    m =>
                        m.Name == "Invoke"
                ),
                null
            );
            methodIL.Emit(OpCodes.Ret);
        }

        private TypeBuilder GenerateDelegateType(MethodInfo method, string uniqueIdentifier, NativeFunctionAttribute metadataAttribute, IEnumerable<ParameterInfo> parameters)
        {
            // Declare a delegate type
            var delegateBuilder = TargetModule.DefineType
            (
                $"{method.Name}_dt_{uniqueIdentifier}",
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.AnsiClass | TypeAttributes.AutoClass,
                typeof(MulticastDelegate)
            );

            var attributeConstructors = typeof(UnmanagedFunctionPointerAttribute).GetConstructors();
            var functionPointerAttributeBuilder = new CustomAttributeBuilder
            (
                attributeConstructors[1],
                new object[] { metadataAttribute.CallingConvention }
            );

            delegateBuilder.SetCustomAttribute(functionPointerAttributeBuilder);

            var delegateCtorBuilder = delegateBuilder.DefineConstructor
            (
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                CallingConventions.Standard,
                new[] { typeof(object), typeof(IntPtr) }
            );

            delegateCtorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);

            var delegateMethodBuilder = delegateBuilder.DefineMethod
            (
                "Invoke",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                method.ReturnType,
                parameters.Select(p => p.ParameterType).ToArray()
            );

            delegateMethodBuilder.SetImplementationFlags(MethodImplAttributes.Runtime | MethodImplAttributes.Managed);
            return delegateBuilder;
        }
    }
}
