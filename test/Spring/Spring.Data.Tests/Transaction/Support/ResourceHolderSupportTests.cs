using NUnit.Framework;

namespace Spring.Transaction.Support;

[TestFixture]
public class ResourceHolderSupportTests : ResourceHolderSupport
{
    [TearDown]
    public void Destroy()
    {
        Clear();
    }

    [Test]
    public void PropertiesTest()
    {
        Assert.That(Deadline, Is.EqualTo(DateTime.MinValue));
        Assert.That(!SynchronizedWithTransaction, Is.True);
        Assert.That(!RollbackOnly, Is.True);

        //TODO investigate
        /*
        Deadline = DateTime.Now.AddDays(2);
        Assert.That(HasDeadline, Is.True);
        Assert.That(Deadline.Date, Is.EqualTo(DateTime.Now.AddDays(2).Date));
        */

        SynchronizedWithTransaction = true;
        Assert.That(SynchronizedWithTransaction, Is.True);

        RollbackOnly = true;
        Assert.That(RollbackOnly, Is.True);
    }

    [Test]
    public void InvalidDeadline()
    {
        int temp;
        Assert.Throws<ArgumentException>(() => temp = TimeToLiveInSeconds);
    }

    /*
    [Ignore("investigate...")]
    [Test]
    public void ZeroMilliseconds()
    {
        Deadline = DateTime.Now.AddDays(-1);
        Assert.That(TimeToLiveInMilliseconds, Is.EqualTo(0));
    }
    */
    /*
    [Test]
    public void MillisecondToLive()
    {
        Deadline = DateTime.Now.AddDays(1);
        Assert.That(TimeToLiveInMilliseconds >= 0, Is.True);
    }
    */
    /*
    [Test]
    public void SetDeadline()
    {
        SetDeadlineInSeconds( 10 );
        Assert.That(TimeToLiveInSeconds >= 0, Is.True);
    }
    */
    /*
    [Test]
    public void SetDeadlineMillis()
    {
        SetDeadlineInMilliseconds(1000);
        Assert.That(TimeToLiveInMilliseconds >= 0, Is.True);
    }
    */
}
