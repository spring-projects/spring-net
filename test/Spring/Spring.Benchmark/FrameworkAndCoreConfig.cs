using System.Runtime.InteropServices;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;

namespace Spring.Benchmark;

/// <summary>Compares .NET with .NET Framework where there is one.</summary>
public class FrameworkAndCoreConfig : ManualConfig
{
    public FrameworkAndCoreConfig()
    {
        AddJob(Job.Default.WithRuntime(CoreRuntime.Core10_0));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            AddJob(Job.Default.WithRuntime(ClrRuntime.Net472));
        }
    }
}
