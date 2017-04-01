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

using System;
using System.Web;

using FakeItEasy;

using NUnit.Framework;

using Spring.Context;
using Spring.Objects.Factory.Support;

namespace Spring.Web.Support
{
    /// <summary>
    ///
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class AbstractHandlerFactoryTests
    {
        #region TestFindWebObjectDefinition Helper

        private class TestFindWebObjectDefinitionHandlerFactory : AbstractHandlerFactory
        {
            private class Type1 { }

            public void TestFindWebObjectDefinition()
            {
                NamedObjectDefinition nod;

                nod = Find( "/path/o1.ext", "/path/o1.ext" );
                Assert.AreEqual( typeof( Type1 ), nod.ObjectDefinition.ObjectType );
                Assert.AreEqual( "/path/o1.ext", nod.Name );

                nod = Find( "/path/o1.ext", "/o1.ext" );
                Assert.IsNull( nod );

                nod = Find( "/path/o1.ext", "/path/o1" );
                Assert.IsNull( nod );

                nod = Find( "/path/o1.ext", "o1.ext" );
                Assert.AreEqual( typeof( Type1 ), nod.ObjectDefinition.ObjectType );
                Assert.AreEqual( "o1.ext", nod.Name );

                nod = Find( "/path/o1.ext", "o1" );
                Assert.AreEqual( typeof( Type1 ), nod.ObjectDefinition.ObjectType );
                Assert.AreEqual( "o1", nod.Name );
            }

            private static NamedObjectDefinition Find( string url, string objectName )
            {
                DefaultListableObjectFactory of = new DefaultListableObjectFactory();
                RootObjectDefinition rod = new RootObjectDefinition( typeof( Type1 ) );
                of.RegisterObjectDefinition( objectName, rod );

                return FindWebObjectDefinition( url, of );
            }

            #region AbstractHandlerFactory implementations

            protected override IHttpHandler CreateHandlerInstance( IConfigurableApplicationContext appContext, HttpContext context, string requestType, string url, string physicalPath )
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion TestFindWebObjectDefinition Helper

        [Test]
        public void FindWebObjectDefinition()
        {
            TestFindWebObjectDefinitionHandlerFactory f = new TestFindWebObjectDefinitionHandlerFactory();
            f.TestFindWebObjectDefinition();
        }

        #region TestHandlerFactory

        public abstract class TestHandlerFactory : AbstractHandlerFactory
        {
            public new IConfigurableApplicationContext GetCheckedApplicationContext(string virtualPath)
            {
                return base.GetCheckedApplicationContext(virtualPath);
            }

            protected override IHttpHandler CreateHandlerInstance(IConfigurableApplicationContext appContext, HttpContext context, string requestType, string url, string physicalPath )
            {
                return CreateHandlerInstanceStub(appContext, context, requestType, url, physicalPath);
            }

            public abstract IHttpHandler CreateHandlerInstanceStub(IConfigurableApplicationContext appContext, HttpContext context, string requestType, string url, string physicalPath);

            protected override IApplicationContext GetContext( string virtualPath )
            {
                return GetContextStub( virtualPath );
            }

            public abstract IApplicationContext GetContextStub( string virtualPath );
        }


        #endregion

        [Test]
        public void GetCheckedApplicationContextThrowsExceptionsOnNonConfigurableContexts()
        {
            TestHandlerFactory f = A.Fake<TestHandlerFactory>(options => options.CallsBaseMethods());
            IApplicationContext simpleAppContext = A.Fake<IApplicationContext>();
            IConfigurableApplicationContext allowedAppContext = A.Fake<IConfigurableApplicationContext>();

            A.CallTo(() => f.GetContextStub("/NullContext")).Returns(null);
            A.CallTo(() => f.GetContextStub("/NonConfigurableContext")).Returns(simpleAppContext);
            A.CallTo(() => f.GetContextStub("/AllowedContext")).Returns(allowedAppContext);

            // (context == null) -> ArgumentException
            try
            {
                f.GetCheckedApplicationContext("/NullContext");
                Assert.Fail("should throw ArgumentException");
            }
            catch (ArgumentException)
            {
            }

            // !(context is IConfigurableApplicationContext) -> InvalidOperationException
            try
            {
                f.GetCheckedApplicationContext("/NonConfigurableContext");
                Assert.Fail("should throw InvalidOperationException");
            }
            catch (InvalidOperationException)
            {
            }

            // (context is IConfigurableApplicationContext) -> OK
            Assert.AreSame(allowedAppContext, f.GetCheckedApplicationContext("/AllowedContext"));
        }

        [Test]
        public void CachesReusableHandlers()
        {
            TestHandlerFactory f = A.Fake<TestHandlerFactory>(options => options.CallsBaseMethods());
            IHttpHandler reusableHandler = A.Fake<IHttpHandler>();
            IConfigurableApplicationContext appCtx = A.Fake<IConfigurableApplicationContext>();

            // if (IHttpHandler.IsReusable == true) => always returns the same handler instance
            // - CreateHandlerInstance() is only called once
            A.CallTo(() => reusableHandler.IsReusable).Returns(true);
            A.CallTo(() => f.GetContextStub("reusable")).Returns(appCtx);
            A.CallTo(() => f.CreateHandlerInstanceStub(appCtx, null, null, "reusable", null)).Returns(reusableHandler);

            Assert.AreSame(reusableHandler, f.GetHandler(null, null, "reusable", null));
            Assert.AreSame(reusableHandler, f.GetHandler(null, null, "reusable", null));
        }

        [Test]
        public void DoesntCacheNonReusableHandlers()
        {
            TestHandlerFactory f = A.Fake<TestHandlerFactory>(options => options.CallsBaseMethods());
            IHttpHandler nonReusableHandler = A.Fake<IHttpHandler>();
            A.CallTo(() => nonReusableHandler.IsReusable).Returns(false);
            IHttpHandler nonReusableHandler2 = A.Fake<IHttpHandler>();
            A.CallTo(() => nonReusableHandler2.IsReusable).Returns(false);
            IConfigurableApplicationContext appCtx = A.Fake<IConfigurableApplicationContext>();

            // if (IHttpHandler.IsReusable == false) => always create new handler instance
            // - CreateHandlerInstance() is called for each request

            A.CallTo(() => f.GetContextStub("notreusable")).Returns(appCtx);
            A.CallTo(() => f.CreateHandlerInstanceStub(appCtx, null, null, "notreusable", null))
                .Returns(nonReusableHandler).Once()
                .Then.Returns(nonReusableHandler2).Once();

            Assert.AreSame(nonReusableHandler, f.GetHandler(null, null, "notreusable", null));
            Assert.AreSame(nonReusableHandler2, f.GetHandler(null, null, "notreusable", null));
        }
    }
}
