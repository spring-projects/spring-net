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

using System;
using System.Reflection;

using FakeItEasy;

using NUnit.Framework;

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Unit tests for the LookupMethodReplacer class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class LookupMethodReplacerTests
	{
        [SetUp]
        public void Setup()
        {
        }

		[Test]
		public void InstantiationWithNullDefinition()
		{
		    IObjectFactory objectFactory = A.Fake<IObjectFactory>();
            Assert.Throws<ArgumentNullException>(() => new LookupMethodReplacer(null, objectFactory));
		}

		[Test]
		public void InstantiationWithNullFactory()
		{
		    var configurableObjectDefinition = A.Fake<IConfigurableObjectDefinition>();
            Assert.Throws<ArgumentNullException>(() => new LookupMethodReplacer(configurableObjectDefinition, null));
		}

		[Test]
		public void SunnyDayPath()
		{
            var objectFactory = A.Fake<IObjectFactory>();
            var configurableObjectDefinition = A.Fake<IConfigurableObjectDefinition>();

            object expectedLookup = new object();
			const string LookupObjectName = "foo";

		    A.CallTo(() => objectFactory.GetObject(LookupObjectName)).Returns(expectedLookup);

			LookupMethodOverride ovr = new LookupMethodOverride("SunnyDayPath", LookupObjectName);
			MethodOverrides overrides = new MethodOverrides();
			overrides.Add(ovr);
            A.CallTo(() => configurableObjectDefinition.MethodOverrides).Returns(overrides);

            LookupMethodReplacer replacer = new LookupMethodReplacer(configurableObjectDefinition, objectFactory);
			MethodInfo method = (MethodInfo) MethodBase.GetCurrentMethod();

			object lookup = replacer.Implement(this, method, new object[] {});
			Assert.AreSame(expectedLookup, lookup);
		}
	}
}