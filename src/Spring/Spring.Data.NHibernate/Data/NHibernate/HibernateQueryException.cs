#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System;
using System.Runtime.Serialization;
using NHibernate;
using Spring.Dao;

#endregion

namespace Spring.Data.NHibernate
{
    /// <summary>
    /// Hibernate-specific subclass of InvalidDataAccessResourceUsageException,
    /// thrown on invalid HQL query syntax.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    [Serializable]
    public class HibernateQueryException : InvalidDataAccessResourceUsageException
    {
        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateQueryException"/> class.
        /// </summary>
        public HibernateQueryException()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="HibernateQueryException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public HibernateQueryException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateQueryException"/> class.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public HibernateQueryException(QueryException ex) : base(ex.Message, ex)
        {
        }

        /// <summary>
        /// Creates a new instance of the HibernateQueryException class with the specified message
        /// and root cause.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public HibernateQueryException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Gets the query string that was invalid.
        /// </summary>
        /// <value>The query string that was invalid.</value>
        public string QueryString
        {
            get { return ((QueryException) this.InnerException).QueryString; }
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="HibernateQueryException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected HibernateQueryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion
    }
}