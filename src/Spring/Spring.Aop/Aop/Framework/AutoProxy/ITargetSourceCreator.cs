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

using Spring.Objects.Factory;

#endregion

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// Implementations can create special target sources, such as pooling target
    /// sources, for particular objects. For example, they may base their choice
    /// on attributes, such as a pooling attribute, on the target type.
    /// </summary>
    /// <remarks><p>AbstractAutoProxyCreator can support a number of TargetSourceCreators,
    /// which will be applied in order.</p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Adhari C Mahendra (.NET)</author>
    public interface ITargetSourceCreator
    {
        /// <summary>
        /// Create a special TargetSource for the given object, if any.
        /// </summary>
        /// <param name="objectType">The type of the object to create a TargetSource for</param>
        /// <param name="objectName">the name of the object</param>
        /// <param name="factory">the containing factory</param>
        /// <returns>a special TargetSource or null if this TargetSourceCreator isn't
        ///  interested in the particular object</returns>
        ITargetSource GetTargetSource(Type objectType, string objectName, IObjectFactory factory);
    }
}
