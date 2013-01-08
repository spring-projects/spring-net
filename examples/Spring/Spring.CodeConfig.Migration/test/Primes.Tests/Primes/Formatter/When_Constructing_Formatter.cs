using NUnit.Framework;
using Primes;

namespace Tests.Formatter
{
    [TestFixture]
    public class When_Constructing_Formatter
    {
        [Test]
        public void Can_Create_Instance()
        {
            var outputFormatter = new OutputFormatter();

            Assert.IsNotNull(outputFormatter);
        }
    }
}