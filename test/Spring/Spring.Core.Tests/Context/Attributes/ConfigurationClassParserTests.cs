#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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

using NUnit.Framework;

using Spring.Context.Attributes;
using Spring.Objects.Factory.Parsing;

namespace Spring.Context.Attributes
{
    [TestFixture]
    public class ConfigurationClassParserTests
    {
        private ConfigurationClassParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new ConfigurationClassParser(new FailFastProblemReporter());
        }

        [Test]
        public void ShouldBeAbleToRegisterSameNamedConfigurationClassesFromDifferentNamespaces()
        {
            _parser.Parse(typeof(ConfigurationNameSpace1.SpringConfiguration), "1");
            _parser.Parse(typeof(ConfigurationNameSpace2.SpringConfiguration), "2");

            Assert.That(_parser.ConfigurationClasses.Count, Is.EqualTo(2), "Did not find two configuration classes");
        }
    }
}

namespace ConfigurationNameSpace1
{
    [Configuration]
    public class SpringConfiguration
    {
        [ObjectDef]
        public virtual string ConfigurationNameSpaceObjectA()
        {
            return "A";
        }
    }
}

namespace ConfigurationNameSpace2
{
    [Configuration]
    public class SpringConfiguration
    {
        [ObjectDef]
        public virtual string ConfigurationNameSpaceObjectB()
        {
            return "B";
        }
    }
}