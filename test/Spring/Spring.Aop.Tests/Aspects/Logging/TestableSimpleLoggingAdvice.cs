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

using AopAlliance.Intercept;

namespace Spring.Aspects.Logging;

/// <summary>
/// This is simple wrapper to expose the protected methood InvokeUnderLog in the class
/// SimpleLoggingAdvice for testing purposes.
/// </summary>
/// <author>Mark Pollack</author>
public class TestableSimpleLoggingAdvice : SimpleLoggingAdvice
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestableSimpleLoggingAdvice"/> class.
    /// </summary>
    /// <param name="useDynamicLogger">if set to <c>true</c> [use dynamic logger].</param>
    public TestableSimpleLoggingAdvice(bool useDynamicLogger) : base(useDynamicLogger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestableSimpleLoggingAdvice"/> class.
    /// </summary>
    public TestableSimpleLoggingAdvice()
    {
    }

    /// <summary>
    /// Calls the protected InvokeUnderLog method
    /// </summary>
    /// <param name="invocation">The invocation.</param>
    /// <param name="log">The log.</param>
    /// <returns>The result of the call to IMethodInvocation.Proceed()</returns>
    public object CallInvokeUnderLog(IMethodInvocation invocation, ILog log)
    {
        return InvokeUnderLog(invocation, log);
    }

    /// <summary>
    /// Calls the IsInterceptorEnabled method.
    /// </summary>
    /// <param name="invocation">The invocation.</param>
    /// <param name="log">The log.</param>
    /// <returns>The result of the protected method IsInterceptorEnabled</returns>
    public bool CallIsInterceptorEnabled(IMethodInvocation invocation, ILog log)
    {
        return IsInterceptorEnabled(invocation, log);
    }
}
