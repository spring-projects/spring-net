namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Generic scheduling exception.
    /// </summary>
    public class SchedulingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SchedulingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchedulingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The original exception.</param>
        public SchedulingException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}
