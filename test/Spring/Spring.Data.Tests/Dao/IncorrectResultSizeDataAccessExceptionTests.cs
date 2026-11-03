using NUnit.Framework;

namespace Spring.Dao;

[TestFixture]
public class IncorrectResultSizeDataAccessExceptionTests
{
    [Test]
    public void SizeGetter()
    {
        IncorrectResultSizeDataAccessException ex = new IncorrectResultSizeDataAccessException(1, 4);
        Assert.That(ex.ExpectedSize, Is.EqualTo(1));
        Assert.That(ex.ActualSize, Is.EqualTo(4));
    }
}
