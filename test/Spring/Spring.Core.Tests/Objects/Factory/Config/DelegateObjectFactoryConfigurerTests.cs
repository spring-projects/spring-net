#region License

/*
 * Copyright 2002-2010 the original author or authors.
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
using Spring.Context;
using Spring.Context.Support;

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class DelegateObjectFactoryConfigurerIntegrationTests
    {
        private class MockObjectFactoryPostProcessor : IObjectFactoryPostProcessor
        {
            public bool Called;

            public void PostProcessObjectFactory(IConfigurableListableObjectFactory factory)
            {
                Called = true;
            }
        }

        [Test]
        public void ExecutesBeforeObjectFactoryPostProcessing()
        {
            MockObjectFactoryPostProcessor mofp = new MockObjectFactoryPostProcessor();

            IConfigurableApplicationContext ctx = new XmlApplicationContext(false, "name", false, null);
            ctx.AddObjectFactoryPostProcessor(new DelegateObjectFactoryConfigurer(of => of.RegisterSingleton("mofp", mofp)));

            ctx.Refresh();
            Assert.IsTrue(mofp.Called);
        }

        [Test]
        public void CanBeUsedToReconfigureAnApplicationContextOnRefresh()
        {
            MockObjectFactoryPostProcessor mofp = new MockObjectFactoryPostProcessor();

            IConfigurableApplicationContext ctx = new XmlApplicationContext(false, "name", false, null);
            ctx.AddObjectFactoryPostProcessor(new DelegateObjectFactoryConfigurer(of => of.RegisterSingleton("mofp", mofp)));

            ctx.Refresh();

            mofp.Called = false;
            ctx.Refresh();
            Assert.IsTrue(mofp.Called);
        }
    }
}
