using NUnit.Framework;
using Primes;

namespace Tests.EvaluationEngine
{
    [TestFixture]
    public class When_Candidate_Is_Even
    {
        [Test]
        public void Can_Reject_Candidate()
        {
            var primeEvaluationEngine = new PrimeEvaluationEngine();

            Assert.IsFalse(primeEvaluationEngine.IsPrime(8));
        }
    }
}