#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Immutable placeholder class used for the value of a
    /// <see cref="Spring.Objects.PropertyValue"/> object when it's a reference
    /// to another object in this factory to be resolved at runtime.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class RuntimeObjectReference
    {
        #region Constructor (s) / Destructor

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This does <b>not</b> mark this object as being a reference to
        /// another object in any parent factory.
        /// </p>
        /// </remarks>
        /// <param name="objectName">The name of the target object.</param>
        public RuntimeObjectReference(string objectName)
            : this(objectName, false)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>
        /// class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This variant constructor allows a client to specifiy whether or not
        /// this object is a reference to another object in a parent factory.
        /// </p>
        /// </remarks>
        /// <param name="objectName">The name of the target object.</param>
        /// <param name="isToParent">
        /// Whether this object is an explicit reference to an object in a
        /// parent factory.
        /// </param>
        public RuntimeObjectReference(string objectName, bool isToParent)
        {
            _objectName = objectName;
            _isToParent = isToParent;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Return the target object name.
        /// </summary>
        public string ObjectName
        {
            get { return _objectName; }
        }

        /// <summary>
        /// Is this is an explicit reference to an object in the parent
        /// factory?
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this is an explicit reference to an
        /// object in the parent factory.
        /// </value>
        public bool IsToParent
        {
            get { return _isToParent; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a string representation of this instance.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture, "<{0}>", ObjectName);
        }

        #endregion

        #region Fields

        private string _objectName;

        private bool _isToParent;

        #endregion

        #region Equality Members

        /// <summary>
        /// Determines whether the specified RuntimeObjectReference is equal to the current RuntimeObjectReference.
        /// </summary>
        /// <returns>true if the specified RuntimeObjectReference is equal to the current RuntimeObjectReference; otherwise, false.</returns>
        protected bool Equals(RuntimeObjectReference other)
        {
            return string.Equals(_objectName, other._objectName) && _isToParent.Equals(other._isToParent);
        }

        /// <summary>
        /// Determines whether the specified System.Object is equal to the current RuntimeObjectReference.
        /// </summary>
        /// <returns>true if the specified RuntimeObjectReference is equal to the current RuntimeObjectReference; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RuntimeObjectReference)obj);
        }

        /// <summary>
        /// Serves as a hash function for RuntimeObjectReference.
        /// </summary>
        /// <returns>A hash code for the currentRuntimeObjectReference.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (_objectName.GetHashCode() * 397) ^ _isToParent.GetHashCode();
            }
        }

        #endregion
    }
}
