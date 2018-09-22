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
	/// Unit tests for the DelegatingMethodReplacer class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class DelegatingMethodReplacerTests
	{
        [SetUp]
        public void SetUp()
        {
        }

		[Test]
		public void InstantiationWithNullDefinition()
		{
		    IObjectFactory factory = A.Fake<IObjectFactory>();
            Assert.Throws<ArgumentNullException>(() => new DelegatingMethodReplacer(null, factory));
		}

	    [Test]
		public void InstantiationWithNullFactory()
		{
            IConfigurableObjectDefinition mock = A.Fake<IConfigurableObjectDefinition>();
            Assert.Throws<ArgumentNullException>(() => new DelegatingMethodReplacer(mock, null));
		}

		[Test]
		public void SunnyDayPath()
		{
            IObjectFactory mockFactory = A.Fake<IObjectFactory>();
            IConfigurableObjectDefinition mockDefinition = A.Fake<IConfigurableObjectDefinition>();
            IMethodReplacer mockReplacer = A.Fake<IMethodReplacer>();


			const string ReplacerObjectName = "replacer";
            A.CallTo(() => mockFactory.GetObject(ReplacerObjectName)).Returns(mockReplacer);

			ReplacedMethodOverride ovr = new ReplacedMethodOverride("SunnyDayPath", ReplacerObjectName);
			MethodOverrides overrides = new MethodOverrides();
			overrides.Add(ovr);
            A.CallTo(() => mockDefinition.MethodOverrides).Returns(overrides);

            A.CallTo(() => mockReplacer.Implement(null, null, null)).WithAnyArguments().Returns(null);

            DelegatingMethodReplacer replacer = new DelegatingMethodReplacer(mockDefinition, mockFactory);
			MethodInfo method = (MethodInfo) MethodBase.GetCurrentMethod();
			replacer.Implement(this, method, new object[] {});
		}
	}
}