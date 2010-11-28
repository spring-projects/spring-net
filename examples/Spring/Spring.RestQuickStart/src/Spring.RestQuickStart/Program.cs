using System;
#if NET_3_5
using System.Linq;
using System.Xml.Linq;
#endif

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

#if NET_3_5
                XElement result = rt.GetForObject<XElement>("/statuses/user_timeline.xml?id={id}&count={2}", "SpringForNet", "10");
                var tweets = from el in result.Elements("status")
                             select el.Element("text").Value;
                foreach (string tweet in tweets)
                {
                    Console.WriteLine(String.Format("* {0}", tweet));
                    Console.WriteLine();
                }
#else
                string result = rt.GetForObject<string>("/statuses/user_timeline.xml?id={id}&count={2}", "SpringForNet", "10");
                Console.WriteLine(result);
#endif

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
