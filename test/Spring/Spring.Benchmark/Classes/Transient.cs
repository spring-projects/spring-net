using System;

namespace Spring.Benchmark.Classes
{
    public interface ITransient1
    {
        void DoSomething();
    }

    public interface ITransient2
    {
        void DoSomething();
    }

    public interface ITransient3
    {
        void DoSomething();
    }

    public class Transient1 : ITransient1
    {
        private static int counter;

        public Transient1()
        {
            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }

        public void DoSomething()
        {
            Console.WriteLine("World");
        }
    }

    public class Transient2 : ITransient2
    {
        private static int counter;

        public Transient2()
        {
            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }

        public void DoSomething()
        {
            Console.WriteLine("World");
        }
    }

    public class Transient3 : ITransient3
    {
        private static int counter;

        public Transient3()
        {
            System.Threading.Interlocked.Increment(ref counter);
        }

        public static int Instances
        {
            get { return counter; }
            set { counter = value; }
        }

        public void DoSomething()
        {
            Console.WriteLine("World");
        }
    }
}
