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
using NHibernate.Context;
using NHibernate.Engine;

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// Implementation of NHibernates 1.2's ICurrentSessionContext interface
    /// that delegates to Spring's SessionFactoryUtils for providing a
    /// Spirng-managed current Session.
    /// </summary>
    /// <remarks>Used by Spring's LocalSessionFactoryBean if told to expose
    /// a transaction-aware SessionFactory.
    /// <p>This ICurrentSessionContext implementation can also be specified in
    /// custom ISessionFactory setup through the 
    /// "hibernate.current_session_context_class" property, with the fully
    /// qualified name of this class as value.</p></remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <see cref="SessionFactoryUtils.DoGetSession(ISessionFactory, bool)"/>
    public class SpringSessionContext : ICurrentSessionContext
    {
        private readonly ISessionFactoryImplementor sessionFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpringSessionContext"/> class
        /// </summary>
        /// <param name="sessionFactory">The NHibernate session factory.</param>
        public SpringSessionContext(ISessionFactoryImplementor sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        /// <summary>
        /// Retrieve the Spring-managed Session for the current thread.
        /// </summary>
        /// <returns>Current session associated with the thread</returns>
        /// <exception cref="HibernateException">On errors retrieving thread bound session.</exception>
        public ISession CurrentSession()
        {
            try
            {
                return SessionFactoryUtils.DoGetSession(sessionFactory, false);
            }
            catch (InvalidOperationException ex)
            {
                throw new HibernateException(ex.Message);
            }
        }
    }
}
