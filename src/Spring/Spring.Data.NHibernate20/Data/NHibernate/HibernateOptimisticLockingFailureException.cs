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
    /// Hibernate-specific subclass of ObjectOptimisticLockingFailureException.
    /// </summary>
    /// <remarks>
    /// Converts Hibernate's StaleObjectStateException.
    /// </remarks>
    /// <author>Mark Pollack (.NET)</author>
    /// <version>$Id: HibernateOptimisticLockingFailureException.cs,v 1.2 2008/04/23 11:41:41 lahma Exp $</version>
    /// 
    [Serializable]
    public class HibernateOptimisticLockingFailureException : ObjectOptimisticLockingFailureException
    {
        #region Fields

        #endregion

        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateOptimisticLockingFailureException"/> class.
        /// </summary>
        public HibernateOptimisticLockingFailureException()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="HibernateOptimisticLockingFailureException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public HibernateOptimisticLockingFailureException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateOptimisticLockingFailureException"/> class.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public HibernateOptimisticLockingFailureException(StaleObjectStateException ex) : base(ex.EntityName, ex.Identifier, ex.Message, ex)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateOptimisticLockingFailureException"/> class.
        /// </summary>
        /// <param name="ex">The StaleStateException.</param>
        public HibernateOptimisticLockingFailureException(StaleStateException ex) : base(ex.Message, ex)
 	    {
 	    }

        /// <summary>
        /// Creates a new instance of the HibernateOptimisticLockingFailureException class with the specified message
        /// and root cause.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public HibernateOptimisticLockingFailureException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="HibernateOptimisticLockingFailureException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected HibernateOptimisticLockingFailureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }



        #endregion

        #region Properties

        #endregion

        #region Methods

        #endregion
    }
}