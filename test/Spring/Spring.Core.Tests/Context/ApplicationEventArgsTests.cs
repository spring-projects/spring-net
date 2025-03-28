using NUnit.Framework;

namespace Spring.Context;

[TestFixture]
public class ApplicationEventArgsTests
{
    private ApplicationEventArgs _args;

    [SetUp]
    public void Init()
    {
        _args = new ApplicationEventArgs();
    }

    [TearDown]
    public void Destroy()
    {
        _args = null;
    }

    [Test]
    public void ArgsTimeStamp()
    {
        Assert.IsTrue(_args.TimeStamp.ToString("MM/dd/yyyy").Equals(DateTime.Now.ToString("MM/dd/yyyy")));
    }

    [Test]
    public void ArgsMilliTimestamp()
    {
        Assert.IsTrue((_args.TimeStamp.Ticks - 621355968000000000) / 10000 == _args.EventTimeMilliseconds);
    }
}
