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

using System;
using NUnit.Framework;

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
        public void GetPageNameWithNullUrl()
        {
            Assert.Throws<ArgumentNullException>(() => WebUtils.GetPageName(null));
        }

        [Test]
        public void GetPageNameWithEmptyUrl()
        {
            Assert.Throws<ArgumentNullException>(() => WebUtils.GetPageName(string.Empty));
        }

        [Test]
        public void GetPageNameWithWhitespacedUrl()
        {
            Assert.Throws<ArgumentNullException>(() => WebUtils.GetPageName("   "));
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
        public void CreateAbsolutePath_EmptyApplicationPathAndNullRelativePath_ReturnsRootPath()
        {
            Assert.AreEqual("/", WebUtils.CreateAbsolutePath("", null));
        }

        [Test]
        public void CreateAbsolutePath_RootApplicationPathAndNullRelativePath_ReturnsRootPath()
        {
            Assert.AreEqual("/", WebUtils.CreateAbsolutePath("/", null));
        }

        [Test]
        public void CreateAbsolutePath_NullRelativePath_ReturnsApplicationPath()
        {
            Assert.AreEqual("/MyApp/", WebUtils.CreateAbsolutePath("/MyApp", null));
        }

        [Test]
        public void CreateAbsolutePath_EmptyRelativePath_ReturnsApplicationPath()
        {
            Assert.AreEqual("/MyApp/", WebUtils.CreateAbsolutePath("/MyApp", string.Empty));
        }

        [Test]
        public void CreateAbsolutePath_NullApplicationPath_ReturnsRelativePath()
        {
            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath(null, "/MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_NullApplicationPathAndAppRootRelativePath_ReturnsRelativePath()
        {
            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath(null, "~/MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_EmptyApplicationPath_ReturnsRelativePath()
        {
            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath(string.Empty, "/MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_EmptyApplicationPathAndAppRootRelativePath_ReturnsRelativePath()
        {
            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath(string.Empty, "~/MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_RootApplicationPath_ReturnsRelativePath()
        {
            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath("/", "/MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_RootApplicationPathAndAppRootRelativePath_ReturnsRelativePath()
        {
            Assert.AreEqual("/MyPath", WebUtils.CreateAbsolutePath("/", "~/MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_ApplicationPathAndRelativePath_ReturnsConcatenatedPath()
        {
            Assert.AreEqual("/MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp", "MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_ApplicationPathAndAppRootRelativePath_ReturnsConcatenatedPath()
        {
            Assert.AreEqual("/MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp", "~/MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_ApplicationPathWithTrailingSlashAndRelativePath_ReturnsConcatenatedPath()
        {
            Assert.AreEqual("/MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp/", "MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_ApplicationPathWithTrailingSlashAndAppRootRelativePath_ReturnsConcatenatedPath()
        {
            Assert.AreEqual("/MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp/", "~/MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_WhenRelativePathBeginsWithApplicationPath_ReturnsConcatenatedPath()
        {
            Assert.AreEqual("/MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp", "/MyApp/MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_WhenRelativePathIsHttpQualifiedUrl_ReturnsRelativePath()
        {
            Assert.AreEqual("http://MyApp/MyPath", WebUtils.CreateAbsolutePath(null, "http://MyApp/MyPath"));
            Assert.AreEqual("http://MyApp/MyPath", WebUtils.CreateAbsolutePath("/", "http://MyApp/MyPath"));
            Assert.AreEqual("http://MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp", "http://MyApp/MyPath"));
            Assert.AreEqual("http://MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp/", "http://MyApp/MyPath"));
        }

        [Test]
        public void CreateAbsolutePath_WhenRelativePathIsHttpsQualifiedUrl_ReturnsRelativePath()
        {
            Assert.AreEqual("https://MyApp/MyPath", WebUtils.CreateAbsolutePath(null, "https://MyApp/MyPath"));
            Assert.AreEqual("https://MyApp/MyPath", WebUtils.CreateAbsolutePath("/", "https://MyApp/MyPath"));
            Assert.AreEqual("https://MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp", "https://MyApp/MyPath"));
            Assert.AreEqual("https://MyApp/MyPath", WebUtils.CreateAbsolutePath("/MyApp/", "https://MyApp/MyPath"));
        }

        [Test]
        public void CombineVirtualPathsDoesntAcceptNullRoot()
        {
            Assert.Throws<ArgumentNullException>(() => WebUtils.CombineVirtualPaths(null, string.Empty));
        }

        [Test]
        public void CombineVirtualPathsDoesntAcceptEmptyRoot()
        {
            Assert.Throws<ArgumentNullException>(() => WebUtils.CombineVirtualPaths(string.Empty, string.Empty));
        }
        
        [Test]
        public void CombineVirtualPathsDoesntAcceptNonRootedRoot()
        {
            Assert.Throws<ArgumentException>(() => WebUtils.CombineVirtualPaths("mypath", string.Empty));
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

        [Test]
        public void GetNormalizedVirtualPath()
        {
            Assert.AreEqual(null, WebUtils.GetNormalizedVirtualPath(null));
            Assert.AreEqual(String.Empty, WebUtils.GetNormalizedVirtualPath(String.Empty));
            Assert.AreEqual("~test.aspx", WebUtils.GetNormalizedVirtualPath("~test.aspx"));
            Assert.AreEqual("/test.aspx", WebUtils.GetNormalizedVirtualPath("~/test.aspx"));
            Assert.AreEqual("/Complex.Path/~/test.aspx", WebUtils.GetNormalizedVirtualPath("~/Complex.Path/~/test.aspx"));
        }
    }
}