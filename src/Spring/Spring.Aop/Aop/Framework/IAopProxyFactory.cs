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

namespace Spring.Aop.Framework
{
	/// <summary> 
	/// Factory interface for the creation of AOP proxies based on
	/// <see cref="Spring.Aop.Framework.AdvisedSupport"/> configuration
	/// objects.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	public interface IAopProxyFactory
	{
		/// <summary>
		/// Creates an <see cref="Spring.Aop.Framework.IAopProxy"/> for the
		/// supplied <paramref name="advisedSupport"/> configuration.
		/// </summary>
		/// <param name="advisedSupport">The AOP configuration.</param>
		/// <returns>An <see cref="Spring.Aop.Framework.IAopProxy"/>.</returns>
		/// <exception cref="AopConfigException">
		/// If the supplied <paramref name="advisedSupport"/> configuration is
		/// invalid.
		/// </exception>
		IAopProxy CreateAopProxy(AdvisedSupport advisedSupport);
	}
}