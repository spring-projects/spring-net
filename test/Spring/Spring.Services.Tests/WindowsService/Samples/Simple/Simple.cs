using System.IO;
using Spring.Core.IO;

namespace Spring.Services.WindowsService.Samples
{
    public class Simple
    {
        private readonly string both;
        string appName, appFullPath;

        public Simple(string both)
        {
            this.both = both;
        }

        public void Start ()
        {
            using (TextWriter writer = File.CreateText(new FileSystemResource("~/simple.txt").File.FullName))
            {
                writer.WriteLine(AppName);
                writer.WriteLine(AppFullPath);
                writer.WriteLine(both);
            }
        }

        public void Stop ()
        {
        }

        public string AppName
        {
            get { return appName; }
            set { appName = value; }
        }
        public string AppFullPath
        {
            get { return appFullPath; }
            set { appFullPath = value; }
        }
    }
}