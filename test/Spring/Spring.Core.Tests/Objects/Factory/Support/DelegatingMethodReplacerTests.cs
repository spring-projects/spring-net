#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using DotNetMock.Dynamic;
using NUnit.Framework;
using Spring.Objects.Factory.Config;

#endregion

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Unit tests for the DelegatingMethodReplacer class.
	/// </summary>
	/// <author>Rick Evans</author>
	/// <version>$Id: DelegatingMethodReplacerTests.cs,v 1.3 2007/07/30 17:52:33 markpollack Exp $</version>
	[TestFixture]
	public sealed class DelegatingMethodReplacerTests
	{
		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InstantiationWithNullDefinition()
		{
			IDynamicMock mock = new DynamicMock(typeof (IObjectFactory));
			new DelegatingMethodReplacer(null, (IObjectFactory) mock.Object);
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void InstantiationWithNullFactory()
		{
            IDynamicMock mock = new DynamicMock(typeof(IConfigurableObjectDefinition));
            new DelegatingMethodReplacer((IConfigurableObjectDefinition)mock.Object, null);
		}

		[Test]
		public void SunnyDayPath()
		{
			IDynamicMock mockFactory = new DynamicMock(typeof (IObjectFactory));
			IDynamicMock mockDefinition = new DynamicMock(typeof (IConfigurableObjectDefinition));
			IDynamicMock mockReplacer = new DynamicMock(typeof (IMethodReplacer));

			const string ReplacerObjectName = "replacer";
			mockFactory.ExpectAndReturn("GetObject", mockReplacer.Object, ReplacerObjectName);

			ReplacedMethodOverride ovr = new ReplacedMethodOverride("SunnyDayPath", ReplacerObjectName);
			MethodOverrides overrides = new MethodOverrides();
			overrides.Add(ovr);
			mockDefinition.ExpectAndReturn("MethodOverrides", overrides);

			mockReplacer.ExpectAndReturn("Implement", null);
            DelegatingMethodReplacer replacer = new DelegatingMethodReplacer((IConfigurableObjectDefinition)mockDefinition.Object, (IObjectFactory)mockFactory.Object);
			MethodInfo method = (MethodInfo) MethodBase.GetCurrentMethod();
			replacer.Implement(this, method, new object[] {});

			mockFactory.Verify();
			mockDefinition.Verify();
			mockReplacer.Verify();
		}
	}
}