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
using NUnit.Framework;
using TIBCO.EMS;

#endregion

namespace Spring.Messaging.Ems.Jndi
{
    /// <summary>
    /// Test for JndiLookupFactoryObject
    /// </summary>
    [TestFixture]
    public class JndiLookupFactoryObjectTests
    {
        [Test]
        [ExpectedException(typeof(ConfigurationException))]
        public void NoJndiName()
        {
            JndiLookupFactoryObject jfo = new JndiLookupFactoryObject();
            jfo.AfterPropertiesSet();

        }
       
        [Test]
        public void LookupWithExpectedTypeAndMatch()
        {
            JndiLookupFactoryObject jfo = new JndiLookupFactoryObject();
            string s = "";
            jfo.JndiLookupContext = new ExpectedLookupContext("foo", s);
            jfo.JndiName = "foo";
            jfo.ExpectedType = typeof (string);
            jfo.AfterPropertiesSet();
            Assert.AreEqual(s, jfo.GetObject());
        }

        [Test]
        public void LookupWithExpectedTypeAndNoMatch()
        {
            JndiLookupFactoryObject jfo = new JndiLookupFactoryObject();
            object o = new object();
            jfo.JndiLookupContext = new ExpectedLookupContext("foo", o);
            jfo.JndiName = "foo";
            jfo.ExpectedType = typeof (string);
            try
            {
                jfo.AfterPropertiesSet();
                Assert.Fail("Should have thrown NamingException");
            } catch (NamingException ex)
            {
                Assert.True(ex.Message.IndexOf("is not assignable to [String]") != -1);                
            }
        }

        [Test]
        public void LookupWithDefaultObjectAndExpectedType()
        {
            JndiLookupFactoryObject jfo = new JndiLookupFactoryObject();
            string s = "";
            jfo.JndiLookupContext = new ExpectedLookupContext("foo", s);
            jfo.JndiName = "myFoo";
            jfo.ExpectedType = typeof (string);
            jfo.DefaultObject = "myString";
            jfo.AfterPropertiesSet();
            Assert.AreEqual("myString", jfo.GetObject());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void LookupWithDefaultObjectAndExpectedTypeNoMatch()
        {
            JndiLookupFactoryObject jfo = new JndiLookupFactoryObject();
            string s = "";
            jfo.JndiLookupContext = new ExpectedLookupContext("foo", s);
            jfo.JndiName = "myFoo";
            jfo.ExpectedType = typeof(string);
            jfo.DefaultObject = true;
            jfo.AfterPropertiesSet();            
        }
    }
}