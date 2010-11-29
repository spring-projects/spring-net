using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.Diagnostics;
using Spring.Context.Config;
using Spring.Context.Support;
using Spring.Context.Attributes;

namespace Spring.Objects.Factory.Support
{
    [TestFixture]
    public class AssemblyScanningExtensionMethodsTests
    {
        [Test]
        public void Integration_Scenario_With_Assembly_Filtering()
        {
            GenericApplicationContext context = new GenericApplicationContext();
            context.Scan(a => a.GetName().Name.StartsWith("Spring.Core.Configuration."));
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context);
        }


        [Test]
        //TODO: double check to ensure that this test really SHOULD pass...seems like its finding too wide a collection of assy's to scan... :(
        public void Integration_Scenario_With_Assembly_Filtering_Containing_Specific_Type()
        {
            GenericApplicationContext context = new GenericApplicationContext();
            context.Scan(assy => assy.GetTypes().Any(type => type.FullName.Contains(typeof(MarkerTypeForScannerToFind).Name)));
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context);
        }

        [Test]
        public void Integration_Scenario_With_Type_Filtering()
        {
            GenericApplicationContext context = new GenericApplicationContext();
            context.Scan(type => ((Type)type).FullName.Contains(typeof(TheConfigurationClass).Name));
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context);
        }
        

        [Test]
        public void Integration_Scenario_With_Default_of_No_Filtering()
        {
            GenericApplicationContext context = new GenericApplicationContext();
            context.Scan();
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context);
        }

        private void AssertExpectedObjectsAreRegisteredWith(GenericApplicationContext context)
        {
            Assert.That(context.DefaultListableObjectFactory.ObjectDefinitionCount, Is.EqualTo(13));
        }

    }

    public class MarkerTypeForScannerToFind
    {

    }
}
