using Quartz;

namespace Spring.Scheduling.Quartz.Integration.Tests;

public class TestJob : IJob
{
    /// <summary>
    /// Set when the scheduler has run the job once.
    /// </summary>
    public static readonly ManualResetEventSlim Executed = new(false);

    public Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Executing Execute!");
        Executed.Set();
        return Task.FromResult(true);
    }

    public void DoIt()
    {
        Console.WriteLine("Executing DoIt!");
    }
}
