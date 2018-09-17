#region License

/*
 * Copyright 2004 the original author or authors.
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

#region Imports

using System;
using System.Resources;

using NUnit.Framework;

#endregion

namespace Spring.Core.TypeConversion
{
	/// <summary>
	/// Unit tests for the ResourceManagerConverter class.
    /// </summary>
    /// <author>Mark Pollack</author>
	[TestFixture]
    public sealed class ResourceManagerConverterTests
    {
        /// <summary>
        /// Test that we indicate we can convert from strings only.
        /// </summary>
        [Test]
        public void CanConvertFrom () 
        {
            ResourceManagerConverter cvt = new ResourceManagerConverter ();
            Assert.IsTrue (cvt.CanConvertFrom (typeof (string)), "Conversion from a string instance must be supported.");
            Assert.IsFalse (cvt.CanConvertFrom (null));
        }

        /// <summary>
        /// Test sunny day scenario to convert from resource name, assembly name string pair
        /// </summary>
        [Test]
        public void ConvertFrom () 
        {
            
            ResourceManagerConverter cvt = new ResourceManagerConverter();
            object actual = cvt.ConvertFrom ("Spring.TestResource.txt, Spring.Core.Tests");
            Assert.IsNotNull (actual);
            Assert.AreEqual (typeof (ResourceManager), actual.GetType());

        }
        
        /// <summary>
        /// Test passing a null instance and see if expected exception is raised
        /// </summary>
        [Test]
        public void ConvertFromNullReference ()
        {
            ResourceManagerConverter cvt = new ResourceManagerConverter();
            Assert.Throws<NotSupportedException>(() => cvt.ConvertFrom (null));
        }

        /// <summary>
        /// Test passing a single string with no ','
        /// </summary>
        [Test]
        public void ConvertFromSingleString ()
        {
            ResourceManagerConverter cvt = new ResourceManagerConverter();
            Assert.Throws<ArgumentException>(() => cvt.ConvertFrom ("Spring.TestResource.txt"));
        }

        /// <summary>
        /// Test passing a single ','
        /// </summary>
        [Test]
        public void ConvertFromSingleComma ()
        {
            ResourceManagerConverter cvt = new ResourceManagerConverter();
            Assert.Throws<ArgumentException>(() => cvt.ConvertFrom (","));
        }

        /// <summary>
        /// Test passing only assembly name
        /// </summary>
        [Test]
        public void ConvertFromOnlyAssemblyNAme ()
        {
            ResourceManagerConverter cvt = new ResourceManagerConverter();
            Assert.Throws<ArgumentException>(() => cvt.ConvertFrom (",Spring.Core.Tests"));
        }

        /// <summary>
        /// Test passing only assembly name
        /// </summary>
        [Test]
        public void ConvertFromOnlyResourceName()
        {
            ResourceManagerConverter cvt = new ResourceManagerConverter();
            Assert.Throws<ArgumentException>(() => cvt.ConvertFrom ("Spring.TestResource.txt,"));
        }

#if NETFRAMEWORK
        [Test]
        public void ConvertFromBadAssembly()
        {
            ResourceManagerConverter cvt = new ResourceManagerConverter();
            Assert.Throws<ArgumentException>(() => cvt.ConvertFrom ("Spring.TestResource.txt, FooAssembly"));
        }
#endif

        [Test]
        public void ConvertFromBad_App_GlobalResources()
        {
            ResourceManagerConverter cvt = new ResourceManagerConverter();
            Assert.Throws<ArgumentException>(() => cvt.ConvertFrom("Spring.TestResource.txt, "+ResourceManagerConverter.APP_GLOBALRESOURCES_ASSEMBLYNAME));
        }        
	}
}
