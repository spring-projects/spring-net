using NUnit.Framework;
using Primes;

namespace Tests.EvaluationEngine
{
    [TestFixture]
    public class When_Number_is_evaluated
    {
        private const int NON_PRIME_TEST_VALUE = 35;

        private const int PRIME_TEST_VALUE = 17;

        [Test]
        public void Can_Accept_Candidate_When_Prime()
        {
            var primeEvaluationEngine = new PrimeEvaluationEngine();

            Assert.IsTrue(primeEvaluationEngine.IsPrime(PRIME_TEST_VALUE));
        }

        [Test]
        public void Can_Reject_Candidate_When_Not_Prime()
        {
            var primeEvaluationEngine = new PrimeEvaluationEngine();

            Assert.IsFalse(primeEvaluationEngine.IsPrime(NON_PRIME_TEST_VALUE));
        }
    }
}