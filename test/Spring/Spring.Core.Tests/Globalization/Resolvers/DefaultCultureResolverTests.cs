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

using System.Globalization;
using NUnit.Framework;

namespace Spring.Globalization.Resolvers;

/// <summary>
/// Unit tests for the DefaultCultureResolver class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class DefaultCultureResolverTests
{
    [Test]
    public void ResolveCultureYieldsThreadsCultureAfterInitialization()
    {
        DefaultCultureResolver resolver = new DefaultCultureResolver();
        Assert.AreEqual(Thread.CurrentThread.CurrentUICulture, resolver.ResolveCulture(),
            "Not defaulting to the culture of the current thread.");
    }

    [Test]
    public void DefaultCultureIsNullAfterInitialization()
    {
        DefaultCultureResolver resolver = new DefaultCultureResolver();
        Assert.IsNull(resolver.DefaultCulture, "Must be null until explicitly set.");
    }

    [Test]
    public void DefaultCultureIsYieldedForResolveCulture()
    {
        DefaultCultureResolver resolver = new DefaultCultureResolver();
        resolver.DefaultCulture = CultureInfo.InvariantCulture;
        Assert.AreEqual(CultureInfo.InvariantCulture, resolver.ResolveCulture(),
            "Not returning the DefaultCulture (it must if the DefaultCulture " +
            "property has been set explicitly).");
    }

    [Test]
    public void NullingDefaultCultureYieldsThreadsCultureForResolveCulture()
    {
        DefaultCultureResolver resolver = new DefaultCultureResolver();
        resolver.DefaultCulture = CultureInfo.InvariantCulture;
        Assert.AreEqual(CultureInfo.InvariantCulture, resolver.ResolveCulture(),
            "Not returning the DefaultCulture (it must if the DefaultCulture " +
            "property has been set explicitly).");
        resolver.DefaultCulture = null;
        Assert.AreEqual(Thread.CurrentThread.CurrentUICulture, resolver.ResolveCulture(),
            "Not falling back to the culture of the current thread after " +
            "DefaultCulture property is nulled out (it must).");
    }
}
