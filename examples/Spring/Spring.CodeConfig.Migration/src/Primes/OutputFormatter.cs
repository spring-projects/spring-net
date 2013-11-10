using System.Collections.Generic;
using System.Text;

namespace Primes
{
    public class OutputFormatter : IOutputFormatter
    {
        public IList<string> Format(string input)
        {
            string[] elements = input.Split(",".ToCharArray());

            IList<string> result = new List<string>();

            int lineCount = 0;

            for (int i = 0; i < elements.Length; i += 5)
            {

                var sb = new StringBuilder();

                for (int j = i; j < i + 5; j++)
                {
                    sb.Append(elements[j]);
                    sb.Append(",");
                    if (j == elements.Length - 1)
                        break;
                }

                string theString = sb.ToString();

                if (theString.EndsWith(","))
                    theString = theString.Substring(0, theString.Length - 1);

                if (lineCount > 0 && (lineCount % 10 == 0))
                    result.Add("Count: " + i);

                lineCount += 1;

                result.Add(theString);
            }


            return result;
        }




    }
}