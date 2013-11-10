using System.Collections.Generic;

namespace Primes
{
    public interface IOutputFormatter
    {
        IList<string> Format(string input);
    }
}