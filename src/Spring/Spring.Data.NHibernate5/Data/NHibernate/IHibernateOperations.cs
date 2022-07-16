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

using System.Collections;
using NHibernate;
using NHibernate.Type;
using Spring.Dao;

namespace Spring.Data.NHibernate
{
	/// <summary>
    /// Interface that specifies a basic set of Hibernate operations.
    /// </summary>
    /// <remarks>
    /// Implemented by HibernateTemplate. Not often used, but a useful option
    /// to enhance testability, as it can easily be mocked or stubbed.
    /// <p>Provides HibernateTemplate's data access methods that mirror
    /// various Session methods. See the NHibernate ISession documentation
    /// for details on those methods.
    /// </p>
    /// </remarks>
    /// <threadsafety statis="true" instance="true"/> 
	/// <author>Mark Pollack (.NET)</author>
	public interface IHibernateOperations : ICommonHibernateOperations
	{

        /// <summary>
        /// Delete all given persistent instances.
        /// </summary>
        /// <param name="entities">The persistent instances to delete.</param>
        /// <remarks>
        /// This can be combined with any of the find methods to delete by query
        /// in two lines of code, similar to Session's delete by query methods.
        /// </remarks>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void DeleteAll(ICollection entities);

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning a result object, i.e. a domain
        /// object or a collection of domain objects.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        /// <param name="del">The delegate callback object that specifies the Hibernate action.</param>
        /// <returns>a result object returned by the action, or <code>null</code>
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        object Execute(HibernateDelegate del);

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning a result object, i.e. a domain
        /// object or a collection of domain objects.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        /// <param name="action">The callback object that specifies the Hibernate action.</param>
        /// <returns>a result object returned by the action, or <code>null</code>
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        object Execute(IHibernateCallback action);


        /// <summary>
        /// Execute the specified action assuming that the result object is a List.
        /// </summary>
        /// <remarks>
        /// This is a convenience method for executing Hibernate find calls or
        /// queries within an action.
        /// </remarks>
        /// <param name="action">The calback object that specifies the Hibernate action.</param>
        /// <returns>A IList returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList ExecuteFind(IHibernateCallback action);

	    /// <summary>
        /// Execute a query for persistent instances.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <returns>a List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList Find(string queryString);


        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="value">the value of the parameter</param>
        /// <returns>a List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList Find(string queryString, object value);

        /// <summary>
        /// Execute a query for persistent instances, binding one value
        /// to a "?" parameter of the given type in the query string.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="type">Hibernate type of the parameter (or <code>null</code>)</param>
        /// <returns>a List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList Find(string queryString, object value, IType type);


        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// </summary>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="values">the values of the parameters</param>
        /// <returns>a List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList Find(string queryString, object[] values);



        /// <summary>
        /// Execute a query for persistent instances, binding a number of
        /// values to "?" parameters of the given types in the query string.
        /// </summary>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or <code>null</code>)</param>
        /// <returns>a List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentException">If values and types are not null and their lengths are not equal</exception>          
        IList Find(string queryString, object[] values, IType[] types);


        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a named parameter in the query string.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>a List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByNamedParam(string queryName, string paramName, object value);

        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a named parameter in the query string.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">Hibernate type of the parameter (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByNamedParam(string queryName, string paramName, object value, IType type);

        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to named parameters in the query string.
        /// </summary>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByNamedParam(string queryString, string[] paramNames, object[] values);


        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to named parameters in the query string.
        /// </summary>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If paramNames length is not equal to values length or
        /// if paramNames length is not equal to types length (when types is not null)</exception>        
        IList FindByNamedParam(string queryString, string[] paramNames, object[] values, IType[] types);

        /// <summary>
        /// Execute a named query for persistent instances.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByNamedQuery(string queryName);

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        ///  A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByNamedQuery(string queryName, object value);

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">Hibernate type of the parameter (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByNamedQuery(string queryName, object value, IType type);

        
        /// <summary>
        /// Execute a named query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="values">The values of the parameters</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByNamedQuery(string queryName, object[] values);


        /// <summary>
        /// Execute a named query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If values and types are not null and their lengths differ.</exception>        
        IList FindByNamedQuery(string queryName, object[] values, IType[] types);

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a named parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByNamedQueryAndNamedParam(string queryName, string paramName, object value);
			
        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a named parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">The Hibernate type of the parameter (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByNamedQueryAndNamedParam(string queryName, string paramName, object value, IType type);
			
        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// number of values to named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters.</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByNamedQueryAndNamedParam(string queryName, string[] paramNames, object[] values);


        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// number of values to named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters.</param>
        /// <param name="types">Hibernate types of the parameters (or <code>null</code>)</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If paramNames length is not equal to values length or
        /// if paramNames length is not equal to types length (when types is not null)</exception>        
        IList FindByNamedQueryAndNamedParam(string queryName, string[] paramNames, object[] values, IType[] types);


        /// <summary>
        /// Execute a named query for persistent instances, binding the properties
        /// of the given object to named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="valueObject">The values of the parameters</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByNamedQueryAndValueObject(string queryName, object valueObject);

        /// <summary>
        /// Execute a query for persistent instances, binding the properties
        /// of the given object to <i>named</i> parameters in the query string.
        /// </summary>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>       
        /// <param name="valueObject">The values of the parameters</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList FindByValueObject(string queryString, object valueObject);

	    /// <summary>
        /// Return the persistent instance of the given entity type
        /// with the given identifier, or <code>null</code> if not found.
        /// </summary>
        /// <param name="entityType">a persistent type.</param>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <returns>the persistent instance, or null if not found</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        object Get(Type entityType, object id);

        /// <summary>
        /// Return the persistent instance of the given entity type
        /// with the given identifier, or <code>null</code> if not found.
        /// Obtains the specified lock mode if the instance exists.
        /// </summary>
        /// <param name="entityType">A persistent class.</param>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <param name="lockMode">The lock mode.</param>
        /// the persistent instance, or <code>null</code> if not found
        /// <returns>the persistent instance, or null if not found</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        object Get(Type entityType, object id, LockMode lockMode);

        /// <summary>
        /// Return the persistent instance of the given entity class
        ///  with the given identifier, throwing an exception if not found.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <returns>The persistent instance</returns>
        /// <exception cref="ObjectRetrievalFailureException">If not found</exception>     
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception> 
        object Load(Type entityType, object id);

        /// <summary>
        /// Return the persistent instance of the given entity class
        /// with the given identifier, throwing an exception if not found.
        /// Obtains the specified lock mode if the instance exists.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <param name="lockMode">The lock mode.</param>
        /// <returns>The persistent instance</returns>
        /// <exception cref="ObjectRetrievalFailureException">If not found</exception>     
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception> 
        object Load(Type entityType, object id, LockMode lockMode);



        /// <summary>
        /// Return all persistent instances of the given entity class.
        /// Note: Use queries or criteria for retrieving a specific subset.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns>A List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception> 
        IList LoadAll(Type entityType);


	    /// <summary>
        /// Save or update all given persistent instances,
        /// according to its id (matching the configured "unsaved-value"?).
        /// </summary>
        /// <param name="entities">The persistent instances to save or update
        /// (to be associated with the Hibernate Session)he entities.</param>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        void SaveOrUpdateAll(ICollection entities);
	}
}
