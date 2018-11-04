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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private static readonly ConcurrentDictionary<Type, Type> proxyTypeCache = new ConcurrentDictionary<Type, Type>();

        private readonly ConfigurationClassInterceptor interceptor;

        /// <summary>
        /// Creates a new instance of the <see cref="ConfigurationClassEnhancer"/> class.
        /// </summary>
        /// <param name="objectFactory">
        /// The supplied ObjectFactory to check for the existence of object definitions.
        /// </param>
    	public ConfigurationClassEnhancer(IConfigurableListableObjectFactory objectFactory)
        {
		    AssertUtils.ArgumentNotNull(objectFactory, "objectFactory");

            interceptor = new ConfigurationClassInterceptor(objectFactory);
	    }

        /// <summary>
        /// Generates a dynamic subclass of the specified Configuration class with a
        /// container-aware interceptor capable of respecting scoping and other bean semantics.
        /// </summary>
        /// <param name="configClass">The Configuration class.</param>
        /// <returns>The enhanced subclass.</returns>
        public Type Enhance(Type configClass)
        {
            var proxyTypeBuilder = new ConfigurationClassProxyTypeBuilder(configClass, interceptor);
            var buildProxyType = proxyTypeBuilder.BuildProxyType();
            return buildProxyType;
        }

        public sealed class ConfigurationClassInterceptor
        {
            private static readonly ILog Logger = LogManager.GetLogger<ConfigurationClassInterceptor>();

            private readonly ConcurrentDictionary<string, bool> checkedObjects = new ConcurrentDictionary<string, bool>();
            private readonly IConfigurableListableObjectFactory _configurableListableObjectFactory;

            public ConfigurationClassInterceptor(IConfigurableListableObjectFactory configurableListableObjectFactory)
            {
                _configurableListableObjectFactory = configurableListableObjectFactory;
            }

            // ReSharper disable once UnusedMember.Local
            public bool ProcessDefinition(MethodInfo method, out object instance)
            {
                instance = null;

                if (method == null)
                {
                    // it didn't survive condition checks
                    return false;
                }

                string objectName = method.Name;

                var debugEnabled = Logger.IsDebugEnabled;
                if (_configurableListableObjectFactory.IsCurrentlyInCreation(objectName))
                {
                    if (debugEnabled)
                    {
                        Logger.Debug($"Object '{objectName}' currently in creation, created one");
                    }
                    return false;
                }

                if (debugEnabled)
                {
                    Logger.Debug($"Object '{objectName}' not in creation, asked the application context for one");
                }

                instance = _configurableListableObjectFactory.GetObject(objectName);
                return true;
            }
        }

        private sealed class ConfigurationClassProxyTypeBuilder : InheritanceProxyTypeBuilder
        {
            private FieldBuilder interceptorField;
            private readonly ConfigurationClassInterceptor interceptor;

            public ConfigurationClassProxyTypeBuilder(Type configurationClassType, ConfigurationClassInterceptor interceptor)
            {
                if (configurationClassType.IsSealed)
                {
                    throw new ArgumentException($"[Configuration] classes '{configurationClassType.FullName}' cannot be sealed [{configurationClassType.FullName}].");
                }

                Name = "ConfigurationClassProxy";
                DeclaredMembersOnly = false;
                BaseType = configurationClassType;
                TargetType = configurationClassType;

                this.interceptor = interceptor;
            }

            public override Type BuildProxyType()
            {
                Dictionary<string, MethodInfo> targetMethods = new Dictionary<string, MethodInfo>();

                TypeBuilder typeBuilder = CreateTypeBuilder(Name, BaseType);

                // apply custom attributes to the proxy type.
                //ApplyTypeAttributes(typeBuilder, BaseType);

                // declare interceptor field
                interceptorField = typeBuilder.DefineField("__Interceptor", typeof(ConfigurationClassInterceptor),
                    FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);

                // create constructors
                ImplementConstructors(typeBuilder);

                 // proxy base virtual methods
                InheritType(typeBuilder,
                    new ConfigurationClassProxyMethodBuilder(typeBuilder, this, false, targetMethods),
                    BaseType, this.DeclaredMembersOnly);

                Type proxyType = typeBuilder.CreateTypeInfo();

                // set target method references
                foreach (var entry in targetMethods)
                {
                    // only set value if it's usable for configuration
                    if (entry.Value.Name.StartsWith("set_") || entry.Value.Name.StartsWith("get_"))
                    {
                        continue;
                    }

                    object[] attribs = entry.Value.GetCustomAttributes(typeof(ObjectDefAttribute), true);
                    if (attribs.Length == 0)
                    {
                        continue;
                    }

                    FieldInfo targetMethodFieldInfo = proxyType.GetField(entry.Key, BindingFlags.NonPublic | BindingFlags.Static);
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
            private static readonly MethodInfo ProcessDefinitionMethod =
                typeof(ConfigurationClassInterceptor).GetMethod("ProcessDefinition", BindingFlags.Instance | BindingFlags.Public);

            private readonly ConfigurationClassProxyTypeBuilder customProxyGenerator;

            private readonly Dictionary<string, MethodInfo> targetMethods;

            public ConfigurationClassProxyMethodBuilder(
                TypeBuilder typeBuilder,
                ConfigurationClassProxyTypeBuilder proxyGenerator,
                bool explicitImplementation,
                Dictionary<string, MethodInfo> targetMethods)
                : base(typeBuilder, proxyGenerator, explicitImplementation)
            {
                customProxyGenerator = proxyGenerator;
                this.targetMethods = targetMethods;
            }

            protected override void GenerateMethod(ILGenerator il, MethodInfo method, MethodInfo interfaceMethod)
            {
                // Declare local variables
                LocalBuilder interceptedReturnValue = il.DeclareLocal(typeof(object));
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
                    FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.InitOnly);

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
    }
}
