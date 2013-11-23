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
            return new CachedSession(targetSession, new LinkedList(), new CachingConnectionFactory());
        }
    }
}
