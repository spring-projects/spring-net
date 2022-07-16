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

using System.ComponentModel;

#endregion

namespace Spring.Core
{
    /// <summary>
    /// Provides additional data for the <c>PropertyChanged</c> event.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Provides some additional properties over and above the name of the
    /// property that has changed (which is inherited from the
    /// <see cref="System.ComponentModel.PropertyChangedEventArgs"/> base class).
    /// This allows calling code to determine whether or not a property has
    /// actually changed (i.e. a <c>PropertyChanged</c> event may have been
    /// raised, but the value itself may be equivalent).
    /// </p>
    /// </remarks>
    /// <seealso cref="System.ComponentModel.PropertyChangedEventArgs"/>
    [Serializable]
    public class PropertyChangeEventArgs : PropertyChangedEventArgs
    {
        private object oldValue;
        private object newValue;

        /// <summary>
        /// Create a new instance of the
        /// <see cref="Spring.Core.PropertyChangeEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that was changed.</param>
        /// <param name="oldValue">The old value of the property.</param>
        /// <param name="newValue">the new value of the property.</param>
        public PropertyChangeEventArgs(
            string propertyName, object oldValue, object newValue) : base(propertyName)
        {
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        /// <summary>
        /// Get the old value for the property.
        /// </summary>
        /// <seealso cref="System.ComponentModel.PropertyChangedEventArgs.PropertyName"/>
        public object OldValue
        {
            get { return oldValue; }
        }

        /// <summary>
        /// Get the new value of the property.
        /// </summary>
        /// <seealso cref="System.ComponentModel.PropertyChangedEventArgs.PropertyName"/>
        public object NewValue
        {
            get { return newValue; }
        }
    }
}
