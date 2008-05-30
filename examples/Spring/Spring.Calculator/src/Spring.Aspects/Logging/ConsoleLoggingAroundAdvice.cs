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
using System.IO;

using AopAlliance.Intercept;

#endregion

namespace Spring.Aspects.Logging
{
    /// <summary>
    /// Basic implementation of a logging aspect using <see cref="System.Console"/>.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <version>$Id: ConsoleLoggingAroundAdvice.cs,v 1.1 2006/11/26 12:26:34 bbaia Exp $</version>
	public class ConsoleLoggingAroundAdvice : IMethodInterceptor
	{
		#region Fields

#if NET_2_0
        private ConsoleColor _foregroundColor = ConsoleColor.Gray;
#endif

		#endregion

		#region Properties

#if NET_2_0
        /// <summary>
        /// Gets or sets the foreground color of the console.
        /// </summary>
        public ConsoleColor ForegroundColor
		{
            get { return _foregroundColor; }
            set { _foregroundColor = value; }
		}
#else
        /// <summary>
        /// The ConsoleColor is only avalaible since 2.0 only.
        /// </summary>
        /// <remarks>
        /// Avoid dependency injection errors.
        /// </remarks>
        public string ForegroundColor
        {
            get { throw new Exception("The method or operation is not supported."); }
            set { }
        }
#endif

        #endregion

        #region IMethodInterceptor Members

        public object Invoke(IMethodInvocation invocation)
		{
#if NET_2_0
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = _foregroundColor;
#endif
			Console.Out.WriteLine(String.Format("Intercepted call : about to invoke method '{0}'", invocation.Method.Name));
			object returnValue = invocation.Proceed();
            Console.Out.WriteLine(String.Format("Intercepted call : returned '{0}'", returnValue));
#if NET_2_0
            Console.ForegroundColor = previousColor;
#endif
			return returnValue;
		}

		#endregion
	}
}