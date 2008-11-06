namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// </summary>
    /// <author>Rob Harrop</author>
    public class QuartzTestObject
    {
        private int exportCount;
        private int importCount;

        public void DoImport()
        {
            ++importCount;
        }

        public void DoExport()
        {
            ++exportCount;
        }

        public int ImportCount
        {
            get { return importCount; }
        }

        public int ExportCount
        {
            get { return exportCount; }
        }
    }
}