#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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

using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using Spring.Core.IO;

#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// This class contains tests for setting properties that do not parse easily in other locales, such
    /// as comma delimited strings.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class LocaleTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void LocaleTest()
        {
            CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");

                IResource resource = new ReadOnlyXmlTestResource("locale.xml", GetType());
                XmlObjectFactory xof = new XmlObjectFactory(resource);
                TestObject to = xof.GetObject("jenny") as TestObject;
                Assert.IsNotNull(to);
                DateTime d = new DateTime(2007, 10, 30);
                Assert.AreEqual(d, to.Date);
                Assert.AreEqual(30, to.Size.Height);
                Assert.AreEqual(30, to.Size.Width);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }
        }

        
    }
}