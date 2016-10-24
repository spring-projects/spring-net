using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

using NUnit.Framework;

namespace Spring
{
    [SetUpFixture]
    public class TestAssemblySetup
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            // while R# is broken (https://youtrack.jetbrains.com/issue/RSRP-451142)
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            var pathToUse = Path.GetDirectoryName(path);
            Directory.SetCurrentDirectory(pathToUse);

            CultureInfo enUs = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = enUs;
            Thread.CurrentThread.CurrentUICulture = enUs;
        }
    }
}