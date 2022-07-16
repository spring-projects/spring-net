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
using Spring.Util;

namespace Spring.Objects.Support
{
	/// <summary>
	/// Base class implementation for classes that describe an event handler.
    /// </summary>
    /// <author>Rick Evans</author>
    public abstract class AbstractEventHandlerValue : IEventHandlerValue
    {
        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Support.AbstractEventHandlerValue"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        protected AbstractEventHandlerValue() {}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Support.AbstractEventHandlerValue"/> class.
        /// </summary>
        /// <param name="source">
        /// The object (possibly unresolved) that is exposing the event.
        /// </param>
        /// <param name="methodName">
        /// The name of the method on the handler that is going to handle the event.
        /// </param>
        /// <remarks>
        /// <p>
        /// This is an <see langword="abstract"/> class, and as such exposes no public constructors.
        /// </p>
        /// </remarks>
        protected AbstractEventHandlerValue (object source, string methodName)
        {
            _source = source;
            _methodName = methodName;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The source of the event (may be unresolved, as in the case
        /// of a <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>
        /// value).
        /// </summary>
        public virtual object Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
            }
        }

        /// <summary>
        /// The name of the method that is going to handle the event.
        /// </summary>
        public virtual string MethodName
        {
            get
            {
                return _methodName;
            }
            set
            {
                _methodName = StringUtils.HasText (value) ? value.Trim () : string.Empty;
            }
        }

        /// <summary>
        /// The name of the event that is being wired up.
        /// </summary>
        public virtual string EventName
        {
            get
            {
                return _eventName;
            }
            set
            {
                _eventName = StringUtils.HasText (value) ? value.Trim () : string.Empty;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Wires up the specified handler to the named event on the
        /// supplied event source.
        /// </summary>
        /// <param name="source">
        /// The object (an object instance, a <see cref="System.Type"/>, etc)
        /// exposing the named event.
        /// </param>
        /// <param name="handler">
        /// The handler for the event (an object instance, a
        /// <see cref="System.Type"/>, etc).
        /// </param>
        public abstract void Wire (object source, object handler);

        /// <summary>
        /// Returns a stringified representation of this object.
        /// </summary>
        /// <returns>A stringified representation of this object.</returns>
        public override string ToString()
        {
            return string.Format (
                CultureInfo.InvariantCulture,
                "{0} [Source = '{1}', Method = '{2}']", GetType ().FullName, Source, MethodName);
        }
        #endregion

        #region Fields
        private object _source;
        private string _methodName;
        private string _eventName;
        #endregion
	}
}
