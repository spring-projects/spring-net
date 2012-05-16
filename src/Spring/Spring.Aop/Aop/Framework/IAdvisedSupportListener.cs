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
	/// Callback interface for
	/// <see cref="Spring.Aop.Framework.AdvisedSupport"/> listeners.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Allows <see cref="Spring.Aop.Framework.IAdvisedSupportListener"/>
	/// implementations to be notified of notable lifecycle events relating
	/// to the creation of a proxy, and changes to the configuration data of a
	/// proxy.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	public interface IAdvisedSupportListener
	{
	    /// <summary>
	    /// Invoked when the first proxy is created.
	    /// </summary>
	    /// <param name="source">
	    /// The relevant <see cref="Spring.Aop.Framework.AdvisedSupport"/> source.
	    /// </param>
	    void Activated(AdvisedSupport source);

	    /// <summary>
	    /// Invoked when advice is changed after a proxy is created.
	    /// </summary>
	    /// <param name="source">
	    /// The relevant <see cref="Spring.Aop.Framework.AdvisedSupport"/> source.
	    /// </param>
	    void AdviceChanged(AdvisedSupport source);

	    /// <summary>
	    /// Invoked when interfaces are changed after a proxy is created.
	    /// </summary>
	    /// <param name="source">
	    /// The relevant <see cref="Spring.Aop.Framework.AdvisedSupport"/> source.
	    /// </param>
	    void InterfacesChanged(AdvisedSupport source);
	}
}