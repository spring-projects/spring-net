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
using Spring.Core;
using Spring.Util;

namespace Spring.Context.Attributes
{
    [TestFixture]
    public class AssemblyTypeScannerTests
    {
        #region Setup/Teardown

        [SetUp]
        public void _TestSetup()
        {
            _scanner = new AssemblyObjectDefinitionScanner();
        }

        #endregion

        [Test]
        public void AssemblyHavingType_T_Adds_Assembly()
        {
            _scanner.AssemblyHavingType<IOrdered>();
            Assert.That(TypeSources.Any(t => t.Contains(typeof(IOrdered))));
        }

        [Test]
        public void IncludeType_T_Adds_Type()
        {
            _scanner.IncludeType<IOrdered>();
            _scanner.IncludeType<IPriorityOrdered>();

            IncludePredicates.Any(p => p(typeof(IOrdered)));
            IncludePredicates.Any(p => p(typeof(IPriorityOrdered)));
        }

        [Test]
        public void WithExcludeFilter_Excludes_Type()
        {
            //var scanner1 = new AssemblyObjectDefinitionScanner();

            _scanner.IncludeType<TheConfigurationClass>();
            _scanner.IncludeType<TheImportedConfigurationClass>();
            _scanner.WithExcludeFilter(t => t.Name.StartsWith("TheImported"));

            IEnumerable<Type> types = _scanner.Scan();

            //Assert.That(types.Any(t => t.Name == "TheConfigurationClass"));
            //Assert.False(types.Any(t => t.Name == "TheImportedConfigurationClass"));

            Assert.That(types, Contains.Item((typeof(TheConfigurationClass))));
            Assert.False(types.Contains(typeof(TheImportedConfigurationClass)));
        }

        [Test]
        public void WithIncludeFilter_Includes_Types()
        {
            _scanner.WithIncludeFilter(t => t.Name.Contains("ConfigurationClass"));

            var types = _scanner.Scan().ToList();

            Assert.That(types, Contains.Item((typeof(TheConfigurationClass))));
            Assert.That(types, Contains.Item((typeof(TheImportedConfigurationClass))));
            Assert.That(types.Count, Is.EqualTo(2));
        }

        private AssemblyObjectDefinitionScanner _scanner;

        private List<Func<Type, bool>> ExcludePredicates
        {
            get
            {
                //get at the collection of excludePredicates from the private field
                //(yuck!-- test smell, but at least its wrapped up in a neat private property getter!)
                return
                    (List<Func<Type, bool>>)(ReflectionUtils.GetInstanceFieldValue(_scanner, "TypeExclusionPredicates"));
            }
        }

        private List<Func<Type, bool>> IncludePredicates
        {
            get
            {
                //get at the collection of includePredicates from the private field
                //(yuck!-- test smell, but at least its wrapped up in a neat private property getter!)
                return
                    (List<Func<Type, bool>>)(ReflectionUtils.GetInstanceFieldValue(_scanner, "TypeInclusionPredicates"));
            }
        }

        private List<IEnumerable<Type>> TypeSources
        {
            get
            {
                //get at the collection of typeSources from the private field
                //(yuck!-- test smell, but at least its wrapped up in a neat private property getter!)
                return (List<IEnumerable<Type>>)(ReflectionUtils.GetInstanceFieldValue(_scanner, "TypeSources"));
            }
        }
    }
}