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

namespace Spring.Util;

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
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
        pageName = WebUtils.GetPageName("~/foo.aspx");
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
        pageName = WebUtils.GetPageName("~/Bingo/foo.aspx");
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
        pageName = WebUtils.GetPageName("/Bingo/foo.aspx");
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
        pageName = WebUtils.GetPageName("Bingo/foo.aspx");
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
        pageName = WebUtils.GetPageName("~/Bingo/Rope/foo.aspx");
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
        pageName = WebUtils.GetPageName("/Bingo/Rope/foo.aspx");
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
        pageName = WebUtils.GetPageName("Bingo/Rope/foo.aspx");
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
        pageName = WebUtils.GetPageName("http://www.madeleine.org/Bingo/Rope/foo.aspx");
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
    }

    [Test]
    public void GetPageNameWherePageHasOddExtension()
    {
        string pageName = WebUtils.GetPageName("foo.odd");
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
    }

    /// <summary>
    /// Dont quite know how this would get mapped by IIS, but
    /// WebUtils.GetPageName should return the page as-is in any case.
    /// </summary>
    [Test]
    public void GetPageNameWherePageHasNoExtension()
    {
        string pageName = WebUtils.GetPageName("foo");
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
        pageName = WebUtils.GetPageName("http://www.madeleine.org/Bingo/Rope/foo");
        Assert.That(pageName, Is.EqualTo(ExpectedPageName));
    }

    [Test]
    public void CreateAbsolutePath_EmptyApplicationPathAndNullRelativePath_ReturnsRootPath()
    {
        Assert.That(WebUtils.CreateAbsolutePath("", null), Is.EqualTo("/"));
    }

    [Test]
    public void CreateAbsolutePath_RootApplicationPathAndNullRelativePath_ReturnsRootPath()
    {
        Assert.That(WebUtils.CreateAbsolutePath("/", null), Is.EqualTo("/"));
    }

    [Test]
    public void CreateAbsolutePath_NullRelativePath_ReturnsApplicationPath()
    {
        Assert.That(WebUtils.CreateAbsolutePath("/MyApp", null), Is.EqualTo("/MyApp/"));
    }

    [Test]
    public void CreateAbsolutePath_EmptyRelativePath_ReturnsApplicationPath()
    {
        Assert.That(WebUtils.CreateAbsolutePath("/MyApp", string.Empty), Is.EqualTo("/MyApp/"));
    }

    [Test]
    public void CreateAbsolutePath_NullApplicationPath_ReturnsRelativePath()
    {
        Assert.That(WebUtils.CreateAbsolutePath(null, "/MyPath"), Is.EqualTo("/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_NullApplicationPathAndAppRootRelativePath_ReturnsRelativePath()
    {
        Assert.That(WebUtils.CreateAbsolutePath(null, "~/MyPath"), Is.EqualTo("/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_EmptyApplicationPath_ReturnsRelativePath()
    {
        Assert.That(WebUtils.CreateAbsolutePath(string.Empty, "/MyPath"), Is.EqualTo("/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_EmptyApplicationPathAndAppRootRelativePath_ReturnsRelativePath()
    {
        Assert.That(WebUtils.CreateAbsolutePath(string.Empty, "~/MyPath"), Is.EqualTo("/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_RootApplicationPath_ReturnsRelativePath()
    {
        Assert.That(WebUtils.CreateAbsolutePath("/", "/MyPath"), Is.EqualTo("/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_RootApplicationPathAndAppRootRelativePath_ReturnsRelativePath()
    {
        Assert.That(WebUtils.CreateAbsolutePath("/", "~/MyPath"), Is.EqualTo("/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_ApplicationPathAndRelativePath_ReturnsConcatenatedPath()
    {
        Assert.That(WebUtils.CreateAbsolutePath("/MyApp", "MyPath"), Is.EqualTo("/MyApp/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_ApplicationPathAndAppRootRelativePath_ReturnsConcatenatedPath()
    {
        Assert.That(WebUtils.CreateAbsolutePath("/MyApp", "~/MyPath"), Is.EqualTo("/MyApp/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_ApplicationPathWithTrailingSlashAndRelativePath_ReturnsConcatenatedPath()
    {
        Assert.That(WebUtils.CreateAbsolutePath("/MyApp/", "MyPath"), Is.EqualTo("/MyApp/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_ApplicationPathWithTrailingSlashAndAppRootRelativePath_ReturnsConcatenatedPath()
    {
        Assert.That(WebUtils.CreateAbsolutePath("/MyApp/", "~/MyPath"), Is.EqualTo("/MyApp/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_WhenRelativePathBeginsWithApplicationPath_ReturnsConcatenatedPath()
    {
        Assert.That(WebUtils.CreateAbsolutePath("/MyApp", "/MyApp/MyPath"), Is.EqualTo("/MyApp/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_WhenRelativePathIsHttpQualifiedUrl_ReturnsRelativePath()
    {
        Assert.That(WebUtils.CreateAbsolutePath(null, "http://MyApp/MyPath"), Is.EqualTo("http://MyApp/MyPath"));
        Assert.That(WebUtils.CreateAbsolutePath("/", "http://MyApp/MyPath"), Is.EqualTo("http://MyApp/MyPath"));
        Assert.That(WebUtils.CreateAbsolutePath("/MyApp", "http://MyApp/MyPath"), Is.EqualTo("http://MyApp/MyPath"));
        Assert.That(WebUtils.CreateAbsolutePath("/MyApp/", "http://MyApp/MyPath"), Is.EqualTo("http://MyApp/MyPath"));
    }

    [Test]
    public void CreateAbsolutePath_WhenRelativePathIsHttpsQualifiedUrl_ReturnsRelativePath()
    {
        Assert.That(WebUtils.CreateAbsolutePath(null, "https://MyApp/MyPath"), Is.EqualTo("https://MyApp/MyPath"));
        Assert.That(WebUtils.CreateAbsolutePath("/", "https://MyApp/MyPath"), Is.EqualTo("https://MyApp/MyPath"));
        Assert.That(WebUtils.CreateAbsolutePath("/MyApp", "https://MyApp/MyPath"), Is.EqualTo("https://MyApp/MyPath"));
        Assert.That(WebUtils.CreateAbsolutePath("/MyApp/", "https://MyApp/MyPath"), Is.EqualTo("https://MyApp/MyPath"));
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
        using (new VirtualEnvironmentMock("/somedir/some.file", null, null, "/", true))
        {
            CombineVirtualPathsSuite("/");
        }
    }

    [Test]
    public void CombineVirtualPathsInChildWeb()
    {
        // emulate child website context
        using (new VirtualEnvironmentMock("/somedir/some.file", null, null, "/myapp", true))
        {
            CombineVirtualPathsSuite("/myapp/");
        }
    }

    private void CombineVirtualPathsSuite(string appPath)
    {
        Assert.That(WebUtils.CombineVirtualPaths("/myotherdir", "mypath"), Is.EqualTo("/myotherdir/mypath"));
        Assert.That(WebUtils.CombineVirtualPaths("/irrelevantdir", "/mypath"), Is.EqualTo("/mypath")); // note: that's the difference from CreateAbsolutePath()
        Assert.That(WebUtils.CombineVirtualPaths("/irrelevantdir", "~/mypath"), Is.EqualTo(appPath + "mypath"));
        Assert.That(WebUtils.CombineVirtualPaths("/mydir", "~/some/~/mypath"), Is.EqualTo(appPath + "some/~/mypath"));
        Assert.That(WebUtils.CombineVirtualPaths("/mydir", "some/~/mypath"), Is.EqualTo("/mydir/some/~/mypath"));
        Assert.That(WebUtils.CombineVirtualPaths("/myotherdir/some.file", "mypath/my.file"), Is.EqualTo("/myotherdir/mypath/my.file"));
        Assert.That(WebUtils.CombineVirtualPaths("/myotherdir/some.file", "~/mypath/my.file"), Is.EqualTo(appPath + "mypath/my.file"));
    }

    [Test]
    public void GetRelativePath()
    {
        // case-insensitive
        Assert.That(WebUtils.GetRelativePath("/mydir/", "/myDir/Mypath"), Is.EqualTo("/Mypath"));
        Assert.That(WebUtils.GetRelativePath("/Mydir", "/mYdir/mYpath"), Is.EqualTo("/mYpath"));
        Assert.That(WebUtils.GetRelativePath("/mydir", "/myotherdir/mypath"), Is.EqualTo("/myotherdir/mypath"));
    }

    [Test]
    public void GetNormalizedVirtualPath()
    {
        Assert.That(WebUtils.GetNormalizedVirtualPath(null), Is.EqualTo(null));
        Assert.That(WebUtils.GetNormalizedVirtualPath(String.Empty), Is.Empty);
        Assert.That(WebUtils.GetNormalizedVirtualPath("~test.aspx"), Is.EqualTo("~test.aspx"));
        Assert.That(WebUtils.GetNormalizedVirtualPath("~/test.aspx"), Is.EqualTo("/test.aspx"));
        Assert.That(WebUtils.GetNormalizedVirtualPath("~/Complex.Path/~/test.aspx"), Is.EqualTo("/Complex.Path/~/test.aspx"));
    }
}
