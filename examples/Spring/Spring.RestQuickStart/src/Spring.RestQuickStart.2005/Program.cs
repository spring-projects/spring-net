using System;
using System.Net;

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
                Console.WriteLine("Allowed methods : ");
                foreach (HttpMethod method in rt.OptionsForAllow("/statuses/"))
                {
                    Console.WriteLine(method);
                }

                // Exemple async call
                rt.GetForObjectAsync<string>("/statuses/user_timeline.xml?screen_name={name}&count={count}", new string[] { "SpringForNet", "5" },
                    delegate(MethodCompletedEventArgs<string> eventArgs)
                    {
                        if (eventArgs.Error != null)
                        {
                            Console.WriteLine(eventArgs.Error);
                        }
                        else
                        {
                            Console.WriteLine(eventArgs.Response);
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
