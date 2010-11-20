using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.Diagnostics;
using Spring.Context.Config;
using Spring.Context.Support;

namespace Spring.Objects.Factory.Support
{
    [TestFixture]
    public class AssemblyScanningExtensionMethodsTests
    {
        [Test]
        public void Integration_Scenario_With_Defaults()
        {
            GenericApplicationContext context = new GenericApplicationContext();
            context.ScanAssembliesAndRegisterDefinitions();// (assy => assy.GetTypes().Any(type => type.FullName.Contains("SomeType")));
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context);
        }

        [Test]
        public void Integration_Scenario_With_Filtering_Example()
        {
            GenericApplicationContext context = new GenericApplicationContext();
            context.ScanAssembliesAndRegisterDefinitions(assy => assy.GetTypes().Any(type => type.FullName.Contains(typeof(MarkerTypeForScannerToFind).Name)));
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context);
        }

        private static void AssertExpectedObjectsAreRegisteredWith(GenericApplicationContext context)
        {
            Assert.That(context.DefaultListableObjectFactory.ObjectDefinitionCount, Is.EqualTo(13));
        }
        [Test]
        public void Integration_Scenario_With_Complex_Filtering_Example()
        {
            GenericApplicationContext context = new GenericApplicationContext();
            context.ScanAssembliesAndRegisterDefinitions(fn => fn.StartsWith("Spring."), assy => assy.GetTypes().Any(type => type.FullName.Contains(typeof(MarkerTypeForScannerToFind).Name)));
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context);
        }
    }


    public class MarkerTypeForScannerToFind
    {

    }
}
