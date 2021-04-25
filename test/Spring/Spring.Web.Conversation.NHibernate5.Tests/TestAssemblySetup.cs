using System;
using System.IO;
using NUnit.Framework;

namespace Spring
{
    [SetUpFixture]
    public class TestAssemblySetup
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            // work around getting interop assembly to correct directory
            var requiredFile = Path.Combine(Environment.CurrentDirectory, "SQLite.Interop.dll");
            var fileToUse = Path.Combine(Environment.CurrentDirectory, "x86", "SQLite.Interop.dll");
            if (!File.Exists(requiredFile) && File.Exists(fileToUse))
            {
                File.Copy(fileToUse, requiredFile);
            }
        }
    }
}