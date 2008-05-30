#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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

using System;

using AopAlliance.Intercept;

#endregion

namespace Spring.AopQuickStart.Aspects
{
    /// <summary>
    /// Simple implementation of the <see cref="AopAlliance.Intercept.IMethodInterceptor"/> interface 
    /// for a logging aspect using <see cref="System.Console"/>.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: ConsoleLoggingAroundAdvice.cs,v 1.2 2006/12/03 23:56:17 bbaia Exp $</version>
	public class ConsoleLoggingAroundAdvice : IMethodInterceptor
	{
		public object Invoke(IMethodInvocation invocation)
		{
            Console.Out.WriteLine(String.Format(
                "Intercepted call : about to invoke method '{0}'", invocation.Method.Name));

			object returnValue = invocation.Proceed();

			Console.Out.WriteLine(String.Format(
                "Intercepted call : returned '{0}'", returnValue));

			return returnValue;
		}
	}
}