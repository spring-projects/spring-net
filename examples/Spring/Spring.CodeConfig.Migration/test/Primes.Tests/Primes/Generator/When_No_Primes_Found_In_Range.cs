using NUnit.Framework;
using Primes;

namespace Tests.Generator
{
    [TestFixture]
    public class When_No_Primes_Found_In_Range
    {
        [Test]
        public void Can_Return_Empty_String()
        {
            var primeGenerator = new PrimeGenerator(new PrimeEvaluationEngine());

            Assert.AreEqual(string.Empty, primeGenerator.GeneratePrimesUpTo(0));
        }
    }
}