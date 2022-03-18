using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace Spring
{
    [SetUpFixture]
    public class TestAssemblySetup
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            try
            {
                // while R# is broken (https://youtrack.jetbrains.com/issue/RSRP-451142)
                string codeBase = Assembly.GetExecutingAssembly().Location;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                var pathToUse = Path.GetDirectoryName(path);
                Directory.SetCurrentDirectory(pathToUse);
            }
            catch
            {
                // ignored
            }
        }
    }
}