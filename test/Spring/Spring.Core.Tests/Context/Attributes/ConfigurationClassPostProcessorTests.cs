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
using Spring.Context.Support;
using Spring.Objects.Factory.Support;

namespace Spring.Context.Attributes
{
    [TestFixture]
    public class ConfigurationClassPostProcessorTests : AbstractConfigurationClassPostProcessorTests
    {

        protected override void CreateApplicationContext()
        {
            GenericApplicationContext ctx = new GenericApplicationContext();

            var configDefinitionBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(TheConfigurationClass));
            ctx.RegisterObjectDefinition(configDefinitionBuilder.ObjectDefinition.ObjectTypeName, configDefinitionBuilder.ObjectDefinition);

            var postProcessorDefintionBuilder = ObjectDefinitionBuilder.GenericObjectDefinition(typeof(ConfigurationClassPostProcessor));
            ctx.RegisterObjectDefinition(postProcessorDefintionBuilder.ObjectDefinition.ObjectTypeName, postProcessorDefintionBuilder.ObjectDefinition);

            Assert.That(ctx.ObjectDefinitionCount, Is.EqualTo(2));

            ctx.Refresh();

            _ctx = ctx;
        }


        [Test]
        public void ShouldAllowConfigurationClassInheritance()
        {
            var factory = new DefaultListableObjectFactory();
            factory.RegisterObjectDefinition("DerivedConfiguration", new GenericObjectDefinition
                                                                         {
                                                                             ObjectType = typeof(DerivedConfiguration)
                                                                         });

            var processor = new ConfigurationClassPostProcessor();
            
            processor.PostProcessObjectFactory(factory);

            // we should get singleton instances only
            TestObject testObject = (TestObject) factory.GetObject("DerivedDefinition");
            string singletonParent = (string) factory.GetObject("BaseDefinition");


            Assert.That(testObject.Value, Is.SameAs(singletonParent));
        }
    }

}
