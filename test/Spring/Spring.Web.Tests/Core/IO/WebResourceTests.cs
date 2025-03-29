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

using NUnit.Framework;
using Spring.TestSupport;

namespace Spring.Core.IO;

/// <summary>
/// Unit tests for the WebResource class.
/// </summary>
/// <author>Erich Eichinger</author>
[TestFixture]
public class WebResourceTests : FileSystemResourceCommonTests
{
    private VirtualEnvironmentMock testVirtualEnvironment;

    [OneTimeSetUp]
    public void SetUpFixture()
    {
        testVirtualEnvironment = new VirtualEnvironmentMock("/some.request", "somepathinfo", null, "/", true);
    }

    [OneTimeTearDown]
    public void ShutDownFixture()
    {
        testVirtualEnvironment.Dispose();
    }

    protected override FileSystemResource CreateResourceInstance(string resourceName)
    {
        return new WebResource(resourceName);
    }
}
