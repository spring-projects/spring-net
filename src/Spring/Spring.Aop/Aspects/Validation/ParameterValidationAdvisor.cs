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

using Spring.Aop.Support;
using Spring.Validation;
using Spring.Context;

#endregion

namespace Spring.Aspects.Validation
{
    /// <summary>
    /// Convinience advisor implementation that applies <see cref="ParameterValidationAdvice"/>
    /// to all the methods that have <see cref="ValidatedAttribute"/> defined on one or
    /// more of their parameters.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class ParameterValidationAdvisor : AttributeMatchMethodPointcutAdvisor, IApplicationContextAware
    {
        /// <summary>
        /// Creates new advisor instance.
        /// </summary>
        public ParameterValidationAdvisor()
        {
            Advice = new ParameterValidationAdvice();
            Attribute = typeof(ValidatedAttribute);
            Inherit = false;
        }

        /// <summary>
        /// Sets the <see cref="Spring.Context.IApplicationContext"/> that this
        /// object runs in.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Normally this call will be used to initialize the object.
        /// </p>
        /// <p>
        /// Invoked after population of normal object properties but before an
        /// init callback such as
        /// <see cref="Spring.Objects.Factory.IInitializingObject"/>'s
        /// <see cref="Spring.Objects.Factory.IInitializingObject.AfterPropertiesSet"/>
        /// or a custom init-method. Invoked after the setting of any
        /// <see cref="Spring.Context.IResourceLoaderAware"/>'s
        /// <see cref="Spring.Context.IResourceLoaderAware.ResourceLoader"/>
        /// property.
        /// </p>
        /// </remarks>
        /// <exception cref="Spring.Context.ApplicationContextException">
        /// In the case of application context initialization errors.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If thrown by any application context methods.
        /// </exception>
        /// <exception cref="Spring.Objects.Factory.ObjectInitializationException"/>
        public IApplicationContext ApplicationContext
        {
            set { ((IApplicationContextAware)Advice).ApplicationContext = value; }
        }

        /// <summary>
        /// Returns <c>true</c> if any of the parameters of the specified <paramref name="method"/>
        /// has <see cref="ValidatedAttribute"/> applied.
        /// </summary>
        /// <param name="method">
        /// Method to check.
        /// </param>
        /// <param name="targetType">
        /// Type of target object.
        /// </param>
        /// <returns>
        /// <c>true</c> if any of the parameters of the specified <paramref name="method"/>
        /// has <see cref="ValidatedAttribute"/> applied; <c>false</c> otherwise.
        /// </returns>
        public override bool Matches(MethodInfo method, Type targetType)
        {
            ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];
                if (p.IsDefined(Attribute, Inherit))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
