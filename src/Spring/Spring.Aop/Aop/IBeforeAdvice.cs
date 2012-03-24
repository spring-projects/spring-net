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

using AopAlliance.Aop;

#endregion

namespace Spring.Aop
{
	/// <summary>
	/// Superinterface for all before advice.
	/// </summary>
	/// <remarks>
	/// <p>
	/// <i>Before</i> advice is advice that executes before a joinpoint, but
	/// which does not have the ability to prevent execution flow proceeding to
	/// the joinpoint (unless it throws an <see cref="System.Exception"/>).
	/// </p>
	/// <p>
	/// Spring.NET only supports <i>method</i> before advice. Although this
	/// is unlikely to change, this API is designed to allow <i>field</i>
	/// before advice in future if desired.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <seealso cref="Spring.Aop.IMethodBeforeAdvice"/>
	/// <seealso cref="Spring.Aop.IAfterReturningAdvice"/>
	/// <seealso cref="Spring.Aop.IThrowsAdvice"/>
	/// <seealso cref="AopAlliance.Intercept.IMethodInterceptor"/>
	public interface IBeforeAdvice : IAdvice
	{
	}
}