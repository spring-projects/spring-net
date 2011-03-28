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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Web;
using System.Web.Routing;
using Spring.Core.IO;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;
using System.Web.Mvc;
using Spring.Web.Mvc.Tests.Controllers;
using Spring.Context.Support;
using System.Reflection;

namespace Spring.Web.Mvc.Tests
{
    internal class MockContext : HttpContextBase { }

    [TestFixture]
    public class SpringControllerFactoryTests
    {
        private MvcApplicationContext _context;

        private SpringControllerFactory _factory;

        private MvcApplicationContext _mvcNamedContext;

        [SetUp]
        public void _TestSetup()
        {
            ContextRegistry.Clear();
            _context = new MvcApplicationContext("file://objects.xml");
            _mvcNamedContext = new MvcApplicationContext("named", false, "file://namedContextObjects.xml");

            ContextRegistry.RegisterContext(_context);
            ContextRegistry.RegisterContext(_mvcNamedContext);

            _factory = new SpringControllerFactory();

            //due to ridiculous internal methods in DefaultControllerFactory, have to set the ControllerTypeCache using this extension method
            // see http://stackoverflow.com/questions/727181/asp-net-mvc-system-web-compilation-compilationlock for more info
            _factory.InitializeWithControllerTypes(new[] 
            {
                typeof(FirstContainerRegisteredController),
                typeof(SecondContainerRegisteredController),
                typeof(NotInContainerController),
                typeof(NamedContextController),
            });

            SpringControllerFactory.ApplicationContextName = string.Empty;
        }

        [Test]
        public void CanPreferIdMatchOverTypeMatch()
        {
            IController controller = _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "FirstContainerRegistered");
            Assert.AreEqual("Should_Be_Matched_By_Id", ((FirstContainerRegisteredController)controller).TestValue);
        }

        [Test]
        public void CanRetrieveControllersNotRegisteredWithContainer()
        {
            _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "NotInContainer");
        }

        [Test]
        public void CanRevertToTypeMatchIfIdMatchUnsuccessful()
        {
            MvcApplicationContext context = new MvcApplicationContext("file://objectsMatchByType.xml");

            ContextRegistry.Clear();
            ContextRegistry.RegisterContext(context);

            IController controller = _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "FirstContainerRegistered");

            Assert.AreEqual("Should_Be_Matched_By_Type", ((FirstContainerRegisteredController)controller).TestValue);
        }

        [Test]
        public void CanUseNamedContextToResolveController()
        {
            SpringControllerFactory.ApplicationContextName = "named";
            IController controller = _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "NamedContext");

            Assert.NotNull(controller);
        }

        [Test]
        public void ProperlyResolvesCaseInsensitiveControllerNames()
        {
            IController pascalcaseController = _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "FirstContainerRegistered");
            IController lowercaseController = _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "firstcontainerregistered");

            Assert.AreEqual(typeof(FirstContainerRegisteredController), pascalcaseController.GetType());
            Assert.AreEqual(typeof(FirstContainerRegisteredController), lowercaseController.GetType());
        }

    }
}
