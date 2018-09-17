using System.Collections.Generic;
using NUnit.Framework;
using Primes;

namespace Tests.Formatter
{
    [TestFixture]
    public class When_Formatting_Output
    {
        private string _generatedPrimes;


        [OneTimeSetUp]
        public void _TestFixtureSetUp()
        {
            _generatedPrimes = new PrimeGenerator(new PrimeEvaluationEngine()).GeneratePrimesUpTo(20);
        }


        [Test]
        public void Collection_of_Output_Lines_is_Returned()
        {
            var outputFormatter = new OutputFormatter();

            IList<string> formattedOutput = outputFormatter.Format(_generatedPrimes);

            Assert.IsAssignableFrom(typeof (List<string>), formattedOutput);
        }


        [Test]
        public void Each_Output_Line_Contains_No_More_Than_Five_Items()
        {
            var outputFormatter = new OutputFormatter();

            IList<string> formattedOutput = outputFormatter.Format(_generatedPrimes);

            string[] splitPrimes;

            foreach (string item in formattedOutput)
            {
                splitPrimes = item.Split(",".ToCharArray());

                Assert.LessOrEqual(splitPrimes.Length, 5);
            }
        }

        [Test]
        public void Every_Tenth_Line_Contains_Count()
        {
            var generatedPrimes = new PrimeGenerator(new PrimeEvaluationEngine()).GeneratePrimesUpTo(2000);

            var outputFormatter = new OutputFormatter();

            IList<string> formattedOutput = outputFormatter.Format(generatedPrimes);

            for (int i = 10; i < formattedOutput.Count; i += 11)
            {
                Assert.IsTrue(formattedOutput[i].Contains("Count:"));
            }
        }
    }
}