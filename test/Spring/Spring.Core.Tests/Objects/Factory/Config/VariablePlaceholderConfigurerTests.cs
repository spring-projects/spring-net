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

using System;
using System.Collections;
using NUnit.Framework;
using Spring.Context.Support;

#endregion

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// This calss contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class VariablePlaceholderConfigurerTests
    {
        private class DictionaryVariableSource : IVariableSource
        {
            private Hashtable variables = new Hashtable();

            public DictionaryVariableSource(params string[] args)
            {
                for(int i=0;i<args.Length;i+=2)
                {
                    variables[args[i]] = args[i+1];
                }
            }

            public string ResolveVariable(string name)
            {
                return (string) variables[name];
            }
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void SunnyDay()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("age", "${maxResults}");
            pvs.Add("name", "${name}");
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            IList variableSources = new ArrayList();
            CommandLineArgsVariableSource vs1 = new CommandLineArgsVariableSource(
                new string[] { "program.exe", "file.txt", "/name:Aleks Seovic", "/framework:Spring.NET" });
            variableSources.Add(vs1);

            ConfigSectionVariableSource vs2 = new ConfigSectionVariableSource();
            vs2.SectionName = "DaoConfiguration";
            variableSources.Add(vs2);


            pvs = new MutablePropertyValues();
            pvs.Add("VariableSources", variableSources);
            
            ac.RegisterSingleton("configurer", typeof(VariablePlaceholderConfigurer), pvs);
            ac.Refresh();

            TestObject tb1 = (TestObject)ac.GetObject("tb1");
            Assert.AreEqual(1000, tb1.Age);
            Assert.AreEqual("Aleks Seovic", tb1.Name);

        }

        [Test]
        public void UsesCustomVariablePrefixSuffix()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("age", "%[maxResults]%");
            pvs.Add("name", "%[name]%");
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            IList variableSources = new ArrayList();
            variableSources.Add( new DictionaryVariableSource( new string[] { "maxResults", "35", "name", "Erich" } ) );


            pvs = new MutablePropertyValues();
            pvs.Add("VariableSources", variableSources);
            pvs.Add("PlaceholderPrefix", "%[");
            pvs.Add("PlaceholderSuffix", "]%");

            ac.RegisterSingleton("configurer", typeof(VariablePlaceholderConfigurer), pvs);
            ac.Refresh();

            TestObject tb1 = (TestObject)ac.GetObject("tb1");
            Assert.AreEqual(35, tb1.Age);
            Assert.AreEqual("Erich", tb1.Name);            
        }  
      
        [Test]
        [Ignore("Does not work yet because IVariableSource cannot differentiate between invalid key and key with a null value")]
        public void WhitespaceHandling()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("name", "${name}");
            pvs.Add("nickname", "${nickname}");
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            IList variableSources = new ArrayList();
            variableSources.Add(new DictionaryVariableSource(new string[] { "name", string.Empty, "nickname", null }));
            pvs = new MutablePropertyValues();
            pvs.Add("VariableSources", variableSources);
            ac.RegisterSingleton("configurer", typeof(VariablePlaceholderConfigurer), pvs);
            ac.Refresh();

            TestObject tb1 = (TestObject)ac.GetObject("tb1");
            Assert.AreEqual(string.Empty, tb1.Name);
            Assert.AreEqual(null, tb1.Nickname);
        }

        [Test]
        public void BailsOnUnresolvableVariable()
        {
            StaticApplicationContext ac = new StaticApplicationContext();

            MutablePropertyValues pvs = new MutablePropertyValues();
            pvs.Add("nickname", "${nickname}");
            ac.RegisterSingleton("tb1", typeof(TestObject), pvs);

            IList variableSources = new ArrayList();
            variableSources.Add(new DictionaryVariableSource(new string[] { }));
            pvs = new MutablePropertyValues();
            pvs.Add("VariableSources", variableSources);
            ac.RegisterSingleton("configurer", typeof(VariablePlaceholderConfigurer), pvs);

            try
            {
                ac.Refresh();
                Assert.Fail("something changed wrt VariablePlaceholder resolution");
            }
            catch (ObjectDefinitionStoreException ex)
            {
                Assert.IsTrue( ex.Message.IndexOf("nickname") > -1 );
            }
        }
    }
}