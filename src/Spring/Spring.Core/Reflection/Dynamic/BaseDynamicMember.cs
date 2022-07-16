#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.Reflection;
using System.Reflection.Emit;

namespace Spring.Reflection.Dynamic
{
    /// <summary>
    /// Base class for dynamic members.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public class BaseDynamicMember
    {
        /// <summary>
        /// Method attributes constant.
        /// </summary>
        protected const MethodAttributes METHOD_ATTRIBUTES =
            MethodAttributes.Public | MethodAttributes.HideBySig
            | MethodAttributes.NewSlot | MethodAttributes.Virtual
            | MethodAttributes.Final;

        private static ConstructorInfo invalidOperationException =
            typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) });

        /// <summary>
        /// Sets up target instance for invocation.
        /// </summary>
        /// <param name="il">IL generator to use.</param>
        /// <param name="targetType">Type of target instance.</param>
        protected static void SetupTargetInstance(ILGenerator il, Type targetType)
        {
            il.Emit(OpCodes.Ldarg_1);
            if (targetType.IsValueType)
            {
                LocalBuilder target = il.DeclareLocal(targetType);
            	il.Emit(OpCodes.Unbox, targetType);
            	il.Emit(OpCodes.Ldobj, targetType);
            	il.Emit(OpCodes.Stloc, target);
                il.Emit(OpCodes.Ldloca, target);
            }
            else
            {
                il.Emit(OpCodes.Castclass, targetType);
            }
        }

        /// <summary>
        /// Sets up invocation argument.
        /// </summary>
        /// <param name="il">IL generator to use.</param>
        /// <param name="argumentType">Argument type.</param>
        /// <param name="argumentPosition">Argument position.</param>
        protected static void SetupArgument(ILGenerator il, Type argumentType, int argumentPosition)
        {
            il.Emit(OpCodes.Ldarg, argumentPosition);
            if (argumentType.IsValueType)
            {
				il.Emit(OpCodes.Unbox, argumentType);
				il.Emit(OpCodes.Ldobj, argumentType);
            }
            else
            {
                il.Emit(OpCodes.Castclass, argumentType);
            }
        }

        /// <summary>
        /// Generates method invocation code.
        /// </summary>
        /// <param name="il">IL generator to use.</param>
        /// <param name="isStatic">Flag specifying whether method is static.</param>
        /// <param name="isValueType">Flag specifying whether method is on the value type.</param>
        /// <param name="method">Method to invoke.</param>
        protected static void InvokeMethod(ILGenerator il, bool isStatic, bool isValueType, MethodInfo method)
        {
            if (isStatic || isValueType)
            {
                il.EmitCall(OpCodes.Call, method, null);
            }
            else
            {
                il.EmitCall(OpCodes.Callvirt, method, null);
            }
        }

        /// <summary>
        /// Generates code to process return value if necessary.
        /// </summary>
        /// <param name="il">IL generator to use.</param>
        /// <param name="returnValueType">Type of the return value.</param>
        protected static void ProcessReturnValue(ILGenerator il, Type returnValueType)
        {
            if (returnValueType == typeof(void))
            {
                il.Emit(OpCodes.Ldnull);
            }
            else if (returnValueType.IsValueType)
            {
                il.Emit(OpCodes.Box, returnValueType);
            }
        }

        /// <summary>
        /// Generates code that throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="il">IL generator to use.</param>
        /// <param name="message">Error message to use.</param>
        protected static void ThrowInvalidOperationException(ILGenerator il, string message)
        {
            il.Emit(OpCodes.Ldstr, message);
            il.Emit(OpCodes.Newobj, invalidOperationException);
            il.Emit(OpCodes.Throw);
        }
//#endif
    }
}
