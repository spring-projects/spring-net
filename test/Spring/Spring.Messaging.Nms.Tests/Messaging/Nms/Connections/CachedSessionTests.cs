#region License

/*
 * Copyright © 2002-2013 the original author or authors.
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

using System.Collections.Generic;
using Apache.NMS;
using NUnit.Framework;
using Spring.Collections;

namespace Spring.Messaging.Nms.Connections
{
    /// <summary>
    /// Test suite for <see cref="CachedSession"/>.
    /// </summary>
    /// <author>Andreas Kluth</author>
    [TestFixture]
    public class CachedSessionTests
    {
        /// <summary>
        /// Validates that events raised by the session cached are propagated to a registered consumer.
        /// </summary>
        [Test]
        public void EventsArePropagated()
        {
            TestSession targetSession = new TestSession();
            CachedSession session = CreateCachedSession(targetSession);

            bool committedWasRaised = false;
            bool rolledBackWasRaised = false;
            bool startedWasRaised = false;

            session.TransactionCommittedListener += _ => committedWasRaised = true;
            session.TransactionRolledBackListener += _ => rolledBackWasRaised = true;
            session.TransactionStartedListener += _ => startedWasRaised = true;

            targetSession.TransactionCommitted();
            targetSession.TransactionRolledBack();
            targetSession.TransactionStarted();

            Assert.IsTrue(committedWasRaised);
            Assert.IsTrue(rolledBackWasRaised);
            Assert.IsTrue(startedWasRaised);
        }

        private CachedSession CreateCachedSession(ISession targetSession)
        {
            return new CachedSession(targetSession, new List<ISession>(), new CachingConnectionFactory());
        }
    }
}
