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

#region Imports

using System.IO;
using System.Xml;
using NUnit.Framework;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class ConfigXmlDocumentTests
    {
        [Test]
        public void LoadXmlStoresTextPosition()
        {
            ConfigXmlDocument xmlDoc = new ConfigXmlDocument();
            string xmlText = TestResourceLoader.GetText( this, "_SampleConfig.xml" );
            xmlDoc.LoadXml( "MYXML", xmlText );
            ITextPosition pos = ((ITextPosition)xmlDoc.SelectSingleNode("//property"));
            Assert.AreEqual("MYXML", pos.Filename);
            Assert.AreEqual(5, pos.LineNumber);
            Assert.AreEqual(14, pos.LinePosition);
        }

        [Test]
        public void LoadReaderStoresTextPosition()
        {
            ConfigXmlDocument xmlDoc = new ConfigXmlDocument();
            Stream istm = TestResourceLoader.GetStream( this, "_SampleConfig.xml" );
            xmlDoc.Load( "MYXML", new XmlTextReader( istm ) );
            ITextPosition pos = ((ITextPosition)xmlDoc.SelectSingleNode("//property"));
            Assert.AreEqual("MYXML", pos.Filename);
            Assert.AreEqual(5, pos.LineNumber);
            Assert.AreEqual(14, pos.LinePosition);
        }

        [Test]
        public void LoadStreamStoresTextPosition()
        {
            ConfigXmlDocument xmlDoc = new ConfigXmlDocument();
            Stream istm = TestResourceLoader.GetStream( this, "_SampleConfig.xml" );
            xmlDoc.Load( "MYXML", istm );
            ITextPosition pos = ((ITextPosition)xmlDoc.SelectSingleNode("//property"));
            Assert.AreEqual("MYXML", pos.Filename);
            Assert.AreEqual(5, pos.LineNumber);
            Assert.AreEqual(14, pos.LinePosition);
        }

        [Test]
        public void LoadTextReaderStoresTextPosition()
        {
            ConfigXmlDocument xmlDoc = new ConfigXmlDocument();
            Stream istm = TestResourceLoader.GetStream( this, "_SampleConfig.xml" );
            xmlDoc.Load( "MYXML", new StreamReader(istm) );
            ITextPosition pos = ((ITextPosition)xmlDoc.SelectSingleNode("//property"));
            Assert.AreEqual("MYXML", pos.Filename);
            Assert.AreEqual(5, pos.LineNumber);
            Assert.AreEqual(14, pos.LinePosition);
        }

        [Test]
        public void CanConfigFilenameAndLine()
        {
            ConfigXmlDocument xmlDoc = new ConfigXmlDocument();
            Stream istm = TestResourceLoader.GetStream( this, "_SampleConfig.xml" );
            xmlDoc.Load( "MYXML", new StreamReader(istm) );
            XmlNode node = xmlDoc.SelectSingleNode("//property");
            Assert.AreEqual("MYXML", ConfigurationUtils.GetFileName(node) );
            Assert.AreEqual(5, ConfigurationUtils.GetLineNumber(node) );
            //Assert.AreEqual(14, pos.LinePosition); <- IConfigErrorInfo/IConfigXmlNode do not support LinePosition
        }
    }
}