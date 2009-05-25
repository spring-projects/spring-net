#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using NUnit.Framework;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Support;
using Spring.TestSupport;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Unit tests for the WebUtilsTests class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class WebUtilsTests
    {
        private const string ExpectedPageName = "foo";

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetPageNameWithNullUrl()
        {
            WebUtils.GetPageName(null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetPageNameWithEmptyUrl()
        {
            WebUtils.GetPageName(string.Empty);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void GetPageNameWithWhitespacedUrl()
        {
            WebUtils.GetPageName("   ");
        }

        [Test]
        public void GetPageNameSunnyDay()
        {
            string pageName = WebUtils.GetPageName("foo.aspx");
            Assert.AreEqual(ExpectedPageName, pageName);
            pageName = WebUtils.GetPageName("~/foo.aspx");
            Assert.AreEqual(ExpectedPageName, pageName);
            pageName = WebUtils.GetPageName("~/Bingo/foo.aspx");
            Assert.AreEqual(ExpectedPageName, pageName);
            pageName = WebUtils.GetPageName("/Bingo/foo.aspx");
            Assert.AreEqual(ExpectedPageName, pageName);
            pageName = WebUtils.GetPageName("Bingo/foo.aspx");
            Assert.AreEqual(ExpectedPageName, pageName);
            pageName = WebUtils.GetPageName("~/Bingo/Rope/foo.aspx");
            Assert.AreEqual(ExpectedPageName, pageName);
            pageName = WebUtils.GetPageName("/Bingo/Rope/foo.aspx");
            Assert.AreEqual(ExpectedPageName, pageName);
            pageName = WebUtils.GetPageName("Bingo/Rope/foo.aspx");
            Assert.AreEqual(ExpectedPageName, pageName);
            pageName = WebUtils.GetPageName("http://www.madeleine.org/Bingo/Rope/foo.aspx");
            Assert.AreEqual(ExpectedPageName, pageName);
        }

        [Test]
        public void GetPageNameWherePageHasOddExtension()
        {
            string pageName = WebUtils.GetPageName("foo.odd");
            Assert.AreEqual(ExpectedPageName, pageName);
        }

        /// <summary>
        /// Dont quite know how this would get mapped by IIS, but
        /// WebUtils.GetPageName should return the page as-is in any case.
        /// </summary>
        [Test]
        public void GetPageNameWherePageHasNoExtension()
        {
            string pageName = WebUtils.GetPageName("foo");
            Assert.AreEqual(ExpectedPageName, pageName);
            pageName = WebUtils.GetPageName("http://www.madeleine.org/Bingo/Rope/foo");
            Assert.AreEqual(ExpectedPageName, pageName);
        }

        [Test]
        public void TestCreateAbsolutePath()
        {
            Assert.AreEqual("/", WebUtils.CreateAbsolutePath("", null));
            Assert.AreEqual("/", WebUtils.CreateAbsolutePath("/", null));
            Assert.AreEqual("/MyApp/", WebUtils.CreateAbsolutePath("/MyApp", null));
            Assert.AreEqual("/MyApp/", WebUtils.CreateAbsolutePath("/MyApp", string.Empty));

            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath(null, "/MyPath"));
            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath(null, "~/MyPath"));

            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath(string.Empty, "/MyPath"));
            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath(string.Empty, "~/MyPath"));

            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath("/", "MyPath"));
            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath("/", "~/MyPath"));

            Assert.AreEqual("/MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp", "MyPath"));
            Assert.AreEqual("/MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp", "~/MyPath"));
            Assert.AreEqual("/MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp/", "MyPath"));
            Assert.AreEqual("/MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp/", "~/MyPath"));
            Assert.AreEqual("/MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp", "/MyApp/MyPath"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CombineVirtualPathsDoesntAcceptNullRoot()
        {
            WebUtils.CombineVirtualPaths(null, string.Empty);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CombineVirtualPathsDoesntAcceptEmptyRoot()
        {
            WebUtils.CombineVirtualPaths(string.Empty, string.Empty);
        }
        
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CombineVirtualPathsDoesntAcceptNonRootedRoot()
        {
            WebUtils.CombineVirtualPaths("mypath", string.Empty);
        }
        
        [Test]
        public void CombineVirtualPathsInRootWeb()
        {
            // emulate root website context
            using( new VirtualEnvironmentMock("/somedir/some.file", null, null, "/", true) )
            {
                CombineVirtualPathsSuite( "/" );
            }            
        }
        
        [Test]
        public void CombineVirtualPathsInChildWeb()
        {
            // emulate child website context
            using( new VirtualEnvironmentMock("/somedir/some.file", null, null, "/myapp", true) )
            {
                CombineVirtualPathsSuite( "/myapp/" );
            }            
        }
        
        private void CombineVirtualPathsSuite( string appPath )
        {
            Assert.AreEqual("/myotherdir/mypath", WebUtils.CombineVirtualPaths("/myotherdir", "mypath"));
            Assert.AreEqual("/mypath", WebUtils.CombineVirtualPaths("/irrelevantdir", "/mypath")); // note: that's the difference from CreateAbsolutePath()
            Assert.AreEqual(appPath + "mypath", WebUtils.CombineVirtualPaths("/irrelevantdir", "~/mypath"));            
            Assert.AreEqual(appPath + "some/~/mypath", WebUtils.CombineVirtualPaths("/mydir", "~/some/~/mypath"));            
            Assert.AreEqual("/mydir/some/~/mypath", WebUtils.CombineVirtualPaths("/mydir", "some/~/mypath"));            
            Assert.AreEqual("/myotherdir/mypath/my.file", WebUtils.CombineVirtualPaths("/myotherdir/some.file", "mypath/my.file"));
            Assert.AreEqual(appPath + "mypath/my.file", WebUtils.CombineVirtualPaths("/myotherdir/some.file", "~/mypath/my.file"));
        }

        [Test]
        public void GetRelativePath()
        {
            // case-insensitive
            Assert.AreEqual("/Mypath", WebUtils.GetRelativePath("/mydir/", "/myDir/Mypath"));
            Assert.AreEqual("/mYpath", WebUtils.GetRelativePath("/Mydir", "/mYdir/mYpath"));
            Assert.AreEqual("/myotherdir/mypath", WebUtils.GetRelativePath("/mydir", "/myotherdir/mypath"));
        }
    }
}