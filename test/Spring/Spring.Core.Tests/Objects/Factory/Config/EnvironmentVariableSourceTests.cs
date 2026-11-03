/*
 * Copyright © 2002-2011 the original author or authors.
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

namespace Spring.Objects.Factory.Config;

/// <summary>
/// Unit tests for the EnvironmentVariableSource class.
/// </summary>
/// <author>Aleksandar Seovic</author>
[TestFixture]
public sealed class EnvironmentVariableSourceTests
{
    [Test]
    public void TestVariablesResolution()
    {
        EnvironmentVariableSource vs = new EnvironmentVariableSource();

        // existing vars
        Assert.That(vs.ResolveVariable("path"), Is.EqualTo(Environment.GetEnvironmentVariable("path")));
        Assert.That(vs.ResolveVariable("PATH"), Is.EqualTo(Environment.GetEnvironmentVariable("PATH")));
        Assert.That(vs.ResolveVariable("computerName"), Is.EqualTo(Environment.GetEnvironmentVariable("ComputerName")));

        // non-existant variable
        Assert.That(vs.ResolveVariable("dummy"), Is.Null);
    }
}
