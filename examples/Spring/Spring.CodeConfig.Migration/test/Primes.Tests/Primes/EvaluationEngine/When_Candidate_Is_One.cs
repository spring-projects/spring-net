using NUnit.Framework;
using Primes;

namespace Tests.EvaluationEngine
{
    [TestFixture]
    public class When_Candidate_Is_One
    {
        [Test]
        public void Can_Reject_Candidate()
        {
            var primeEvaluationEngine = new PrimeEvaluationEngine();

            Assert.That(primeEvaluationEngine.IsPrime(1), Is.False);
        }
    }
}