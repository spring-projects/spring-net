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

using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Spring.Core;
using Spring.Util;

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Thrown when an <see cref="Spring.Objects.Factory.IObjectFactory"/>
	/// encounters an error when attempting to create an object from an object
	/// definition.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	[Serializable]
	public class ObjectCreationException : FatalObjectException
	{
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.ObjectCreationException"/> class.
		/// </summary>
		public ObjectCreationException()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.ObjectCreationException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public ObjectCreationException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.ObjectCreationException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
        /// <param name="objectName">
		/// The name of the object that triggered the exception.
		/// </param>
		public ObjectCreationException(string objectName, string message)
            : this(null, objectName, message, null)
		{
		}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.ObjectCreationException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ObjectCreationException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.ObjectCreationException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
        /// <param name="objectName">
		/// The name of the object that triggered the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public ObjectCreationException(
            string objectName, string message, Exception rootCause)
            : this(null, objectName, message, rootCause)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.ObjectCreationException"/> class.
		/// </summary>
		/// <param name="resourceDescription">
		/// The description of the resource associated with the object.
		/// </param>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
        /// <param name="objectName">
		/// The name of the object that triggered the exception.
		/// </param>
		public ObjectCreationException(
			string resourceDescription,
            string objectName,
			string message)
            : this(resourceDescription, objectName, message, null)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.ObjectCreationException"/> class.
		/// </summary>
		/// <param name="resourceDescription">
		/// The description of the resource associated with the object.
		/// </param>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
        /// <param name="objectName">
		/// The name of the object that triggered the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public ObjectCreationException(
			string resourceDescription,
            string objectName,
			string message,
			Exception rootCause)
			: base(message, rootCause)
		{
			_resourceDescription = resourceDescription;
            _objectName = objectName;
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.ObjectCreationException"/> class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected ObjectCreationException(
			SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_resourceDescription = (string) info.GetValue("_resourceDescription", typeof (string));
			_objectName = (string) info.GetValue("_objectName", typeof (string));
			_callStack = (string) info.GetValue("_callStack", typeof (string));
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
			base.GetObjectData(info, context);
			info.AddValue("_resourceDescription", ResourceDescription);
			info.AddValue("_objectName", ObjectName);
			info.AddValue("_callStack", _callStack);
		}

		/// <summary>
		/// The name of the object that triggered the exception (if any).
		/// </summary>
		public string ObjectName
		{
			get { return _objectName; }
		}

		/// <summary>
		/// The description of the resource associated with the object (if any).
		/// </summary>
		public string ResourceDescription
		{
			get { return _resourceDescription; }
		}

		/// <summary>
		/// Describes the creation failure trace of this exception.
		/// </summary>
		public override string Message
		{
			get
			{
				string message = ObjectCreationException.FormatMessage(
					_resourceDescription,
					_objectName,
					base.Message,
					_callStack);
				return message;
			}
		}

		private static string FormatMessage(
			string resourceDescription,
            string objectName,
			string message,
			string callStack)
		{
			if (StringUtils.IsNullOrEmpty(callStack))
			{
				return StringUtils.IsNullOrEmpty(resourceDescription) ?
					string.Format(
						"Error creating object with name '{0}' : {1}",
                        objectName,
						message)
					:
					string.Format(
						"Error creating object with name '{0}' defined in '{1}' : {2}",
                        objectName,
						resourceDescription,
						message);
			}
			else
			{
				return StringUtils.IsNullOrEmpty(resourceDescription) ?
					string.Format(
						"Error thrown by a dependency of object '{0}' : {1}{2}",
                        objectName,
						message,
						callStack)
					:
					string.Format(
						"Error thrown by a dependency of object '{0}' defined in '{1}' : {2}{3}",
                        objectName,
						resourceDescription,
						message,
						callStack);
			}
		}

		internal static ObjectCreationException GetObjectCreationException(
			Exception ex, string objectName, string propertyName,
			string resourceDescription, string referenceName)
		{
			ObjectCreationException ocex = ex as ObjectCreationException;
			if (ocex != null)
			{
				StringBuilder newCause = new StringBuilder();
				newCause.AppendFormat("{0} while resolving '{1}' to '{2}' ",
				                      Environment.NewLine, propertyName, ocex._objectName);
				if (StringUtils.HasText(ocex._resourceDescription))
				{
					newCause.Append("defined in '")
						.Append(ocex._resourceDescription)
						.Append("'");
				}
				if (StringUtils.IsNullOrEmpty(ocex._callStack))
				{
					ocex._callStack = newCause.ToString();
				}
				else
				{
					ocex._callStack = newCause.Append(ocex._callStack).ToString();
				}
				ocex._objectName = objectName;
				ocex._resourceDescription = resourceDescription;
				return ocex;
			}
			else if (ex is PropertyAccessExceptionsException
				&& ex.InnerException is TypeMismatchException)
			{
				return new ObjectCreationException(
					resourceDescription,
					objectName,
					string.Format(
						CultureInfo.InvariantCulture,
						"Invalid type for property '{0}' of object '{1}' in '{2}'.",
						propertyName,
						objectName,
						resourceDescription),
					ex);
			}
			return new ObjectCreationException(
				resourceDescription,
				objectName,
				string.Format(
					CultureInfo.InvariantCulture,
					"Can't resolve reference to object '{0}' while setting '{1}'.",
					referenceName,
					propertyName),
				ex);
		}

		private string _callStack;
		private string _resourceDescription = string.Empty;
		private string _objectName = string.Empty;
	}
}
