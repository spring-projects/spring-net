/*
 * Copyright 2004 the original author or authors.
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

using System.Reflection;
using NUnit.Framework;
using Spring.Messaging.Nms.Connections;

namespace Spring;

/// <summary>
/// Unit tests for all of the exception classes in the Spring.Data library...
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
//[Ignore("Spring inherits from NMS Exceptions which do not ")]
public sealed class NmsExceptionTests : ExceptionsTest
{
    [OneTimeSetUp]
    public void FixtureSetUp()
    {
        AssemblyToCheck = Assembly.GetAssembly(typeof(SynchedLocalTransactionFailedException));
    }
}
