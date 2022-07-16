#region Licence

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

namespace Spring.Dao
{
    /// <summary>
    /// Exception thrown on an optimistic locking violation for a mapped object.
    ///  Provides information about the persistent class and the identifier.
    /// </summary>
    /// <author>Mark Pollack (.NET)</author>
    [Serializable]
    public class ObjectOptimisticLockingFailureException : OptimisticLockingFailureException, ISerializable
    {
        #region Fields

        private object persistentClass;

        private object identifier;

        #endregion

        #region Constructor (s)

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectOptimisticLockingFailureException"/> class.
        /// </summary>
        public ObjectOptimisticLockingFailureException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectOptimisticLockingFailureException"/> class.
        /// </summary>
        /// <param name="message">A message about the exception..</param>
        public ObjectOptimisticLockingFailureException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="ObjectRetrievalFailureException"/> class.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception from the underlying data access API
        /// </param>
        public ObjectOptimisticLockingFailureException(string message, Exception rootCause) : base(message, rootCause)
        {
        }

        public ObjectOptimisticLockingFailureException(Type persistentClass, Object identifier) : this(persistentClass, identifier,
            "Object of class [" + persistentClass.Name + "] with identifier [" + identifier +
            "]: optimistic locking failed", null)
        {

        }

        public ObjectOptimisticLockingFailureException(
            Type persistentClass, Object identifier, String msg, Exception ex) : base(msg, ex)
        {
            this.persistentClass = persistentClass;
            this.identifier = identifier;
        }

        public ObjectOptimisticLockingFailureException(String persistentClassName, Object identifier) : this(persistentClassName, identifier,
            "Object of class [" + persistentClassName + "] with identifier [" + identifier +
            "]: optimistic locking failed", null)
        {

        }

        public ObjectOptimisticLockingFailureException(
            String persistentClassName, Object identifier, String msg, Exception ex) : base(msg, ex)
        {
            this.persistentClass = persistentClassName;
            this.identifier = identifier;
        }

        /// <inheritdoc />
        protected ObjectOptimisticLockingFailureException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion

        public Type PersistentClass
        {
            get { return persistentClass as Type;}
        }

        public object Identifier
        {
            get { return identifier; }
        }

        public string PersistentClassName
        {
            get
            {
                if (this.persistentClass is Type)
                {
                    return ((Type) this.persistentClass).Name;
                }
                return (this.persistentClass != null ? this.persistentClass.ToString() : null);

            }
        }




        #region Properties

        #endregion

        #region Methods

        #endregion

        #region ISerializable Members

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/>
        /// with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is a null reference (<see langword="Nothing"/> in Visual Basic).</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue( "persistentClass", persistentClass );
            info.AddValue( "identifier", identifier );
            base.GetObjectData( info, context );
        }

        #endregion
    }
}
