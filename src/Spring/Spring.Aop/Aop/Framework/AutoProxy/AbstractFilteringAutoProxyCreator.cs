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

#region Imports

#endregion

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// The base class for AutoProxyCreator implementations that mark objects
    /// eligible for proxying based on arbitrary criteria.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public abstract class AbstractFilteringAutoProxyCreator : AbstractAutoProxyCreator
    {
        /// <summary>
        ///Overridden to call <see cref="IsEligibleForProxying"/>.
        /// </summary>
        /// <param name="targetType">the type of the object</param>
        /// <param name="targetName">the name of the object</param>
        /// <returns>if remarkable to skip</returns>
        protected override bool ShouldSkip( Type targetType, string targetName )
        {
            bool shouldSkip = !IsEligibleForProxying( targetType, targetName );
            return shouldSkip;
        }

        /// <summary>
        /// Override to always return <see cref="AbstractAutoProxyCreator.PROXY_WITHOUT_ADDITIONAL_INTERCEPTORS"/>.
        /// </summary>
        /// <remarks>
        /// Whether an object shall be proxied or not is determined by the result of <see cref="IsEligibleForProxying"/>.
        /// </remarks>
        /// <param name="targetType">ingored</param>
        /// <param name="targetName">ignored</param>
        /// <param name="customTargetSource">ignored</param>
        /// <returns>
        /// Always <see cref="AbstractAutoProxyCreator.PROXY_WITHOUT_ADDITIONAL_INTERCEPTORS"/> to indicate, that the object shall be proxied.
        /// </returns>
        /// <seealso cref="AbstractAutoProxyCreator.PROXY_WITHOUT_ADDITIONAL_INTERCEPTORS"/>
        protected override IList<object> GetAdvicesAndAdvisorsForObject(Type targetType, string targetName, ITargetSource customTargetSource)
        {
            return PROXY_WITHOUT_ADDITIONAL_INTERCEPTORS;
        }

        /// <summary>
        /// Decide, whether the given object is eligible for proxying.
        /// </summary>
        /// <remarks>
        /// Override this method to allow or reject proxying for the given object.
        /// </remarks>
        /// <param name="targetType">the object's type</param>
        /// <param name="targetName">the name of the object</param>
        /// <seealso cref="AbstractAutoProxyCreator.ShouldSkip"/>
        /// <returns>whether the given object shall be proxied.</returns>
        protected abstract bool IsEligibleForProxying( Type targetType, string targetName );
    }
}
