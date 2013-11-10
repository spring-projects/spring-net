using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Primes
{
    public class LinqPrimeGenerator : IPrimeGenerator
    {
        #region IPrimeGenerator Members

        public string GeneratePrimesUpTo(int maxNumber)
        {

            var sb = new StringBuilder();

            if (maxNumber <= 0)
            {
                return sb.ToString();
            }

            Func<int, IEnumerable<int>> primeNumbers = max =>
                                                       from i in Enumerable.Range(2, max - 1)
                                                       where Enumerable.Range(2, i - 2).All(j => i%j != 0)
                                                       select i;

            IEnumerable<int> result = primeNumbers(maxNumber);

            foreach (int i in result)
            {
                sb.Append(i.ToString());
                sb.Append(",");
            }

            string theString = sb.ToString();

            if (theString.Contains(","))
                theString = theString.Remove(theString.LastIndexOf(","));

            return theString;
        }

        #endregion
    }
}