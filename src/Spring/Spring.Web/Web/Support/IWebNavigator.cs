#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Any component capable of navigating and participating in the navigation logic must implement this interface.
    /// </summary>
    /// <seealso cref="IHierarchicalWebNavigator"/>
    /// <seealso cref="IResultWebNavigator"/>
    /// <seealso cref="DefaultResultWebNavigator"/>
    /// <seealso cref="WebFormsResultWebNavigator"/>
    /// <author>Erich Eichinger</author>
    public interface IWebNavigator
    {
        /// <summary>
        /// Determines, whether this navigator or one of its parents can
        /// navigate to the destination specified in <paramref name="destination"/>.
        /// </summary>
        /// <param name="destination">the name of the navigation destination</param>
        /// <returns>true, if this navigator can navigate to the destination.</returns>
        bool CanNavigateTo( string destination );

        /// <summary>
        /// Instruct the navigator to navigate to the specified navigation destination.
        /// </summary>
        /// <param name="destination">the destination to navigate to.</param>
        /// <param name="sender">the sender that issued the navigation request.</param>
        /// <param name="context">the context to evaluate this navigation request in.</param>
        /// <exception cref="ArgumentOutOfRangeException">if this navigator cannot navigate to the specified <paramref name="destination"/> (<see cref="CanNavigateTo"/>).</exception>
        void NavigateTo( string destination, object sender, object context );

        /// <summary>
        /// Creates an uri poiniting to the specified navigation destination.
        /// </summary>
        /// <param name="destination">the destination to navigate to.</param>
        /// <param name="sender">the sender that issued the navigation request.</param>
        /// <param name="context">the context to evaluate this navigation request in.</param>
        /// <exception cref="ArgumentOutOfRangeException">if this navigator cannot navigate to the specified <paramref name="destination"/> (<see cref="CanNavigateTo"/>).</exception>
        string GetResultUri( string destination, object sender, object context );
    }
}
