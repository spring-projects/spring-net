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

using AopAlliance.Aop;
using AopAlliance.Intercept;
using Spring.Aop.Support;

#endregion

namespace Spring.Aop.Framework.Adapter
{
	/// <summary>
	/// Default implementation of the
	/// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapterRegistry"/>
	/// interface.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	[Serializable]
	public class DefaultAdvisorAdapterRegistry : IAdvisorAdapterRegistry
	{
        private readonly IList<IAdvisorAdapter> adapters = new List<IAdvisorAdapter>();

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.Aop.Framework.Adapter.DefaultAdvisorAdapterRegistry"/> class.
        /// </summary>
        /// <remarks>
        /// <p>
        /// This constructor will also register the well-known
        /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/> types.
        /// </p>
        /// </remarks>
        /// <seealso cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/>
        public DefaultAdvisorAdapterRegistry()
        {
            // register well-known adapters...
            RegisterAdvisorAdapter(new BeforeAdviceAdapter());
            RegisterAdvisorAdapter(new AfterReturningAdviceAdapter());
            RegisterAdvisorAdapter(new ThrowsAdviceAdapter());
        }

        /// <summary>
        /// Returns an <see cref="Spring.Aop.IAdvisor"/> wrapping the supplied
        /// <paramref name="advice"/>.
        /// </summary>
        /// <param name="advice">
        /// The object that should be an advice, such as
        /// <see cref="Spring.Aop.IBeforeAdvice"/> or
        /// <see cref="Spring.Aop.IThrowsAdvice"/>.
        /// </param>
        /// <returns>
        /// An <see cref="Spring.Aop.IAdvisor"/> wrapping the supplied
        /// <paramref name="advice"/>. Never returns <cref lang="null"/>. If
        /// the <paramref name="advice"/> parameter is an
        /// <see cref="Spring.Aop.IAdvisor"/>, it will simply be returned.
        /// </returns>
        /// <exception cref="UnknownAdviceTypeException">
        /// If no registered
        /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/> can wrap
        /// the supplied <paramref name="advice"/>.
        /// </exception>
	    public virtual IAdvisor Wrap(object advice)
	    {
	        if (advice is IAdvisor)
	        {
	            return (IAdvisor) advice;
	        }
	        if (!(advice is IAdvice))
	        {
	            throw new UnknownAdviceTypeException(advice);
	        }
	        IAdvice adviceObject = (IAdvice) advice;
	        if (adviceObject is IInterceptor)
	        {
	            // so well-known it doesn't even need an adapter...
	            return new DefaultPointcutAdvisor(adviceObject);
	        }
            foreach (IAdvisorAdapter adapter in this.adapters)
            {
                // check that it is supported...
                if (adapter.SupportsAdvice(adviceObject))
                {
                    return new DefaultPointcutAdvisor(adviceObject);
                }
            }
	        throw new UnknownAdviceTypeException(advice);
	    }

        /// <summary>
        /// Returns an <see cref="AopAlliance.Intercept.IInterceptor"/> to
        /// allow the use of the supplied <paramref name="advisor"/> in an
        /// interception-based framework.
        /// </summary>
        /// <param name="advisor">The advisor to find an interceptor for.</param>
        /// <returns>
        /// An interceptor to expose this advisor's behaviour.
        /// </returns>
        /// <exception cref="UnknownAdviceTypeException">
        /// If the advisor type is not understood by any registered
        /// <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/>.
        /// </exception>
	    public virtual IInterceptor GetInterceptor(IAdvisor advisor)
	    {
	        IAdvice advice = advisor.Advice;
	        if (advice is IInterceptor)
	        {
	            return (IInterceptor) advice;
	        }
            foreach (IAdvisorAdapter adapter in this.adapters)
            {
                if (adapter.SupportsAdvice(advice))
                {
                    return adapter.GetInterceptor(advisor);
                }
            }
	        throw new UnknownAdviceTypeException(advice);
	    }

        /// <summary>
        /// Register the given <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/>.
        /// </summary>
        /// <param name="adapter">
        /// An <see cref="Spring.Aop.Framework.Adapter.IAdvisorAdapter"/> that
        /// understands the particular advisor and advice types.
        /// </param>
	    public virtual void RegisterAdvisorAdapter(IAdvisorAdapter adapter)
	    {
	        this.adapters.Add(adapter);
	    }
	}
}
