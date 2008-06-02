#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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

#region Imports

using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Text;

using NUnit.Framework;

using Spring.Core.IO;

#endregion

namespace Spring.Objects.Factory.Config
{
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

        [Test]
        public void ReadSunnyDay()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_ReadSunnyDay)).Run();
        }

        private void _ReadSunnyDay(out Stream stream)
        {
            stream = new MemoryStream(Encoding.UTF8.GetBytes(SunnyDayXml));
            NameValueCollection props
                = ConfigurationReader.Read(new InputStreamResource(stream, ""), "foo");
            Assert.IsNotNull(props, "Failed to read in any properties at all (props is null).");
            Assert.AreEqual(2, props.Count, "Wrong number of properties read in.");
            Assert.AreEqual("kiley", props["rilo"], "Wrong value for second property");
            Assert.AreEqual("lewis", props["jenny"], "Wrong value for second property");
        }

        [Test]
        public void ReadWithOverrideOfPreviouslyExistingValues()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_ReadWithOverrideOfPreviouslyExistingValues)).Run();
        }

        private void _ReadWithOverrideOfPreviouslyExistingValues(out Stream stream)
        {
            stream = new MemoryStream(Encoding.UTF8.GetBytes(SunnyDayXml));
            NameValueCollection defaults = new NameValueCollection();
            defaults.Add("jenny", "agutter");
            NameValueCollection props
                = ConfigurationReader.Read(new InputStreamResource(stream, ""), "foo", defaults);
            Assert.IsTrue(ReferenceEquals(defaults, props), "Must have got same collection as was passed in.");
            Assert.AreEqual("lewis", props["jenny"], "Wrong value for overridden property (was not overridden");
        }

        [Test]
        public void ReadWithOverrideOfPreviouslyExistingValuesButWithOverrideSwitchedOff()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_ReadWithOverrideOfPreviouslyExistingValuesButWithOverrideSwitchedOff)).Run();
        }

        private void _ReadWithOverrideOfPreviouslyExistingValuesButWithOverrideSwitchedOff(out Stream stream)
        {
            stream = new MemoryStream(Encoding.UTF8.GetBytes(SunnyDayXml));
            NameValueCollection defaults = new NameValueCollection();
            defaults.Add("jenny", "agutter");
            NameValueCollection props
                = ConfigurationReader.Read(new InputStreamResource(stream, ""), "foo", defaults, false);
            Assert.IsTrue(ReferenceEquals(defaults, props), "Must have got same collection as was passed in.");
            Assert.AreEqual("agutter,lewis", props["jenny"], "Wrong value for overridden property (was not overridden");
        }

        [Test]
        public void ReadWithNullExistingValuesPassedIn()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_ReadWithNullExistingValuesPassedIn)).Run();
        }

        private void _ReadWithNullExistingValuesPassedIn(out Stream stream)
        {
            stream = new MemoryStream(Encoding.UTF8.GetBytes(SunnyDayXml));
            NameValueCollection props
                = ConfigurationReader.Read(new InputStreamResource(stream, ""), "foo", null);
            Assert.IsNotNull(props, "Failed to read in any properties at all (props is null).");
            Assert.AreEqual(2, props.Count, "Wrong number of properties read in.");
            Assert.AreEqual("kiley", props["rilo"], "Wrong value for second property");
            Assert.AreEqual("lewis", props["jenny"], "Wrong value for second property");
        }

        [Test]
        public void ReadWithNoConfigSectionSectionDefaultsToNameValueSectionHandler()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_ReadWithNoConfigSectionSectionDefaultsToNameValueSectionHandler)).Run();
        }

        private void _ReadWithNoConfigSectionSectionDefaultsToNameValueSectionHandler(out Stream stream)
        {
            const string NoConfigSectionXml = @"<?xml version='1.0' encoding='UTF-8' ?>
<configuration>
	<foo>
		<add key='rilo' value='kiley'/>
		<add key='jenny' value='lewis'/>
	</foo>
</configuration>";
            stream = new MemoryStream(Encoding.UTF8.GetBytes(NoConfigSectionXml));
            NameValueCollection props
                = ConfigurationReader.Read(new InputStreamResource(stream, ""), "foo", null);
            Assert.IsNotNull(props, "Failed to read in any properties at all (props is null).");
            Assert.AreEqual(2, props.Count, "Wrong number of properties read in.");
            Assert.AreEqual("kiley", props["rilo"], "Wrong value for second property");
            Assert.AreEqual("lewis", props["jenny"], "Wrong value for second property");
        }

        [Test]
#if !NET_2_0
        [ExpectedException(typeof(ConfigurationException), "Cannot read properties; config section 'ELNOMBRE' not found.")]
#else
        [ExpectedException(typeof(ConfigurationErrorsException), "Cannot read properties; config section 'ELNOMBRE' not found.")]
#endif        
        public void TryReadFromNonExistantConfigSection()
        {
            new StreamHelperDecorator(new StreamHelperCallback(_TryReadFromNonExistantConfigSection)).Run();
        }

        private void _TryReadFromNonExistantConfigSection(out Stream stream)
        {
            stream = new MemoryStream(Encoding.UTF8.GetBytes(SunnyDayXml));
            ConfigurationReader.Read(new InputStreamResource(stream, ""), "ELNOMBRE", null);
        }
    }
}