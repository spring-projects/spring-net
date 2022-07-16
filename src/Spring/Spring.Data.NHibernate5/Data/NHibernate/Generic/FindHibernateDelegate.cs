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

using NHibernate;

namespace Spring.Data.NHibernate.Generic
{
    /// <summary>
    /// Gets called by HibernateTemplate with an active
    /// Hibernate Session. Does not need to care about activating or closing
    /// the Session, or handling transactions.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Allows for returning an IList of result objects created within the callback. 
    /// Note that there's special support for single step actions: 
    /// see HibernateTemplate.find etc.
    /// </p>
    /// </remarks>
    /// <typeparam name="T">The type of result object</typeparam>
    /// <author>Sree Nivask (.NET)</author>
    public delegate IList<T> FindHibernateDelegate<T>(ISession session);
}
