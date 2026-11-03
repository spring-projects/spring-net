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

using System.Collections.Specialized;
using System.Configuration;
using System.Text;
using NUnit.Framework;
using Spring.Core.IO;

namespace Spring.Objects.Factory.Config;

/// <summary>
/// Unit tests for the ConfigurationReader class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class ConfigurationReaderTests
{
    private const string SunnyDayXml = @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration>
    <configSections>
        <section name='foo' type='System.Configuration.NameValueSectionHandler, System'/>
    </configSections>
    <foo>
        <add key='rilo' value='kiley'/>
        <add key='jenny' value='lewis'/>
    </foo>
</configuration>";

    /// <summary>
    /// Unfortunately ConfigurationManager doesn't accept uri's.
    /// </summary>
    [Test]
    public void ConfigurationManagerCannotReadFromUrl()
    {
        try
        {
            ConfigurationManager.OpenExeConfiguration("http://localhost/something.config");
            Assert.Fail();
        }
        catch (ConfigurationErrorsException cfgex)
        {
#if NETFRAMEWORK
            Assert.That(cfgex.InnerException, Is.InstanceOf(typeof(NotSupportedException)));
#else
            Assert.That(cfgex.InnerException, Is.InstanceOf(typeof(ArgumentException)));
#endif
        }
    }

    [Test]
    public void ReadSunnyDay()
    {
        using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(SunnyDayXml)))
        {
            NameValueCollection props = ConfigurationReader.Read(new InputStreamResource(stream, ""), "foo");
            Assert.That(props,
                Is.Not.Null,
                "Failed to read in any properties at all (props is null).");
            Assert.That(props.Count, Is.EqualTo(2),
                "Wrong number of properties read in.");
            Assert.That(props["rilo"],
                Is.EqualTo("kiley"),
                "Wrong value for second property");
            Assert.That(props["jenny"],
                Is.EqualTo("lewis"),
                "Wrong value for second property");
        }
    }

    [Test]
    public void GetSectionLocalSectionHandler()
    {
        string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration>
    <configSections>
        <section name='connectionStrings' type='System.Configuration.ConnectionStringsSection, System.Configuration, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' requirePermission='false' />
    </configSections>
    <connectionStrings>
      <add name='Sales' 
           providerName='System.Data.SqlClient'
           connectionString= 'server=myserver;database=Products;uid=user name;pwd=secure password' />
    </connectionStrings>
</configuration>
";
        using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
        {
            ConnectionStringsSection css = ConfigurationReader.GetSection<ConnectionStringsSection>(new InputStreamResource(stream, ""), "connectionStrings");
            Assert.That(css, Is.Not.Null, "Failed to read in any properties at all (props is null).");
            Assert.That(css.ConnectionStrings["Sales"], Is.Not.Null);
            Assert.That(css.ConnectionStrings["Sales"].ProviderName, Is.EqualTo("System.Data.SqlClient"));
            Assert.That(css.ConnectionStrings["Sales"].ConnectionString, Is.EqualTo("server=myserver;database=Products;uid=user name;pwd=secure password"));
        }
    }

    [Test]
    public void GetSectionMachineInheritedSectionHandler()
    {
        string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration>
    <connectionStrings>
      <add name='Sales' 
           providerName='System.Data.SqlClient'
           connectionString= 'server=myserver;database=Products;uid=user name;pwd=secure password' />
    </connectionStrings>
</configuration>
";
        using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
        {
            ConnectionStringsSection css = ConfigurationReader.GetSection<ConnectionStringsSection>(new InputStreamResource(stream, ""), "connectionStrings");
            Assert.That(css, Is.Not.Null, "Failed to read in any properties at all (props is null).");
            Assert.That(css.ConnectionStrings["Sales"], Is.Not.Null);
            Assert.That(css.ConnectionStrings["Sales"].ProviderName, Is.EqualTo("System.Data.SqlClient"));
            Assert.That(css.ConnectionStrings["Sales"].ConnectionString, Is.EqualTo("server=myserver;database=Products;uid=user name;pwd=secure password"));
        }
    }

    [Test]
    public void GetSectionSunnyDay()
    {
        using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(SunnyDayXml)))
        {
            NameValueCollection props = ConfigurationReader.Read(new InputStreamResource(stream, ""), "foo");
            Assert.That(props,
                Is.Not.Null,
                "Failed to read in any properties at all (props is null).");
            Assert.That(props.Count, Is.EqualTo(2),
                "Wrong number of properties read in.");
            Assert.That(props["rilo"],
                Is.EqualTo("kiley"),
                "Wrong value for second property");
            Assert.That(props["jenny"],
                Is.EqualTo("lewis"),
                "Wrong value for second property");
        }
    }

    [Test]
    public void ReadWithOverrideOfPreviouslyExistingValues()
    {
        using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(SunnyDayXml)))
        {
            NameValueCollection defaults = new NameValueCollection();
            defaults.Add("jenny", "agutter");
            NameValueCollection props
                = ConfigurationReader.Read(new InputStreamResource(stream, ""), "foo", defaults);
            Assert.That(ReferenceEquals(defaults, props), Is.True, "Must have got same collection as was passed in.");
            Assert.That(props["jenny"], Is.EqualTo("lewis"), "Wrong value for overridden property (was not overridden");
        }
    }

    [Test]
    public void ReadWithOverrideOfPreviouslyExistingValuesButWithOverrideSwitchedOff()
    {
        using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(SunnyDayXml)))
        {
            NameValueCollection defaults = new NameValueCollection();
            defaults.Add("jenny", "agutter");
            NameValueCollection props
                = ConfigurationReader.Read(new InputStreamResource(stream, ""), "foo", defaults, false);
            Assert.That(ReferenceEquals(defaults, props), Is.True, "Must have got same collection as was passed in.");
            Assert.That(props["jenny"], Is.EqualTo("agutter,lewis"), "Wrong value for overridden property (was not overridden");
        }
    }

    [Test]
    public void ReadWithNullExistingValuesPassedIn()
    {
        using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(SunnyDayXml)))
        {
            NameValueCollection props
                = ConfigurationReader.Read(new InputStreamResource(stream, ""), "foo", null);
            Assert.That(props, Is.Not.Null, "Failed to read in any properties at all (props is null).");
            Assert.That(props.Count, Is.EqualTo(2), "Wrong number of properties read in.");
            Assert.That(props["rilo"], Is.EqualTo("kiley"), "Wrong value for second property");
            Assert.That(props["jenny"], Is.EqualTo("lewis"), "Wrong value for second property");
        }
    }

    [Test]
    public void ReadWithNoConfigSectionSectionDefaultsToNameValueSectionHandler()
    {
        const string NoConfigSectionXml = @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration>
    <foo>
        <add key='rilo' value='kiley'/>
        <add key='jenny' value='lewis'/>
    </foo>
</configuration>";
        using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(NoConfigSectionXml)))
        {
            NameValueCollection props
                = ConfigurationReader.Read(new InputStreamResource(stream, ""), "foo", null);
            Assert.That(props, Is.Not.Null, "Failed to read in any properties at all (props is null).");
            Assert.That(props.Count, Is.EqualTo(2), "Wrong number of properties read in.");
            Assert.That(props["rilo"], Is.EqualTo("kiley"), "Wrong value for second property");
            Assert.That(props["jenny"], Is.EqualTo("lewis"), "Wrong value for second property");
        }
    }

    [Test]
    public void TryReadFromNonExistantConfigSection()
    {
        Assert.Throws<ConfigurationErrorsException>(() =>
        {
            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(SunnyDayXml)))
            {
                ConfigurationReader.Read(new InputStreamResource(stream, ""), "ELNOMBRE", null);
            }
        }, "Cannot read config section 'ELNOMBRE' - section not found.");
    }
}
