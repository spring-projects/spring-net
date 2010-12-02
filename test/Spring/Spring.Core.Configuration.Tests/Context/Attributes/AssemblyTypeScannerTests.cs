using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spring.Util;

namespace Spring.Context.Attributes
{
    [TestFixture]
    public class AssemblyTypeScannerTests
    {

        private class Scanner : AssemblyTypeScanner
        {
            public Scanner(string folderScanPath)
                : base(folderScanPath)
            { }

            public Scanner()
                : base(null)
            { }
        }

        private Scanner _scanner;

        private List<Predicate<Type>> _excludePredicates
        {
            get
            {
                //get at the collection of excludePredicates from the private field
                //(yuck!-- test smell, but at least its wrapped up in a neat private property getter!)
                return (List<Predicate<Type>>)(ReflectionUtils.GetInstanceFieldValue(_scanner, "_excludePredicates"));
            }
        }

        private List<Predicate<Type>> _includePredicates
        {
            get
            {
                //get at the collection of includePredicates from the private field
                //(yuck!-- test smell, but at least its wrapped up in a neat private property getter!)
                return (List<Predicate<Type>>)(ReflectionUtils.GetInstanceFieldValue(_scanner, "_includePredicates"));
            }
        }

        private List<IEnumerable<Type>> _typeSources
        {
            get
            {
                //get at the collection of typeSources from the private field
                //(yuck!-- test smell, but at least its wrapped up in a neat private property getter!)
                return (List<IEnumerable<Type>>)(ReflectionUtils.GetInstanceFieldValue(_scanner, "_typeSources"));
            }
        }

        [SetUp]
        public void _TestSetup()
        {
            _scanner = new Scanner();
        }

        [Test]
        public void AssemblyHavingType_T_Adds_Assembly()
        {
            _scanner.AssemblyHavingType<Spring.Core.IOrdered>();
            Assert.That(_typeSources.Any(t => t.Contains(typeof(Spring.Core.IOrdered))));
        }

        [Test]
        public void IncludeType_T_Adds_Type()
        {
            _scanner.IncludeType<Spring.Core.IOrdered>();
            _scanner.IncludeType<Spring.Core.IPriorityOrdered>();

            _includePredicates.Any(p => p(typeof(Spring.Core.IOrdered)));
            _includePredicates.Any(p => p(typeof(Spring.Core.IPriorityOrdered)));
        }

        [Test]
        public void WithExcludeFilter_Excludes_Type()
        {
            _scanner.IncludeType<TheConfigurationClass>();
            _scanner.IncludeType<TheImportedConfigurationClass>();
            _scanner.WithExcludeFilter(t => t.Name.StartsWith("TheImported"));

            Assert.That(_scanner.Scan(), Contains.Item((typeof(TheConfigurationClass))));
            Assert.False(_scanner.Scan().Contains(typeof(TheImportedConfigurationClass)));
        }

        [Test]
        public void WithIncludeFilter_Includes_Types()
        {
            _scanner.WithIncludeFilter(t => t.Name.Contains("ConfigurationClass"));

            Assert.That(_scanner.Scan(), Contains.Item((typeof(TheConfigurationClass))));
            Assert.That(_scanner.Scan(), Contains.Item((typeof(TheImportedConfigurationClass))));
        }

    }
}
