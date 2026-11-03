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

using System.Web;
using NUnit.Framework;
using NUnitAspEx;
using NUnitAspEx.Client;
using Spring.TestSupport;

namespace Spring.Web.Support;

/// <summary>
///
/// </summary>
/// <author>Erich Eichinger</author>
[TestFixture]
public class PageHandlerFactoryTests : WebFormTestCase
{
    public PageHandlerFactoryTests()
        : base("/Test", "/Spring/Web/Support/PageHandlerFactoryTests")
    {
    }

    [Test]
    public void DisablesSessions()
    {
        HttpWebClient client = Host.CreateWebClient();
        // a session-less page - checks, if session is correctly disabled
        string result = client.GetPage("DisablesSession.aspx");
        Assert.That(result, Is.EqualTo("OK"));
    }

    [Test, Explicit]
    public void UsesReadonlySession()
    {
        HttpWebClient client = Host.CreateWebClient();
        string result = client.GetPage("ReadOnlySession.aspx");
        Assert.That(result, Is.EqualTo("OK"));
    }

    [Test]
    public void MaintainsSession()
    {
        HttpWebClient client = Host.CreateWebClient();
        string result = client.GetPage("MaintainsSession1.aspx");
        Assert.That(result, Is.EqualTo("OK"));
//            Assert.That(AspTestContext.HttpContext.Session["maintainsSession"], Is.EqualTo("somevalue"));

        // checks previously set session variable
        result = client.GetPage("MaintainsSession2.aspx");
        Assert.That(result, Is.EqualTo("OK"));
    }

    /// <summary>
    /// Tests the behavior of the System.Web.PageHandlerFactory class
    /// </summary>
    [Test]
    public void BCLPageHandlerFactoryBehavior()
    {
        Host.Execute(new TestAction(BCLPageHandlerFactoryBehaviorImpl));
    }

    public static void BCLPageHandlerFactoryBehaviorImpl()
    {
        using (TestWebContext ctx = new TestWebContext("/Test", "DoesNotExist.oaspx"))
        {
            try
            {
                IHttpHandlerFactory phf = (IHttpHandlerFactory) Activator.CreateInstance(typeof(System.Web.UI.Page).Assembly.GetType("System.Web.UI.PageHandlerFactory"), true);
                phf.GetHandler(HttpContext.Current, "GET", ctx.HttpWorkerRequest.GetFilePath(), ctx.HttpWorkerRequest.GetFilePathTranslated());
            }
            catch (HttpException e)
            {
                Assert.That(e.GetHttpCode(), Is.EqualTo(404));
                Assert.That(e.Message.IndexOf(ctx.HttpWorkerRequest.GetFilePath()) > 0, Is.True);
            }
        }
    }
}

[TestFixture]
public class PageHandlerFactoryStandaloneTests
{
    [Test]
    public void PageUsesReadonlySessionState()
    {
    }
}
