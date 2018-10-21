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

using Spring.Aop;

namespace Spring.AopQuickStart.Aspects
{
    /// <summary>
    /// Simple implementation of the <see cref="Spring.Aop.IThrowsAdvice"/> interface 
    /// for a logging aspect using <see cref="System.Console"/>.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: ConsoleLoggingThrowsAdvice.cs,v 1.2 2006/12/02 13:30:00 bbaia Exp $</version>
    public class ConsoleLoggingThrowsAdvice : IThrowsAdvice
    {
        public void AfterThrowing(Exception ex)
        {
            Console.Error.WriteLine($"Advised method threw this exception : {ex.Message}");
        }
    }
}