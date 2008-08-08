using System;

using Quartz;

using Spring.Scheduling.Quartz;

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
            set { userName = value; }
        }

		/// <summary>
		/// Execute.
		/// </summary>
		/// <param name="context"></param>
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            Console.WriteLine("{0}: ExecuteInternal called, user name: {1}, next fire time {2}", 
                DateTime.Now, userName, context.NextFireTimeUtc.Value.ToLocalTime());
        }

    }
}
