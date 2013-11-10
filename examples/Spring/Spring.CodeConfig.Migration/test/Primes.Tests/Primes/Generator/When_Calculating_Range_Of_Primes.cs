using NUnit.Framework;
using Primes;

namespace Tests.Generator
{
    [TestFixture]
    public class When_Calculating_Range_Of_Primes
    {
        [Test]
        public void Can_Return_Values_Comma_Delimited()
        {
            var primeGenerator = new PrimeGenerator(new PrimeEvaluationEngine());

            string actual = primeGenerator.GeneratePrimesUpTo(10);

            Assert.AreEqual("2,3,5,7,9", actual);
        }
    }
}