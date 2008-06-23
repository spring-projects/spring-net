#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Reflection;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// A utility  class for raising events in a generic and consistent fashion.
    /// </summary>
    /// <author>Rick Evans</author>
    public class EventRaiser
    {
        /// <summary>
        /// Raises the event encapsulated by the supplied
        /// <paramref name="source"/>, passing the supplied <paramref name="arguments"/>
        /// to the event.
        /// </summary>
        /// <param name="source">The event to be raised.</param>
        /// <param name="arguments">The arguments to the event.</param>
        public virtual void Raise (Delegate source, params object [] arguments)  
        {
            if (source == null) 
            {
                return;
            }
            Delegate [] delegates = source.GetInvocationList ();
            foreach (Delegate sink in delegates) 
            {
                Invoke (sink, arguments);
            }
        }

        /// <summary>
        /// Invokes the supplied <paramref name="sink"/>, passing the supplied
        /// <paramref name="arguments"/> to the sink.
        /// </summary>
        /// <param name="sink">The sink to be invoked.</param>
        /// <param name="arguments">The arguments to the sink.</param>
        protected virtual void Invoke (Delegate sink, object [] arguments) 
        {
            try 
            {
                sink.DynamicInvoke (arguments);
            } 
            catch (TargetInvocationException ex) 
            {
                // unwrap the exception that actually caused the TargetInvocationException and throw that...
                throw ReflectionUtils.UnwrapTargetInvocationException(ex);
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
        protected override void Invoke (Delegate sink, object [] arguments) 
        {
            try 
            {
                sink.DynamicInvoke (arguments);
            }
            catch
            {
            }
        }
    }
}
