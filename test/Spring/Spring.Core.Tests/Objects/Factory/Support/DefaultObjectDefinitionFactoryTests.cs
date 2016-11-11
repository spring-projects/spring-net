#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using NUnit.Framework;

#endregion

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Unit tests for the DefaultObjectDefinitionFactory class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class DefaultObjectDefinitionFactoryTests
	{
		[Test]
		public void CreateRootDefinition()
		{
			DefaultObjectDefinitionFactory factory = new DefaultObjectDefinitionFactory();
			IConfigurableObjectDefinition definition
				= factory.CreateObjectDefinition(
					typeof (TestObject).FullName, null, AppDomain.CurrentDomain);
			Assert.IsNotNull(definition, "CreateObjectDefinition with no parent is returning null (it must never do so).");
			Assert.AreEqual(typeof (TestObject), definition.ObjectType);
			Assert.AreEqual(0, definition.PropertyValues.PropertyValues.Count,
			                "Must not have any property values as none were passed in.");
			Assert.AreEqual(0, definition.ConstructorArgumentValues.ArgumentCount,
			                "Must not have any ctor args as none were passed in.");
		}

		[Test]
		public void CreateChildDefinition()
		{
			DefaultObjectDefinitionFactory factory = new DefaultObjectDefinitionFactory();
			IConfigurableObjectDefinition definition
				= factory.CreateObjectDefinition(
					typeof (TestObject).FullName, "Aimee Mann", AppDomain.CurrentDomain);
			Assert.IsNotNull(definition, "CreateObjectDefinition with no parent is returning null (it must never do so).");
			Assert.AreEqual(typeof (TestObject), definition.ObjectType);
			Assert.AreEqual(0, definition.PropertyValues.PropertyValues.Count,
			                "Must not have any property values as none were passed in.");
			Assert.AreEqual(0, definition.ConstructorArgumentValues.ArgumentCount,
			                "Must not have any ctor args as none were passed in.");
		}

		[Test]
		public void DoesNotResolveTypeNameToFullTypeInstanceIfAppDomainIsNull()
		{
			DefaultObjectDefinitionFactory factory = new DefaultObjectDefinitionFactory();
			IConfigurableObjectDefinition definition
				= factory.CreateObjectDefinition(
					typeof (TestObject).FullName, null, null);
			Assert.IsNotNull(definition, "CreateObjectDefinition with no parent is returning null (it must never do so).");
			Assert.AreEqual(typeof (TestObject).FullName, definition.ObjectTypeName);
			Assert.AreEqual(0, definition.PropertyValues.PropertyValues.Count,
			                "Must not have any property values as none were passed in.");
			Assert.AreEqual(0, definition.ConstructorArgumentValues.ArgumentCount,
			                "Must not have any ctor args as none were passed in.");
		}
	}
}