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

#region Imports

using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Spring.Core;

#endregion

namespace Spring.Objects
{
	/// <summary>
	/// Combined exception, composed of individual binding
	/// <see cref="PropertyAccessException"/>s.
	/// </summary>
	/// <remarks>
	/// <p>
	/// An object of this class is created at the beginning of the binding
	/// process, and errors added to it as necessary.
	/// </p>
	/// <p>
	/// The binding process continues when it encounters application-level
	/// <see cref="PropertyAccessException"/>s, applying those changes
	/// that can be applied and storing rejected changes in an instance of this class.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Mark Pollack (.NET)</author>
	[Serializable]
	public class PropertyAccessExceptionsException : ObjectsException
	{
		#region Constants

		private static PropertyAccessException[] EmptyPropertyAccessExceptions
			= new PropertyAccessException[] {};

		#endregion

		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the PropertyAccessExceptionsException class.
		/// </summary>
		public PropertyAccessExceptionsException()
		{
		}

		/// <summary>
		/// Creates a new instance of the PropertyAccessExceptionsException class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public PropertyAccessExceptionsException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of the PropertyAccessExceptionsException class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public PropertyAccessExceptionsException(string message, Exception rootCause)
			: base(message, rootCause)
		{
		}

		/// <summary>
		/// Create new empty PropertyAccessExceptionsException.
		/// We'll add errors to it as we attempt to bind properties.
		/// </summary>
		public PropertyAccessExceptionsException(
			IObjectWrapper objectWrapper,
			PropertyAccessException[] propertyAccessExceptions)
			: base(string.Empty)
		{
			_objectWrapper = objectWrapper;
			_propertyAccessExceptions = propertyAccessExceptions ?? EmptyPropertyAccessExceptions;
		}

		/// <summary>
		/// Creates a new instance of the PropertyAccessExceptionsException class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected PropertyAccessExceptionsException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion

		/// <summary>
		/// Return the <see cref="Spring.Objects.IObjectWrapper"/> that generated
		/// this exception.
		/// </summary>
		public IObjectWrapper ObjectWrapper
		{
			get { return _objectWrapper; }
		}

		/// <summary>
		/// Return the object we're binding to.
		/// </summary>
		public object BindObject
		{
			get { return ObjectWrapper.WrappedInstance; }
		}

		/// <summary>
		/// If this returns zero (0), no errors were encountered during binding.
		/// </summary>
		public int ExceptionCount
		{
			get { return PropertyAccessExceptions.Length; }
		}

		/// <summary>
		/// Return an array of the <see cref="PropertyAccessException"/>s
		/// stored in this object.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Will return the empty array (not <see langword="null"/>) if there were no errors.
		/// </p>
		/// </remarks>
		public virtual PropertyAccessException[] PropertyAccessExceptions
		{
			get { return _propertyAccessExceptions; }
		}

		/// <summary>
		/// Describe the group of exceptions.
		/// </summary>
		public override string Message
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("PropertyAccessExceptionsException (")
				.Append(ExceptionCount)
				.Append(" errors)")
				.Append("; nested PropertyAccessExceptions are: \n");
				for (int i = 0; i < PropertyAccessExceptions.Length; ++i)
				{
					PropertyAccessException pae = PropertyAccessExceptions[i];
					sb.Append("[");
					sb.Append(pae.GetType().FullName);
					sb.Append(": ");
					sb.Append(pae.Message);
					if (pae.InnerException != null)
					{
						sb.Append(", Inner Exception: ");
						sb.Append(pae.InnerException.ToString());
					}
					sb.Append(']');
					if (i < PropertyAccessExceptions.Length - 1)
					{
						sb.Append(", ");
					}
				}
				return sb.ToString();
			}
		}

		/// <summary>
		/// Populates a <see cref="System.Runtime.Serialization.SerializationInfo"/> with
		/// the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/> to populate
		/// with data.
		/// </param>
		/// <param name="context">
		/// The destination (see <see cref="System.Runtime.Serialization.StreamingContext"/>)
		/// for this serialization.
		/// </param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(
			SerializationInfo info, StreamingContext context)
		{
			// TODO serialize me
			base.GetObjectData(info, context);
		}

		/// <summary>
		/// The IObjectWrapper wrapping the target object at the root of the exception.
		/// </summary>
		private IObjectWrapper _objectWrapper;

		/// <summary>The list of PropertyAccessException objects.</summary>
		private PropertyAccessException[] _propertyAccessExceptions
			= EmptyPropertyAccessExceptions;

		/// <summary>
		/// Return the <see cref="PropertyAccessException"/>
		/// for the supplied <paramref name="propertyName"/>, or <see langword="null"/>
		/// if there isn't one.
		/// </summary>
		public PropertyAccessException GetPropertyAccessException(
			string propertyName)
		{
			foreach (PropertyAccessException pae in PropertyAccessExceptions)
			{
				if (propertyName.Equals(pae.PropertyChangeArgs.PropertyName))
				{
					return pae;
				}
			}
			return null;
		}

		/// <summary>
		/// Describe the number of exceptions contained in this container class.
		/// </summary>
		/// <returns>A description of the instance contents.</returns>
		public override string ToString()
		{
			return Message;
		}
	}
}
