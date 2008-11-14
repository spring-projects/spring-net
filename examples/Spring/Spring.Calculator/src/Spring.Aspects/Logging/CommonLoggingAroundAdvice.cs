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
using Common.Logging;

#endregion

namespace Spring.Aspects.Logging
{
    /// <summary>
    /// Basic implementation of a logging aspect using Common.Logging library.
    /// </summary>
    /// <author>Bruno Baia</author>
	public class CommonLoggingAroundAdvice : IMethodInterceptor
	{
		#region Logging

        private static readonly ILog LOG = LogManager.GetLogger(typeof(CommonLoggingAroundAdvice));

		#endregion

		#region Fields

        private LogLevel _level = LogLevel.All;

		#endregion

		#region Properties

        public LogLevel Level
		{
			get { return _level; }	
			set { _level = value; }
		}

		#endregion

		#region IMethodInterceptor Members

		public object Invoke(IMethodInvocation invocation)
		{
			Log("Intercepted call : about to invoke method '{0}'", invocation.Method.Name);
			object returnValue = invocation.Proceed();
			Log("Intercepted call : returned '{0}'", returnValue);
			return returnValue;
		}

		#endregion

		#region Private Methods

		private void Log(string text, params object[] args)
		{
			switch(Level)
			{
                case LogLevel.All :
				case LogLevel.Debug :
					if (LOG.IsDebugEnabled) LOG.Debug(String.Format(text, args));
					break;
				case LogLevel.Error :
                    if (LOG.IsErrorEnabled) LOG.Error(String.Format(text, args));
					break;
				case LogLevel.Fatal :
                    if (LOG.IsFatalEnabled) LOG.Fatal(String.Format(text, args));
					break;
				case LogLevel.Info :
                    if (LOG.IsInfoEnabled) LOG.Info(String.Format(text, args));
					break;
				case LogLevel.Warn :
                    if (LOG.IsWarnEnabled) LOG.Warn(String.Format(text, args));
					break;
                case LogLevel.Off:
				default :
                    break;
			}
		}

		#endregion
	}
}