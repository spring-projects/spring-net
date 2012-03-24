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

using AopAlliance.Intercept;

#endregion

namespace Spring.Aop.Target
{
	/// <summary>
	/// Statistics for a thread local <see cref="Spring.Aop.ITargetSource"/>.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Federico Spinazzi (.NET)</author>
	public interface IThreadLocalTargetSourceStats
	{
		/// <summary>
		/// Gets the number of invocations of the
		/// <see cref="ThreadLocalTargetSource.GetTarget()"/> and
		/// <see cref="ThreadLocalTargetSource.Invoke(IMethodInvocation)"/> methods.
		/// </summary>
		/// <value>
		/// The number of invocations of the
		/// <see cref="ThreadLocalTargetSource.GetTarget()"/> and
		/// <see cref="ThreadLocalTargetSource.Invoke(IMethodInvocation)"/> methods.
		/// </value>
		int Invocations { get; }

		/// <summary>
		/// Gets the number of hits that were satisfied by a thread bound object.
		/// </summary>
		/// <value>
		/// The number of hits that were satisfied by a thread bound object.
		/// </value>
		int Hits { get; }

		/// <summary>
		/// Gets the number of thread bound objects created.
		/// </summary>
		/// <value>The number of thread bound objects created.</value>
		int Objects { get; }
	}
}