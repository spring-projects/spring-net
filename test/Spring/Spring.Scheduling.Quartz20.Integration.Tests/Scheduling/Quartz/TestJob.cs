using System;

using Quartz;

namespace Spring.Scheduling.Quartz.Integration.Tests
{
    public class TestJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Executing Execute!");
        }

        public void DoIt()
        {
            Console.WriteLine("Executing DoIt!");
        }

    }
}