using System;
using System.Linq;
using System.Net;

using Spring.Http;
using Spring.Http.Client;
using Spring.Http.Rest;
using Newtonsoft.Json.Linq;

using Spring.HttpMessageConverterQuickStart.Converters;

namespace Spring.HttpMessageConverterQuickStart
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RestTemplate rt = new RestTemplate("http://twitter.com");
                rt.MessageConverters.Add(new NJsonHttpMessageConverter());

                rt.GetForObjectAsync<JArray>("/statuses/user_timeline.json?screen_name={name}&count={count}", new string[] { "SpringForNet", "10" }, 
                    r =>
                    {
                        if (r.Error == null)
                        {
                            var tweets = from el in r.Response.Children()
                                         select el.Value<string>("text");
                            foreach (string tweet in tweets)
                            {
                                Console.WriteLine(String.Format("* {0}", tweet));
                                Console.WriteLine();
                            }
                        }
                        else
                        {
                            Console.WriteLine(r.Error);
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

