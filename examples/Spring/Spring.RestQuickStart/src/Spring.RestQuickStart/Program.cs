using System;
using System.Net;
using System.Linq;
using System.Xml.Linq;

using Spring.Http;
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

                // Exemple sync call
                Console.WriteLine("Resource headers : ");
                HttpHeaders headers = rt.HeadForHeaders("/statuses/");
                foreach (string header in headers)
                {
                    Console.WriteLine(String.Format("{0}: {1}", header, headers[header]));
                }

                // Exemple async call
                rt.GetForObjectAsync<XElement>("/statuses/user_timeline.xml?screen_name={name}", new string[] { "SpringForNet" },
                    r =>
                    {
                        if (r.Error != null)
                        {
                            Console.WriteLine(r.Error);
                        }
                        else
                        {
                            var tweets = from el in r.Response.Elements("status")
                                         select el.Element("text").Value;
                            foreach (string tweet in tweets)
                            {
                                Console.WriteLine(String.Format("* {0}", tweet));
                                Console.WriteLine();
                            }
                        }

                    });
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
