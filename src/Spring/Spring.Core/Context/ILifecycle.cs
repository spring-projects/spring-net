#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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

namespace Spring.Context
{
    /// <summary>
    /// Interface defining methods for start/stop lifecycle control.
    /// The typical use case for this is to control asynchronous processing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Can be implemented by both components (typically a Spring object defined in
    /// a spring <see cref="Spring.Objects.Factory.IObjectFactory"/> and containers
    /// (typically a spring <see cref="IApplicationContext"/>.  Containers will 
    /// propagate start/stop signals to all components that apply.
    /// </para>
    /// </remarks>
    /// <author>Juergen Hoeller</author>
    /// <author>Mark Pollack (.NET)</author>
    public interface ILifecycle
    {
        /// <summary>
        /// Starts this component.
        /// </summary>
        /// <remarks>Should not throw an exception if the component is already running.
        /// In the case of a container, this will propagate the start signal
        /// to all components that apply.
        /// </remarks>
        void Start();

        /// <summary>
        /// Stops this component.
        /// </summary>
        /// <remarks>
        /// Should not throw an exception if the component isn't started yet.
        /// In the case of a container, this will propagate the stop signal
        /// to all components that apply.
        /// </remarks>
        void Stop();

        /// <summary>
        /// Gets a value indicating whether this component is currently running.
        /// </summary>
        /// <remarks>
        /// In the case of a container, this will return <code>true</code>
        /// only if <i>all</i> components that apply are currently running.
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if this component is running; otherwise, <c>false</c>.
        /// </value>
        bool IsRunning
        {
            get;
        }
    }
}
