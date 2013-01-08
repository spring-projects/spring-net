using System.Text;

namespace Primes
{
    public class PrimeGenerator : IPrimeGenerator
    {
        private PrimeEvaluationEngine _primeEvaluationEngine;

        public string GeneratePrimesUpTo(int max)
        {

            var sb = new StringBuilder();

            for (int i = 0; i < max; i++)
            {
                if (_primeEvaluationEngine.IsPrime(i))
                {
                    sb.Append(i.ToString());
                    sb.Append(",");
                }
            }

            string theString = sb.ToString();

            if (theString.Contains(","))
                theString = theString.Remove(theString.LastIndexOf(","));

            return theString;
        }


        public PrimeGenerator(PrimeEvaluationEngine primeEvaluationEngine)
        {
            _primeEvaluationEngine = primeEvaluationEngine;
        }
    }
}