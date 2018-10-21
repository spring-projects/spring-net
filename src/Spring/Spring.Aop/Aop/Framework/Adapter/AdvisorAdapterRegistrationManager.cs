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

using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Aop.Framework.Adapter
{
    /// <summary>
    /// <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/> implementation
    /// that registers instances of any non-default
    /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/> instances with the
    /// <see cref="Spring.Aop.Framework.Adapter.GlobalAdvisorAdapterRegistry"/>
    /// singleton.
    /// </summary>
    /// <remarks>
    /// <p>
    /// The only requirement for it to work is that it needs to be defined
    /// in an application context along with any arbitrary "non-native" Spring.NET
    /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/> instances that need
    /// to be recognized by Spring.NET's AOP framework.
    /// </p>
    /// </remarks>
    /// <author>Dmitriy Kopylenko</author>
    /// <author>Aleksandar Seovic (.NET)</author>
    public class AdvisorAdapterRegistrationManager : IObjectPostProcessor
    {
        /// <summary>
        /// Apply this <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/>
        /// to the given new object instance <i>before</i> any object initialization callbacks.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Does nothing, simply returns the supplied <paramref name="instance"/> as is.
        /// </p>
        /// </remarks>
        /// <param name="instance">
        /// The new object instance.
        /// </param>
        /// <param name="name">
        /// The name of the object.
        /// </param>
        /// <returns>
        /// The object instance to use, either the original or a wrapped one.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public virtual object PostProcessBeforeInitialization(object instance, string name)
        {
            return instance;
        }

        /// <summary>
        /// Apply this <see cref="Spring.Objects.Factory.Config.IObjectPostProcessor"/> to the
        /// given new object instance <i>after</i> any object initialization callbacks.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Registers the supplied <paramref name="instance"/> with the
        /// <see cref="Spring.Aop.Framework.Adapter.GlobalAdvisorAdapterRegistry"/>
        /// singleton if it is an <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/>
        /// instance.
        /// </p>
        /// </remarks>
        /// <param name="instance">
        /// The new object instance.
        /// </param>
        /// <param name="objectName">
        /// The name of the object.
        /// </param>
        /// <returns>
        /// The object instance to use, either the original or a wrapped one.
        /// </returns>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In case of errors.
        /// </exception>
        public virtual object PostProcessAfterInitialization(object instance, string objectName)
        {
            IAdvisorAdapter adapter = instance as IAdvisorAdapter;
            if (adapter != null)
            {
                GlobalAdvisorAdapterRegistry.Instance.RegisterAdvisorAdapter(adapter);
            }
            return instance;
        }
    }
}