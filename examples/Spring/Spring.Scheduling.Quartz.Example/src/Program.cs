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
				Console.WriteLine("Spring configuration succeeded, quartz jobs running.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.Out.WriteLine("--- Press <return> to quit ---");
                Console.ReadLine();
            }
            Console.Out.WriteLine("--- Press <return> to quit ---");
            Console.ReadLine();
        }
    }
}
