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

using Common.Logging;

using Spring.Caching;
using Spring.Context;
using Spring.Expressions;

#endregion

namespace Spring.Aspects.Cache
{
    /// <summary>
    /// Base class for different cache advice implementations that provide
    /// access to common functionality, such as obtaining a cache instance.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public abstract class BaseCacheAdvice : IApplicationContextAware
    {
        /// <summary>
        /// Shared logger instance
        /// </summary>
        protected readonly ILog logger;

        private IApplicationContext applicationContext;

        /// <summary>
        /// Create a new default instance.
        /// </summary>
        protected BaseCacheAdvice()
        {
            logger = LogManager.GetLogger(this.GetType());
        }

        /// <summary>
        /// Sets the <see cref="Spring.Context.IApplicationContext"/> that this
        /// object runs in.
        /// </summary>
        public IApplicationContext ApplicationContext
        {
            set { applicationContext = value; }
        }

        /// <summary>
        /// Returns an <see cref="ICache"/> instance based on the cache name.
        /// </summary>
        /// <param name="name">The name of the cache.</param>
        /// <returns>
        /// Cache instance for the specified <paramref name="name"/> if one
        /// is registered in the application context, or <c>null</c> if it isn't.
        /// </returns>
        /// <exception cref="Spring.Objects.Factory.NoSuchObjectDefinitionException">
        /// If there's no cache instance registered for the specified <paramref name="name"/>.
        /// </exception>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// If the cache instance could not be created.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// If the cache instance registered does not implement the <see cref="Spring.Caching.ICache"/> interface.
        /// </exception>
        public ICache GetCache(string name)
        {
            ICache cache = applicationContext.GetObject(name) as ICache;
            if (cache == null)
            {
                throw new ArgumentException(String.Format(
                    "Cache with the specified name [{0}] does not implement the 'Spring.Caching.ICache' interface.", name));
            }
            return cache;
        }

        /// <summary>
        /// Prepares variables for expression evaluation by packaging all
        /// method arguments into a dictionary, keyed by argument name.
        /// </summary>
        /// <param name="method">
        /// Method to get parameters info from.
        /// </param>
        /// <param name="arguments">
        /// Argument values to package.
        /// </param>
        /// <returns>
        /// A dictionary containing all method arguments, keyed by method name.
        /// </returns>
        protected static IDictionary<string, object> PrepareVariables(MethodInfo method, object[] arguments)
        {
            IDictionary<string, object> vars = new Dictionary<string, object>();

            vars[method.Name] = method;

            ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo p = parameters[i];
                vars[p.Name] = arguments[i];
            }
            return vars;
        }

        /// <summary>
        /// Evaluates a SpEL expression as a boolean value.
        /// </summary>
        /// <param name="condition">
        /// The expression string that should be evaluated.
        /// </param>
        /// <param name="conditionExpression">
        /// The SpEL expression instance that should be evaluated.
        /// </param>
        /// <param name="context">
        /// The object to evaluate expression against.
        /// </param>
        /// <param name="variables">
        /// The expression variables dictionary.
        /// </param>
        /// <returns>
        /// The evaluated boolean.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If the SpEL expression could not be successfuly resolved to a boolean.
        /// </exception>
        protected static bool EvalCondition(string condition,
            IExpression conditionExpression, object context, IDictionary<string, object> variables)
        {
            if (conditionExpression == null)
            {
                return true;
            }
            else
            {
                object value = conditionExpression.GetValue(context, variables);
                if (value is bool)
                {
                    return (bool)value;
                }
                else
                {
                    throw new InvalidOperationException(String.Format(
                        "The SpEL expression '{0}' could not be successfuly resolved to a boolean.",
                        condition));
                }
            }
        }

        /// <summary>
        /// Retrieves custom attribute for the specified attribute type.
        /// </summary>
        /// <param name="attributeProvider">
        /// Method/Parameter to get attribute from.
        /// </param>
        /// <param name="attributeType">
        /// Attribute type.
        /// </param>
        /// <returns>
        /// Attribute instance if one is found, <c>null</c> otherwise.
        /// </returns>
        protected object GetCustomAttribute(ICustomAttributeProvider attributeProvider, Type attributeType)
        {
            object[] attributes = attributeProvider.GetCustomAttributes(attributeType, false);
            if (attributes.Length > 0)
            {
                return attributes[0];
            }
            return null;
        }

        /// <summary>
        /// Retrieves custom attribute for the specified attribute type.
        /// </summary>
        /// <param name="attributeProvider">
        /// Method/Parameter to get attribute from.
        /// </param>
        /// <param name="attributeType">
        /// Attribute type.
        /// </param>
        /// <returns>
        /// Attribute instance if one is found, <c>null</c> otherwise.
        /// </returns>
        protected object[] GetCustomAttributes(ICustomAttributeProvider attributeProvider, Type attributeType)
        {
            object[] attributes = attributeProvider.GetCustomAttributes(attributeType, false);
            return attributes;
        }
    }
}
