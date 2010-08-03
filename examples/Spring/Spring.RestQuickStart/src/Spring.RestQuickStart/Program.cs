using System;
using System.Linq;
using System.Xml.Linq;

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

                //string result = rt.GetForObject<string>("/statuses/user_timeline.xml?id={id}&count={2}", "SpringForNet", "10");
                //Console.WriteLine(result);

                XElement result = rt.GetForObject<XElement>("/statuses/user_timeline.xml?id={id}&count={2}", "SpringForNet", "10");
                var tweets = from el in result.Elements("status")
                             select el.Element("text").Value;
                foreach (string tweet in tweets)
                {
                    Console.WriteLine(String.Format("* {0}", tweet));
                    Console.WriteLine();
                }

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
