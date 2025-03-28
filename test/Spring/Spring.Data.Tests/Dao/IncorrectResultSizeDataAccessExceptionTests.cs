using NUnit.Framework;

namespace Spring.Dao;

[TestFixture]
public class IncorrectResultSizeDataAccessExceptionTests
{
    [Test]
    public void SizeGetter()
    {
        IncorrectResultSizeDataAccessException ex = new IncorrectResultSizeDataAccessException(1, 4);
        Assert.AreEqual(1, ex.ExpectedSize);
        Assert.AreEqual(4, ex.ActualSize);
    }
}
