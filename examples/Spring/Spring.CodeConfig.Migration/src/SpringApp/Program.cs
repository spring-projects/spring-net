using System;
using System.Reflection;
using Primes;
using Spring.Context;
using Spring.Context.Attributes;
using Spring.Context.Support;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using System.Linq;

namespace SpringApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //uncomment this next line to to use XML to assemble the container
            //IApplicationContext ctx = CreateContainerUsingXML();
            
            //uncomment this next line to to use CodeConfig to assemble the container
            IApplicationContext ctx = CreateContainerUsingCodeConfig();

            ConsoleReport report = ctx["ConsoleReport"] as ConsoleReport;
            report.Write();

            ctx.Dispose();

            Console.WriteLine("--- hit enter to exit --");
            Console.ReadLine();
        }


        private static IApplicationContext CreateContainerUsingXML()
        {
            return new XmlApplicationContext("application-context.xml");
        }

        private static IApplicationContext CreateContainerUsingCodeConfig()
        {
            CodeConfigApplicationContext ctx = new CodeConfigApplicationContext();
            ctx.ScanAllAssemblies();
            ctx.Refresh();
            return ctx;
        }

    }
}
