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

#region Imports

using System.Reflection;
using System.Reflection.Emit;

using Spring.Util;

#endregion

namespace Spring.Proxy
{
    /// <summary>
    /// Base class for method builders that contains common functionalities.
    /// </summary>
    /// <author>Bruno Baia</author>
    public abstract class AbstractProxyMethodBuilder : IProxyMethodBuilder
    {
        #region Fields

        /// <summary>
        /// The type builder to use.
        /// </summary>
        protected TypeBuilder typeBuilder;

        /// <summary>
        /// The <see cref="IProxyTypeGenerator"/> implementation to use.
        /// </summary>
        protected IProxyTypeGenerator proxyGenerator;

        /// <summary>
        /// Indicates whether interfaces should be implemented explicitly.
        /// </summary>
        protected bool explicitImplementation;

        #endregion

        #region Constructor(s) / Destructor

        /// <summary>
        /// Creates a new instance of the method builder.
        /// </summary>
        /// <param name="typeBuilder">The type builder to use.</param>
        /// <param name="proxyGenerator">
        /// The <see cref="IProxyTypeGenerator"/> implementation to use.
        /// </param>
        /// <param name="explicitImplementation">
        /// <see langword="true"/> if the interface is to be
        /// implemented explicitly; otherwise <see langword="false"/>.
        /// </param>
        public AbstractProxyMethodBuilder(TypeBuilder typeBuilder,
            IProxyTypeGenerator proxyGenerator, bool explicitImplementation)
        {
            this.typeBuilder = typeBuilder;
            this.proxyGenerator = proxyGenerator;
            this.explicitImplementation = explicitImplementation;
        }

        #endregion

        #region IProxyMethodBuilder Members

        /// <summary>
        /// Dynamically builds proxy method.
        /// </summary>
        /// <param name="method">The method to proxy.</param>
        /// <param name="interfaceMethod">
        /// The interface definition of the method, if applicable.
        /// </param>
        /// <returns>
        /// The <see cref="System.Reflection.Emit.MethodBuilder"/> for the proxy method.
        /// </returns>
        public virtual MethodBuilder BuildProxyMethod(MethodInfo method, MethodInfo interfaceMethod)
        {
            MethodBuilder methodBuilder =
                DefineMethod(method, interfaceMethod, explicitImplementation);

            ILGenerator il = methodBuilder.GetILGenerator();

            GenerateMethod(il, method, interfaceMethod);

            il.Emit(OpCodes.Ret);

            if (explicitImplementation ||
                (interfaceMethod != null && interfaceMethod.Name != method.Name))
            {
                typeBuilder.DefineMethodOverride(methodBuilder, interfaceMethod);
            }

            return methodBuilder;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the proxy instance on stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        protected virtual void PushProxy(ILGenerator il)
        {
            proxyGenerator.PushProxy(il);
        }

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the target instance on which calls should be delegated to.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        protected virtual void PushTarget(ILGenerator il)
        {
            proxyGenerator.PushTarget(il);
        }

        /// <summary>
        /// Defines proxy method for the target object.
        /// </summary>
        /// <param name="method">The method to proxy.</param>
        /// <param name="intfMethod">
        /// The interface definition of the method, if applicable.
        /// </param>
        /// <param name="explicitImplementation">
        /// <see langword="true"/> if the supplied <paramref name="intfMethod"/> is to be
        /// implemented explicitly; otherwise <see langword="false"/>.
        /// </param>
        /// <returns>
        /// The <see cref="System.Reflection.Emit.MethodBuilder"/> for the proxy method.
        /// </returns>
        protected virtual MethodBuilder DefineMethod(
            MethodInfo method, MethodInfo intfMethod, bool explicitImplementation)
        {
            MethodBuilder methodBuilder;
            string name;
            MethodAttributes attributes;

            if (intfMethod == null)
            {
                name = method.Name;
                attributes = MethodAttributes.Public | MethodAttributes.ReuseSlot
                    | MethodAttributes.HideBySig | MethodAttributes.Virtual;
            }
            else
            {
                attributes = MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot | MethodAttributes.Virtual
                    | MethodAttributes.Final;

                if (explicitImplementation || method.Name.IndexOf('.') != -1)
                {
                    name = String.Format("{0}.{1}",
                        intfMethod.DeclaringType.FullName, intfMethod.Name);
                    attributes |= MethodAttributes.Private;
                }
                else
                {
                    name = intfMethod.Name;
                    attributes |= MethodAttributes.Public;
                }
            }

            if ((intfMethod != null && intfMethod.IsSpecialName) || method.IsSpecialName)
            {
                attributes |= MethodAttributes.SpecialName;
            }

            methodBuilder = typeBuilder.DefineMethod(name, attributes,
                method.CallingConvention, method.ReturnType,
                ReflectionUtils.GetParameterTypes(method.GetParameters()));

            DefineGenericParameters(methodBuilder, method);
            //DefineParameters(methodBuilder, method);

            return methodBuilder;
        }

        /*
        /// <summary>
        /// Defines method parameters based on proxied method metadata.
        /// </summary>
        /// <param name="methodBuilder">
        /// The <see cref="System.Reflection.Emit.MethodBuilder"/> to use.
        /// </param>
        /// <param name="method">The method to proxy.</param>
        protected void DefineParameters(MethodBuilder methodBuilder, MethodInfo method)
        {
            int n = 1;
            foreach (ParameterInfo param in method.GetParameters())
            {
                ParameterBuilder pb = methodBuilder.DefineParameter(n, param.Attributes, param.Name);
                n++;
            }
        }
        */

        /// <summary>
        /// Defines generic method parameters based on proxied method metadata.
        /// </summary>
        /// <param name="methodBuilder">
        /// The <see cref="System.Reflection.Emit.MethodBuilder"/> to use.
        /// </param>
        /// <param name="method">The method to proxy.</param>
        protected void DefineGenericParameters(MethodBuilder methodBuilder, MethodInfo method)
        {
            if (method.IsGenericMethodDefinition)
            {
                Type[] genericArguments = method.GetGenericArguments();

                // define generic parameters
                GenericTypeParameterBuilder[] gtpBuilders =
                    methodBuilder.DefineGenericParameters(ReflectionUtils.GetGenericParameterNames(genericArguments));

                // define constraints for each generic parameter
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    gtpBuilders[i].SetGenericParameterAttributes(genericArguments[i].GenericParameterAttributes);

                    Type[] constraints = genericArguments[i].GetGenericParameterConstraints();
                    System.Collections.Generic.List<Type> interfaces = new System.Collections.Generic.List<Type>(constraints.Length);
                    foreach (Type constraint in constraints)
                    {
                        if (constraint.IsClass)
                            gtpBuilders[i].SetBaseTypeConstraint(constraint);
                        else
                            interfaces.Add(constraint);
                    }
                    gtpBuilders[i].SetInterfaceConstraints(interfaces.ToArray());
                }
            }
        }

        /// <summary>
        /// Generates the proxy method.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        /// <param name="interfaceMethod">
        /// The interface definition of the method, if applicable.
        /// </param>
        protected abstract void GenerateMethod(
            ILGenerator il, MethodInfo method, MethodInfo interfaceMethod);

        /// <summary>
        /// Calls target method directly.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="targetMethod">The method to invoke.</param>
        protected virtual void CallDirectTargetMethod(
            ILGenerator il, MethodInfo targetMethod)
        {
            // TODO (EE): check for null and interface type and throw NotSupportedException
            // setup target instance for CallAssertUnderstands
            PushTarget(il);
            CallAssertUnderstands(il, targetMethod, "target");

            // setup target and cast to type method is on
            PushTarget(il);
            il.Emit(OpCodes.Castclass, targetMethod.DeclaringType);

            // setup parameters for call
            ParameterInfo[] paramArray = targetMethod.GetParameters();
            for (int i = 0; i < paramArray.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_S, i + 1);
            }

            // call method
            il.EmitCall(OpCodes.Callvirt, targetMethod, null);
        }

        /// <summary>
        /// Emits code to ensure that target on stack understands the method and throw a sensible exception otherwise.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to test for</param>
        /// <param name="targetName">the name of the target to be used in error messages</param>
        protected virtual void CallAssertUnderstands(ILGenerator il, MethodInfo method, string targetName)
        {
            il.Emit(OpCodes.Ldstr, targetName);
            il.Emit(OpCodes.Ldtoken, method.DeclaringType);
            il.Emit(OpCodes.Call, References.GetTypeFromHandleMethod);
            //il.Emit(OpCodes.Ldstr, string.Format("Interface method '{0}.{1}()' was not handled by any interceptor and the target does not implement this method.", method.DeclaringType.FullName, method.Name));
            il.Emit(OpCodes.Call, References.UnderstandsMethod);
        }

        /// <summary>
        /// Calls base method directly.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="method">The method to proxy.</param>
        protected virtual void CallDirectBaseMethod(ILGenerator il, MethodInfo method)
        {
            // TODO (EE): check for null and interface type and throw NotSupportedException
            // setup proxy instance for CallAssertUnderstands
            PushProxy(il);
            CallAssertUnderstands(il, method, "base");

            // setup proxy and cast to type method is on
            PushProxy(il);
            il.Emit(OpCodes.Castclass, method.DeclaringType);

            // setup parameters for call
            ParameterInfo[] paramArray = method.GetParameters();
            for (int i = 0; i < paramArray.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_S, i + 1);
            }

            // call method
            il.EmitCall(OpCodes.Call, method, null);
        }

        /// <summary>
        /// Replaces a raw reference with a reference to a proxy.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the target object returns reference to itself -- 'this' --
        /// we need to treat it as a special case and return a reference
        /// to a proxy object instead.
        /// </p>
        /// </remarks>
        /// <param name="il">The IL generator to use.</param>
        /// <param name="returnValue">The location of the return value.</param>
        protected virtual void ProcessReturnValue(ILGenerator il, LocalBuilder returnValue)
        {
            Label jmpMethodReturn = il.DefineLabel();

            // check if target method returned 'this', reference to target
            PushTarget(il);
            il.Emit(OpCodes.Ldloc, returnValue);
            il.Emit(OpCodes.Bne_Un_S, jmpMethodReturn);

            // check if proxy can be casted to the return type
            PushProxy(il);
            il.Emit(OpCodes.Isinst, returnValue.LocalType);
            il.Emit(OpCodes.Brfalse_S, jmpMethodReturn);

            // if it did, return reference to this proxy instead
            PushProxy(il);
            il.Emit(OpCodes.Stloc, returnValue);

            il.MarkLabel(jmpMethodReturn);
        }

        /// <summary>
        /// Generates code that throws <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <param name="il">IL generator to use.</param>
        /// <param name="exceptionType">the type of the exception to throw</param>
        /// <param name="message">Error message to use.</param>
        protected static void EmitThrowException(ILGenerator il, Type exceptionType, string message)
        {
            ConstructorInfo NewException = exceptionType.GetConstructor(new Type[] { typeof(string) });

            il.Emit(OpCodes.Ldstr, message);
            il.Emit(OpCodes.Newobj, NewException);
            il.Emit(OpCodes.Throw);
        }

        #endregion
    }

    #region References helper class definition

    internal struct References
    {
        // methods
        public static readonly MethodInfo GetTypeFromHandleMethod =
            typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });

        public static readonly MethodInfo UnderstandsMethod =
            typeof(AssertUtils).GetMethod("Understands", new Type[] { typeof(object), typeof(string), typeof(Type) });
    }

    #endregion
}
