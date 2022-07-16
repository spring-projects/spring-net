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

using System.Collections;

namespace Spring.Objects
{
    /// <summary>
    /// This interface should be implemented by classes that want to
    /// have access to the shared state.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Shared state is very useful if you have data that needs to be shared by all instances
    /// of e.g. the same webform (or other <c>IHttpHandler</c>s).
    /// </p>
    /// <p>
    /// For example, <c>Spring.Web.UI.Page</c> class implements this interface, which allows
    /// each page derived from it to cache localizalization resources and parsed data binding
    /// expressions only once and then reuse the cached values, regardless of how many instances
    /// of the page are created.
    /// </p>
    /// </remarks>
    public interface ISharedStateAware
    {
        /// <summary>
        /// Gets or sets the <see cref="IDictionary"/> that should be used
        /// to store shared state for this instance.
        /// </summary>
        IDictionary SharedState { get; set; }
    }
}
