using NUnit.Framework;

namespace Spring.Transaction;

[TestFixture]
public class InvalidTimeoutExceptionTests
{
    [Test]
    public void TimeoutGetter()
    {
        InvalidTimeoutException ex = new InvalidTimeoutException("bad timeout", 2000);
        Assert.That(2000 == ex.Timeout, Is.True);
    }
}
