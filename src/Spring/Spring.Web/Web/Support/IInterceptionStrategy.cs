#region License
/*
 * Copyright © 2002-2010 the original author or authors.
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

using Spring.Context;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Any concrete interception strategy must implement this interface
    /// </summary>
    /// <author>Erich Eichinger</author>
    internal interface IInterceptionStrategy
    {
        /// <summary>
        /// Any implementation must never throw an exception from this method. 
        /// Instead <c>false</c> must be returned to indicate interception failure.
        /// </summary>
        /// <returns>
        /// <c>true</c>, if interception succeeded. <c>false</c> otherwise.
        /// </returns>
        bool Intercept(IApplicationContext defaultApplicationContext,
                       ControlAccessor ctlAccessor, ControlCollectionAccessor ctlColAccessor);		
    }
}