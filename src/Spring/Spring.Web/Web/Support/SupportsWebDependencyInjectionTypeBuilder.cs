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

using Spring.Context;
using Spring.Proxy;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// This TypeBuilder dynamically implements the <see cref="ISupportsWebDependencyInjection"/> contract on a given type.
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal class SupportsWebDependencyInjectionTypeBuilder : InheritanceProxyTypeBuilder
    {
        private const string PROXY_TYPE_NAME = "SupportsWebDependencyInjectionControlProxy";

        private MethodInfo[] _methodsToIntercept;
        private MethodInfo _staticCallbackMethod;

        /// <summary>
        /// Creates a new TypeBuilder instance.
        /// </summary>
        /// <param name="targetType">The base type the proxy shall be derived from</param>
        /// <param name="methodsToIntercept">The methods to be injected with a call to <c>staticCallbackMethod</c></param>
        /// <param name="staticCallbackMethod">The <b>static</b> callback method to be injected into <c>methodsToIntercept</c></param>
        public SupportsWebDependencyInjectionTypeBuilder(Type targetType, MethodInfo[] methodsToIntercept, MethodInfo staticCallbackMethod)
        {
            Name = PROXY_TYPE_NAME;

            base.TargetType = targetType;

            if (methodsToIntercept == null || methodsToIntercept.Length == 0)
            {
                throw new ArgumentException("No methods specified to be intercepted");
            }
            _methodsToIntercept = methodsToIntercept;

            if (!staticCallbackMethod.IsStatic)
            {
                throw new ArgumentException("CallbackMethod must be static");
            }
            _staticCallbackMethod = staticCallbackMethod;
        }

        /// <summary>
        /// Creates a proxy that inherits the proxied object's class.
        /// </summary>
        /// <remarks/>
        /// <returns>The generated proxy type.</returns>
        public override Type BuildProxyType()
        {
            BaseType = TargetType;
            if (BaseType.IsSealed ||
                !ReflectionUtils.IsTypeVisible(BaseType, DynamicProxyManager.ASSEMBLY_NAME))
            {
                throw new ArgumentException("Inheritance proxy cannot be created for a sealed or non-public class [" +
                                            BaseType.FullName + "]");
            }

            TypeBuilder typeBuilder = CreateTypeBuilder(Name, BaseType);

            // declare field to hold reference to applicationContext
            FieldBuilder appContextField = DeclareApplicationContextInstanceField(typeBuilder);

            // create constructors
            ImplementConstructors(typeBuilder);

            // Implement IDependencyInjectionAware if necessary
            if (!typeof (ISupportsWebDependencyInjection).IsAssignableFrom(BaseType))
            {
                ImplementIDependencyInjectionAware(typeBuilder, BaseType, appContextField);
            }

            // proxy base virtual methods
            BaseProxyMethodBuilder proxyMethodBuilder =
                new SupportsWebDependencyInjectionMethodBuilder(typeBuilder, this, appContextField, _staticCallbackMethod);
            foreach (MethodInfo methodToIntercept in _methodsToIntercept)
            {
                proxyMethodBuilder.BuildProxyMethod(methodToIntercept, null);
            }

            return typeBuilder.CreateType();
        }

        /// <summary>
        /// Actually implements the <see cref="ISupportsWebDependencyInjection"/> interface.
        /// </summary>
        private void ImplementIDependencyInjectionAware(TypeBuilder typeBuilder, Type targetType, FieldInfo appContextField)
        {
            Type intf = typeof (ISupportsWebDependencyInjection);

            // Add interface declaration to type
            typeBuilder.AddInterfaceImplementation(intf);

            // get interface property
            PropertyInfo piApplicationContext = intf.GetProperty("DefaultApplicationContext");

            // define property
            string fullPropertyName = typeof (ISupportsWebDependencyInjection).FullName + "." + piApplicationContext.Name;
            PropertyBuilder appContextProperty =
                typeBuilder.DefineProperty(fullPropertyName, PropertyAttributes.None, typeof (IApplicationContext), null);

            MethodAttributes methodAtts = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                                          MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;

            // implement getter
            MethodInfo getApplicationContextMethod = piApplicationContext.GetGetMethod();
            string getterMethodName = typeof (ISupportsWebDependencyInjection).FullName + "." + getApplicationContextMethod.Name;
            MethodBuilder mbGet =
                typeBuilder.DefineMethod(getterMethodName,
                                         methodAtts,
                                         getApplicationContextMethod.CallingConvention, getApplicationContextMethod.ReturnType,
                                         Type.EmptyTypes);


            ILGenerator ilGet = mbGet.GetILGenerator();
            ilGet.Emit(OpCodes.Ldarg_0);
            ilGet.Emit(OpCodes.Ldfld, appContextField);
            ilGet.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(mbGet, getApplicationContextMethod);
            appContextProperty.SetGetMethod(mbGet);

            // implement setter
            MethodInfo setApplicationContextMethod = piApplicationContext.GetSetMethod();
            string setterMethodName = typeof (ISupportsWebDependencyInjection).FullName + "." + setApplicationContextMethod.Name;
            MethodBuilder mbSet = typeBuilder.DefineMethod(setterMethodName, methodAtts,
                                                           setApplicationContextMethod.CallingConvention,
                                                           setApplicationContextMethod.ReturnType,
                                                           new Type[] {typeof (IApplicationContext)});

            ILGenerator ilSet = mbSet.GetILGenerator();
            ilSet.Emit(OpCodes.Ldarg_0);
            ilSet.Emit(OpCodes.Ldarg_1);
            ilSet.Emit(OpCodes.Stfld, appContextField);
            ilSet.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(mbSet, setApplicationContextMethod);
            appContextProperty.SetSetMethod(mbSet);
        }

        /// <summary>
        /// Declares field that holds the <see cref="ISupportsWebDependencyInjection.DefaultApplicationContext"/>.
        /// </summary>
        private FieldBuilder DeclareApplicationContextInstanceField(TypeBuilder builder)
        {
            FieldBuilder applicationContextField;
            applicationContextField =
                builder.DefineField("_defaultApplicationContext", typeof (IApplicationContext), FieldAttributes.Private);
            return applicationContextField;
        }

        #region Public Methods

        /// <summary>
        /// Determines if the specified <paramref name="type"/>
        /// is one of those generated by this builder.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        /// <see langword="true"/> if the type is a SpringAwareControl-based proxy;
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool IsSpringAwareControlProxy(Type type)
        {
            return type.FullName.StartsWith(PROXY_TYPE_NAME);
        }

        #endregion
    }
}
