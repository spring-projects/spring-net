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
using Spring.Util;

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Exception thrown when an <see cref="Spring.Objects.Factory.IObjectFactory"/>
	/// is asked for an object instance name for which it cannot find a definition.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Rick Evans (.NET)</author>
	[Serializable]
	public class NoSuchObjectDefinitionException : ObjectsException
	{
		#region Constructor (s) / Destructor

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.NoSuchObjectDefinitionException"/> class.
		/// </summary>
		public NoSuchObjectDefinitionException()
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.NoSuchObjectDefinitionException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public NoSuchObjectDefinitionException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.NoSuchObjectDefinitionException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public NoSuchObjectDefinitionException(string message, Exception rootCause)
			: base(message, rootCause)
		{
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.NoSuchObjectDefinitionException"/> class.
		/// </summary>
		/// <param name="name">
		/// Name of the missing object.
		/// </param>
		/// <param name="message">
		/// A further, detailed message describing the problem.
		/// </param>
		public NoSuchObjectDefinitionException(string name, string message)
			: base(string.Format(
				CultureInfo.CurrentCulture,
				"No object named '{0}' is defined : {1}",
				name,
				StringUtils.HasText(message) ? message : "not found."))
		{
			_objectName = name;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="NoSuchObjectDefinitionException"/> class.
        /// </summary>
        /// <param name="type">The required type of the object.</param>
        /// <param name="dependencyDescription">A description of the originating dependency.</param>
        /// <param name="message">A message describing the problem.</param>
        public NoSuchObjectDefinitionException(Type type, string dependencyDescription, string message)
            : base(string.Format(
                CultureInfo.CurrentCulture,
                "No matching object of type [{0}] found for dependency [{1}]: {2}",
                type.FullName, dependencyDescription, message))

        {
            _objectType = type;
        }

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.NoSuchObjectDefinitionException"/> class.
		/// </summary>
		/// <param name="type">
		/// The <see cref="System.Type"/> of the missing object.
		/// </param>
		/// <param name="message">
		/// A further, detailed message describing the problem.
		/// </param>
		public NoSuchObjectDefinitionException(Type type, string message)
			: base(string.Format(
				CultureInfo.CurrentCulture,
				"No unique object of type [{0}] is defined : {1}",
				type != null ? type.FullName : "<< no Type specified >>",
				StringUtils.HasText(message) ? message : "not found."))
		{
			_objectType = type;
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.NoSuchObjectDefinitionException"/> class.
		/// </summary>
		/// <param name="info">
		/// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
		/// that holds the serialized object data about the exception being thrown.
		/// </param>
		/// <param name="context">
		/// The <see cref="System.Runtime.Serialization.StreamingContext"/>
		/// that contains contextual information about the source or destination.
		/// </param>
		protected NoSuchObjectDefinitionException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_objectName = info.GetString("ObjectName");
			var typeName = info.GetString("ObjectTypeName");
			_objectType = typeName != null ? Type.GetType(typeName) : null;
		}

		#endregion

		#region Methods

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
			info.AddValue("ObjectName", ObjectName);
			info.AddValue("ObjectTypeName", ObjectType?.AssemblyQualifiedNameWithoutVersion());
		}

		#endregion

		#region Properties

		/// <summary>
		/// Return the required <see cref="System.Type"/> of object, if it was a
		/// lookup by <see cref="System.Type"/> that failed.
		/// </summary>
		public Type ObjectType
		{
			get { return _objectType; }
		}

		/// <summary>
		/// Return the name of the missing object, if it was a lookup by name that
		/// failed.
		/// </summary>
		public string ObjectName
		{
			get { return _objectName; }
		}

		#endregion

		#region Fields

		private Type _objectType;
		private string _objectName;

		#endregion
	}
}
