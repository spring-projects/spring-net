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
using System.Collections.Generic;

using AopAlliance.Intercept;
using Spring.Util;

#endregion

/// <summary>
/// A simple interception around advice that filters 
/// a string array matching a given pattern.
/// </summary>
/// <author>Bruno Baia</author>
/// <version>$Id: StringArrayFilterAdvice.cs,v 1.1 2007/05/31 18:34:54 markpollack Exp $</version>
public class StringArrayFilterAdvice : IMethodInterceptor
{
    private string _pattern = string.Empty;

    /// <summary>
    /// A pattern to be used for filtering a string array.
    /// </summary>
    public string Pattern
    {
        get { return _pattern; }
        set { _pattern = value; }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="StringArrayFilterAdvice"/> class.
    /// </summary>
    public StringArrayFilterAdvice()
    {
    }

    #region IMethodInterceptor Members

    public object Invoke(IMethodInvocation invocation)
    {
        if (invocation.Method.ReturnType != typeof(string[]))
            throw new NotSupportedException(
                "StringArrayFilterAdvice must be applied on methods " +
                "that return an array of string.");

        string[] stringArray = (string[])invocation.Proceed();

        if (_pattern.Length > 0)
        {
            List<string> strings = new List<string>();
            foreach (string item in stringArray)
            {
                if (PatternMatchUtils.SimpleMatch(_pattern, item))
                {
                    strings.Add(item);
                }
            }

            return strings.ToArray();
        }

        return stringArray;
    }

    #endregion
}