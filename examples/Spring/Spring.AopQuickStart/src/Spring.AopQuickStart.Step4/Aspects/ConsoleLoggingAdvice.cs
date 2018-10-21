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

using System;

using AopAlliance.Intercept;

namespace Spring.AopQuickStart.Aspects
{
    /// <summary>
    /// Simple implementation of the <see cref="AopAlliance.Intercept.IMethodInterceptor"/> interface 
    /// for a logging aspect using <see cref="System.Console"/> and informations from the 
    /// <see cref="Spring.AopQuickStart.Attributes.ConsoleLoggingAttribute" /> attribute.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <version>$Id: ConsoleLoggingAdvice.cs,v 1.2 2006/12/03 23:56:28 bbaia Exp $</version>
	public class ConsoleLoggingAdvice : IMethodInterceptor
	{
#if NET_2_0
        private ConsoleColor _color = ConsoleColor.Gray;
        public ConsoleColor Color
        {
            get { return _color; }
            set { _color = value; }
        }

		public object Invoke(IMethodInvocation invocation)
		{
            ConsoleLoggingAttribute[] consoleLoggingInfo =
                (ConsoleLoggingAttribute[])invocation.Method.GetCustomAttributes(typeof(ConsoleLoggingAttribute), false);

            if (consoleLoggingInfo.Length > 0)
            {
                Color = consoleLoggingInfo[0].Color;
            }

            ConsoleColor currentColor = Console.ForegroundColor;

            Console.ForegroundColor = Color;

            Console.Out.WriteLine(String.Format(
                "Intercepted call : about to invoke method '{0}'", invocation.Method.Name));

            Console.ForegroundColor = currentColor;

            object returnValue = invocation.Proceed();

            Console.ForegroundColor = Color;

            Console.Out.WriteLine(String.Format(
                "Intercepted call : returned '{0}'", returnValue));

            Console.ForegroundColor = currentColor;

			return returnValue;
		}
#else
        public object Invoke(IMethodInvocation invocation)
        {
            Console.Out.WriteLine("Intercepted call : about to invoke next item in chain...");
            object returnValue = invocation.Proceed();
            Console.Out.WriteLine("Intercepted call : returned " + returnValue);
            return returnValue;
        }
#endif
	}
}