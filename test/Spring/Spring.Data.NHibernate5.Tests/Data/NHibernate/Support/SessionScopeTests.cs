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

using FakeItEasy;
using NHibernate;
using NUnit.Framework;
using Spring.Transaction.Support;
using static FakeItEasy.A;

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

        [TearDown]
        public void TearDown()
        {
            expectedSessionFactory?.Dispose();
        }

        [Test]
        public void CanCreateAndClose()
        {
            using (SessionScope scope = new SessionScope(expectedSessionFactory, expectedEntityInterceptor, expectedSingleSession, expectedDefaultFlushMode, false))
            {
                // no op - just create & dispose
                Assert.That(scope.SessionFactory, Is.SameAs(expectedSessionFactory));
                Assert.That(scope.EntityInterceptor, Is.SameAs(expectedEntityInterceptor));
                Assert.That(scope.SingleSession, Is.EqualTo(expectedSingleSession));
                Assert.That(scope.DefaultFlushMode, Is.EqualTo(expectedDefaultFlushMode));

                // ensure nothing got registered with TSM
                Assert.That(TransactionSynchronizationManager.HasResource(expectedSessionFactory), Is.False);

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
                Assert.That(scope.SessionFactory, Is.SameAs(expectedSessionFactory));
                //Assert.That(scope.EntityInterceptor, Is.SameAs(null));
                Assert.That(scope.SingleSession, Is.EqualTo(expectedSingleSession));
                Assert.That(scope.DefaultFlushMode, Is.EqualTo(expectedDefaultFlushMode));

                // ensure SessionHolder object is registered with TSM
                Assert.That(TransactionSynchronizationManager.HasResource(expectedSessionFactory), Is.True);

                SessionHolder sessionHolder =
                    (SessionHolder) TransactionSynchronizationManager.GetResource(expectedSessionFactory);
                // by default session is lazy, so ask for it.
                Assert.That(sessionHolder.Session, Is.Not.Null);
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
            Assert.That(scope.IsOpen, Is.True);
            scope.Dispose();
            Assert.That(scope.IsOpen, Is.False);
        }

        [Test]
        public void DoesOpenImmediatelyOnOpenIsTrue()
        {
            SessionScope scope = null;
            using (scope = new SessionScope(expectedSessionFactory, true))
            {
                // ensure is open
                Assert.That(scope.IsOpen, Is.True);
                Assert.That(scope.IsParticipating, Is.False);

                scope.Close();
                // ensure is closed
                Assert.That(scope.IsOpen, Is.False);
                Assert.That(scope.IsParticipating, Is.False);
            }

            Assert.That(scope.IsOpen, Is.False);
            Assert.That(scope.IsParticipating, Is.False);

            using (scope = new SessionScope(expectedSessionFactory, true))
            {
                // ensure is open
                Assert.That(scope.IsOpen, Is.True);
                Assert.That(scope.IsParticipating, Is.False);
            }

            // ensure dispose closes scope
            Assert.That(scope.IsOpen, Is.False);
            Assert.That(scope.IsParticipating, Is.False);
        }

        [Test]
        public void DoesNotOpenImmediatelyOnOpenIsFalse()
        {
            SessionScope scope = null;
            using (scope = new SessionScope(expectedSessionFactory, false))
            {
                // ensure is *not* open
                Assert.That(scope.IsOpen, Is.False);
                Assert.That(scope.IsParticipating, Is.False);

                scope.Open();
                // ensure is open now
                Assert.That(scope.IsOpen, Is.True);

                scope.Close();
                // ensure is closed
                Assert.That(scope.IsOpen, Is.False);
                Assert.That(scope.IsParticipating, Is.False);
            }

            // ensure is closed
            Assert.That(scope.IsOpen, Is.False);
            Assert.That(scope.IsParticipating, Is.False);
        }

        [Test]
        public void SingleSessionRegistersSessionHolderWithTSM()
        {
            SessionScope scope = null;
            using (scope = new SessionScope(expectedSessionFactory, null, true, FlushMode.Auto, true))
            {
                // ensure is open
                Assert.That(scope.IsOpen, Is.True);
                Assert.That(scope.IsParticipating, Is.False);

                // ensure registered sessionholder with TSM
                Assert.That(TransactionSynchronizationManager.HasResource(expectedSessionFactory), Is.True);
                // ensure a sessionHolder is registered
                SessionHolder sessionHolder = TransactionSynchronizationManager.GetResource(expectedSessionFactory) as SessionHolder;
                Assert.That(sessionHolder, Is.Not.Null);
            }

            // ensure scope is closed and sessionHolder is unregistered from TSM
            Assert.That(scope.IsOpen, Is.False);
            Assert.That(TransactionSynchronizationManager.HasResource(expectedSessionFactory), Is.False);
        }

        [Test]
        public void SingleSessionAppliesDefaultFlushModeOnOpenSessionAndClosesSession()
        {
            ISession expectedSession = Fake<ISession>();

            CallTo(() => expectedSessionFactory.OpenSession()).Returns(expectedSession);

            SessionScope scope = null;
            using (scope = new SessionScope(expectedSessionFactory, null, true, FlushMode.Auto, true))
            {
                SessionHolder sessionHolder = (SessionHolder) TransactionSynchronizationManager.GetResource(expectedSessionFactory);
                Assert.That(sessionHolder.ContainsSession(expectedSession), Is.True);
            }

            // ensure scope is closed and sessionHolder is unregistered from TSM
            Assert.That(scope.IsOpen, Is.False);
            Assert.That(TransactionSynchronizationManager.HasResource(expectedSessionFactory), Is.False);

            CallToSet(() => expectedSession.FlushMode).WhenArgumentsMatch(x => x.Get<FlushMode>(0) == FlushMode.Auto).MustHaveHappenedOnceExactly();
            CallTo(() => expectedSession.Close()).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void SingleSessionNestedSessionParticipatesInParentScopeSessionFactory()
        {
            SessionScope scope = null;
            using (scope = new SessionScope(expectedSessionFactory, null, true, FlushMode.Auto, true))
            {
                Assert.That(scope.IsOpen, Is.True);
                Assert.That(scope.IsParticipating, Is.False);

                using (SessionScope innerScope = new SessionScope(expectedSessionFactory, true))
                {
                    // outer scope didn't change
                    Assert.That(scope.IsOpen, Is.True);
                    Assert.That(scope.IsParticipating, Is.False);

                    // participating only - no SessionHolder will be registered!
                    Assert.That(innerScope.IsOpen, Is.True);
                    Assert.That(innerScope.IsParticipating, Is.True);

                    innerScope.Close();

                    Assert.That(innerScope.IsOpen, Is.False);
                    Assert.That(innerScope.IsParticipating, Is.False);

                    // outer scope didn't change
                    Assert.That(scope.IsOpen, Is.True);
                    Assert.That(scope.IsParticipating, Is.False);
                    Assert.That(TransactionSynchronizationManager.HasResource(expectedSessionFactory), Is.True);
                }
            }

            // ensure scope is closed and sessionHolder is unregistered from TSM
            Assert.That(scope.IsOpen, Is.False);
            Assert.That(TransactionSynchronizationManager.HasResource(expectedSessionFactory), Is.False);
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
                .WithArgumentsForConstructor(new[] { expectedSessionFactory })
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
