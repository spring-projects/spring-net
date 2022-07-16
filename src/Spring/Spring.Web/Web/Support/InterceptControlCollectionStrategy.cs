#region License
/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Web.UI;
using Spring.Context;
using Spring.Proxy;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// This strategy enhances a ControlCollection's type by
    /// dynamically implementing ISupportsWebDependencyInjection on this type
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal class InterceptControlCollectionStrategy : IInterceptionStrategy
    {
        /// <summary>
        /// Holds a reference to the static(!) callback-method to be used during generation of intercepted ControlCollection-Types
        /// </summary>
        private delegate Control InjectDependenciesCallbackHandler(IApplicationContext defaultApplicationContext, Control control);

        /// <summary>
        /// The list of methods to be intercepted for a ControlCollection
        /// </summary>
        private static readonly MethodInfo[] s_collectionMethods = new MethodInfo[]
            {
                typeof(ControlCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.Public)
                , typeof (ControlCollection).GetMethod("AddAt", BindingFlags.Instance | BindingFlags.Public)
            };

        /// <summary>
        /// Holds a table of already known intercepted ControlCollection types
        /// </summary>
        private static readonly Hashtable s_interceptedCollectionTypeCache = new Hashtable();

        /// <summary>
        /// Intercepts the given <see cref="ControlCollection"/> by dynamically deriving
        /// the original type and let it implement <see cref="ISupportsWebDependencyInjection"/>.
        /// </summary>
        /// <param name="defaultApplicationContext">the ApplicationContext to be set on the collection instance.</param>
        /// <param name="ctlAccessor">a wrapper around the owner control instance.</param>
        /// <param name="ctlColAccessor">a wrapper around the collection instance.</param>
        /// <returns><value>true</value>, if interception was successful.  <value>false</value> otherwise</returns>
        public bool Intercept(IApplicationContext defaultApplicationContext,
            ControlAccessor ctlAccessor, ControlCollectionAccessor ctlColAccessor)
        {
            Type collectionType = ctlColAccessor.GetTargetType();
            if (collectionType.IsSealed ||
                !ReflectionUtils.IsTypeVisible(collectionType, DynamicProxyManager.ASSEMBLY_NAME))
            //  || (null == collectionType.GetConstructor(new Type[] {typeof (Control)}))
            {
                return false;
            }

            // this will enhance the collection's type and create a new instance of this type with fields copied from original collection
            try
            {
                ControlCollection childControls = InterceptCollection(ctlAccessor.GetTarget(), ctlColAccessor.GetTarget() );
                ((ISupportsWebDependencyInjection)childControls).DefaultApplicationContext = defaultApplicationContext;
                ctlAccessor.Controls = childControls;
            }
            catch
            {
                // this may happen, if the ControlCollection doesn't contain a standard-ctor ControlCollection( Control owner)
                return false;
            }
            return true;
        }

        private static ControlCollection InterceptCollection(Control owner, ControlCollection originalCollection)
        {
            CreateControlCollectionDelegate factoryMethod = GetInterceptedCollectionFactory(owner.GetType(), originalCollection.GetType());
            ControlCollection interceptedCollection = factoryMethod(owner);
            ReflectionUtils.MemberwiseCopy(originalCollection, interceptedCollection);
            return interceptedCollection;
        }

        internal static ControlCollection TryCreateCollection(Control owner)
        {
            CreateControlCollectionDelegate factoryMethod = (CreateControlCollectionDelegate)s_collectionFactoryCache[owner.GetType()];
            if (factoryMethod != null)
            {
                return factoryMethod(owner);
            }
            return null;
        }

        private delegate ControlCollection CreateControlCollectionDelegate(Control owner);
        private static readonly Hashtable s_collectionFactoryCache = new Hashtable();

        private static CreateControlCollectionDelegate GetInterceptedCollectionFactory(Type ownerType, Type collectionType)
        {
            AssertUtils.State( typeof(Control).IsAssignableFrom(ownerType), "ownerType must be of type Control" );
            AssertUtils.State( typeof(ControlCollection).IsAssignableFrom(collectionType), "collectionType must be of type ControlCollection" );

            CreateControlCollectionDelegate factoryMethod = (CreateControlCollectionDelegate)s_collectionFactoryCache[ownerType];
            if (factoryMethod == null)
            {
                lock (s_collectionFactoryCache)
                {
                    factoryMethod = (CreateControlCollectionDelegate)s_collectionFactoryCache[ownerType];
                    if (factoryMethod == null)
                    {
                        Type interceptedCollectionType = GetInterceptedCollectionType(
                                                              collectionType
                                                            , WebDependencyInjectionUtils.InjectDependenciesRecursive
                                                         );

                        ConstructorInfo ctor = interceptedCollectionType.GetConstructor(new Type[] { typeof(Control) });
                        DynamicMethod dm = new DynamicMethod(string.Empty, typeof(ControlCollection), new Type[] { typeof(Control) });
                        ILGenerator il = dm.GetILGenerator();
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Newobj, ctor);
                        il.Emit(OpCodes.Ret);
                        factoryMethod = (CreateControlCollectionDelegate)dm.CreateDelegate(typeof(CreateControlCollectionDelegate));
                        s_collectionFactoryCache[ownerType] = factoryMethod;
                    }
                }
            }
            return factoryMethod;
        }

        private static Type GetInterceptedCollectionType(Type controlCollectionType, InjectDependenciesCallbackHandler staticCallback)
        {
            AssertUtils.State( typeof(ControlCollection).IsAssignableFrom(controlCollectionType), "controlCollectionType must be of type ControlCollection" );

            Type interceptedCollectionType = (Type)s_interceptedCollectionTypeCache[controlCollectionType];
            if (interceptedCollectionType == null)
            {
                lock (s_interceptedCollectionTypeCache)
                {
                    MethodInfo callbackMethod = staticCallback.Method;
                    AssertUtils.State(callbackMethod.IsStatic && callbackMethod.IsPublic, "staticCallback must be a public static method");
                    interceptedCollectionType = (Type)s_interceptedCollectionTypeCache[controlCollectionType];
                    if (interceptedCollectionType == null)
                    {
                        SupportsWebDependencyInjectionTypeBuilder builder = new SupportsWebDependencyInjectionTypeBuilder(controlCollectionType, s_collectionMethods, callbackMethod);
                        interceptedCollectionType = builder.BuildProxyType();
                        s_interceptedCollectionTypeCache[controlCollectionType] = interceptedCollectionType;
                    }
                }
            }
            return interceptedCollectionType;
        }
    }
}
