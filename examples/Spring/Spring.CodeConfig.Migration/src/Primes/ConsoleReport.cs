using System;
using System.Collections.Generic;

namespace Primes
{
    public class ConsoleReport
    {
        private IOutputFormatter _outputFormatter;

        private IPrimeGenerator _primeGenerator;

        private int _maxNumber;

        public ConsoleReport(IOutputFormatter outputFormatter, IPrimeGenerator primeGenerator)
        {
            _outputFormatter = outputFormatter;
            _primeGenerator = primeGenerator;
        }
        public IOutputFormatter OutputFormatter
        {
            set { _outputFormatter = value; }
        }

        public int MaxNumber
        {
            get { return _maxNumber; }
            set { _maxNumber = value; }
        }

        public void Write()
        {
            IList<string> results = _outputFormatter.Format(_primeGenerator.GeneratePrimesUpTo(_maxNumber));

            foreach (string item in results)
            {
                Console.WriteLine(item);
            }
        }
    }
}