#region License
/*
 * Copyright 2002-2010 the original author or authors.
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
using System.Resources;
using NUnit.Framework;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the ResourceManagerFactoryObject class.
	/// </summary>
	/// <author>Mark Pollack</author>
	[TestFixture]
	public sealed class ResourceManagerFactoryObjectTests
	{
        /// <summary>
        /// Test basic sunny day usage of the ResourceManagerFactoryObject.
        /// </summary>
        [Test]
        public void CreateResourceManager()
        {
            ResourceManagerFactoryObject fac = new ResourceManagerFactoryObject();
            fac.BaseName = "Spring.Resources.SampleResources";
            fac.AssemblyName = "Spring.Core.Tests"; 
            fac.AfterPropertiesSet();
            Assert.AreEqual( typeof(ResourceManager), fac.ObjectType);
            object actual = fac.GetObject();
            Assert.IsNotNull(actual);
            Assert.AreEqual( typeof(ResourceManager), actual.GetType());
            ResourceManager rm = (ResourceManager) actual;
            string message = rm.GetString("message");
            Assert.AreEqual("Hello {0} {1}", message);
		}

		[Test]
		public void MissingBaseName()
		{
			ResourceManagerFactoryObject fac = new ResourceManagerFactoryObject();
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet());
		}

		[Test]
		public void MissingAssemblyName()
		{
			ResourceManagerFactoryObject fac = new ResourceManagerFactoryObject();
			fac.BaseName = "Spring.Resources.SampleResources";
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet());
		}

		[Test]
        public void WithRubbishAssemblyName()
		{
			ResourceManagerFactoryObject fac = new ResourceManagerFactoryObject();
			fac.BaseName = "Spring.Resources.SampleResources";
			fac.AssemblyName = "I'mAJumpedUpPantryBoy";
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet());
		}

		[Test]
		public void ObjectTypeReallyIsResourceManager()
		{
			Assert.AreEqual(typeof (ResourceManager), new ResourceManagerFactoryObject().ObjectType);
		}
	}
}
