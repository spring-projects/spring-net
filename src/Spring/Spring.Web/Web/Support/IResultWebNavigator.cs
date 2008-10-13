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

#region Imports

using System.Collections;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Defines the interface, all hierarchical navigators capable of 
    /// dealing with <see cref="IResult"/> instances must implement.
    /// </summary>
    public interface IResultWebNavigator : IHierarchicalWebNavigator
    {
        /// <summary>
        /// Contains the mappings of navigation destination names to <see cref="IResult"/> 
        /// instances or their corresponding textual representations.<br/>
        /// See <see cref="ResultFactoryRegistry"/> for more information on how textual representations are resolved.
        /// </summary>
        /// <seealso cref="IWebNavigator"/>
        /// <seealso cref="IHierarchicalWebNavigator"/>
        /// <seealso cref="DefaultResultWebNavigator"/>
        /// <seealso cref="ResultFactoryRegistry"/>
        /// <seealso cref="DefaultResultFactory"/>
        IDictionary Results { get; set; }
    }
}