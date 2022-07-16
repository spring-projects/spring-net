#region License

/*
 * Copyright  2002-2005 the original author or authors.
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
using System.Security.Permissions;
using Spring.Core.IO;

namespace Spring.Objects.Factory
{
    /// <summary>
    /// Thrown when an <see cref="Spring.Objects.Factory.IObjectFactory"/>
    /// encounters an internal error, and its definitions are invalid.
    /// </summary>
    /// <remarks>
    /// <p>
    /// An example of a situation when this exception would be thrown is
    /// in the case of an XML document containing object definitions being
    /// malformed.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class ObjectDefinitionStoreException : FatalObjectException
    {
        /// <summary>
        /// Creates a new instance of the ObjectDefinitionStoreException class.
        /// </summary>
        public ObjectDefinitionStoreException()
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectDefinitionStoreException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public ObjectDefinitionStoreException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectDefinitionStoreException class.
        /// </summary>
        /// <param name="resourceDescription">
        /// The description of the resource that the object definition came from
        /// </param>
        /// <param name="name">
        /// The name of the object that triggered the exception.
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public ObjectDefinitionStoreException(
            string resourceDescription, string name, string message)
            : this(resourceDescription, name, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDefinitionStoreException"/> class.
        /// </summary>
        /// <param name="resourceDescription">
        /// The description of the resource that the object definition came from
        /// </param>
        /// <param name="msg">The detail message (used as exception message as-is)</param>
        /// <param name="cause">The root cause. (may be <code>null</code></param>
        public ObjectDefinitionStoreException(string resourceDescription, string msg, Exception cause)
            : this(msg, cause)
        {
            _resourceDescription = resourceDescription;
        }

        /// <summary>
        /// Creates a new instance of the ObjectDefinitionStoreException class.
        /// </summary>
        /// <param name="resourceLocation">
        /// The resource location (e.g. an XML object definition file) associated
        /// with the offending object definition.
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="name">
        /// The name of the object that triggered the exception.
        /// </param>
        public ObjectDefinitionStoreException(
            IResource resourceLocation,
            string name,
            string message)
            : this
                (resourceLocation == null ? string.Empty : resourceLocation.Description,
                name, message)
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectDefinitionStoreException class.
        /// </summary>
        /// <param name="resourceLocation">
        /// The resource location (e.g. an XML object definition file) associated
        /// with the offending object definition.
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="name">
        /// The name of the object that triggered the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ObjectDefinitionStoreException(
            IResource resourceLocation,
            string name,
            string message,
            Exception rootCause)
            : this((resourceLocation == null ? string.Empty : resourceLocation.Description), name, message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectDefinitionStoreException class.
        /// </summary>
        /// <param name="resourceDescription">
        /// The description of the resource that the object definition came from
        /// </param>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="name">
        /// The name of the object that triggered the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ObjectDefinitionStoreException(
            string resourceDescription,
            string name,
            string message,
            Exception rootCause)
            : base(
                string.Format(
                    "Error registering object {0}defined in '{1}' : {2}",
                    name == null ? string.Empty : string.Format("with name '{0}' ", name),
                    resourceDescription,
                    message),
                rootCause)
        {
            _resourceDescription = resourceDescription;
            _objectName = name;
        }

        /// <summary>
        /// Creates a new instance of the ObjectDefinitionStoreException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ObjectDefinitionStoreException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectDefinitionStoreException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected ObjectDefinitionStoreException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _resourceDescription = info.GetValue("_resourceDescription", typeof(object)) as string;
            _objectName = info.GetValue("_objectName", typeof(object)) as string;
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
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(
            SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_resourceDescription", ResourceDescription);
            info.AddValue("_objectName", ObjectName);
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
        /// The description of the resource associated with the object
        /// </summary>
        private readonly string _resourceDescription = string.Empty;

        /// <summary>
        /// The name of the object that trigger the exception.
        /// </summary>
        private readonly string _objectName = string.Empty;
    }
}
