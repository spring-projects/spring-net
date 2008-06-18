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
        private int timeout;

		/// <summary>
		/// Simple property that can be injected.
		/// </summary>
        public int Timeout
        {
            set { timeout = value; }
        }

		/// <summary>
		/// Execute.
		/// </summary>
		/// <param name="context"></param>
        protected override void ExecuteInternal(JobExecutionContext context)
        {
            Console.WriteLine("{0}: ExecuteInternal called, timeout: {1}", DateTime.Now, timeout);
        }

		/// <summary>
		/// Custom job execution method.
		/// </summary>
        public void DoIt()
        {
			Console.WriteLine("{0}: DoIt called, timeout: {1}", DateTime.Now, timeout);
		}
    }
}
