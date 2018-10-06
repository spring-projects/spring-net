using System;
using System.Threading.Tasks;
using Quartz;

namespace Spring.Scheduling.Quartz.Integration.Tests
{
    public class TestJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Executing Execute!");
            return Task.FromResult(true);
        }

        public void DoIt()
        {
            Console.WriteLine("Executing DoIt!");
        }

    }
}