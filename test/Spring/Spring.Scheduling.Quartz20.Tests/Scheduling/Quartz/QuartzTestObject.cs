namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// A simple test object for Quartz.NET to run
    /// that simulates imports and exports.
    /// </summary>
    /// <author>Rob Harrop</author>
    public class QuartzTestObject
    {
        private int exportCount;
        private int importCount;

        /// <summary>
        /// Executes a fake import and increments counter.
        /// </summary>
        public void DoImport()
        {
            ++importCount;
        }

        ///<summary>
        /// Executes a fake export and increments counter.
        ///</summary>
        public void DoExport()
        {
            ++exportCount;
        }

        /// <summary>
        /// Tells how many times import has been done.
        /// </summary>
        public int ImportCount
        {
            get { return importCount; }
        }

        /// <summary>
        /// Tells how many times export has been done.
        /// </summary>
        public int ExportCount
        {
            get { return exportCount; }
        }
    }
}