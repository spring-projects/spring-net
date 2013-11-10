using NUnit.Framework;
using Primes;

namespace Tests.EvaluationEngine
{
    [TestFixture]
    public class When_Candidate_Is_Two
    {
        [Test]
        public void Can_Accept_Candidate()
        {
            var primeEvaluationEngine = new PrimeEvaluationEngine();

            Assert.IsTrue(primeEvaluationEngine.IsPrime(2));
        }
    }
}