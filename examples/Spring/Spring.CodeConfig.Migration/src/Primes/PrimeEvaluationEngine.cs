using System;

namespace Primes
{
    public class PrimeEvaluationEngine
    {
        public bool IsPrime(int candidate)
        {
            if (candidate <= 1)
                return false;

            if (candidate == 2)
                return true;

            if (candidate % 2 == 0)
                return false;

            for (int i = 3; i < Math.Sqrt(candidate); i += 2)
            {
                if (candidate % i == 0)
                    return false;
            }


            return true;
        }
    }
}