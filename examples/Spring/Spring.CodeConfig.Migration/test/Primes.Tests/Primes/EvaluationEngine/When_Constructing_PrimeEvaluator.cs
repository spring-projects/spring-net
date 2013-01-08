using NUnit.Framework;
using Primes;

namespace Tests.EvaluationEngine
{
    [TestFixture]
    public class When_Constructing_PrimeEvaluator
    {
        [Test]
        public void Can_Create_Instance()
        {
            var primeNumberEngine = new PrimeEvaluationEngine();

            Assert.IsNotNull(primeNumberEngine);
        }
    }
}