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

using System.Text;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Expressions
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class ReferenceNodeTests
    {
        public class MyTestObject
        {
            public object MyField;    
        }

        [Test]
        public void DoesntCallContextRegistryForLocalObjectFactoryReferences()
        {
            string xml = string.Format(
                @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
	<object id='foo' type='{0}'>
        <property name='MyField' expression='@(theObject)' />
    </object>
</objects>"
                            , typeof(MyTestObject).AssemblyQualifiedName
                            );

            XmlObjectFactory of = new XmlObjectFactory(new StringResource(xml, Encoding.UTF8));
            object theObject = new object();
            of.RegisterSingleton("theObject", theObject);

            MyTestObject to = (MyTestObject) of.GetObject("foo");
            Assert.AreSame( theObject, to.MyField );
        }
    }
}