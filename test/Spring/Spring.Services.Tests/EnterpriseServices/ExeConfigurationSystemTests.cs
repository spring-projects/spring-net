
namespace Spring.EnterpriseServices
{
#if NET_2_0
    using System.Configuration;
    using System.Configuration.Internal;
    using System.IO;
    using Common.Logging;
    using Common.Logging.Simple;
    using NUnit.Framework;
    using Spring.Util;

    [TestFixture]
    public class ExeConfigurationSystemTests
    {
        [Test]
        public void SunnyDay()
        {
            FileInfo resFile = TestResourceLoader.ExportResource(this, ".config", new FileInfo(Path.GetTempFileName()+".config"));
            string exePath = resFile.FullName.Substring(0, resFile.FullName.Length - ".config".Length);
            Assert.IsTrue(resFile.Exists);
            IInternalConfigSystem prevConfig = null;
            try
            {
                ExeConfigurationSystem ccs = new ExeConfigurationSystem(exePath);
                prevConfig = ConfigurationUtils.SetConfigurationSystem(ccs, true);
                LogSetting settings = (LogSetting) ConfigurationManager.GetSection("common/logging");
                Assert.AreEqual(typeof (TraceLoggerFactoryAdapter), settings.FactoryAdapterType);

                Assert.AreEqual("from custom config!", ConfigurationManager.AppSettings["key"]);

                Assert.IsNull(ConfigurationManager.GetSection("spring/context"));
            }
            finally
            {
                if (prevConfig != null)
                {
                    ConfigurationUtils.SetConfigurationSystem(prevConfig, true);
                }
                resFile.Delete();
            }
        }
    }
#endif
}
