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

namespace Spring.Data.NHibernate.Support
{
    /// <summary>
    /// Tests <see cref="SessionScopeSettings"/> functionality.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class SessionScopeSettingsTests
    {
        private class DerivedSessionScopeSettings : SessionScopeSettings
        {
            public DerivedSessionScopeSettings()
                : base() // note, that we're calling default ctor here
            {
            }
        }

        private class LazyResolvingSessionScopeSettings : SessionScopeSettings
        {
            private readonly ISessionFactory sessionFactory;
            private readonly IInterceptor entityInterceptor;

            public LazyResolvingSessionScopeSettings(ISessionFactory sessionFactory, IInterceptor entityInterceptor)
                : base() // note, that we're calling default ctor here
            {
                this.sessionFactory = sessionFactory;
                this.entityInterceptor = entityInterceptor;
            }

            protected override ISessionFactory ResolveSessionFactory()
            {
                // simulates lazy resolving
                return this.sessionFactory;
            }

            protected override IInterceptor ResolveEntityInterceptor()
            {
                // simulates lazy resolving
                return this.entityInterceptor;
            }
        }

        [Test]
        public void CheckDefaults()
        {
            Assert.That(SessionScopeSettings.SINGLESESSION_DEFAULT, Is.True);
            Assert.That(SessionScopeSettings.FLUSHMODE_DEFAULT, Is.EqualTo(FlushMode.Never));
        }

        [Test]
        public void WorksAsExpected()
        {
            ISessionFactory sessionFactory = A.Fake<ISessionFactory>();
            IInterceptor entityInterceptor = A.Fake<IInterceptor>();
            Assert.That(SessionScopeSettings.FLUSHMODE_DEFAULT, Is.Not.EqualTo(FlushMode.Auto)); // ensure noone changed our assumptions
            SessionScopeSettings sss = new SessionScopeSettings(sessionFactory, entityInterceptor, !SessionScopeSettings.SINGLESESSION_DEFAULT, FlushMode.Auto);

            Assert.That(sss.SessionFactory, Is.EqualTo(sessionFactory));
            Assert.That(sss.EntityInterceptor, Is.EqualTo(entityInterceptor));
            Assert.That(sss.SingleSession, Is.EqualTo(!SessionScopeSettings.SINGLESESSION_DEFAULT));
            Assert.That(sss.DefaultFlushMode, Is.EqualTo(FlushMode.Auto));
        }

        [Test]
        public void CallingDefaultConstructorLeavesReferencesMarkedUninitialized()
        {
            DerivedSessionScopeSettings sss = new DerivedSessionScopeSettings();
            // config values are set to their defaults
            Assert.That(sss.SingleSession, Is.EqualTo(SessionScopeSettings.SINGLESESSION_DEFAULT));
            Assert.That(sss.DefaultFlushMode, Is.EqualTo(SessionScopeSettings.FLUSHMODE_DEFAULT));

            IInterceptor interceptor = sss.EntityInterceptor;
            Assert.That(interceptor, Is.Null);

            try
            {
                ISessionFactory sessionFactory = sss.SessionFactory;
                Assert.Fail("should fail, because derived classes must override ResolveSessionFactory()");
            }
            catch (NotSupportedException)
            {
            }
        }

        [Test]
        public void CallingDefaultConstructorCausesLazyResolvingReferences()
        {
            ISessionFactory expectedSessionFactory = A.Fake<ISessionFactory>();
            IInterceptor expectedEntityInterceptor = A.Fake<IInterceptor>();

            SessionScopeSettings sss = new LazyResolvingSessionScopeSettings(expectedSessionFactory, expectedEntityInterceptor);

            // config values are set to their defaults
            Assert.That(sss.SingleSession, Is.EqualTo(SessionScopeSettings.SINGLESESSION_DEFAULT));
            Assert.That(sss.DefaultFlushMode, Is.EqualTo(SessionScopeSettings.FLUSHMODE_DEFAULT));

            Assert.That(sss.SessionFactory, Is.SameAs(expectedSessionFactory));
            Assert.That(sss.EntityInterceptor, Is.SameAs(expectedEntityInterceptor));
        }

        [Test]
        public void MissingSessionFactoryCausesArgumentExceptionDuringLazyResolving()
        {
            SessionScopeSettings sss = new LazyResolvingSessionScopeSettings(null, null);

            // config values are set to their defaults
            Assert.That(sss.SingleSession, Is.EqualTo(SessionScopeSettings.SINGLESESSION_DEFAULT));
            Assert.That(sss.DefaultFlushMode, Is.EqualTo(SessionScopeSettings.FLUSHMODE_DEFAULT));

            ISessionFactory temp;
            Assert.Throws<ArgumentException>(() => temp = sss.SessionFactory);
        }

        [Test]
        public void MissingEntityInterceptorIsOkDuringLazyResolving()
        {
            ISessionFactory expectedSessionFactory = A.Fake<ISessionFactory>();

            SessionScopeSettings sss = new LazyResolvingSessionScopeSettings(expectedSessionFactory, null);

            // config values are set to their defaults
            Assert.That(sss.SingleSession, Is.EqualTo(SessionScopeSettings.SINGLESESSION_DEFAULT));
            Assert.That(sss.DefaultFlushMode, Is.EqualTo(SessionScopeSettings.FLUSHMODE_DEFAULT));

            Assert.That(sss.SessionFactory, Is.SameAs(expectedSessionFactory));
            Assert.That(sss.EntityInterceptor, Is.Null);
        }
    }
}
