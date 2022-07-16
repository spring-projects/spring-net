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

using System.Reflection;
using AopAlliance.Intercept;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// An AOP Alliance <see cref="AopAlliance.Intercept.IMethodInterceptor"/> providing
	/// declarative transaction management using the common Spring.NET transaction infrastructure.
	/// </summary>
	/// <remarks>
	/// <p>
	/// That class contains the necessary calls into Spring.NET's underlying
	/// transaction API: subclasses such as this are responsible for calling
	/// superclass methods such as
    /// <see cref="Spring.Transaction.Interceptor.TransactionAspectSupport.CreateTransactionIfNecessary(MethodInfo, Type) "/>
	/// in the correct order, in the event of normal invocation return or an exception.
	/// </p>
	/// <p>
	/// <see cref="Spring.Transaction.Interceptor.TransactionInterceptor"/>s are thread-safe.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Mark Pollack (.NET)</author>
	[Serializable]
	public class TransactionInterceptor : TransactionAspectSupport, IMethodInterceptor
	{
		/// <summary>
		/// AOP Alliance invoke call that handles all transaction plumbing.
		/// </summary>
		/// <param name="invocation">
		/// The method that is to execute in the context of a transaction.
		/// </param>
		/// <returns>The return value from the method invocation.</returns>
		public object Invoke(IMethodInvocation invocation)
		{
            // Work out the target class: may be <code>null</code>.
            // The TransactionAttributeSource should be passed the target class
            // as well as the method, which may be from an interface.
			Type targetType = ( invocation.This != null ) ? invocation.This.GetType() : null;

            // If the transaction attribute is null, the method is non-transactional.
			TransactionInfo txnInfo = CreateTransactionIfNecessary( invocation.Method, targetType );
			object returnValue = null;

			try
			{
                // This is an around advice.
                // Invoke the next interceptor in the chain.
                // This will normally result in a target object being invoked.
				returnValue = invocation.Proceed();
			}
			catch ( Exception ex )
			{
                // target invocation exception
				CompleteTransactionAfterThrowing( txnInfo, ex );
                throw;
			}
			finally
			{
				CleanupTransactionInfo( txnInfo );
			}
			CommitTransactionAfterReturning( txnInfo );
			return returnValue;
		}
	}
}
