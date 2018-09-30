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
	/// Simple marker interface for throws advice.
	/// </summary>
	/// <remarks>
	/// <p>
	/// There are no methods on this interface, as methods are discovered and
    /// invoked via reflection. Please do see read the API documentation for the
    /// <see cref="Spring.Aop.Framework.Adapter.ThrowsAdviceInterceptor"/> class;
    /// said documention describes in detail the signature of the methods that
    /// implementations of the <see cref="Spring.Aop.IThrowsAdvice"/> interface
    /// must adhere to in the specific case of Spring.NET's implementation of
    /// throws advice.
	/// </p>
	/// <p>
	/// There are any number of possible uses for this type of advice. Some
	/// examples would include the ubiquitous logging of any such exceptions,
	/// monitoring the number and type of exceptions and sending emails to
	/// a support desk once certain criteria have been met, wrapping generic
	/// exceptions such as System.Data.SqlClient.SqlException in
	/// exceptions that are more meaningful to your business logic, etc.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Aleksandar Seovic (.NET)</author>
	/// <seealso cref="Spring.Aop.IMethodBeforeAdvice"/>
	/// <seealso cref="Spring.Aop.IAfterReturningAdvice"/>
	/// <seealso cref="AopAlliance.Intercept.IMethodInterceptor"/>
	public interface IThrowsAdvice : IAdvice
	{
	}
}