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

namespace Spring.Data.NHibernate.Generic
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
    /// <author>Sree Nivask (.NET)</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface IHibernateOperations : ICommonHibernateOperations
    {
        
        /// <summary>
        /// Return the persistent instance of the given entity type
        /// with the given identifier, or <see langword="null" /> if not found.
        /// Obtains the specified lock mode if the instance exists.
        /// </summary>
        /// <typeparam name="T">The object type to get.</typeparam>
        /// <param name="id">The id of the object to get.</param>
        /// <returns>the persistent instance, or <see langword="null" /> if not found</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>        
        T Get<T>(object id);

        /// <summary>
        /// Return the persistent instance of the given entity type
        /// with the given identifier, or null if not found.
        /// Obtains the specified lock mode if the instance exists.
        /// </summary>
        /// <typeparam name="T">The object type to get.</typeparam>
        /// <param name="id">The lock mode to obtain.</param>
        /// <param name="lockMode">The lock mode.</param>
        /// <returns>the persistent instance, or null if not found</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>     
        T Get<T>(object id, LockMode lockMode);

        /// <summary>
        /// Return the persistent instance of the given entity class
        /// with the given identifier, throwing an exception if not found.
        /// </summary>
        /// <typeparam name="T">The object type to load.</typeparam>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <returns>The persistent instance</returns>
        /// <exception cref="ObjectRetrievalFailureException">If not found</exception>     
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>     
        T Load<T>(object id);

        /// <summary>
        /// Return the persistent instance of the given entity class
        ///  with the given identifier, throwing an exception if not found.
        /// Obtains the specified lock mode if the instance exists.
        /// </summary>
        /// <typeparam name="T">The object type to load.</typeparam>
        /// <param name="id">An identifier of the persistent instance.</param>
        /// <param name="lockMode">The lock mode.</param>
        /// <returns>The persistent instance</returns>
        /// <exception cref="ObjectRetrievalFailureException">If not found</exception>     
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>  
        T Load<T>(object id, LockMode lockMode);

        /// <summary>
        /// Return all persistent instances of the given entity class.
        /// Note: Use queries or criteria for retrieving a specific subset.
        /// </summary>
        /// <typeparam name="T">The object type to load.</typeparam>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> LoadAll<T>();

        /// <summary>
        /// Execute a query for persistent instances.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <returns>
        /// a generic List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>  
        IList<T> Find<T>(string queryString);

        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="value">the value of the parameter</param>
        /// <returns>
        /// a generic List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>  
        IList<T> Find<T>(string queryString, object value);

        /// <summary>
        /// Execute a query for persistent instances, binding one value
        /// to a "?" parameter of the given type in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="type">Hibernate type of the parameter (or null)</param>
        /// <returns>
        /// a generic List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>  
        IList<T> Find<T>(string queryString, object value, IType type);

        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">a query expressed in Hibernate's query language</param>
        /// <param name="values">the values of the parameters</param>
        /// <returns>a generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>  
        IList<T> Find<T>(string queryString, object[] values);

        /// <summary>
        /// Execute a query for persistent instances, binding a number of
        /// values to "?" parameters of the given types in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or null)</param>
        /// <returns>
        /// a generic List containing 0 or more persistent instances
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception> 
        /// <exception cref="ArgumentException">If values and types are not null and their lenths are not equal</exception>          
        IList<T> Find<T>(string queryString, object[] values, IType[] types);

        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a  named parameter in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>a generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByNamedParam<T>(string queryName, string paramName, object value);

        /// <summary>
        /// Execute a query for persistent instances, binding
        /// one value to a  named parameter in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">The name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">Hibernate type of the parameter (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByNamedParam<T>(string queryName, string paramName, object value, IType type);

        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to  named parameters in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByNamedParam<T>(string queryString, string[] paramNames, object[] values);

        /// <summary>
        /// Execute a query for persistent instances, binding a
        /// number of values to  named parameters in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If paramNames length is not equal to values length or
        /// if paramNames length is not equal to types length (when types is not null)</exception>        
        IList<T> FindByNamedParam<T>(string queryString, string[] paramNames, object[] values, IType[] types);

        /// <summary>
        /// Execute a named query for persistent instances.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByNamedQuery<T>(string queryName);

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        ///  A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByNamedQuery<T>(string queryName, object value);

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a "?" parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">Hibernate type of the parameter (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByNamedQuery<T>(string queryName, object value, IType type);

        /// <summary>
        /// Execute a named query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="values">The values of the parameters</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByNamedQuery<T>(string queryName, object[] values);

        /// <summary>
        /// Execute a named query for persistent instances, binding a
        /// number of values to "?" parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="values">The values of the parameters</param>
        /// <param name="types">Hibernate types of the parameters (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If values and types are not null and their lengths differ.</exception>        
        IList<T> FindByNamedQuery<T>(string queryName, object[] values, IType[] types);

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a  named parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByNamedQueryAndNamedParam<T>(string queryName, string paramName, object value);

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// one value to a  named parameter in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramName">Name of the parameter</param>
        /// <param name="value">The value of the parameter</param>
        /// <param name="type">The Hibernate type of the parameter (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByNamedQueryAndNamedParam<T>(string queryName, string paramName, object value, IType type);

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// number of values to named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters.</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByNamedQueryAndNamedParam<T>(string queryName, string[] paramNames, object[] values);

        /// <summary>
        /// Execute a named query for persistent instances, binding
        /// number of values to  named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="paramNames">The names of the parameters</param>
        /// <param name="values">The values of the parameters.</param>
        /// <param name="types">Hibernate types of the parameters (or null)</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        /// <exception cref="ArgumentOutOfRangeException">If paramNames length is not equal to values length or
        /// if paramNames length is not equal to types length (when types is not null)</exception>                
        IList<T> FindByNamedQueryAndNamedParam<T>(string queryName, string[] paramNames, object[] values, IType[] types);

        /// <summary>
        /// Execute a named query for persistent instances, binding the properties
        /// of the given object to  named parameters in the query string.
        /// A named query is defined in a Hibernate mapping file.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryName">The name of a Hibernate query in a mapping file</param>
        /// <param name="valueObject">The values of the parameters</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByNamedQueryAndValueObject<T>(string queryName, object valueObject);

        /// <summary>
        /// Execute a query for persistent instances, binding the properties
        /// of the given object to <i>named</i> parameters in the query string.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="queryString">A query expressed in Hibernate's query language</param>       
        /// <param name="valueObject">The values of the parameters</param>
        /// <returns>A generic List containing 0 or more persistent instances</returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> FindByValueObject<T>(string queryString, object valueObject);

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning the result object.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        /// <typeparam name="T">The object type retrieved.</typeparam>
        /// <param name="del">The delegate callback object that specifies the Hibernate action.</param>
        /// <returns>a result object returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        T Execute<T>(HibernateDelegate<T> del);

        /// <summary>
        /// Execute the action specified by the delegate within a Session.
        /// </summary>
        /// <typeparam name="T">The object type retrieved.</typeparam>
        /// <param name="del">The HibernateDelegate that specifies the action
        /// to perform.</param>
        /// <param name="exposeNativeSession">if set to <c>true</c> expose the native hibernate session to
        /// callback code.</param>
        /// <returns>a result object returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        T Execute<T>(HibernateDelegate<T> del, bool exposeNativeSession);

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <typeparam name="T">The object type retrieved.</typeparam>
        /// <param name="action">The callback object that specifies the Hibernate action.</param>
        /// <returns>
        /// a result object returned by the action, or null
        /// </returns>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning the result object.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        T Execute<T>(IHibernateCallback<T> action);
        
        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <typeparam name="T">The object type retrieved.</typeparam>
        /// <param name="action">callback object that specifies the Hibernate action.</param>
        /// <param name="exposeNativeSession">if set to <c>true</c> expose the native hibernate session to
        /// callback code.</param>
        /// <returns>
        /// a result object returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        T Execute<T>(IHibernateCallback<T> action, bool exposeNativeSession);

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning the result object.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="del">The delegate callback object that specifies the Hibernate action.</param>
        /// <returns>A generic IList returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> ExecuteFind<T>(FindHibernateDelegate<T> del);

        /// <summary>
        /// Execute the action specified by the delegate within a Session.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="del">The FindHibernateDelegate that specifies the action
        /// to perform.</param>
        /// <param name="exposeNativeSession">if set to <c>true</c> expose the native hibernate session to
        /// callback code.</param>
        /// <returns>A generic IList returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> ExecuteFind<T>(FindHibernateDelegate<T> del, bool exposeNativeSession);

        /// <summary>
        /// Execute the action specified by the given action object within a Session.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="action">The callback object that specifies the Hibernate action.</param>
        /// <returns>
        /// A generic IList returned by the action, or null
        /// </returns>
        /// <remarks>
        /// Application exceptions thrown by the action object get propagated to the
        /// caller (can only be unchecked). Hibernate exceptions are transformed into
        /// appropriate DAO ones. Allows for returning the result object.
        /// <p>Note: Callback code is not supposed to handle transactions itself!
        /// Use an appropriate transaction manager like HibernateTransactionManager.
        /// Generally, callback code must not touch any Session lifecycle methods,
        /// like close, disconnect, or reconnect, to let the template do its work.
        /// </p>
        /// </remarks>
        IList<T> ExecuteFind<T>(IFindHibernateCallback<T> action);

        /// <summary>
        /// Execute the action specified by the given action object within a Session assuming that an IList is returned.
        /// </summary>
        /// <typeparam name="T">The object type to find.</typeparam>
        /// <param name="action">callback object that specifies the Hibernate action.</param>
        /// <param name="exposeNativeSession">if set to <c>true</c> expose the native hibernate session to
        /// callback code.</param>
        /// <returns>
        /// an IList returned by the action, or null
        /// </returns>
        /// <exception cref="DataAccessException">In case of Hibernate errors</exception>
        IList<T> ExecuteFind<T>(IFindHibernateCallback<T> action, bool exposeNativeSession);
    }
}
