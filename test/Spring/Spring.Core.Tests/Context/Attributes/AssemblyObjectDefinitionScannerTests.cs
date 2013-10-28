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

using NUnit.Framework;

using Spring.Objects.Factory.Support;

namespace Spring.Context.Attributes
{
    [TestFixture]
    public class AssemblyObjectDefinitionScannerTests
    {
        [Test]
        public void Can_Create_Custom_Scan_Routine()
        {
            var scanner = new ScanOverridingAssemblyObjectDefinitionScanner();
            var registry = new DefaultListableObjectFactory();
            scanner.ScanAndRegisterTypes(registry);
            Assert.That(registry.ObjectDefinitionCount, Is.EqualTo(1), "found multiple definitions");
            Assert.That(registry.GetObject<ComponentScan.ScanComponentsAndAddToContext.ConfigurationImpl>(), Is.Not.Null,
                "correct single defintion was not registered");
        }

        private class ScanOverridingAssemblyObjectDefinitionScanner : AssemblyObjectDefinitionScanner
        {
            public override IEnumerable<Type> Scan()
            {
                return new Type[] {typeof (ComponentScan.ScanComponentsAndAddToContext.ConfigurationImpl)};
            }
        }
    }
}