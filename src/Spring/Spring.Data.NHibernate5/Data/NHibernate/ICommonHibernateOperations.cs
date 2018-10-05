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
using NHibernate.Type;
using Spring.Dao;

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// Interface that specifies a set of Hibernate operations that
    /// are common across versions of Hibernate.
    /// </summary>
    /// <remarks>
    /// Base interface for generic and non generic IHibernateOperations interfaces
    /// Not often used, but a useful option
    /// to enhance testability, as it can easily be mocked or stubbed.
    /// <p>Provides HibernateTemplate's data access methods that mirror
    /// various Session methods. See the NHibernate ISession documentation
    /// for details on those methods.
    /// </p>
    /// </remarks> 
    /// <author>Mark Pollack (.NET)</author>  
    /// <threadsafety statis="true" instance="true"/> 
    public interface ICommonHibernateOperations
    {
        /// <summary>
        /// Remove all objects from the Session cache, and cancel all pending saves,
        /// updates and deletes.
        /// </summary>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Clear();


        /// <summary>
        /// Delete the given persistent instance.
        /// </summary>
        /// <param name="entity">The persistent instance to delete.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Delete(object entity);

        /// <summary>
        /// Delete the given persistent instance.
        /// </summary>
        /// <remarks>
        /// Obtains the specified lock mode if the instance exists, implicitly
        /// checking whether the corresponding database entry still exists
        /// (throwing an OptimisticLockingFailureException if not found).
        /// </remarks>
        /// <param name="entity">The persistent instance to delete.</param>
        /// <param name="lockMode">The lock mode to obtain.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Delete(object entity, LockMode lockMode);


        /// <summary>
        /// Delete all objects returned by the query.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language.</param>
        /// <returns>The number of entity instances deleted.</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        int Delete(string queryString);

        /// <summary>
        ///  Delete all objects returned by the query.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="type">The Hibernate type of the parameter (or <code>null</code>).</param>
        /// <returns>The number of entity instances deleted.</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        int Delete(string queryString, object value, IType type);


        /// <summary>
        /// Delete all objects returned by the query.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language.</param>
        /// <param name="values">The values of the parameters.</param>
        /// <param name="types"> Hibernate types of the parameters (or <code>null</code>)</param>
        /// <returns>The number of entity instances deleted.</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        int Delete(string queryString, object[] values, IType[] types);


        /// <summary>
        /// Flush all pending saves, updates and deletes to the database.
        /// </summary>
        /// <remarks>
        /// Only invoke this for selective eager flushing, for example when ADO.NET code
        /// needs to see certain changes within the same transaction. Else, it's preferable
        /// to rely on auto-flushing at transaction completion.
        /// </remarks>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Flush();

        /// <summary>
        /// Load the persistent instance with the given identifier
        /// into the given object, throwing an exception if not found.
        /// </summary>
        /// <param name="entity">Entity the object (of the target class) to load into.</param>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <exception cref="ObjectRetrievalFailureException">If object not found.</exception>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Load(object entity, object id);


        /// <summary>
        /// Re-read the state of the given persistent instance.
        /// </summary>
        /// <param name="entity">The persistent instance to re-read.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Refresh(object entity);

        /// <summary>
        /// Re-read the state of the given persistent instance.
        /// Obtains the specified lock mode for the instance.
        /// </summary>
        /// <param name="entity">The persistent instance to re-read.</param>
        /// <param name="lockMode">The lock mode to obtain.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Refresh(object entity, LockMode lockMode);

        /// <summary>
        /// Determines whether the given object is in the Session cache.
        /// </summary>
        /// <param name="entity">the persistence instance to check.</param>
        /// <returns>
        /// 	<c>true</c> if session cache contains the specified entity; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        bool Contains(object entity);

        /// <summary>
        /// Remove the given object from the Session cache.
        /// </summary>
        /// <param name="entity">The persistent instance to evict.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Evict(object entity);

        /// <summary>
        /// Obtain the specified lock level upon the given object, implicitly
        /// checking whether the corresponding database entry still exists
        /// (throwing an OptimisticLockingFailureException if not found).
        /// </summary>
        /// <param name="entity">The he persistent instance to lock.</param>
        /// <param name="lockMode">The lock mode to obtain.</param>
        /// <exception cref="ObjectOptimisticLockingFailureException">If not found</exception>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Lock(object entity, LockMode lockMode);


        /// <summary>
        /// Persist the given transient instance.
        /// </summary>
        /// <param name="entity">The transient instance to persist.</param>
        /// <returns>The generated identifier.</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        object Save(object entity);

        /// <summary>
        /// Persist the given transient instance with the given identifier.
        /// </summary>
        /// <param name="entity">The transient instance to persist.</param>
        /// <param name="id">The identifier to assign.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Save(object entity, object id);

        /// <summary>
        /// Update the given persistent instance.
        /// </summary>
        /// <param name="entity">The persistent instance to update.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Update(object entity);

        /// <summary>
        /// Update the given persistent instance.
        /// Obtains the specified lock mode if the instance exists, implicitly
        /// checking whether the corresponding database entry still exists
        /// (throwing an OptimisticLockingFailureException if not found).
        /// </summary>
        /// <param name="entity">The persistent instance to update.</param>
        /// <param name="lockMode">The lock mode to obtain.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void Update(object entity, LockMode lockMode);

        /// <summary>
        /// Save or update the given persistent instance,
        /// according to its id (matching the configured "unsaved-value"?).
        /// </summary>
        /// <param name="entity">The persistent instance to save or update
        /// (to be associated with the Hibernate Session).</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void SaveOrUpdate(object entity);

        /// <summary>
        /// Copy the state of the given object onto the persistent object with the same identifier. 
        /// If there is no persistent instance currently associated with the session, it will be loaded.
        /// Return the persistent instance. If the given instance is unsaved, 
        /// save a copy of and return it as a newly persistent instance.
        /// The given instance does not become associated with the session. 
        /// This operation cascades to associated instances if the association is mapped with cascade="merge".
        /// The semantics of this method are defined by JSR-220. 
        /// </summary>
        /// <param name="entity">The persistent object to merge.
        /// (<i>not</i> necessarily to be associated with the Hibernate Session)
        /// </param>
        /// <returns>An updated persistent instance</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        object Merge(object entity);
    }
}