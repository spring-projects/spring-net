using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Spring.Threading;

namespace Spring.Services.WindowsService.Common.Deploy.FileSystem
{
    class FileSystemMonitorTester
    {
        private string _path;
        AutoResetEvent _resetEvent = new AutoResetEvent(false);

        public FileSystemMonitorTester(string path)
        {
            this._path = path;
        }

        public void CreateFile(string fileName)
        {
            Directory.CreateDirectory(Path.Combine(_path, Path.GetDirectoryName(fileName)));
            using (File.CreateText(Path.Combine(_path, fileName)))
            {}
        }

        public void DeleteFile(string file)
        {
            File.Delete(Path.Combine(_path, file));
        }

        public void ChangeFile(string aFile)
        {
            using (StreamWriter writer = File.AppendText(Path.Combine(_path, aFile)))
            {
                writer.WriteLine("Random data");
            }
        }

        public void RenameFile(string file, string newName)
        {
            File.Move(Path.Combine(_path, file), Path.Combine(_path, newName));
        }

        public bool TryToCauseAnError (ISync sync, long msecs)
        {
            new Thread(new ThreadStart(Go)).Start();
            bool got = sync.Attempt(msecs);
            _resetEvent.Set();
            return got;
        }

        private void Go ()
        {
            string subDir = Guid.NewGuid().ToString();
            Directory.CreateDirectory(Path.Combine (_path, subDir));
            while (true)
            {
                if (_resetEvent.WaitOne(0, true))
                    break;
                string file = Path.Combine(subDir, Guid.NewGuid().ToString());
                CreateFile(file);
                ChangeFile(file);
                DeleteFile(file);
            }
        }
    }

    [TestFixture]
    public class FileSystemMonitorTest
    {
        FileSystemMonitor monitor;
        string _path;
        ISync sync;
        long msecs;
        FileSystemMonitorTester tester;
        string aFile;

        [SetUp]
        public void SetUp ()
        {
            _path = Path.GetFullPath(Guid.NewGuid().ToString());
            Directory.CreateDirectory(_path);
            monitor = new FileSystemMonitor(_path);
            monitor.Start();
            tester = new FileSystemMonitorTester(_path);
            sync = new Semaphore(0);
            msecs = 1000;

            aFile = "foo.txt";
        }

        [TearDown]
        public void TearDown ()
        {
        	try
        	{
        		monitor.Stop();
        		Directory.Delete(_path, true);
        	}
        	catch (IOException ex)
        	{
        		Console.Write(ex);
        	}
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void DoesNotAcceptANonExistingDirectory ()
        {
            new FileSystemMonitor("oh-no");
        }

        [Test]
        public void CanBeStoppedEvenIfNotStarted ()
        {
            monitor.Stop();

            monitor = new FileSystemMonitor(_path);
            monitor.Stop();
            Assert.IsTrue(true);
        }

        [Test]
        public void AllowRegisteringToCreatedEvents ()
        {
            monitor.Created += new FileSystemEventHandler(OnCreated);
            tester.CreateFile(aFile);
            Assert.IsTrue(sync.Attempt(msecs), "created event timed out");
        }

        [Test]
        public void AllowRegisteringToDeletedEvents ()
        {
            tester.CreateFile(aFile);

            monitor.Deleted += new FileSystemEventHandler(OnDeleted);
            tester.DeleteFile(aFile);
            Assert.IsTrue(sync.Attempt(msecs), "deleted event timed out");
        }

        [Test]
        public void AllowRegisteringToChangedEvents ()
        {
            tester.CreateFile(aFile);
            monitor.Changed += new FileSystemEventHandler(OnChanged);
            tester.ChangeFile(aFile);
            Assert.IsTrue(sync.Attempt(msecs), "changed event timed out");
        }

        [Test]
        public void AllowRegisteringToRenamedEvents ()
        {
            tester.CreateFile(aFile);
            monitor.Renamed += new RenamedEventHandler(OnRenamed);
            tester.RenameFile(aFile, "oh-what");
            Assert.IsTrue(sync.Attempt(msecs), "renamed event timed out");
        }

        [Test]
        public void ErrorEventsAreNotFiltered ()
        {            
            monitor.InternalBufferSize = 4096;
            monitor.PathMatcherExcludes = "**/*.*";
            monitor.Error += new ErrorEventHandler(OnError);
            monitor.Created += new FileSystemEventHandler(noOp);
            monitor.Deleted += new FileSystemEventHandler(noOp);
            Assert.IsTrue(tester.TryToCauseAnError(sync, msecs * 10), "error event timed out");
        }

        [Test]
        public void SupportsNantLikePattensToFilterAllowedEvents ()
        {
            monitor.NotifyFilter = NotifyFilters.FileName;
            monitor.IncludeSubdirectories = true;
            monitor.PathMatcherIncludes = "**/sub/*.txt";

            monitor.Created += new FileSystemEventHandler(OnCreated);
            tester.CreateFile("sub/Bar.bar");
            Assert.IsFalse(sync.Attempt(msecs), "created event NOT filtered and propagated");

            tester.CreateFile("sub/Bar.txt");
            Assert.IsTrue(sync.Attempt(msecs), "created event filtered and NOT propagated");
        }

        [Test]
        public void SupportsNantLikePattensToFilterNotAllowedEvents ()
        {
            monitor.NotifyFilter = NotifyFilters.FileName;
            monitor.IncludeSubdirectories = true;
            monitor.PathMatcherExcludes = "**/sub/*.txt";

            monitor.Created += new FileSystemEventHandler(OnCreated);
            tester.CreateFile("sub/Bar.bar");
            Assert.IsTrue(sync.Attempt(msecs), "created event filtered and NOT propagated");

            tester.CreateFile("sub/Bar.txt");
            Assert.IsFalse(sync.Attempt(msecs), "created event NOT filtered and propagated");
        }

        [Test]
        public void CanBeStoppedOnAnEvent ()
        {
            //TODO: test with a buffer of capacity 1 to have blocked thread 
            // when shut down

            monitor.Created += new FileSystemEventHandler(ShutDownOnCreated);

            // TODO: simulate a big number of 
            // events and randomly shut down
            tester.CreateFile(aFile);
            tester.CreateFile("buzzWords");
            Assert.IsTrue(sync.Attempt(msecs), "created event timed out");            

            tester.DeleteFile(aFile);

            // no more events: the monitor is stopped
            sync = new Latch(); 
            tester.CreateFile(aFile);
            Assert.IsFalse(sync.Attempt(msecs), "created event NOT timed out");            
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            Assert.AreEqual(WatcherChangeTypes.Created, e.ChangeType);
            sync.Release();
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            Assert.AreEqual(WatcherChangeTypes.Deleted, e.ChangeType);
            sync.Release();
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Assert.AreEqual(WatcherChangeTypes.Changed, e.ChangeType);
            sync.Release();
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            Assert.AreEqual(WatcherChangeTypes.Renamed, e.ChangeType);
            sync.Release();
        }

        private void OnError (object sender, ErrorEventArgs e)
        {
            sync.Release();
        }

        private void ShutDownOnCreated(object sender, FileSystemEventArgs e)
        {
            monitor.Stop();
            sync.Release();
        }

        private void noOp (object sender, FileSystemEventArgs e)
        {            
        }
    }
}