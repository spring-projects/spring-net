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

#region Imports

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Holder for event handler values for an object.
    /// </summary>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class EventValues 
    {
        #region Constants
        /// <summary>
        /// The empty array of <see cref="Spring.Objects.IEventHandlerValue"/>s.
        /// </summary>
        private static readonly IEventHandlerValue [] EmptyHandlers = new IEventHandlerValue [] {};
        #endregion

        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Config.EventValues"/> class.
        /// </summary>
        public EventValues() {}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Config.EventValues"/> class.
        /// </summary>
        /// <param name="other">
        /// The <see cref="Spring.Objects.Factory.Config.EventValues"/>
        /// to be used to populate this instance.
        /// </param>
        public EventValues(EventValues other)
        {
            AddAll (other);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The mapping of event names to an
        /// <see cref="System.Collections.ICollection"/> of
        /// <see cref="Spring.Objects.IEventHandlerValue"/>s.
        /// </summary>
        protected IDictionary<string, IList<IEventHandlerValue>> EventHandlers 
        {
            get 
            {
                return _eventHandlers;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Collections.ICollection"/> of events
        /// that have handlers associated with them.
        /// </summary>
        public ICollection<string> Events 
        {
            get 
            {
                return EventHandlers.Keys;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Collections.ICollection"/> of
        /// <see cref="Spring.Objects.IEventHandlerValue"/>s for the supplied
        /// event name.
        /// </summary>
        public ICollection<IEventHandlerValue> this [string eventName] 
        {
            get
            {
                IList<IEventHandlerValue> handlers;
                if (!EventHandlers.TryGetValue(eventName, out handlers))
                {
                    handlers = EventValues.EmptyHandlers;
                }
                return handlers;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Copy all given argument values into this object.
        /// </summary>
        /// <param name="other">
        /// The <see cref="Spring.Objects.Factory.Config.EventValues"/>
        /// to be used to populate this instance.
        /// </param>
        public void AddAll (EventValues other) 
        {
            if (other != null) 
            {
                foreach (IList handlers in other.EventHandlers.Values) 
                {
                    foreach (IEventHandlerValue handler in handlers) 
                    {
                        AddHandler (handler);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the supplied handler to the collection of event handlers.
        /// </summary>
        /// <param name="handler">The handler to be added.</param>
        public void AddHandler (IEventHandlerValue handler)
        {
            IList<IEventHandlerValue> handlers;

            if (!EventHandlers.TryGetValue(handler.EventName, out handlers)) 
            {
                handlers = new List<IEventHandlerValue>();
                EventHandlers [handler.EventName] = handlers;
            }
            handlers.Add (handler);
        }
        #endregion

        #region Fields
        private IDictionary<string, IList<IEventHandlerValue>> _eventHandlers = new Dictionary<string, IList<IEventHandlerValue>>();
        #endregion
	}
}
