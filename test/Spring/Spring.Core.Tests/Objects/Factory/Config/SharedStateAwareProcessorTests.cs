#region License

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

#endregion

#region Imports

using System;
using System.Collections;

using FakeItEasy;
using NUnit.Framework;

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

            ISharedStateFactory ssf1 = A.Fake<ISharedStateFactory>();
            ISharedStateAware ssa = A.Fake<ISharedStateAware>();

            SharedStateAwareProcessor ssap = new SharedStateAwareProcessor();
            ssap.SharedStateFactories = new ISharedStateFactory[] {ssf1};
            of.RegisterSingleton("ssap", ssap);

            // preset SharedState - ssap must ignore it
            A.CallTo(() => ssa.SharedState).Returns(new Hashtable());

            ssap.PostProcessBeforeInitialization(ssa, "myPage");

            A.CallTo(ssa).Where(x => x.Method.Name == "set_SharedState").MustNotHaveHappened();
        }

        [Test]
        public void ProbesSharedStateFactories()
        {
            DefaultListableObjectFactory of = new DefaultListableObjectFactory();

            ISharedStateFactory ssf1 = A.Fake<ISharedStateFactory>();
            ISharedStateFactory ssf2 = A.Fake<ISharedStateFactory>();
            ISharedStateFactory ssf3 = A.Fake<ISharedStateFactory>();
            ISharedStateFactory ssf4 = A.Fake<ISharedStateFactory>();
            IDictionary ssf3ProvidedState = new Hashtable();

            SharedStateAwareProcessor ssap = new SharedStateAwareProcessor();
            ssap.SharedStateFactories = new ISharedStateFactory[] {ssf1, ssf2, ssf3, ssf4};
            of.RegisterSingleton("ssap", ssap);

            ISharedStateAware ssa = A.Fake<ISharedStateAware>();

            // Ensure we iterate over configured SharedStateFactories until 
            // the first provider is found that
            // a) true == provider.CanProvideState( instance, name )
            // b) null != provider.GetSharedState( instance, name )

            A.CallTo(() => ssa.SharedState).Returns(null).Once();
            A.CallTo(() => ssf1.CanProvideState(ssa, "pageName")).Returns(false).Once();
            A.CallTo(() => ssf2.CanProvideState(ssa, "pageName")).Returns(true).Once();
            A.CallTo(() => ssf2.GetSharedStateFor(ssa, "pageName")).Returns(null);
            A.CallTo(() => ssf3.CanProvideState(ssa, "pageName")).Returns(true).Once();
            A.CallTo(() => ssf3.GetSharedStateFor(ssa, "pageName")).Returns(ssf3ProvidedState).Once();

            ssap.PostProcessBeforeInitialization(ssa, "pageName");

            Assert.That(Equals(ssa.SharedState, ssf3ProvidedState));
        }
    }
}