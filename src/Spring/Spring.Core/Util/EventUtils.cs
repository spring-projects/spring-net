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

using System.Diagnostics;
using System.Reflection;

using Common.Logging;

namespace Spring.Util
{
    /// <summary>
    /// A utility  class for raising events in a generic and consistent fashion.
    /// </summary>
    /// <author>Rick Evans</author>
    public class EventRaiser
    {
        protected class EventExceptionsCollector : IEventExceptionsCollector
        {
            private readonly Dictionary<Delegate, Exception> _eventExceptions;

            public EventExceptionsCollector()
            {
                _eventExceptions = new Dictionary<Delegate, Exception>();
            }

            public bool HasExceptions
            {
                get { return _eventExceptions.Count > 0; }
            }

            public IList<Delegate> Sources
            {
                get { return new List<Delegate>(_eventExceptions.Keys); }
            }

            public IList<Exception> Exceptions
            {
                get { return new List<Exception>(_eventExceptions.Values); }
            }

            public Exception this[Delegate source]
            {
                get
                {
                    Exception exception;
                    _eventExceptions.TryGetValue(source, out exception);
                    return exception;
                }
            }

            public void Add(Delegate source, Exception exception)
            {
                _eventExceptions.Add(source, exception);
            }
        }

        protected readonly ILog Log;

        /// <summary>
        /// Create a new EventRaiser instance
        /// </summary>
        public EventRaiser()
        {
            Log = LogManager.GetLogger(this.GetType());
        }

        /// <summary>
        /// Raises the event encapsulated by the supplied
        /// <paramref name="source"/>, passing the supplied <paramref name="arguments"/>
        /// to the event.
        /// </summary>
        /// <param name="source">The event to be raised.</param>
        /// <param name="arguments">The arguments to the event.</param>
        /// <returns>a map of sink/exception entries that occurred during event raising</returns>
        public virtual IEventExceptionsCollector Raise(Delegate source, params object[] arguments)
        {
            EventExceptionsCollector exceptions = new EventExceptionsCollector();

            if (source != null)
            {
                Delegate [] delegates = source.GetInvocationList ();
                foreach (Delegate sink in delegates)
                {
                    Invoke (sink, arguments, exceptions);
                }
            }
            return exceptions;
        }

        /// <summary>
        /// Invokes the supplied <paramref name="sink"/>, passing the supplied
        /// <paramref name="arguments"/> to the sink.
        /// </summary>
        /// <param name="sink">The sink to be invoked.</param>
        /// <param name="arguments">The arguments to the sink.</param>
        /// <param name="exceptions">the map of sink/exception entries to add any exception to</param>
        protected virtual void Invoke(Delegate sink, object[] arguments, EventExceptionsCollector exceptions)
        {
            try
            {
                sink.DynamicInvoke (arguments);
            }
            catch (TargetInvocationException ex)
            {
                // unwrap the exception that actually caused the TargetInvocationException and throw that...
                Exception cause = ReflectionUtils.UnwrapTargetInvocationException(ex);
                exceptions.Add(sink, cause);
                throw cause;
            }
        }
    }

    /// <summary>
    /// Raises events <b>defensively</b>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Raising events defensively means that as the raised event is passed to each handler,
    /// any <see cref="System.Exception"/> thrown by a handler will be caught and silently
    /// ignored.
    /// </p>
    /// </remarks>
    /// <author>Rick Evans</author>
    public class DefensiveEventRaiser : EventRaiser
    {
        /// <summary>
        /// <b>Defensively</b> invokes the supplied <paramref name="sink"/>, passing the
        /// supplied <paramref name="arguments"/> to the sink.
        /// </summary>
        /// <param name="sink">The sink to be invoked.</param>
        /// <param name="arguments">The arguments to the sink.</param>
        /// <param name="exceptions">the map of sink/exception entries to add any exception to</param>
        protected override void Invoke(Delegate sink, object[] arguments, EventExceptionsCollector exceptions)
        {
            try
            {
                sink.DynamicInvoke (arguments);
            }
            catch(Exception ex)
            {
                Log.Warn("Error during raising an event from " + new StackTrace(), ex);
                exceptions.Add(sink, ex);
            }
        }
    }
}
