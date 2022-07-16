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
using Spring.Util;

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Exception thrown when the ObjectFactory cannot load the specified type of a given object.
    /// </summary>
    /// <author>Mark Pollack</author>
    [Serializable]
    public class CannotLoadObjectTypeException : FatalReflectionException
    {
        #region Constructor (s) / Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CannotLoadObjectTypeException"/> class.
        /// </summary>
        public CannotLoadObjectTypeException()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="CannotLoadObjectTypeException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public CannotLoadObjectTypeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="CannotLoadObjectTypeException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public CannotLoadObjectTypeException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CannotLoadObjectTypeException"/> class.
        /// </summary>
        /// <param name="resourceDescription">The resource description that the object definition came from.</param>
        /// <param name="objectName">Name of the object requested</param>
        /// <param name="objectTypeName">Name of the object type.</param>
        /// <param name="rootCause">The root cause.</param>
        public CannotLoadObjectTypeException(string resourceDescription, string objectName, string objectTypeName, Exception rootCause )
            : base("Cannot resolve type [" + objectTypeName + "] for object with name '" + objectName + "' defined in " + resourceDescription, rootCause)
        {
            this.resourceDescription = resourceDescription;
            this.objectName = objectName;
            this.objectTypeName = objectTypeName;
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="InvalidPropertyException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected CannotLoadObjectTypeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

            resourceDescription = info.GetString("ResourceDescription");
            objectName = info.GetString("ObjectName");
            objectTypeName = info.GetString("ObjectTypeName");
        }


        #endregion

        #region Properties

        /// <summary>
        /// Gets he name of the object we are trying to load.
        /// </summary>
        /// <value>The name of the object.</value>
        public string ObjectName
        {
            get { return objectName; }
        }

        /// <summary>
        /// Gets the name of the object type we are trying to load.
        /// </summary>
        /// <value>The name of the object type.</value>
        public string ObjectTypeName
        {
            get { return objectTypeName; }
        }

        /// <summary>
        /// Gets the resource description that the object definition came from
        /// </summary>
        /// <value>The resource description.</value>
        public string ResourceDescription
        {
            get { return resourceDescription; }
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
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ResourceDescription", ResourceDescription);
            info.AddValue("ObjectName", ObjectName);
            info.AddValue("ObjectTypeName", ObjectTypeName);
        }

        #endregion

        #region Fields

        private string resourceDescription;
        private string objectName;
        private string objectTypeName;

        #endregion
    }
}
