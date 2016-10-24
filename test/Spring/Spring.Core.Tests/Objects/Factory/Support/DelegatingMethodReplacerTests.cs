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
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;

#endregion

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Unit tests for the DelegatingMethodReplacer class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class DelegatingMethodReplacerTests
	{
        private MockRepository mocks;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
        }

		[Test]
		public void InstantiationWithNullDefinition()
		{
		    IObjectFactory factory = mocks.StrictMock<IObjectFactory>();
            Assert.Throws<ArgumentNullException>(() => new DelegatingMethodReplacer(null, factory));
		}

	    [Test]
		public void InstantiationWithNullFactory()
		{
            IConfigurableObjectDefinition mock = mocks.StrictMock<IConfigurableObjectDefinition>();
            Assert.Throws<ArgumentNullException>(() => new DelegatingMethodReplacer(mock, null));
		}

		[Test]
		public void SunnyDayPath()
		{
            IObjectFactory mockFactory = mocks.StrictMock<IObjectFactory>();
            IConfigurableObjectDefinition mockDefinition = mocks.StrictMock<IConfigurableObjectDefinition>();
            IMethodReplacer mockReplacer = mocks.StrictMock<IMethodReplacer>();

            
			const string ReplacerObjectName = "replacer";
            Expect.Call(mockFactory.GetObject(ReplacerObjectName)).Return(mockReplacer);

			ReplacedMethodOverride ovr = new ReplacedMethodOverride("SunnyDayPath", ReplacerObjectName);
			MethodOverrides overrides = new MethodOverrides();
			overrides.Add(ovr);
			Expect.Call(mockDefinition.MethodOverrides).Return(overrides);

			Expect.Call(mockReplacer.Implement(null, null, null)).IgnoreArguments().Return(null);
            mocks.ReplayAll();
            
            DelegatingMethodReplacer replacer = new DelegatingMethodReplacer(mockDefinition, mockFactory);
			MethodInfo method = (MethodInfo) MethodBase.GetCurrentMethod();
			replacer.Implement(this, method, new object[] {});

			mocks.VerifyAll();
		}
	}
}