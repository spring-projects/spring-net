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

using System.Collections;
using System.Web;
using FakeItEasy;
using NUnit.Framework;
using Spring.Context;

namespace Spring.Web.Support;

/// <summary>
/// Tests for <see cref="MappingHandlerFactory"/>.
/// </summary>
[TestFixture]
public class MappingHandlerFactoryTests
{
    private const string Url = "/reused.test";
    private const string PhysicalPath = "c:\\reused.test";

    /// <summary>
    /// Routes the mapping through a private <see cref="HandlerMap"/> and issued-handler table,
    /// so the tests don't leave entries in the global <see cref="MappingHandlerFactory.HandlerMap"/>.
    /// </summary>
    private sealed class TestMappingHandlerFactory : MappingHandlerFactory
    {
        private readonly IConfigurableApplicationContext _appContext;
        private readonly HandlerMap _handlerMap = new HandlerMap();
        private readonly Hashtable _issuedHandlers = new Hashtable();

        public TestMappingHandlerFactory(IConfigurableApplicationContext appContext, string urlPattern, string handlerObjectName)
        {
            _appContext = appContext;
            _handlerMap.Add(urlPattern, handlerObjectName);
        }

        public int IssuedHandlerCount => _issuedHandlers.Count;

        protected override IApplicationContext GetContext(string virtualPath)
        {
            return _appContext;
        }

        protected override IHttpHandler CreateHandlerInstance(IConfigurableApplicationContext appContext, HttpContext context, string requestType, string rawUrl, string physicalPath)
        {
            return MapHandlerInstance(appContext, context, requestType, rawUrl, physicalPath, _handlerMap, _issuedHandlers);
        }

        public override void ReleaseHandler(IHttpHandler handler)
        {
            ReleaseHandler(handler, _issuedHandlers);
        }
    }

    private static TestMappingHandlerFactory CreateFactory(IHttpHandler handler, out IHttpHandlerFactory mappedFactory)
    {
        IHttpHandlerFactory issuingFactory = A.Fake<IHttpHandlerFactory>();
        A.CallTo(() => issuingFactory.GetHandler(null, "GET", Url, PhysicalPath)).Returns(handler);
        IConfigurableApplicationContext appContext = A.Fake<IConfigurableApplicationContext>();
        A.CallTo(() => appContext.GetObject("mappedFactory")).Returns(issuingFactory);
        mappedFactory = issuingFactory;
        return new TestMappingHandlerFactory(appContext, "/reused\\.test$", "mappedFactory");
    }

    [Test]
    public void MappedFactoryHandingOutTheSameHandlerTwiceDoesNotThrow()
    {
        IHttpHandler handler = A.Fake<IHttpHandler>();
        TestMappingHandlerFactory factory = CreateFactory(handler, out _);

        IHttpHandler first = factory.GetHandler(null, "GET", Url, PhysicalPath);

        IHttpHandler second = null;
        Assert.That(() => second = factory.GetHandler(null, "GET", Url, PhysicalPath), Throws.Nothing);

        Assert.That(first, Is.SameAs(handler));
        Assert.That(second, Is.SameAs(handler));
    }

    [Test]
    public void ReleaseHandlerReleasesTheHandlerAtTheIssuingFactory()
    {
        IHttpHandler handler = A.Fake<IHttpHandler>();
        TestMappingHandlerFactory factory = CreateFactory(handler, out IHttpHandlerFactory mappedFactory);

        factory.GetHandler(null, "GET", Url, PhysicalPath);
        factory.GetHandler(null, "GET", Url, PhysicalPath);

        factory.ReleaseHandler(handler);

        A.CallTo(() => mappedFactory.ReleaseHandler(handler)).MustHaveHappenedOnceExactly();
        Assert.That(factory.IssuedHandlerCount, Is.EqualTo(0));
    }
}
