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
using System.Collections;
using NUnit.Framework;
using Rhino.Mocks;
using Spring.Objects.Factory.Support;
using Spring.Objects.Support;

#endregion

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class SharedStateAwareProcessorTests
    {
        [Test]
        public void DoesNotAllowNullOrEmptyFactoryList()
        {
            // check default ctor init
            SharedStateAwareProcessor ssap = new SharedStateAwareProcessor();
            Assert.IsNotNull( ssap.SharedStateFactories );
            Assert.AreEqual( 0, ssap.SharedStateFactories.Length );

            // check we accept a list at all
            ssap = new SharedStateAwareProcessor( new ISharedStateFactory[] { new ByTypeSharedStateFactory() }, Int32.MaxValue );

            // now ensure that SharedStateFactories will never be null or empty
            ssap = new SharedStateAwareProcessor();
            try
            {
                ssap.SharedStateFactories = null;
                Assert.Fail( "should throw ArgumentException" );
            }
            catch (ArgumentException)
            { }

            try
            {
                ssap.SharedStateFactories = new ISharedStateFactory[0];
                Assert.Fail( "should throw ArgumentException" );
            }
            catch (ArgumentException)
            { }

            try
            {
                ssap.SharedStateFactories = new ISharedStateFactory[] { null };
                Assert.Fail( "should throw ArgumentException" );
            }
            catch (ArgumentException)
            { }

            try
            {
                ssap = new SharedStateAwareProcessor( null, Int32.MaxValue );
                Assert.Fail( "should throw ArgumentException" );
            }
            catch (ArgumentException)
            { }

            try
            {
                ssap = new SharedStateAwareProcessor( new ISharedStateFactory[0], Int32.MaxValue );
                Assert.Fail( "should throw ArgumentException" );
            }
            catch (ArgumentException)
            { }

            try
            {
                ssap = new SharedStateAwareProcessor( new ISharedStateFactory[] { null }, Int32.MaxValue );
                Assert.Fail( "should throw ArgumentException" );
            }
            catch (ArgumentException)
            { }
        }

        [Test]
        public void BeforeInitializationIsNoOp()
        {
            SharedStateAwareProcessor ssap = new SharedStateAwareProcessor();
            object res = ssap.PostProcessBeforeInitialization( this, null );
            Assert.AreSame( this, res );
        }

        [Test]
        public void IgnoresAlreadyPopulatedState()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();

            MockRepository mocks = new MockRepository();
            ISharedStateFactory ssf1 = (ISharedStateFactory)mocks.CreateMock( typeof( ISharedStateFactory ) );
            ISharedStateAware ssa = (ISharedStateAware)mocks.DynamicMock( typeof( ISharedStateAware ) );

            SharedStateAwareProcessor ssap = new SharedStateAwareProcessor();
            ssap.SharedStateFactories = new ISharedStateFactory[] { ssf1 };
            of.RegisterSingleton( "ssap", ssap );

            using (Record( mocks ))
            {
                // preset SharedState - ssap must ignore it
                Expect.Call( ssa.SharedState ).Return( new Hashtable() );
                // expect nothing else!
            }

            using (Playback( mocks ))
            {
                ssap.PostProcessBeforeInitialization( ssa, "myPage" );
            }
        }

        [Test]
        public void ProbesSharedStateFactories()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();

            MockRepository mocks = new MockRepository();
            ISharedStateFactory ssf1 = (ISharedStateFactory)mocks.CreateMock( typeof( ISharedStateFactory ) );
            ISharedStateFactory ssf2 = (ISharedStateFactory)mocks.CreateMock( typeof( ISharedStateFactory ) );
            ISharedStateFactory ssf3 = (ISharedStateFactory)mocks.CreateMock( typeof( ISharedStateFactory ) );
            ISharedStateFactory ssf4 = (ISharedStateFactory)mocks.CreateMock( typeof( ISharedStateFactory ) );
            IDictionary ssf3ProvidedState = new Hashtable();

            SharedStateAwareProcessor ssap = new SharedStateAwareProcessor();
            ssap.SharedStateFactories = new ISharedStateFactory[] { ssf1, ssf2, ssf3, ssf4 };
            of.RegisterSingleton( "ssap", ssap );

            ISharedStateAware ssa = (ISharedStateAware)mocks.DynamicMock( typeof( ISharedStateAware ) );

            // Ensure we iterate over configured SharedStateFactories until 
            // the first provider is found that
            // a) true == provider.CanProvideState( instance, name )
            // b) null != provider.GetSharedState( instance, name )

            using (Record( mocks ))
            {
                Expect.Call( ssa.SharedState ).Return( null );
                Expect.Call( ssf1.CanProvideState( ssa, "pageName" ) ).Return( false );
                Expect.Call( ssf2.CanProvideState( ssa, "pageName" ) ).Return( true );
                Expect.Call( ssf2.GetSharedStateFor( ssa, "pageName" ) ).Return( null );
                Expect.Call( ssf3.CanProvideState( ssa, "pageName" ) ).Return( true );
                Expect.Call( ssf3.GetSharedStateFor( ssa, "pageName" ) ).Return( ssf3ProvidedState );
                Expect.Call( ssa.SharedState = ssf3ProvidedState );
            }

            using (Playback( mocks ))
            {
                ssap.PostProcessBeforeInitialization( ssa, "pageName" );
            }
        }

        #region Rhino.Mocks Compatibility Adapter

        private static IDisposable Record( MockRepository mocks )
        {
#if NET_2_0
            return mocks.Record();
#else
            return new RecordModeChanger(mocks);
#endif
        }

        private static IDisposable Playback( MockRepository mocks )
        {
#if NET_2_0
            return mocks.Playback();
#else
            return new PlaybackModeChanger(mocks);
#endif
        }

#if !NET_2_0
        private class RecordModeChanger : IDisposable
        {
            private MockRepository _mocks;

            public RecordModeChanger(MockRepository mocks)
            {
                _mocks = mocks;
            }

            public void Dispose()
            {
                _mocks.ReplayAll();
            }
        }

        private class PlaybackModeChanger : IDisposable
        {
            private MockRepository _mocks;

            public PlaybackModeChanger(MockRepository mocks)
            {
                _mocks = mocks;
            }

            public void Dispose()
            {
                _mocks.VerifyAll();
            }
        }
#endif

        #endregion Rhino.Mocks Compatibility Adapter
    }
}