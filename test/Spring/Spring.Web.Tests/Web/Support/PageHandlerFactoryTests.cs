#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using System.Web;
using System.IO;
using NUnit.Extensions.Asp.AspTester;
using NUnit.Framework;
using NUnitAspEx;
using Spring.TestSupport;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Erich Eichinger</author>
    [AspTestFixture("/Test", "/Spring/Web/Support/PageHandlerFactoryTests")]
    public class PageHandlerFactoryTests : WebFormTestCase
    {
        [Test]
        public void DisablesSessions()
        {
            AspTestClient client = new AspTestClient();
            // a session-less page - checks, if session is correctly disabled
            string result = client.GetPage("DisablesSession.aspx");
            Assert.AreEqual("OK", result);
        }

        [Test]
        public void MaintainsSession()
        {
            AspTestClient client = new AspTestClient();
            string result = client.GetPage("MaintainsSession1.aspx");
            Assert.AreEqual("OK", result);
            Assert.AreEqual("somevalue", AspTestContext.HttpContext.Session["maintainsSession"]);

            // checks previously set session variable
            result = client.GetPage("MaintainsSession2.aspx");
            Assert.AreEqual("OK", result);
        }

        [Test]
        public void BCLPageHandlerFactoryBehavior()
        {
            using (TestWebContext ctx = new TestWebContext("/Test", "DoesNotExist.oaspx"))
            {
                try
                {
                    IHttpHandlerFactory phf = (IHttpHandlerFactory)Activator.CreateInstance(typeof(System.Web.UI.Page).Assembly.GetType("System.Web.UI.PageHandlerFactory"), true);
                    phf.GetHandler(HttpContext.Current, "GET", ctx.HttpWorkerRequest.GetFilePath(), ctx.HttpWorkerRequest.GetFilePathTranslated());
                }
#if NET_2_0
                catch (HttpException e)
                {
                    Assert.AreEqual(404, e.GetHttpCode());
                    Assert.IsTrue(e.Message.IndexOf(ctx.HttpWorkerRequest.GetFilePath()) > 0);
                }
#else
                catch (FileNotFoundException)
                {
                }
#endif
            }
        }

        [Test]
        public void PageHandlerFactoryBehavesLikeSystemPageHandlerFactory()
        {
            using (TestWebContext ctx = new TestWebContext("/Test", "DoesNotExist.aspx"))
            {
                try
                {
                    IHttpHandlerFactory phf = new PageHandlerFactory();
                    phf.GetHandler(HttpContext.Current, "GET", ctx.HttpWorkerRequest.GetFilePath(), ctx.HttpWorkerRequest.GetFilePathTranslated());
                }
#if NET_2_0
                catch (HttpException e)
                {
                    Assert.AreEqual(404, e.GetHttpCode());
                    Assert.IsTrue(e.Message.IndexOf(ctx.HttpWorkerRequest.GetFilePath()) > 0);
                }
#else
                catch (FileNotFoundException)
                {
                }
#endif
            }
        }

#if NET_2_0
        [Test]
        public void TransferAfterSetResult()
        {
            TextBoxTester name = new TextBoxTester("name", CurrentWebForm);
            ButtonTester save = new ButtonTester("save", CurrentWebForm);

            Browser.GetPage("asptest://localhost/TransferAfterSetResult.aspx");
            // Note, that page TransferAfterSetResultSave.aspx has 'EnableViewStateMac="false"'
            // otherwise ViewState validation will fail on a Server.Transfer during a Postback!
            save.Click();
            string result = Browser.CurrentPageText;
            Assert.AreEqual("OK", result);
        }
#endif
    }
}