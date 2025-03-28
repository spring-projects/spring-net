#region License

/*
 * Copyright © 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using System.Configuration;
using System.Configuration.Internal;
using NUnit.Framework;
using Spring.Util;

namespace Spring.EnterpriseServices;

[TestFixture]
public class ExeConfigurationSystemTests
{
    [Test]
    public void SunnyDay()
    {
        FileInfo resFile = TestResourceLoader.ExportResource(this, ".config", new FileInfo(Path.GetTempFileName() + ".config"));
        string exePath = resFile.FullName.Substring(0, resFile.FullName.Length - ".config".Length);
        Assert.IsTrue(resFile.Exists);
        IInternalConfigSystem prevConfig = null;
        try
        {
            ExeConfigurationSystem ccs = new ExeConfigurationSystem(exePath);
            prevConfig = ConfigurationUtils.SetConfigurationSystem(ccs, true);
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
