#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

using Spring.Objects.Factory.Config;
using Spring.Util;
using Spring.Proxy;
using Common.Logging;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Enhances Configuration classes by generating a dynamic proxy capable of 
    /// interacting with the Spring container to respect object semantics.
    /// </summary>
    /// <author>Chris Beams</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Bruno Baia (.NET)</author>
    /// <seealso cref="ConfigurationClassPostProcessor"/>
    public class ConfigurationClassEnhancer
    {
        private IConfigurationClassInterceptor interceptor;

        /// <summary>
        /// Creates a new instance of the <see cref="ConfigurationClassEnhancer"/> class.
        /// </summary>
        /// <param name="objectFactory">
        /// The supplied ObjectFactory to check for the existence of object definitions.
        /// </param>
    	public ConfigurationClassEnhancer(IConfigurableListableObjectFactory objectFactory) 
        {
		    AssertUtils.ArgumentNotNull(objectFactory, "objectFactory");

            this.interceptor = new ConfigurationClassInterceptor(objectFactory);
	    }

        /// <summary>
        /// Generates a dynamic subclass of the specified Configuration class with a
        /// container-aware interceptor capable of respecting scoping and other bean semantics.
        /// </summary>
        /// <param name="configClass">The Configuration class.</param>
        /// <returns>The enhanced subclass.</returns>
        public Type Enhance(Type configClass)
        {
            ConfigurationClassProxyTypeBuilder proxyTypeBuilder = new ConfigurationClassProxyTypeBuilder(configClass, this.interceptor);
            return proxyTypeBuilder.BuildProxyType();
        }

        /// <summary>
        /// Intercepts the invocation of any <see cref="ObjectDefAttribute"/>-decorated methods in order 
        /// to ensure proper handling of object semantics such as scoping and AOP proxying.
        /// </summary>
        public interface IConfigurationClassInterceptor
        {
            /// <summary>
            /// Process the <see cref="ObjectDefAttribute"/>-decorated method to check 
            /// for the existence of this object.
            /// </summary>
            /// <param name="method">The method providing the object definition.</param>
            /// <param name="instance">When this method returns true, contains the object definition.</param>
            /// <returns>true if the object exists; otherwise, false.</returns>
            bool ProcessDefinition(MethodInfo method, out object instance);
        }

        private sealed class ConfigurationClassInterceptor : IConfigurationClassInterceptor
        {
            #region Logging

            private static readonly ILog Logger = LogManager.GetLogger<ConfigurationClassInterceptor>();
            
            #endregion

            private readonly IConfigurableListableObjectFactory _configurableListableObjectFactory;

            public ConfigurationClassInterceptor(IConfigurableListableObjectFactory configurableListableObjectFactory)
            {
                this._configurableListableObjectFactory = configurableListableObjectFactory;
            }

            public bool ProcessDefinition(MethodInfo method, out object instance)
            {
                instance = null;

			    string objectName = method.Name;

                if (objectName.StartsWith("set_") || objectName.StartsWith("get_"))
                {
                    return false;
                }

                object[] attribs = method.GetCustomAttributes(typeof(ObjectDefAttribute), true);
                if (attribs.Length == 0)
                {
                    return false;
                }

                if (this._configurableListableObjectFactory.IsCurrentlyInCreation(objectName))
                {
                    Logger.Debug(m => m("Object '{0}' currently in creation, created one", objectName));

                    return false;
                }

                Logger.Debug(m => m("Object '{0}' not in creation, asked the application context for one", objectName)); 

                instance = this._configurableListableObjectFactory.GetObject(objectName);
                return true;
            }
        }

        #region Proxy builder classes definition

        private sealed class ConfigurationClassProxyTypeBuilder : InheritanceProxyTypeBuilder
        {
            private FieldBuilder interceptorField;
            private IConfigurationClassInterceptor interceptor;

            public ConfigurationClassProxyTypeBuilder(Type configurationClassType, IConfigurationClassInterceptor interceptor)
            {
                if (configurationClassType.IsSealed)
                {
                    throw new ArgumentException(String.Format(
                        "[Configuration] classes '{0}' cannot be sealed [{0}].", configurationClassType.FullName));
                }

                this.Name = "ConfigurationClassProxy";
                this.DeclaredMembersOnly = false;
                this.BaseType = configurationClassType;
                this.TargetType = configurationClassType;

                this.interceptor = interceptor;
            }

            public override Type BuildProxyType()
            {
                IDictionary targetMethods = new Hashtable();

                TypeBuilder typeBuilder = CreateTypeBuilder(Name, BaseType);

                // apply custom attributes to the proxy type.
                //ApplyTypeAttributes(typeBuilder, BaseType);

                // declare interceptor field
                interceptorField = typeBuilder.DefineField("__Interceptor", typeof(IConfigurationClassInterceptor),
                    FieldAttributes.Private | FieldAttributes.Static);

                // create constructors
                ImplementConstructors(typeBuilder);

                 // proxy base virtual methods
                InheritType(typeBuilder,
                    new ConfigurationClassProxyMethodBuilder(typeBuilder, this, false, targetMethods),
                    BaseType, this.DeclaredMembersOnly);

                Type proxyType = typeBuilder.CreateTypeInfo();

                // set target method references
                foreach (DictionaryEntry entry in targetMethods)
                {
                    FieldInfo targetMethodFieldInfo = proxyType.GetField((string)entry.Key, BindingFlags.NonPublic | BindingFlags.Static);
                    targetMethodFieldInfo.SetValue(proxyType, entry.Value);
                }

                // set interceptor
                FieldInfo interceptorFieldInfo = proxyType.GetField("__Interceptor", BindingFlags.NonPublic | BindingFlags.Static);
                interceptorFieldInfo.SetValue(proxyType, this.interceptor);

                return proxyType;
            }

            public void PushInterceptor(ILGenerator il)
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, interceptorField);
            }
        }

        private sealed class ConfigurationClassProxyMethodBuilder : AbstractProxyMethodBuilder
        {
            public static readonly MethodInfo ProcessDefinitionMethod =
                typeof(IConfigurationClassInterceptor).GetMethod("ProcessDefinition", BindingFlags.Instance | BindingFlags.Public);

            private ConfigurationClassProxyTypeBuilder customProxyGenerator;

            private IDictionary targetMethods;         

            public ConfigurationClassProxyMethodBuilder(
                TypeBuilder typeBuilder, ConfigurationClassProxyTypeBuilder proxyGenerator,
                bool explicitImplementation, IDictionary targetMethods)
                : base(typeBuilder, proxyGenerator, explicitImplementation)
            {
                this.customProxyGenerator = proxyGenerator;
                this.targetMethods = targetMethods;
            }

            protected override void GenerateMethod(
                ILGenerator il, MethodInfo method, MethodInfo interfaceMethod)
            {
                // Declare local variables
                LocalBuilder interceptedReturnValue = il.DeclareLocal(typeof(Object));
//#if DEBUG
//                interceptedReturnValue.SetLocalSymInfo("interceptedReturnValue");
//#endif
                LocalBuilder returnValue = null;
                if (method.ReturnType != typeof(void))
                {
                    returnValue = il.DeclareLocal(method.ReturnType);
//#if DEBUG
//                    returnValue.SetLocalSymInfo("returnValue");
//#endif
                }

                // Declare static field that will cache base method
                string methodId = "_m" + Guid.NewGuid().ToString("N");
                targetMethods.Add(methodId, method);
                FieldBuilder targetMethodCacheField = typeBuilder.DefineField(methodId, typeof(MethodInfo),
                    FieldAttributes.Private | FieldAttributes.Static);

                // Call IConfigurationClassInterceptor.TryGetObject method
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stloc, interceptedReturnValue);
                customProxyGenerator.PushInterceptor(il);
                il.Emit(OpCodes.Ldsfld, targetMethodCacheField);
                il.Emit(OpCodes.Ldloca_S, interceptedReturnValue);
                il.EmitCall(OpCodes.Callvirt, ProcessDefinitionMethod, null);
                Label jmpBaseCall = il.DefineLabel();
                Label jmpEndIf = il.DefineLabel();
                il.Emit(OpCodes.Brfalse_S, jmpBaseCall);

                // if true
                if (returnValue != null)
                {
                    il.Emit(OpCodes.Ldloc, interceptedReturnValue);
                    if (method.ReturnType.IsValueType || method.ReturnType.IsGenericParameter)
                    {
                        il.Emit(OpCodes.Unbox_Any, method.ReturnType);
                    }
                    il.Emit(OpCodes.Stloc, returnValue);
                    il.Emit(OpCodes.Br, jmpEndIf);
                }

                // if false
                il.MarkLabel(jmpBaseCall);
                CallDirectBaseMethod(il, method);
                if (returnValue != null)
                {
                    il.Emit(OpCodes.Stloc, returnValue);
                }

                // end if
                il.MarkLabel(jmpEndIf);

                // return value
                if (returnValue != null)
                {
                    il.Emit(OpCodes.Ldloc, returnValue);
                }
            }
        }

        #endregion
    }
}
