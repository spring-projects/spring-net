using System;

using Spring.Context.Support;

namespace Spring.Scheduling.Quartz.Example
{
    class Program
    {
        static void Main()
        {
            try
            {
                XmlApplicationContext ctx = new XmlApplicationContext("spring-objects.xml");
				Console.WriteLine("Spring configuration succeeded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();
        }
    }
}
