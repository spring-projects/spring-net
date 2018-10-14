using BenchmarkDotNet.Running;

namespace Spring.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(HybridSetBenchmark).Assembly).Run(args);
        }
    }
}