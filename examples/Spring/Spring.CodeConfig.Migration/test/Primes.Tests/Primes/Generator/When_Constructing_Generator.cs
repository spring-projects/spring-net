using NUnit.Framework;
using Primes;

namespace Tests.Generator
{
    [TestFixture]
    public class When_Constructing_Generator
    {
        [Test]
        public void Can_Create_Instance()
        {
            var generator = new PrimeGenerator(new PrimeEvaluationEngine());

            Assert.IsNotNull(generator);
        }
    }
}