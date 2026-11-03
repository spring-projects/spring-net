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

using NUnit.Framework;
using Spring.Aop.Framework.DynamicProxy;
using Spring.Aop.Interceptor;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Support;

namespace Spring.Aop.Framework.AutoProxy;

/// <summary>
/// Creating an auto-proxy must not borrow a target from a custom <see cref="ITargetSource"/>
/// (or, if it ever does, must give it back).
/// </summary>
[TestFixture]
public class AutoProxyTargetSourceBorrowTests
{
    public class Service
    {
        public virtual int Answer() => 42;
    }

    private sealed class CountingTargetSourceCreator : ITargetSourceCreator
    {
        public readonly MockTargetSource TargetSource = new MockTargetSource();

        public ITargetSource GetTargetSource(Type objectType, string objectName, IObjectFactory factory)
        {
            TargetSource.SetTarget(new Service());
            return TargetSource;
        }
    }

    private sealed class NopAutoProxyCreator : AbstractAutoProxyCreator
    {
        public NopAutoProxyCreator(IObjectFactory objectFactory)
        {
            ObjectFactory = objectFactory;
        }

        protected override IList<object> GetAdvicesAndAdvisorsForObject(Type targetType, string targetName, ITargetSource customTargetSource)
        {
            return new List<object> { new NopInterceptor() };
        }
    }

    [Test]
    public void CreatingProxyDoesNotBorrowFromTargetSource()
    {
        DefaultListableObjectFactory objectFactory = new DefaultListableObjectFactory();
        objectFactory.RegisterObjectDefinition("service", new RootObjectDefinition(typeof(Service)));

        CountingTargetSourceCreator creator = new CountingTargetSourceCreator();
        NopAutoProxyCreator autoProxyCreator = new NopAutoProxyCreator(objectFactory);
        autoProxyCreator.CustomTargetSourceCreators = new List<object> { creator };
        objectFactory.AddObjectPostProcessor(autoProxyCreator);

        Service proxy = (Service) objectFactory.GetObject("service");

        Assert.That(AopUtils.IsAopProxy(proxy), Is.True);
        Assert.That(creator.TargetSource.gets, Is.EqualTo(0), "creating the proxy borrowed a target");
        Assert.That(creator.TargetSource.releases, Is.EqualTo(creator.TargetSource.gets));

        // and the proxy still borrows and releases per call
        Assert.That(proxy.Answer(), Is.EqualTo(42));
        Assert.That(creator.TargetSource.gets, Is.EqualTo(1));
        Assert.That(creator.TargetSource.releases, Is.EqualTo(creator.TargetSource.gets));
    }
}
