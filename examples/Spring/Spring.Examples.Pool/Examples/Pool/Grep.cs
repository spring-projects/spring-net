////<examples>
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Spring.Threading;

namespace Spring.Examples.Pool
{
	public class Grep : IRunnable
	{
	    private string file;
	    private string regexPattern;

	    public class Match
        {
            string fileName, line;
            int lineNum;

            public Match(string fileName, int lineNum, string line)
            {
                this.fileName = fileName;
                this.lineNum = lineNum;
                this.line = line;
            }

	        public void Print()
	        {
	            Console.Out.WriteLine("thread #{0}: {1}: {2}: {3}", Thread.CurrentThread.GetHashCode(), fileName, lineNum, line);
	        }
        }

        public class Error
        {
            string file;
            Exception e;

            public Error(string file, Exception e)
            {
                this.file = file;
                this.e = e;
            }

            public void Print()
            {
                Console.Out.WriteLine("file [{0}]: {1}", file, e.Message);
            }
        }

	    public Grep(string file, string regexPattern)
	    {
	        this.file = file;
	        this.regexPattern = regexPattern;
	    }

	    public void Run()
	    {
            try 
            {
                int lineNum = 1;
                using (TextReader r = File.OpenText(file))
                {
                    string line = null;
                    while ((line = r.ReadLine()) != null)
                    {
                        if (Regex.IsMatch(line, Regex.Escape(regexPattern), RegexOptions.Singleline))
                        {
                            new Grep.Match(file, lineNum, line).Print();
                        }
                        lineNum++;
                    }
                }
            }
            catch (Exception e)
            {
                new Grep.Error(file, e).Print();
            }
        }
	}

    public class ParallelGrep
    {
        //// <example name="parallel-grep-class">
        private PooledQueuedExecutor executor;

        public ParallelGrep(int size)
        {
            executor = new PooledQueuedExecutor(size);
        }

        public void Recurse(string startPath, string filePattern, string regexPattern)
        {            
            foreach (string file in Directory.GetFiles(startPath, filePattern))
            {
                executor.Execute(new Grep(file, regexPattern));
            }
            foreach (string directory in Directory.GetDirectories(startPath))
            {
                Recurse(directory, filePattern, regexPattern);
            }
        }

        public void Stop()
        {
            executor.Stop();
        }
        //// </example>

        //// <example name="parallel-grep-main">
        public static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.Out.WriteLine("usage: {0} regex directory file-pattern [pool-size]", Assembly.GetEntryAssembly().CodeBase);
                Environment.Exit(1);
            }

            string regexPattern = args[0];
            string startPath = args[1];
            string filePattern = args[2];
            int size = 10;
            try
            {
                size = Int32.Parse(args[3]);
            }
            catch
            {
            }
            Console.Out.WriteLine ("pool size {0}", size);

            ParallelGrep grep = new ParallelGrep(size);
            grep.Recurse(startPath, filePattern, regexPattern);
            grep.Stop();
        }
        //// </example>

    }
}
//// </examples>
