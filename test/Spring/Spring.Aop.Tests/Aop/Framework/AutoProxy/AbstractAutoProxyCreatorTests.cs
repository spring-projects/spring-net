#region License

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

#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using NUnit.Framework;
using Spring.Aop.Interceptor;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class AbstractAutoProxyCreatorTests
    {
        public interface ITestObject
        {
            void Foo();
        }

        public class ObjectWithInterface : ITestObject
        {
            void ITestObject.Foo()
            {}
        }

        public class ObjectWithoutInterface
        {
            public virtual void Foo()
            { }
        }

        public class TransparentProxyFactory : IFactoryObject
        {
            public static RemotingProxy GetRealProxy(object transparentProxy)
            {
                RemotingProxy rp = (RemotingProxy)RemotingServices.GetRealProxy(transparentProxy);
                return rp;
            }

            public class RemotingProxy : RealProxy, IRemotingTypeInfo
            {
                public int Count;

                public RemotingProxy() : base(typeof(MarshalByRefObject))
                {}

                public override IMessage Invoke(IMessage msg)
                {
                    Count++;
                    return new ReturnMessage(null, null, 0, null, msg as IMethodCallMessage);
                }

                public bool CanCastTo(Type fromType, object o)
                {
                    return true;
                }

                public string TypeName
                {
                    get { throw new System.NotImplementedException(); }
                    set { throw new System.NotImplementedException(); }
                }
            }

            private readonly Type objectType;
            private readonly RealProxy realProxy;

            public TransparentProxyFactory(Type objectType)
            {
                this.objectType = objectType;
                this.realProxy = new RemotingProxy();
            }

            public object GetObject()
            {
                return realProxy.GetTransparentProxy();
            }

            public Type ObjectType
            {
                get { return objectType; }
            }

            public bool IsSingleton
            {
                get { return true; }
            }
        }

        public class TestAutoProxyCreator : AbstractAutoProxyCreator
        {
            public readonly NopInterceptor NopInterceptor = new NopInterceptor();

            public TestAutoProxyCreator(IObjectFactory objectFactory)
            {
                this.ObjectFactory = objectFactory;
            }

            protected override IList<object> GetAdvicesAndAdvisorsForObject(Type targetType, string targetName, ITargetSource customTargetSource)
            {
                if (typeof(IFactoryObject).IsAssignableFrom(targetType))
                {
                    return DO_NOT_PROXY;
                }
                return new List<object> { NopInterceptor };
            }
        }

        [Test]
        public void ProxyObjectWithInterface()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            of.RegisterObjectDefinition("bar", new RootObjectDefinition(typeof(ObjectWithInterface)));

            TestAutoProxyCreator apc = new TestAutoProxyCreator(of);
            of.AddObjectPostProcessor(apc);

            ITestObject o = of.GetObject("bar") as ITestObject;
            Assert.IsTrue(AopUtils.IsAopProxy(o));
            o.Foo();
            Assert.AreEqual(1, apc.NopInterceptor.Count);
        }

        [Test]
        public void ProxyObjectWithoutInterface()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();
            of.RegisterObjectDefinition("bar", new RootObjectDefinition(typeof(ObjectWithoutInterface)));

            TestAutoProxyCreator apc = new TestAutoProxyCreator(of);
            of.AddObjectPostProcessor(apc);

            ObjectWithoutInterface o = of.GetObject("bar") as ObjectWithoutInterface;
            Assert.IsTrue(AopUtils.IsAopProxy(o));
            o.Foo();
            Assert.AreEqual(1, apc.NopInterceptor.Count);
        }

        [Test]
        [Platform("Win")]
        public void ProxyTransparentProxy()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();

            ConstructorArgumentValues ctorArgs = new ConstructorArgumentValues();
            ctorArgs.AddNamedArgumentValue("objectType", typeof(ITestObject));
            of.RegisterObjectDefinition("bar", new RootObjectDefinition(typeof(TransparentProxyFactory), ctorArgs, null));

            TestAutoProxyCreator apc = new TestAutoProxyCreator(of);
            of.AddObjectPostProcessor(apc);

            ITestObject o = of.GetObject("bar") as ITestObject;
            Assert.IsTrue(AopUtils.IsAopProxy(o));

            // ensure interceptors get called
            o.Foo();
            Assert.AreEqual(1, apc.NopInterceptor.Count);
            IAdvised advised = (IAdvised) o;

            // ensure target was called
            object target = advised.TargetSource.GetTarget();
            Assert.AreEqual(1, TransparentProxyFactory.GetRealProxy(target).Count);
        }
    }
}