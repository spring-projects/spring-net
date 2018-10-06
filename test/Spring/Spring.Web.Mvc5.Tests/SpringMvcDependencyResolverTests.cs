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

using System.Linq;
using System.Web.Mvc;

using NUnit.Framework;
using Spring.Context.Support;
using Spring.Web.Mvc.Tests.Controllers;

namespace Spring.Web.Mvc.Tests
{
    [TestFixture]
    public class SpringMvcDependencyResolverTests
    {
        #region Setup/Teardown

        [SetUp]
        public void _TestSetup()
        {
            ContextRegistry.Clear();
            _context = new MvcApplicationContext("file://objectsMvc.xml");
            _mvcNamedContext = new MvcApplicationContext("named", false, "file://namedContextObjectsMvc.xml");

            ContextRegistry.RegisterContext(_context);
            ContextRegistry.RegisterContext(_mvcNamedContext);

            _resolver = new SpringMvcDependencyResolver(_context);

            _resolver.ApplicationContextName = string.Empty;
        }

        #endregion

        private MvcApplicationContext _context;
        private MvcApplicationContext _mvcNamedContext;
        private SpringMvcDependencyResolver _resolver;

        [Test]
        public void CanUseNamedContextToResolveType()
        {
            _resolver.ApplicationContextName = "named";
            var service = _resolver.GetService(typeof (NamedContextController));

            Assert.NotNull(service);
        }

        [Test]
        public void CanUseUnnamedContextToResolveType()
        {
            Assume.That(_resolver.ApplicationContextName, Is.Empty, "Resolver should not have a named-context set for this test!");
            
            var service = _resolver.GetService<FirstContainerRegisteredController>();
            Assert.NotNull(service);
        }

        [Test]
        public void RequestForNullTypeReturnsNull()
        {
            var service = _resolver.GetService(null);
            Assert.That(service, Is.Null);
        }

        [Test]
        public void CanReturnCollectionOfTypes()
        {
            var services = _resolver.GetServices<FirstContainerRegisteredController>();
            Assert.That(services.Count(), Is.EqualTo(2));

            foreach (var service in services)
            {
                Assert.That(service,  Is.TypeOf(typeof(FirstContainerRegisteredController)), "Service Returned was not of the expected Type.");
            }
        }

        [Test]
        public void RequestForSingleInstanceOfMultiplyRegisteredTypeReturnsFirstDefinition()
        {
            var service = _resolver.GetService<FirstContainerRegisteredController>();
            Assert.That(service.TestValue, Is.EqualTo("First Definition of Type"));
        }

        [Test]
        public void RequestForSingleUndefinedTypeReturnsNull()
        {
            var service = _resolver.GetService<NotInContainerController>();
            Assert.That(service, Is.Null);
        }

        [Test]
        public void RequestForCollectionOfUndefinedTypesReturnsEmptyCollection()
        {
            var services = _resolver.GetServices<NotInContainerController>();
            Assert.That(services, Is.Empty);
        }
        
    }
}