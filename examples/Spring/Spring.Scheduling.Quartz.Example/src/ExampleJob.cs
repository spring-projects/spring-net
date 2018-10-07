using System;
using System.Threading.Tasks;
using Quartz;

namespace Spring.Scheduling.Quartz.Example
{
    /// <summary>
    /// Example job.
    /// </summary>
    public class ExampleJob : QuartzJobObject
    {
        private string userName;

        /// <summary>
        /// Simple property that can be injected.
        /// </summary>
        public string UserName
        {
            set => userName = value;
        }

        /// <inheritdoc />
        protected override Task ExecuteInternal(IJobExecutionContext context)
        {
            Console.WriteLine(
                $"{DateTime.Now}: ExecuteInternal called, user name: {userName}, next fire time {context.NextFireTimeUtc?.ToLocalTime()}");

            return Task.FromResult(true);
        }
    }
}