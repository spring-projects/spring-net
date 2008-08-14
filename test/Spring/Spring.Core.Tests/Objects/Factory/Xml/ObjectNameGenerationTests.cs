#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// This class contains tests for the object naming algorithm used when an object name is not specified.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class ObjectNameGenerationTests
    {

        private DefaultListableObjectFactory objectFactory;

        [SetUp]
        public void Setup()
        {
            objectFactory = new DefaultListableObjectFactory();
            XmlObjectDefinitionReader reader = new XmlObjectDefinitionReader(objectFactory);
            reader.LoadObjectDefinitions(new ReadOnlyXmlTestResource("objectNameGeneration.xml", GetType()));
        }

        [Test]
        public void AssignObjectNames()
        {
            string className = typeof (DependenciesObject).FullName;

            string targetName = className + ObjectDefinitionReaderUtils.GENERATED_OBJECT_NAME_SEPARATOR + "0";
            DependenciesObject topLevel1 = (DependenciesObject) objectFactory.GetObject(targetName);
            Assert.IsNotNull(topLevel1);

            targetName = className + ObjectDefinitionReaderUtils.GENERATED_OBJECT_NAME_SEPARATOR + "1";
            DependenciesObject topLevel2 = (DependenciesObject)objectFactory.GetObject(targetName);
            Assert.IsNotNull(topLevel1);

            targetName = className + ObjectDefinitionReaderUtils.GENERATED_OBJECT_NAME_SEPARATOR + "2";
            DependenciesObject topLevel3 = (DependenciesObject)objectFactory.GetObject(targetName);
            Assert.IsNotNull(topLevel1);


            string childClassName = typeof(TestObject).FullName;
            TestObject child1 = (TestObject) topLevel1.Spouse;
            Assert.IsNotNull(child1);
            Assert.IsTrue(child1.ObjectName.IndexOf(childClassName) != -1);

            TestObject child2 = (TestObject)topLevel2.Spouse;
            Assert.IsNotNull(child2);
            Assert.IsTrue(child2.ObjectName.IndexOf(childClassName) != -1);

            TestObject child3 = (TestObject)topLevel3.Spouse;
            Assert.IsNotNull(child3);
            Assert.IsTrue(child3.ObjectName.IndexOf(childClassName) != -1);

            Assert.AreNotEqual(child1.ObjectName, child2.ObjectName);
        }
        
    }
}