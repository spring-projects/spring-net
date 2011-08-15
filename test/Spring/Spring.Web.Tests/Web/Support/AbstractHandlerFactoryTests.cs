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
using System.Web;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Context;
using Spring.Objects.Factory.Support;

#endregion

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
            MockRepository mocks = new MockRepository();            
            TestHandlerFactory f = (TestHandlerFactory) mocks.PartialMock(typeof(TestHandlerFactory));
            IApplicationContext simpleAppContext = (IApplicationContext) mocks.DynamicMock(typeof(IApplicationContext));
            IConfigurableApplicationContext allowedAppContext = (IConfigurableApplicationContext) mocks.DynamicMock(typeof(IConfigurableApplicationContext));

            using(Record(mocks))
            {
                Expect.Call(f.GetContextStub("/NullContext")).Return(null);
                Expect.Call(f.GetContextStub("/NonConfigurableContext")).Return(simpleAppContext);
                Expect.Call(f.GetContextStub("/AllowedContext")).Return(allowedAppContext);
            }

            // (context == null) -> ArgumentException
            try
            {
                f.GetCheckedApplicationContext("/NullContext");
                Assert.Fail("should throw ArgumentException");
            }
            catch (ArgumentException)
            {}

            // !(context is IConfigurableApplicationContext) -> InvalidOperationException
            try
            {
                f.GetCheckedApplicationContext("/NonConfigurableContext");
                Assert.Fail("should throw InvalidOperationException");
            }
            catch (InvalidOperationException)
            {}

            // (context is IConfigurableApplicationContext) -> OK
            Assert.AreSame(allowedAppContext, f.GetCheckedApplicationContext("/AllowedContext"));
        }

        [Test]
        public void CachesReusableHandlers()
        {
            MockRepository mocks = new MockRepository();            
            TestHandlerFactory f = (TestHandlerFactory) mocks.PartialMock(typeof(TestHandlerFactory));
            IHttpHandler reusableHandler = (IHttpHandler) mocks.DynamicMock(typeof(IHttpHandler));
            IConfigurableApplicationContext appCtx = (IConfigurableApplicationContext) mocks.DynamicMock(typeof(IConfigurableApplicationContext));

            // if (IHttpHandler.IsReusable == true) => always returns the same handler instance 
            // - CreateHandlerInstance() is only called once
            using(Record(mocks))
            {
                Expect.Call(reusableHandler.IsReusable).Return(true);
                Expect.Call(f.GetContextStub("reusable")).Return(appCtx);
                Expect.Call(f.CreateHandlerInstanceStub(appCtx, null, null, "reusable", null)).Return(reusableHandler);
            }
            using (Playback(mocks))
            {
                Assert.AreSame( reusableHandler, f.GetHandler( null, null, "reusable", null ) );
                Assert.AreSame( reusableHandler, f.GetHandler( null, null, "reusable", null ) );
            }
        }

        [Test]
        public void DoesntCacheNonReusableHandlers()
        {
            MockRepository mocks = new MockRepository();            
            TestHandlerFactory f = (TestHandlerFactory) mocks.PartialMock(typeof(TestHandlerFactory));
            IHttpHandler nonReusableHandler = (IHttpHandler) mocks.DynamicMock(typeof(IHttpHandler));
            Expect.Call(nonReusableHandler.IsReusable).Return(false);
            IHttpHandler nonReusableHandler2 = (IHttpHandler) mocks.DynamicMock(typeof(IHttpHandler));
            Expect.Call(nonReusableHandler2.IsReusable).Return(false);
            IConfigurableApplicationContext appCtx = (IConfigurableApplicationContext) mocks.DynamicMock(typeof(IConfigurableApplicationContext));

            // if (IHttpHandler.IsReusable == false) => always create new handler instance 
            // - CreateHandlerInstance() is called for each request
            using(Record(mocks))
            {
                Expect.Call(f.GetContextStub("notreusable")).Return(appCtx);
                Expect.Call(f.CreateHandlerInstanceStub(appCtx, null, null, "notreusable", null)).Return(nonReusableHandler);
                Expect.Call(f.GetContextStub("notreusable")).Return(appCtx);
                Expect.Call(f.CreateHandlerInstanceStub(appCtx, null, null, "notreusable", null)).Return(nonReusableHandler2);
            }
            using (Playback(mocks))
            {
                Assert.AreSame( nonReusableHandler, f.GetHandler( null, null, "notreusable", null ) );
                Assert.AreSame( nonReusableHandler2, f.GetHandler( null, null, "notreusable", null ) );
            }
        }

        #region Rhino.Mocks Compatibility Adapter

        private static IDisposable Record(MockRepository mocks)
        {
            return mocks.Record();
        }

        private static IDisposable Playback(MockRepository mocks)
        {
            return mocks.Playback();
        }


        #endregion Rhino.Mocks Compatibility Adapter
    }
}
