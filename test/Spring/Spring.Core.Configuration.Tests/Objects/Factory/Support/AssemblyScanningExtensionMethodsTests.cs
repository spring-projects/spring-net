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
        private GenericApplicationContext _context;

        [SetUp]
        public void _TestSetup()
        {
            _context = new GenericApplicationContext();
        }

        [Test]
        public void Can_Filter_For_Assembly_Based_On_Assembly_Metadata()
        {
            _context.Scan(a => a.GetName().Name.StartsWith("Spring.Core.Configuration."));
            _context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(_context);
        }

        [Test]
        public void Can_Filter_For_Assembly_Containing_Specific_Type_But_Having_NO_Definitions()
        {
            //specifically filter assemblies for one that we *know* will result in NO [Configuration] types in it
            _context.Scan(assy => assy.GetTypes().Any(type => type.FullName.Contains(typeof(Spring.Core.IOrdered).Name)));
            _context.Refresh();

            Assert.That(_context.DefaultListableObjectFactory.ObjectDefinitionCount, Is.EqualTo(0));
        }

        [Test]
        public void Can_Filter_For_Assembly_Containing_Specific_Type()
        {
            _context.Scan(assy => assy.GetTypes().Any(type => type.FullName.Contains(typeof(MarkerTypeForScannerToFind).Name)));
            _context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(_context);
        }

        [Test]
        public void Can_Filter_For_Specific_Type()
        {
            _context.Scan(type => ((Type)type).FullName.Contains(typeof(TheImportedConfigurationClass).Name));
            _context.Refresh();

            Assert.That(_context.DefaultListableObjectFactory.ObjectDefinitionCount, Is.EqualTo(4));
        }

        [Test]
        public void Can_Filter_For_Specific_Types_With_Compound_Predicate()
        {
            _context.Scan(type => ((Type)type).FullName.Contains(typeof(TheImportedConfigurationClass).Name) || ((Type)type).FullName.Contains(typeof(TheConfigurationClass).Name));
            _context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(_context);
        }

        [Test]
        public void Can_Filter_For_Specific_Types_With_Multiple_Include_Filters()
        {
            var scanner = new AssemblyObjectDefinitionScanner();
            scanner.WithIncludeFilter(type => ((Type)type).FullName.Contains(typeof(TheImportedConfigurationClass).Name));
            scanner.WithIncludeFilter(type => ((Type)type).FullName.Contains(typeof(TheConfigurationClass).Name));

            _context.Scan(scanner);
            _context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(_context);
        }

        [Test]
        public void Can_Perform_Scan_With_No_Filtering()
        {
            _context.Scan();
            _context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(_context);
        }

        private void AssertExpectedObjectsAreRegisteredWith(GenericApplicationContext _context)
        {
            Assert.That(_context.DefaultListableObjectFactory.ObjectDefinitionCount, Is.EqualTo(13));
        }

    }

    public class MarkerTypeForScannerToFind
    {

    }
}
