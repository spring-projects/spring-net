#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace Spring.Proxy
{
    /// <summary>
    /// Defines interface that proxy method builders have to implement.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <author>Bruno Baia</author>
    public interface IProxyMethodBuilder
    {
        /// <summary>
        /// Dynamically builds proxy method.
        /// </summary>
        /// <param name="method">The method to proxy.</param>
        /// <param name="intfMethod">
        /// The interface definition of the method, if applicable.
        /// </param>
        /// <returns>
        /// The <see cref="System.Reflection.Emit.MethodBuilder"/> for the proxy method.
        /// </returns>
        MethodBuilder BuildProxyMethod(MethodInfo method, MethodInfo intfMethod);
    }
}
