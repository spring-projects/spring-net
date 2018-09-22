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

using FakeItEasy;

using NHibernate;
using NUnit.Framework;

using Spring.Transaction.Support;

using static FakeItEasy.A;

#endregion

namespace Spring.Data.NHibernate.Support
{
    /// <summary>
    /// Tests for SessionScope
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class SessionScopeTests
    {
        private ISessionFactory expectedSessionFactory;
        private IInterceptor expectedEntityInterceptor;
        private bool expectedSingleSession;
        private FlushMode expectedDefaultFlushMode;

        [SetUp]
        public void SetUp()
        {
            expectedSessionFactory = Fake<ISessionFactory>();
            expectedEntityInterceptor = Fake<IInterceptor>();
            expectedSingleSession = SessionScopeSettings.SINGLESESSION_DEFAULT;
            expectedDefaultFlushMode = SessionScopeSettings.FLUSHMODE_DEFAULT;
        }

        [Test]
        public void CanCreateAndClose()
        {
            using (SessionScope scope = new SessionScope(expectedSessionFactory, expectedEntityInterceptor, expectedSingleSession, expectedDefaultFlushMode, false))
            {
                // no op - just create & dispose
                Assert.AreSame(expectedSessionFactory, scope.SessionFactory);
                Assert.AreSame(expectedEntityInterceptor, scope.EntityInterceptor);
                Assert.AreEqual(expectedSingleSession, scope.SingleSession);
                Assert.AreEqual(expectedDefaultFlushMode, scope.DefaultFlushMode);

                // ensure nothing got registered with TSM
                Assert.IsFalse(TransactionSynchronizationManager.HasResource(expectedSessionFactory));

                scope.Close();
            }
        }

        [Test]
        public void CanCreateAndCloseSimpleCtor()
        {
            ISession session = Fake<ISession>();
            CallTo(() => expectedSessionFactory.OpenSession()).Returns(session);

            using (SessionScope scope = new SessionScope(expectedSessionFactory, true))
            {
                // no op - just create & dispose
                Assert.AreSame(expectedSessionFactory, scope.SessionFactory);
                //Assert.AreSame(null, scope.EntityInterceptor);
                Assert.AreEqual(expectedSingleSession, scope.SingleSession);
                Assert.AreEqual(expectedDefaultFlushMode, scope.DefaultFlushMode);

                // ensure SessionHolder object is registered with TSM
                Assert.IsTrue(TransactionSynchronizationManager.HasResource(expectedSessionFactory));

                SessionHolder sessionHolder =
                    (SessionHolder) TransactionSynchronizationManager.GetResource(expectedSessionFactory);
                // by default session is lazy, so ask for it.
                Assert.IsNotNull(sessionHolder.Session);
                scope.Close();
            }

            CallToSet(() => session.FlushMode).WhenArgumentsMatch(x => x.Get<FlushMode>(0) == FlushMode.Never).MustHaveHappenedOnceExactly();
            CallTo(() => session.Close()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void OpeningTwiceThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                using (SessionScope scope = new SessionScope(expectedSessionFactory, true))
                {
                    scope.Open();
                }
            });
        }

        [Test]
        public void ClosingTwiceIsIgnored()
        {
            using (SessionScope scope = new SessionScope(expectedSessionFactory, true))
            {
                scope.Close();
                scope.Close();
            }
        }

        [Test]
        public void DisposeClosesScope()
        {
            SessionScope scope = new SessionScope(expectedSessionFactory, true);
            Assert.IsTrue(scope.IsOpen);
            scope.Dispose();
            Assert.IsFalse(scope.IsOpen);
        }

        [Test]
        public void DoesOpenImmediatelyOnOpenIsTrue()
        {
            SessionScope scope = null;
            using (scope = new SessionScope(expectedSessionFactory, true))
            {
                // ensure is open
                Assert.IsTrue(scope.IsOpen);
                Assert.IsFalse(scope.IsParticipating);

                scope.Close();
                // ensure is closed
                Assert.IsFalse(scope.IsOpen);
                Assert.IsFalse(scope.IsParticipating);
            }
            Assert.IsFalse(scope.IsOpen);
            Assert.IsFalse(scope.IsParticipating);

            using (scope = new SessionScope(expectedSessionFactory, true))
            {
                // ensure is open
                Assert.IsTrue(scope.IsOpen);
                Assert.IsFalse(scope.IsParticipating);
            }
            // ensure dispose closes scope
            Assert.IsFalse(scope.IsOpen);
            Assert.IsFalse(scope.IsParticipating);
        }

        [Test]
        public void DoesNotOpenImmediatelyOnOpenIsFalse()
        {
            SessionScope scope = null;
            using (scope = new SessionScope(expectedSessionFactory, false))
            {
                // ensure is *not* open
                Assert.IsFalse(scope.IsOpen);
                Assert.IsFalse(scope.IsParticipating);

                scope.Open();
                // ensure is open now
                Assert.IsTrue(scope.IsOpen);

                scope.Close();
                // ensure is closed
                Assert.IsFalse(scope.IsOpen);
                Assert.IsFalse(scope.IsParticipating);
            }
            // ensure is closed
            Assert.IsFalse(scope.IsOpen);
            Assert.IsFalse(scope.IsParticipating);
        }

        [Test]
        public void SingleSessionRegistersSessionHolderWithTSM()
        {
            SessionScope scope = null;
            using (scope = new SessionScope(expectedSessionFactory, null, true, FlushMode.Auto, true))
            {
                // ensure is open
                Assert.IsTrue(scope.IsOpen);
                Assert.IsFalse(scope.IsParticipating);

                // ensure registered sessionholder with TSM
                Assert.IsTrue(TransactionSynchronizationManager.HasResource(expectedSessionFactory));
                // ensure a sessionHolder is registered
                SessionHolder sessionHolder = TransactionSynchronizationManager.GetResource(expectedSessionFactory) as SessionHolder;
                Assert.IsNotNull(sessionHolder);
            }
            // ensure scope is closed and sessionHolder is unregistered from TSM
            Assert.IsFalse(scope.IsOpen);
            Assert.IsFalse(TransactionSynchronizationManager.HasResource(expectedSessionFactory));
        }

        [Test]
        public void SingleSessionAppliesDefaultFlushModeOnOpenSessionAndClosesSession()
        {
            ISession expectedSession = Fake<ISession>();

            CallTo(() => expectedSessionFactory.OpenSession()).Returns(expectedSession);

            SessionScope scope = null;
            using (scope = new SessionScope(expectedSessionFactory, null, true, FlushMode.Auto, true))
            {
                SessionHolder sessionHolder = (SessionHolder)TransactionSynchronizationManager.GetResource(expectedSessionFactory);
                Assert.IsTrue(sessionHolder.ContainsSession(expectedSession));
            }
            // ensure scope is closed and sessionHolder is unregistered from TSM
            Assert.IsFalse(scope.IsOpen);
            Assert.IsFalse(TransactionSynchronizationManager.HasResource(expectedSessionFactory));

            CallToSet(() => expectedSession.FlushMode).WhenArgumentsMatch(x => x.Get<FlushMode>(0) == FlushMode.Auto).MustHaveHappenedOnceExactly();
            CallTo(() => expectedSession.Close()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void SingleSessionNestedSessionParticipatesInParentScopeSessionFactory()
        {
            SessionScope scope = null;
            using (scope = new SessionScope(expectedSessionFactory, null, true, FlushMode.Auto, true))
            {
                Assert.IsTrue(scope.IsOpen);
                Assert.IsFalse(scope.IsParticipating);

                using (SessionScope innerScope = new SessionScope(expectedSessionFactory, true))
                {
                    // outer scope didn't change
                    Assert.IsTrue(scope.IsOpen);
                    Assert.IsFalse(scope.IsParticipating);

                    // participating only - no SessionHolder will be registered!
                    Assert.IsTrue(innerScope.IsOpen);
                    Assert.IsTrue(innerScope.IsParticipating);

                    innerScope.Close();

                    Assert.IsFalse(innerScope.IsOpen);
                    Assert.IsFalse(innerScope.IsParticipating);

                    // outer scope didn't change
                    Assert.IsTrue(scope.IsOpen);
                    Assert.IsFalse(scope.IsParticipating);
                    Assert.IsTrue(TransactionSynchronizationManager.HasResource(expectedSessionFactory));
                }
            }

            // ensure scope is closed and sessionHolder is unregistered from TSM
            Assert.IsFalse(scope.IsOpen);
            Assert.IsFalse(TransactionSynchronizationManager.HasResource(expectedSessionFactory));
        }

        public class TestSessionScopeSettings : SessionScopeSettings
        {
            public TestSessionScopeSettings(ISessionFactory sessionFactory)
                : base(sessionFactory)
            {
            }

            protected override IInterceptor ResolveEntityInterceptor()
            {
                return DoResolveEntityInterceptor();
            }

            public virtual IInterceptor DoResolveEntityInterceptor()
            {
                return base.ResolveEntityInterceptor();
            }
        }

        [Test]
        public void ResolvesEntityInterceptorOnEachOpen()
        {
            TestSessionScopeSettings sss = Fake<TestSessionScopeSettings>(options => options
                .CallsBaseMethods()
                .WithArgumentsForConstructor(new[] {expectedSessionFactory})
            );
            ISession expectedSession = Fake<ISession>();
            sss.DefaultFlushMode = FlushMode.Never;

            SessionScope sc = new SessionScope(sss, false);
            CallTo(() => sss.DoResolveEntityInterceptor()).Returns(expectedEntityInterceptor);
            CallTo(() => expectedSessionFactory.OpenSession(expectedEntityInterceptor)).Returns(expectedSession);

            sc.Open();
            SessionHolder sessionHolder = (SessionHolder) TransactionSynchronizationManager.GetResource(expectedSessionFactory);
            sessionHolder.ContainsSession(null); // force opening session
            sc.Close();

            sc.Open();
            sessionHolder = (SessionHolder) TransactionSynchronizationManager.GetResource(expectedSessionFactory);
            sessionHolder.ContainsSession(null); // force opening session
            sc.Close();

            CallToSet(() => expectedSession.FlushMode).WhenArgumentsMatch(x => x.Get<FlushMode>(0) == FlushMode.Never).MustHaveHappenedTwiceExactly();
            CallTo(() => expectedSession.Close()).MustHaveHappenedTwiceExactly();
        }
    }
}