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

using System.Globalization;
using NUnit.Framework;

namespace Spring.Context.Support;

[TestFixture]
public class StaticApplicationContextTests
{
    [Test]
    public void AddMessageTest()
    {
        StaticApplicationContext ctx = new StaticApplicationContext();
        ctx.Refresh();
        ctx.AddMessage("code1", CultureInfo.CurrentUICulture, "this is {0}");
        Assert.That(ctx.GetMessage("code1", "Spring.NET"), Is.EqualTo("this is Spring.NET"));
    }

    [Test]
    public void RegisterObjectPrototype()
    {
        StaticApplicationContext ctx = new StaticApplicationContext();
        ctx.RegisterPrototype("my object", typeof(MockContextAwareObject), null);
        MockContextAwareObject ctx1 = (MockContextAwareObject) ctx.GetObject("my object");
        MockContextAwareObject ctx2 = (MockContextAwareObject) ctx.GetObject("my object");
        Assert.That(ctx1 != ctx2, Is.True);
        Assert.That(!ctx.IsSingleton("my object"), Is.True);
    }
}
