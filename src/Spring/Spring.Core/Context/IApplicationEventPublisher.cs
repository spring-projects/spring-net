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

namespace Spring.Context;

/// <summary>
/// Encapsulates event publication functionality.
/// </summary>
/// <remarks>
/// <p>
/// Serves as a super-interface for the
/// <see cref="Spring.Context.IApplicationContext"/> interface.
/// </p>
/// </remarks>
/// <author>Juergen Hoeller</author>
/// <author>Rick Evans (.NET)</author>
public interface IApplicationEventPublisher
{
    /// <summary>
    /// Publishes an application context event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event. May be <see langword="null"/>.
    /// </param>
    /// <param name="e">
    /// The event that is to be raised.
    /// </param>
    void PublishEvent(object sender, ApplicationEventArgs e);
}
