#region License

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

#endregion

#region Imports

using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using NUnit.Framework;
using Spring.TestSupport;

#endregion

namespace Spring.Web.UI.Controls
{
    /// <summary>
    /// Tests behaviour of the &lt;Head&gt; server control
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class HeadTests
    {
        private const string CRLF = "\r\n";

        [Test]
        public void DontRenderHeadTagIfNestedWithinStandardHeadControl()
        {
            using (new TestWebContext("/", "testpage.aspx"))
            {
                TestPage page = new TestPage(HttpContext.Current);
                HtmlHead htmlHead = new HtmlHead();
                Head head = new Head();
                head.Controls.Add(new LiteralControl("literal child"));
                htmlHead.Controls.Add(head);
                page.Controls.Add(htmlHead);

                // initialize page to force head control initialization
                page.InitRecursive(null);
                string result = page.Render(string.Empty);
                string expect = @"<head>literal child<title></title></head>";
                Assert.AreEqual(expect, result);
            }
        }

        [Test]
        public void StyleBlockRendersTypeAttribute()
        {
            TestPage page = new TestPage();
            Head head = new Head();
            page.Controls.Add(head);

            page.Styles.Add("stylename", "stylevalue");

            string result = page.Render(string.Empty);
            string expect = @"<head><style type=""text/css"">stylename { stylevalue }</style></head>";
            Assert.AreEqual(expect, result);
        }

        [Test]
        public void StyleFileRendersTypeAttributeAndUsesCssRootPath()
        {
            const string STYLESHEETPATH = "filepath";

            using (new TestWebContext("/", "testpage.aspx"))
            {
                TestPage page = new TestPage(HttpContext.Current);
                Head head = new Head();
                page.Controls.Add(head);

                page.CssRoot = "testcssroot";
                page.StyleFiles.Add("filename", STYLESHEETPATH);

                string result = page.Render(string.Empty);
                string expect = string.Format(@"<head><link rel=""stylesheet"" type=""text/css"" href=""/testcssroot/{0}"" /></head>", STYLESHEETPATH);
                Assert.AreEqual(expect, result);
            }
        }

        [Test]
        public void ScriptBlockRendersTypeAttribute()
        {
            TestPage page = new TestPage();
            Head head = new Head();
            page.Controls.Add(head);

            page.RegisterHeadScriptBlock( "scriptname", "scriptcode" );

            string result = page.Render(string.Empty);
            string expect = @"<head><script type=""text/javascript"">scriptcode</script></head>";
            Assert.AreEqual(expect, result);
        }

        [Test]
        public void ScriptEventRendersTypeAttribute()
        {
            TestPage page = new TestPage();
            Head head = new Head();
            page.Controls.Add(head);

            page.RegisterHeadScriptEvent("scriptname", "elementname", "eventname", "scriptcode");

            string result = page.Render(string.Empty);
            string expect = @"<head><script type=""text/javascript"" for=""elementname"" event=""eventname"">scriptcode</script></head>";
            Assert.AreEqual(expect, result);
        }

        [Test]
        public void ScriptFileRendersTypeAttributeAndUsesScriptsRootPath()
        {
            const string SCRIPTFILEPATH = "filepath";

            using (new TestWebContext("/", "testpage.aspx"))
            {
                TestPage page = new TestPage(HttpContext.Current);
                Head head = new Head();
                page.Controls.Add(head);

                page.ScriptsRoot = "testscriptroot";
                page.RegisterHeadScriptFile("filename", SCRIPTFILEPATH);

                string result = page.Render(string.Empty);
                string expect = string.Format(@"<head><script type=""text/javascript"" src=""/testscriptroot/{0}""></script></head>", SCRIPTFILEPATH);
                Assert.AreEqual(expect, result);
            }
        }

        [Test]
        public void RenderChildrenFirst()
        {
            TestPage page = new TestPage();
            Head head = new Head();
            page.Controls.Add(head);
            
            head.Controls.Add(new LiteralControl("literal child"));
            page.Styles.Add("stylename", "stylevalue");

            string result = page.Render(string.Empty);
            string expect = @"<head>literal child<style type=""text/css"">stylename { stylevalue }</style></head>";
            Assert.AreEqual( expect, result);
        }
    }
}