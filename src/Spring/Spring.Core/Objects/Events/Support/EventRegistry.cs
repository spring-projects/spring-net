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

using System.Reflection;

namespace Spring.Objects.Events.Support
{
    /// <summary>
    /// Default implementation of the <see cref="Spring.Objects.Events.IEventRegistry"/>
    /// interface.
    /// </summary>
    /// <author>Griffin Caprio</author>
    public class EventRegistry : IEventRegistry
    {
        private readonly IList<object> _publishers;

        /// <summary>
        /// Creates a new instance of the EventRegistry class.
        /// </summary>
        public EventRegistry()
        {
            _publishers = new List<object>();
        }

        /// <summary>
        /// The list of event publishers.
        /// </summary>
        /// <value>The list of event publishers.</value>
        protected IList<object> Publishers
        {
            get { return _publishers; }
        }

        /// <summary>
        /// Adds the input object to the list of publishers.
        /// </summary>
        /// <remarks>
        /// This publishes <b>all</b> events of the source object to any object
        /// wishing to subscribe
        /// </remarks>
        /// <param name="source">The source object to publish.</param>
        public virtual void PublishEvents(object source)
        {
            Publishers.Add(source);
        }

        /// <summary>
        /// Subscribes to <b>all</b> events published, if the subscriber implements
        /// compatible handler methods.
        /// </summary>
        /// <param name="subscriber">The subscriber to use.</param>
        public virtual void Subscribe(object subscriber)
        {
            Subscribe(subscriber, null);
        }

        /// <summary>
        /// Subscribes to published events of all objects of a given type, if the
        /// subscriber implements compatible handler methods.
        /// </summary>
        /// <param name="subscriber">The subscriber to use.</param>
        /// <param name="sourceType">
        /// The target <see cref="System.Type"/> to subscribe to.
        /// </param>
        public virtual void Subscribe(object subscriber, Type sourceType)
        {
            Type currentSubscriberType = subscriber.GetType();
            foreach (object currentPublisher in _publishers)
            {
                if (null == sourceType
                    || sourceType.IsAssignableFrom(currentPublisher.GetType()))
                {
                    WireOrUnwireSubscriberToPublisher(
                        currentPublisher, currentSubscriberType, subscriber, true);
                }
            }
        }

        /// <summary>
        /// Unsubscribes to <b>all</b> events published, if the subscriber
        /// implmenets compatible handler methods.
        /// </summary>
        /// <param name="subscriber">The subscriber to use</param>
        public virtual void Unsubscribe(object subscriber)
        {
            Unsubscribe(subscriber, null);
        }

        /// <summary>
        /// Unsubscribes to the published events of all objects of a given
        /// <see cref="System.Type"/>, if the subscriber implements
        /// compatible handler methods.
        /// </summary>
        /// <param name="subscriber">The subscriber to use.</param>
        /// <param name="sourceType">The target <see cref="System.Type"/> to unsubscribe from</param>
        public virtual void Unsubscribe(object subscriber, Type sourceType)
        {
            Type currentSubscriberType = subscriber.GetType();
            foreach (object currentPublisher in _publishers)
            {
                if (null == sourceType
                    || sourceType.IsAssignableFrom(currentPublisher.GetType()))
                {
                    WireOrUnwireSubscriberToPublisher(
                        currentPublisher, currentSubscriberType, subscriber, false);
                }
            }
        }

        private static void WireOrUnwireSubscriberToPublisher(
            object currentPublisher, Type currentSubscriberType, object subscriber, bool wire)
        {
            Type currentPublisherType = currentPublisher.GetType();
            EventInfo[] events = currentPublisherType.GetEvents();
            foreach (EventInfo currentEvent in events)
            {
                Type eventHandlerType = currentEvent.EventHandlerType;
                MethodInfo invoke = eventHandlerType.GetMethod("Invoke");
                MethodInfo eventHandler
                    = EventManipulationUtils.GetMethodInfoMatchingSignature(
                        invoke, currentSubscriberType);
                if (eventHandler != null)
                {
                    if (wire)
                    {
                        currentEvent.AddEventHandler(
                            currentPublisher,
                            EventManipulationUtils.GetHandlerDelegate(
                                eventHandlerType, subscriber, eventHandler));
                    }
                    else
                    {
                        currentEvent.RemoveEventHandler(
                            currentPublisher,
                            EventManipulationUtils.GetHandlerDelegate(
                                eventHandlerType, subscriber, eventHandler));
                    }
                }
            }
        }
    }
}
