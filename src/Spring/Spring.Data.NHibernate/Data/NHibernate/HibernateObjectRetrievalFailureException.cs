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
	/// Hibernate-specific subclass of ObjectRetrievalFailureException.
	/// </summary>	 
	/// <remarks>
	/// Converts Hibernate's UnresolvableObjectException, ObjectNotFoundException,
	/// ObjectDeletedException, and WrongClassException.
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	[Serializable]
	public class HibernateObjectRetrievalFailureException : ObjectRetrievalFailureException
	{

		#region Constructor (s)
		
        /// <summary>
		/// Initializes a new instance of the <see cref="HibernateObjectRetrievalFailureException"/> class.
        /// </summary>
		public HibernateObjectRetrievalFailureException()
		{
		}


        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateObjectRetrievalFailureException"/> class.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public HibernateObjectRetrievalFailureException(UnresolvableObjectException ex) : base(ex.PersistentClass, ex.Identifier, ex.Message, ex)        
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateObjectRetrievalFailureException"/> class.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public HibernateObjectRetrievalFailureException(ObjectNotFoundException ex) : base(ex.PersistentClass, ex.Identifier, ex.Message, ex)
        
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateObjectRetrievalFailureException"/> class.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public HibernateObjectRetrievalFailureException(ObjectDeletedException ex) : base(ex.PersistentClass, ex.Identifier, ex.Message, ex) 
        {

        }

        //TODO investigate WrongClassException.Type as equivalent to ex.PersistentClass
        /// <summary>
        /// Initializes a new instance of the <see cref="HibernateObjectRetrievalFailureException"/> class.
        /// </summary>
        /// <param name="ex">The ex.</param>
        public HibernateObjectRetrievalFailureException(WrongClassException ex) : base(ex.Type, ex.Identifier, ex.Message, ex)
        {
          
        }
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="HibernateObjectRetrievalFailureException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected HibernateObjectRetrievalFailureException(
            SerializationInfo info, StreamingContext context ) : base( info, context ) {}


		#endregion

	}
}
