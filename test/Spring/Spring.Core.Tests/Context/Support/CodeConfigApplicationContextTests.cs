#region License

/*
 * Copyright © 2010-2011 the original author or authors.
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
using NUnit.Framework;
using Spring.Context.Attributes;

namespace Spring.Context.Support
{
    [TestFixture]
    public class CodeConfigApplicationContextTests
    {
        private CodeConfigApplicationContext context;
        private AssemblyObjectDefinitionScanner scanner;

        [SetUp]
        public void _TestSetup()
        {
            context = new CodeConfigApplicationContext();
            scanner = new AssemblyObjectDefinitionScanner();
        }

        [Test]
        public void Can_Filter_For_Assembly_Based_On_Assembly_Metadata()
        {
            context.ScanWithAssemblyFilter(a => a.GetName().Name.StartsWith("Spring.Core."));
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context, 45);
        }

        [Test]
        public void Can_Filter_For_Assembly_Containing_Specific_Type_But_Having_NO_Definitions()
        {
            //specifically filter assemblies for one that we *know* will result in NO [Configuration] types in it
            context.ScanWithAssemblyFilter(assy => assy.GetTypes().Any(type => type.FullName.Contains(typeof(Spring.Core.IOrdered).Name)));
           context.Refresh();

            Assert.That(context.DefaultListableObjectFactory.ObjectDefinitionCount, Is.EqualTo(4));
        }

        [Test]
        public void Can_Filter_For_Assembly_Containing_Specific_Type()
        {
            context.ScanWithAssemblyFilter(assy => assy.GetTypes().Any(type => type.FullName.Contains(typeof(MarkerTypeForScannerToFind).Name)));
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context, 45);
        }

        [Test]
        public void Can_Filter_For_Specific_Type()
        {
            context.ScanWithTypeFilter(type => type.FullName.Contains(typeof(TheImportedConfigurationClass).Name));
            context.Refresh();

            Assert.That(context.DefaultListableObjectFactory.ObjectDefinitionCount, Is.EqualTo(8));
        }

        [Test]
        public void Can_Filter_For_Specific_Types_With_Compound_Predicate()
        {
            context.ScanWithTypeFilter(type => type.FullName.Contains(typeof(TheImportedConfigurationClass).Name) || type.FullName.Contains(typeof(TheConfigurationClass).Name));
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context, 19);
        }

        [Test]
        public void Can_Filter_For_Specific_Types_With_Multiple_Include_Filters()
        {
            scanner.WithIncludeFilter(type => type.FullName.Contains(typeof(TheImportedConfigurationClass).Name));
            scanner.WithIncludeFilter(type => type.FullName.Contains(typeof(TheConfigurationClass).Name));

            context.Scan(scanner);
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context, 19);
        }

        [Test]
        public void Can_Perform_Scan_With_No_Filtering()
        {
            context.ScanAllAssemblies();
            context.Refresh();

            AssertExpectedObjectsAreRegisteredWith(context, 45);
        }

        private void AssertExpectedObjectsAreRegisteredWith(GenericApplicationContext context, int expectedDefinitionCount)
        {
            // only check names that are not part of configuration namespace test
            List<string> names = new List<string>(context.DefaultListableObjectFactory.GetObjectDefinitionNames());
            names.RemoveAll(x => x.StartsWith("ConfigurationNameSpace"));


            if (names.Count != expectedDefinitionCount)
            {
                Console.WriteLine("Actual types registered with the container:");
                foreach (var name in names)
                {
                    Console.WriteLine(name);
                }
            }


            Assert.That(names.Count, Is.EqualTo(expectedDefinitionCount));
        }

    }

    //DO NOT DELETE: this empty class req'd by the scanning tests!
    public class MarkerTypeForScannerToFind
    {

    }
}
