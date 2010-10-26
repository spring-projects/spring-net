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
        private MvcApplicationContext _mvcNamedContext;
        private SpringControllerFactory _factory;

        [SetUp]
        public void _TestSetup()
        {
            ContextRegistry.Clear();
            _context = new MvcApplicationContext("assembly://Spring.Web.Mvc.Tests/Spring.Web.Mvc.Tests/objects.xml");
            _mvcNamedContext = new MvcApplicationContext("named", false, "assembly://Spring.Web.Mvc.Tests/Spring.Web.Mvc.Tests/namedContextObjects.xml");

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
        }

        [Test]
        public void ProperlyResolvesCaseInsensitiveControllerNames()
        {
            IController pascalcaseController = _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "FirstContainerRegistered");
            IController lowercaseController = _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "firstcontainerregistered");

            Assert.AreEqual(typeof(FirstContainerRegisteredController), pascalcaseController.GetType());
            Assert.AreEqual(typeof(FirstContainerRegisteredController), lowercaseController.GetType());
        }

        [Test]
        public void CanPreferIdMatchOverTypeMatch()
        {
            IController controller = _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "FirstContainerRegistered");
            Assert.AreEqual("Should_Be_Matched_By_Id", ((FirstContainerRegisteredController)controller).TestValue);
        }


        [Test]
        public void CanRevertToTypeMatchIfIdMatchUnsuccessful()
        {
            MvcApplicationContext context = new MvcApplicationContext("assembly://Spring.Web.Mvc.Tests/Spring.Web.Mvc.Tests/objectsMatchByType.xml");

            ContextRegistry.Clear();
            ContextRegistry.RegisterContext(context);

            IController controller = _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "FirstContainerRegistered");

            Assert.AreEqual("Should_Be_Matched_By_Type", ((FirstContainerRegisteredController)controller).TestValue);
        }


        [Test]
        public void CanRetrieveControllersNotRegisteredWithContainer()
        {
            _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "NotInContainer");
        }


        [Test]
        public void CanUseNamedContextToResolveController()
        {
            SpringControllerFactory.ApplicationContextName = "named";
            IController controller = _factory.CreateController(new RequestContext(new MockContext(), new RouteData()), "NamedContext");

            Assert.NotNull(controller);
        }
    }
}
