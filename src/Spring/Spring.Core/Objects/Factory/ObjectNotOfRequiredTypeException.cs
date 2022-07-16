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
using System.Security.Permissions;

namespace Spring.Objects.Factory {
    /// <summary>
    /// Thrown when an object doesn't match the required <see cref="System.Type"/>.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class ObjectNotOfRequiredTypeException : ObjectsException {
        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the ObjectNotOfRequiredTypeException class.
        /// </summary>
        public ObjectNotOfRequiredTypeException () {
        }

        /// <summary>
        /// Creates a new instance of the ObjectNotOfRequiredTypeException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public ObjectNotOfRequiredTypeException (string message)
            : base (message) {
        }

        /// <summary>
        /// Creates a new instance of the ObjectNotOfRequiredTypeException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ObjectNotOfRequiredTypeException (string message, Exception rootCause)
            : base (message, rootCause) {
        }

        /// <summary>
        /// Creates a new instance of the ObjectNotOfRequiredTypeException class.
        /// </summary>
        /// <param name="name">
        /// Name of the object requested.
        /// </param>
        /// <param name="requiredType">
        /// The required <see cref="System.Type"/> of the actual object
        /// instance that was retrieved.
        /// </param>
        /// <param name="actualInstance">
        /// The instance actually returned, whose class did not match the
        /// expected <see cref="System.Type"/>.
        /// </param>
        public ObjectNotOfRequiredTypeException (
            string name, Type requiredType, object actualInstance)
            : base (
            string.Format (
            "Object named '{0}' must be of type [{1}], but was actually of type [{2}]",
            name,
            requiredType.FullName,
            actualInstance.GetType ().FullName)) {
            this.name = name;
            this.actualInstance = actualInstance;
            this.requiredType = requiredType;
        }

        /// <summary>
        /// Creates a new instance of the ObjectNotOfRequiredTypeException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected ObjectNotOfRequiredTypeException (
            SerializationInfo info, StreamingContext context)
            : base (info, context)
        {
            requiredType = info.GetValue ("RequiredType", typeof (Type)) as Type;
            actualInstance = info.GetValue ("ActualInstance", typeof (object));
            name = info.GetString ("Name");
        }
        #endregion

        #region Properties
        /// <summary>
        /// The actual <see cref="System.Type"/> of the actual object
        /// instance that was retrieved.
        /// </summary>
        public Type ActualType {
            get {
                return ActualInstance.GetType ();
            }
        }
        /// <summary>
        /// The required <see cref="System.Type"/> of the actual object
        /// instance that was retrieved.
        /// </summary>
        public Type RequiredType {
            get {
                return requiredType;
            }
        }

        /// <summary>
        /// The instance actually returned, whose class did not match the
        /// expected <see cref="System.Type"/>.
        /// </summary>
        public object ActualInstance {
            get {
                return actualInstance;
            }
        }

        /// <summary>
        /// The name of the object requested.
        /// </summary>
        public string ObjectName {
            get {
                return name;
            }
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
        [SecurityPermission (SecurityAction.Demand,SerializationFormatter=true)]
        public override void GetObjectData (
            SerializationInfo info, StreamingContext context) {
            base.GetObjectData (info, context);
            info.AddValue ("RequiredType", requiredType);
            info.AddValue ("ActualInstance", actualInstance);
            info.AddValue ("Name", name);
        }
        #endregion

        #region Fields
        private Type requiredType;
        private object actualInstance;
        private string name;
        #endregion
    }
}
