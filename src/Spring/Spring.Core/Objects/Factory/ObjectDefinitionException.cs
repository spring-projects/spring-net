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
using Spring.Objects.Factory.Xml;

namespace Spring.Objects.Factory
{
    /// <summary>
    /// Exception thrown when an <see cref="INamespaceParser"/>
    /// encounters an error when attempting to parse an object
    /// definition.
    /// </summary>
    /// <author>Federico Spinazzi (.NET)</author>
    [Serializable]
    public class ObjectDefinitionException : Exception
    {
        #region Fields
        private string _className;
        #endregion

        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the ObjectDefinitionException class.
        /// </summary>
        public ObjectDefinitionException ()
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectDefinitionException class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ObjectDefinitionException (string message, Exception rootCause)
            : base (message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectDefinitionException class.
        /// </summary>
        /// <param name="name">
        /// The value of the xml <code>class</code> attribute thet can be resolved
        /// as a type
        /// </param>
        public ObjectDefinitionException (string name)
        {
            _className = name;
        }

        /// <summary>
        /// Creates a new instance of the ObjectDefinitionException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected ObjectDefinitionException (
            SerializationInfo info, StreamingContext context)
            : base (info, context)
        {
            _className = info.GetString ("MyClassName");
        }
        #endregion

        #region Properties
        /// <summary>
        /// The message about the exception.
        /// </summary>
        public override string Message
        {
            get
            {
                return String.Format("The specified name ('{0}') cannot be used to resolve any System.Type instance", _className);
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
            SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData (info, context);
            info.AddValue ("MyClassName", _className);
        }
        #endregion
    }
}
