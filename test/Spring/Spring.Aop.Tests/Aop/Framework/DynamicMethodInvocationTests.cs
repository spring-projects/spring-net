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

using System.Reflection;
using System.Collections;
using NUnit.Framework;

#endregion

namespace Spring.Aop.Framework;

/// <summary>
/// Unit tests for the DynamicMethodInvocation class.
/// </summary>
/// <author>Bruno Baia</author>
[TestFixture]
public class DynamicMethodInvocationTests : AbstractMethodInvocationTests
{
    protected override AbstractMethodInvocation CreateMethodInvocation(object proxy, object target, MethodInfo method, MethodInfo onProxyMethod, object[] arguments, Type targetType, IList interceptors)
    {
        return new DynamicMethodInvocation(proxy, target, method, onProxyMethod, arguments, targetType, interceptors);
    }
}
