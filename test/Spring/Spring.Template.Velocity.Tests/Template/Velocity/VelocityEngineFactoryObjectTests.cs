#region License

/*
 * Copyright © 2002-2009 the original author or authors.
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

using NUnit.Framework;
using NVelocity.App;
using Spring.Context.Support;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Template.Velocity.Tests.Template.Velocity
{
    /// <summary>
    /// This class contains tests for VelocityEngineFactoryObject
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class VelocityEngineFactoryObjectTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test()
        {
            XmlApplicationContext appContext = new XmlApplicationContext(false,
                                                                         ReadOnlyXmlTestResource.GetFilePath(
                                                                             "VelocityEngineFactoryObjectTests.xml",
                                                                             typeof (VelocityEngineFactoryObjectTests)));
            VelocityEngine velocityEngine = appContext.GetObject("velocityEngine") as VelocityEngine;
            Assert.IsNotNull(velocityEngine);


                 
        }
    }
}