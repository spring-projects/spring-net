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

using NUnit.Framework;

using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;
using Spring.Objects.Factory.Xml;

namespace Spring.Aop.Framework.Adapter
{
	/// <summary>
	/// TestCase for AdvisorAdapterRegistrationManager mechanism.
	/// </summary>
	/// <author>Dmitriy Kopylenko</author>
	/// <author>Simon White (.NET)</author>
	[Platform("Win")]
	public class AdvisorAdapterRegistrationTests
	{
		[Test]
		public void AdvisorAdapterRegistrationManagerNotPresentInContext()
		{
			string configLocation = ReadOnlyXmlTestResource.GetFilePath("withoutBPPContext.xml", typeof(AdvisorAdapterRegistrationTests));
			IApplicationContext ctx = new XmlApplicationContext(configLocation);
			ITestObject to = (ITestObject) ctx.GetObject("testObject");
			// just invoke any method to see if advice fired
			try
			{
				to.ReturnsThis();
				Assert.Fail("Should throw UnknownAdviceTypeException");
			}
			catch (UnknownAdviceTypeException)
			{
				// expected
				Assert.AreEqual(0, GetAdviceImpl(to).InvocationCounter);
			}
		}

		[Test]
		public void AdvisorAdapterRegistrationManagerPresentInContext()
		{
			string configLocation = ReadOnlyXmlTestResource.GetFilePath("withBPPContext.xml", typeof(AdvisorAdapterRegistrationTests));
			IApplicationContext ctx = new XmlApplicationContext(configLocation);
			ITestObject to = (ITestObject) ctx.GetObject("testObject");
			// just invoke any method to see if advice fired
			try
			{
				to.ReturnsThis();
				Assert.AreEqual(1, GetAdviceImpl(to).InvocationCounter);
			}
			catch (UnknownAdviceTypeException)
			{
				Assert.Fail("Should not throw UnknownAdviceTypeException");
			}
		}

		private SimpleBeforeAdviceImpl GetAdviceImpl(ITestObject to)
		{
			IAdvised advised = (IAdvised) to;
			IAdvisor advisor = advised.Advisors[0];
			return (SimpleBeforeAdviceImpl) advisor.Advice;
		}
	}
}
