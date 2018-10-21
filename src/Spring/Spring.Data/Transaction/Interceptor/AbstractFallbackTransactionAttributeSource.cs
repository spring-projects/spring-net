/*
 * Copyright 2002-2010 the original author or authors.
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using Spring.Collections;
using Spring.Util;
using Spring.Core;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// Abstract implementation of
	/// <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/>
	/// that caches attributes for methods, and implements a default fallback policy.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The default fallback policy applied by this class is:
	/// <list type="numbered">
	/// <item>
	/// <description>most specific method</description>
	/// </item>
	/// <item>
	/// <description>target class attribute</description>
	/// </item>
	/// <item>
	/// <description>declaring method</description>
	/// </item>
	/// <item>
	/// <description>declaring class</description>
	/// </item>
	/// </list>
	/// </p>
	/// <p>
	/// Defaults to using class's transaction attribute if none is associated
	/// with the target method. Any transaction attribute associated with the
	/// target method completely overrides a class transaction attribute.
	/// </p>
	/// <p>
	/// This implementation caches attributes by method after they are first used.
	/// If it's ever desirable to allow dynamic changing of transaction attributes
	/// (unlikely) caching could be made configurable. Caching is desirable because
	/// of the cost of evaluating rollback rules.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Mark Pollack (.NET)</author>
	public abstract class AbstractFallbackTransactionAttributeSource : ITransactionAttributeSource
	{
		/// <summary>
		/// Canonical value held in cache to indicate no transaction attibute was found
		/// for this method, and we don't need to look again.
		/// </summary>
		private static readonly object NULL_TX_ATTIBUTE = new object();

		/// <summary>
		/// Cache of <see cref="ITransactionAttribute"/>s, keyed by method and target class.
		/// </summary>
		private readonly IDictionary _transactionAttibuteCache = new Hashtable();

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.AbstractFallbackTransactionAttributeSource"/>
		/// class.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
		/// </p>
		/// </remarks>
		protected AbstractFallbackTransactionAttributeSource()
		{
		}

		/// <summary>
		/// Subclasses should implement this to return all attributes for this method.
		/// May return null.
		/// </summary>
		/// <remarks>
		/// We need all because of the need to analyze rollback rules.
		/// </remarks>
		/// <param name="method">The method to retrieve attributes for.</param>
		/// <returns>The transactional attributes associated with the method.</returns>
		protected abstract Attribute[] FindAllAttributes(MethodInfo method);

		/// <summary>
		/// Subclasses should implement this to return all attributes for this class.
		/// May return null.
		/// </summary>
		/// <param name="targetType">
		/// The <see cref="System.Type"/> to retrieve attributes for.
		/// </param>
		/// <returns>
		/// All attributes associated with the supplied <paramref name="targetType"/>.
		/// </returns>
		protected abstract Attribute[] FindAllAttributes(Type targetType);

		/// <summary>
		/// Return the transaction attribute for this method invocation.
		/// </summary>
		/// <remarks>
		/// Defaults to the class's transaction attribute if no method
		/// attribute is found
		/// </remarks>
		/// <param name="method">method for the current invocation. Can't be null</param>
		/// <param name="targetType">target class for this invocation. May be null.</param>
		/// <returns><see cref="ITransactionAttribute"/> for this method, or null if the method is non-transactional</returns>
		public ITransactionAttribute ReturnTransactionAttribute(MethodInfo method, Type targetType)
		{
			object cacheKey = GetCacheKey(method, targetType);

		    lock (_transactionAttibuteCache)
            {
                object cached = _transactionAttibuteCache[cacheKey];

                if (cached != null)
                {
                    if (NULL_TX_ATTIBUTE == cached)
                    {
                        return null;
                    }
                    {
                        return (ITransactionAttribute)cached;
                    }
                }
                else
                {
                    ITransactionAttribute transactionAttribute = ComputeTransactionAttribute(method, targetType);
                    if (null == transactionAttribute)
                    {
                        _transactionAttibuteCache.Add(cacheKey, NULL_TX_ATTIBUTE);
                    }
                    else
                    {
                        _transactionAttibuteCache.Add(cacheKey, transactionAttribute);
                    }
                    return transactionAttribute;
                }
            }
		}

		/// <summary>
		/// Return the transaction attribute, given this set of attributes
		/// attached to a method or class. Return null if it's not transactional.  
		/// </summary>
		/// <remarks>
		/// Protected rather than private as subclasses may want to customize
		/// how this is done: for example, returning a
		/// <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/>
		/// affected by the values of other attributes.
		/// This implementation takes into account
		/// <see cref="Spring.Transaction.Interceptor.RollbackRuleAttribute"/>s, if
		/// the TransactionAttribute is a RuleBasedTransactionAttribute.
		/// </remarks>
		/// <param name="attributes">
		/// Attributes attached to a method or class. May be null, in which case a null
		/// <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/> will be returned.
		/// </param>
		/// <returns>
		/// The <see cref="ITransactionAttribute"/> configured transaction attribute, or null
		/// if none was found.
		/// </returns>
		protected virtual ITransactionAttribute FindTransactionAttribute(Attribute[] attributes)
		{
			if (null == attributes)
			{
				return null;
			}
			ITransactionAttribute transactionAttribute = null;
			foreach (Attribute currentAttribute in attributes)
			{
				transactionAttribute = currentAttribute as ITransactionAttribute;
				if (null != transactionAttribute)
				{
					break;
				}
			}

			if (transactionAttribute is RuleBasedTransactionAttribute ruleBasedTransactionAttribute)
			{
				var rollbackRules = new List<RollbackRuleAttribute>();
				foreach (Attribute currentAttribute in attributes)
				{
					if (currentAttribute is RollbackRuleAttribute rollbackRuleAttribute)
					{
						rollbackRules.Add(rollbackRuleAttribute);
					}
				}
				ruleBasedTransactionAttribute.RollbackRules = rollbackRules;
				return ruleBasedTransactionAttribute;
			}
			return transactionAttribute;
		}

		private static object GetCacheKey(MethodBase method, Type targetType)
		{
			return string.Intern(targetType.AssemblyQualifiedName + "." + method);
		}

        private ITransactionAttribute ComputeTransactionAttribute(MethodInfo method, Type targetType)
        {
            MethodInfo specificMethod;
            if (targetType == null)
            {
                specificMethod = method;
            }
            else
            {
                ParameterInfo[] parameters = method.GetParameters();

                ComposedCriteria searchCriteria = new ComposedCriteria();
                searchCriteria.Add(new MethodNameMatchCriteria(method.Name));
                searchCriteria.Add(new MethodParametersCountCriteria(parameters.Length));
                searchCriteria.Add(new MethodGenericArgumentsCountCriteria(method.GetGenericArguments().Length));
                searchCriteria.Add(new MethodParametersCriteria(ReflectionUtils.GetParameterTypes(parameters)));

                MemberInfo[] matchingMethods = targetType.FindMembers(
                    MemberTypes.Method,
                    BindingFlags.Instance | BindingFlags.Public,
                    new MemberFilter(new CriteriaMemberFilter().FilterMemberByCriteria),
                    searchCriteria);

                if (matchingMethods != null && matchingMethods.Length == 1)
                {
                    specificMethod = matchingMethods[0] as MethodInfo;
                }
                else
                {
                    specificMethod = method;
                }
            }

            ITransactionAttribute transactionAttribute = GetTransactionAttribute(specificMethod);
            if (null != transactionAttribute)
            {
                return transactionAttribute;
            }
            else if (specificMethod != method)
            {
                transactionAttribute = GetTransactionAttribute(method);
            }
            return null;
        }

		private ITransactionAttribute GetTransactionAttribute(MethodInfo methodInfo)
		{
			ITransactionAttribute transactionAttribute = FindTransactionAttribute(FindAllAttributes(methodInfo));

			if (null != transactionAttribute)
			{
				return transactionAttribute;
			}
			transactionAttribute = FindTransactionAttribute(FindAllAttributes(methodInfo.DeclaringType));
			if (null != transactionAttribute)
			{
				return transactionAttribute;
			}
			return null;
		}
	}
}