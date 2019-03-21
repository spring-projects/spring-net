#region License

/*
 * Copyright � 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
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
using Spring.Aop;
using Spring.Aop.Framework;
using Spring.Context.Support;
using Spring.Dao.Support;
using Spring.Objects;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Dao.Attributes
{
    /// <summary>
    /// Unit tests for PersistenceExceptionTranslationPostProcessor. Does not test
    /// translation; there are separate unit tests for the Spring AOP Advisor.
    /// Just checks whether proxying occurs correctly,
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Mark Pollack (.NET)</author>
    /// <version>$Id:$</version>
    [TestFixture]
    public class PersistenceExceptionTranslationPostProcessorTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void FailsWithNoPersistenceExceptionTranslators()
        {
            GenericApplicationContext gac = new GenericApplicationContext();
            gac.RegisterObjectDefinition("translator", new RootObjectDefinition(typeof(PersistenceExceptionTranslationPostProcessor)));
            gac.RegisterObjectDefinition("proxied", new RootObjectDefinition(typeof(StereotypedRepositoryInterfaceImpl)));
            try
            {
                gac.Refresh();
                Assert.Fail("Should fail with no translators");
            } catch (ObjectsException)
            {
                // expected.
            }
        }

        [Test]
        public void ProxiesCorrectly()
        {
            GenericApplicationContext gac = new GenericApplicationContext();
            gac.RegisterObjectDefinition("translator", new RootObjectDefinition(typeof(PersistenceExceptionTranslationPostProcessor)));
            gac.RegisterObjectDefinition("notProxied", new RootObjectDefinition(typeof(RepositoryInterfaceImpl)));
            gac.RegisterObjectDefinition("proxied",
                                         new RootObjectDefinition(typeof (StereotypedRepositoryInterfaceImpl)));            
            gac.RegisterObjectDefinition("chainedTranslator",
                                         new RootObjectDefinition(typeof (ChainedPersistenceExceptionTranslator)));
            gac.Refresh();
            
            IRepositoryInterface shouldNotBeProxied = (IRepositoryInterface) gac.GetObject("notProxied");
            Assert.IsFalse(AopUtils.IsAopProxy(shouldNotBeProxied));

            IRepositoryInterface shouldBeProxied = (IRepositoryInterface) gac.GetObject("proxied");
            Assert.IsTrue(AopUtils.IsAopProxy(shouldBeProxied));

            CheckWillTranslateExceptions(shouldBeProxied);

        }

        private void CheckWillTranslateExceptions(object o)
        {
            IAdvised advised = o as IAdvised;
            Assert.IsNotNull(advised);
            foreach (IAdvisor advisor in advised.Advisors)
            {
                PersistenceExceptionTranslationAdvisor peta = advisor as PersistenceExceptionTranslationAdvisor;
                if (peta != null) return;
            }
            Assert.Fail("No translation");
        }
    }
}
