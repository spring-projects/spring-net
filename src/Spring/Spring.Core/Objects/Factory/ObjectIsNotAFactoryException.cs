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

using System.Runtime.Serialization;

namespace Spring.Objects.Factory
{
    /// <summary>
    /// Thrown in response to an attempt to lookup a factory object, and
    /// the object identified by the lookup key is not a factory.
    /// </summary>
	/// <remarks>
	/// <p>
	/// An object is a factory if it implements (either directly or indirectly
	/// via inheritance) the <see cref="Spring.Objects.Factory.IFactoryObject"/>
	/// interface.
	/// </p>
	/// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class ObjectIsNotAFactoryException : ObjectNotOfRequiredTypeException
    {
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ObjectIsNotAFactoryException"/> class.
        /// </summary>
        public ObjectIsNotAFactoryException()
        {
        }

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="ObjectIsNotAFactoryException"/> class.
		/// </summary>
    	/// <param name="message">
    	/// A message about the exception.
    	/// </param>
    	public ObjectIsNotAFactoryException(string message)
    		: base(message)
    	{
    	}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="ObjectIsNotAFactoryException"/> class.
		/// </summary>
    	/// <param name="message">
    	/// A message about the exception.
    	/// </param>
    	/// <param name="rootCause">
    	/// The root exception that is being wrapped.
    	/// </param>
    	public ObjectIsNotAFactoryException(string message, Exception rootCause)
    		: base(message, rootCause)
    	{
    	}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="ObjectIsNotAFactoryException"/> class.
		/// </summary>
    	/// <param name="name">
    	/// The name of the object that was being retrieved from the factory.
    	/// </param>
    	/// <param name="actualInstance">
    	/// The object instance that was retrieved.
    	/// </param>
    	public ObjectIsNotAFactoryException(string name, object actualInstance)
    		: base(name, typeof (IFactoryObject), actualInstance)
    	{
    	}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="ObjectIsNotAFactoryException"/> class.
		/// </summary>
    	/// <param name="info">
    	/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
    	/// that holds the serialized object data about the exception being thrown.
    	/// </param>
    	/// <param name="context">
    	/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
    	/// that contains contextual information about the source or destination.
    	/// </param>
    	protected ObjectIsNotAFactoryException(
    		SerializationInfo info, StreamingContext context)
    		: base(info, context)
    	{
    	}
    }
}
