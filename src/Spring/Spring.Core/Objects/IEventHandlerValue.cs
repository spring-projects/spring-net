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


namespace Spring.Objects
{
	/// <summary>
	/// Describes an event handler.
    /// </summary>
    /// <author>Rick Evans</author>
	public interface IEventHandlerValue 
    {
        /// <summary>
        /// The source of the event.
        /// </summary>
        object Source 
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the method that is going to handle the event.
        /// </summary>
        string MethodName 
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the event that is being wired up.
        /// </summary>
        string EventName 
        {
            get;
            set;
        }

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
        void Wire (object source, object handler);
	}
}
