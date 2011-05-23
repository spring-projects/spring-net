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

using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Spring.Context.Support;
using Spring.Web.Mvc.Tests.Controllers;

namespace Spring.Web.Mvc.Tests
{
//    internal class MockContext : HttpContextBase
//    {
//    }

    [TestFixture]
    public class SpringMvcDependencyResolverTests
    {
        #region Setup/Teardown

        [SetUp]
        public void _TestSetup()
        {
            ContextRegistry.Clear();
            _context = new MvcApplicationContext("file://objects.xml");
            _mvcNamedContext = new MvcApplicationContext("named", false, "file://namedContextObjects.xml");

            ContextRegistry.RegisterContext(_context);
            ContextRegistry.RegisterContext(_mvcNamedContext);

            _resolver = new SpringMvcDependencyResolver(_context);

            SpringMvcDependencyResolver.ApplicationContextName = string.Empty;
        }

        #endregion

        private MvcApplicationContext _context;
        private MvcApplicationContext _mvcNamedContext;
        private SpringMvcDependencyResolver _resolver;

        [Test]
        public void CanUseNamedContextToResolveType()
        {
            SpringMvcDependencyResolver.ApplicationContextName = "named";
            var service = _resolver.GetService(typeof (NamedContextController));

            Assert.NotNull(service);
        }

        [Test]
        public void CanUseUnnamedContextToResolveType()
        {
            Assert.That(SpringMvcDependencyResolver.ApplicationContextName, Is.Empty, "PRECONDITION: Resolver has named-context improperly set!");
            
            var service = _resolver.GetService(typeof (FirstContainerRegisteredController));
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
            var services = _resolver.GetServices(typeof(FirstContainerRegisteredController));
            Assert.That(services.Count(), Is.EqualTo(2));
        }

        [Test]
        public void RequestForSingleInstanceOfMultiplyRegisteredTypeReturnsFirstDefinition()
        {
            var service = (FirstContainerRegisteredController)_resolver.GetService(typeof(FirstContainerRegisteredController));
            Assert.That(service.TestValue, Is.EqualTo("First Definition of Type"));
        }

        [Test]
        public void RequestForSingleUndefinedTypeReturnsNull()
        {
            var service = _resolver.GetService(typeof(NotInContainerController));
            Assert.That(service, Is.Null);
        }

        [Test]
        public void RequestForCollectionOfUndefinedTypesReturnsEmptyCollection()
        {
            var services = _resolver.GetServices(typeof(NotInContainerController));
            Assert.That(services, Is.Empty);
        }
        
    }
}