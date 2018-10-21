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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Holder for event handler values for an object.
    /// </summary>
    /// <author>Rick Evans (.NET)</author>
    [Serializable]
    public class EventValues
    {
        /// <summary>
        /// The empty array of <see cref="Spring.Objects.IEventHandlerValue"/>s.
        /// </summary>
        private static readonly IEventHandlerValue[] EmptyHandlers = { };

        private static readonly string[] EmptyKeys = { };

        private Dictionary<string, List<IEventHandlerValue>> _eventHandlers;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Objects.Factory.Config.EventValues"/> class.
        /// </summary>
        public EventValues()
        {
        }

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
            AddAll(other);
        }

        /// <summary>
        /// Gets the <see cref="System.Collections.ICollection"/> of events
        /// that have handlers associated with them.
        /// </summary>
        public ICollection<string> Events => (ICollection<string>) _eventHandlers?.Keys ?? EmptyKeys;

        /// <summary>
        /// Gets the <see cref="System.Collections.ICollection"/> of
        /// <see cref="Spring.Objects.IEventHandlerValue"/>s for the supplied
        /// event name.
        /// </summary>
        public ICollection<IEventHandlerValue> this[string eventName]
        {
            get
            {
                if (_eventHandlers == null || !_eventHandlers.TryGetValue(eventName, out var handlers))
                {
                    return EmptyHandlers;
                }

                return handlers;
            }
        }

        /// <summary>
        /// Copy all given argument values into this object.
        /// </summary>
        /// <param name="other">
        /// The <see cref="Spring.Objects.Factory.Config.EventValues"/>
        /// to be used to populate this instance.
        /// </param>
        public void AddAll(EventValues other)
        {
            if (other?._eventHandlers != null)
            {
                foreach (var pair in other._eventHandlers)
                {
                    var list = pair.Value;
                    for (var i = 0; i < list.Count; i++)
                    {
                        AddHandler(list[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the supplied handler to the collection of event handlers.
        /// </summary>
        /// <param name="handler">The handler to be added.</param>
        public void AddHandler(IEventHandlerValue handler)
        {
            _eventHandlers = _eventHandlers ?? new Dictionary<string, List<IEventHandlerValue>>();

            if (!_eventHandlers.TryGetValue(handler.EventName, out var handlers))
            {
                handlers = new List<IEventHandlerValue>();
                _eventHandlers[handler.EventName] = handlers;
            }

            handlers.Add(handler);
        }
    }
}
