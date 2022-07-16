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
using System.Reflection;

namespace Spring.Objects.Support
{
	/// <summary>
	/// Describes an event handler for a static class method.
    /// </summary>
    /// <author>Rick Evans</author>
    public class StaticEventHandlerValue : AbstractWiringEventHandlerValue
    {
        #region Constants
        private static readonly BindingFlags StaticMethodFlags =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.IgnoreCase;
        #endregion

        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Support.StaticEventHandlerValue"/> class.
        /// </summary>
        public StaticEventHandlerValue ()
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Support.StaticEventHandlerValue"/> class.
        /// </summary>
        /// <param name="source">
        /// The object (possibly unresolved) that is exposing the event.
        /// </param>
        /// <param name="methodName">
        /// The name of the method on the handler that is going to handle the event.
        /// </param>
        public StaticEventHandlerValue (object source, string methodName)
            : base (source, methodName) {}
        #endregion

        #region Methods
        /// <summary>
        /// Gets the event handler.
        /// </summary>
        /// <param name="instance">
        /// The instance that is registering for the event notification.
        /// </param>
        /// <param name="info">
        /// Event metadata about the event.
        /// </param>
        /// <returns>
        /// The event handler.
        /// </returns>
        protected override Delegate GetHandler(object instance, EventInfo info)
        {
            Type type = instance as Type;
            if (type == null)
            {
                throw new FatalObjectException (string.Format (
                    CultureInfo.InvariantCulture,
                    "Only 'System.Type' instances can be wired by instances of " +
                    "the StaticEventHandlerValue class; got '{0}'", instance));
            }
            MethodInfo method = ResolveHandlerMethod (
                type,
                info.EventHandlerType,
                StaticEventHandlerValue.StaticMethodFlags);
            return Delegate.CreateDelegate (info.EventHandlerType, method);
        }
        #endregion
	}
}
