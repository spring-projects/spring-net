using System;

using Spring.Http.Rest;

namespace Spring.RestQuickStart
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RestTemplate rt = new RestTemplate("http://twitter.com");
                string result = rt.GetForObject<string>("/statuses/user_timeline.xml?id={id}", "lancearmstrong");

                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
			finally
			{
				Console.WriteLine("--- hit <return> to quit ---");
				Console.ReadLine();
			}
        }
    }
}
