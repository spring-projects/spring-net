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

using AopAlliance.Aop;
using AopAlliance.Intercept;

using Spring.Aop.Framework.Adapter;
#endregion

namespace Spring.Aop
{
	/// <summary>
	/// 
	/// </summary>
	/// <author>Dmitriy Kopylenko</author>
	/// <author>Simon White (.NET)</author>
	[Serializable]
	public class SimpleBeforeAdviceAdapter : IAdvisorAdapter
	{
		#region IAdvisorAdapter Members
		public bool SupportsAdvice(IAdvice advice)
		{
			return advice is ISimpleBeforeAdvice;
		}

		public IInterceptor GetInterceptor(IAdvisor advisor)
		{
			ISimpleBeforeAdvice advice = (ISimpleBeforeAdvice) advisor.Advice;
			return new SimpleBeforeAdviceInterceptor(advice) ;
		}
		#endregion
	}
}
