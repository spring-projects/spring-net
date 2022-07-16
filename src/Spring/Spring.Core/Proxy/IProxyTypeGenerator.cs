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

using System.Reflection.Emit;

namespace Spring.Proxy
{
	/// <summary>
    /// Describes the operations that generates IL instructions
    /// used to build the proxy type.
	/// </summary>
	/// <author>Bruno Baia</author>
	public interface IProxyTypeGenerator
	{
        // TODO : Why not ?
/*
        /// <summary>
        /// Gets the <see cref="TypeBuilder"/> used to build the proxy type.
        /// </summary>
        TypeBuilder ProxyTypeBuilder { get; }
*/
        /// <summary>
        /// Generates the IL instructions that pushes
        /// the proxy instance on stack.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        void PushProxy(ILGenerator il);

        /// <summary>
        /// Generates the IL instructions that pushes
        /// the target instance on which calls should be delegated to.
        /// </summary>
        /// <param name="il">The IL generator to use.</param>
        void PushTarget(ILGenerator il);
	}
}
