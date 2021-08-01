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

#region Imports

using Spring.Aop.Framework;
using Spring.Aop.Interceptor;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;
using Spring.Util;

using NUnit.Framework;
#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Unit tests for RegularExpressionMethodPointcutAdvisorTests.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Simon White (.NET)</author>
	[TestFixture]
	public class RegularExpressionMethodPointcutAdvisorTests
	{
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            SystemUtils.RegisterLoadedAssemblyResolver();
        }

		/// <summary>
		/// Basic use case, a single pattern defined.
		/// </summary>
		[Test]
		public void SinglePattern()
		{
			IObjectFactory iof = new XmlObjectFactory(new ReadOnlyXmlTestResource("RegularExpressionSetterTests.xml", GetType()));
			IPerson advised = (IPerson) iof.GetObject("SettersAdvised");
			// Interceptor behind regexp advisor
			NopInterceptor nop = (NopInterceptor) iof.GetObject("NopInterceptor");
			Assert.AreEqual(0, nop.Count);

			int newAge = 12;
			// Not advised
			advised.Exceptional(null);
			Assert.AreEqual(0, nop.Count);
			advised.SetAge(newAge);
			Assert.AreEqual(newAge, advised.GetAge());
			// Only setter fired
			Assert.AreEqual(1, nop.Count);
		}

		/// <summary>
		/// Multiple patterns defined within a single advisor.
		/// </summary>
		[Test]
		public void MultiplePatterns()
		{
			IObjectFactory iof = new XmlObjectFactory(new ReadOnlyXmlTestResource("RegularExpressionSetterTests.xml", GetType()));
			IPerson advised = (IPerson) iof.GetObject("SettersAndReturnsThisAdvised");

		    // Interceptor behind regexp advisor
			NopInterceptor nop = (NopInterceptor) iof.GetObject("NopInterceptor");
			Assert.AreEqual(0, nop.Count);

			int newAge = 12;
			// Not advised
			advised.Exceptional(null);
			Assert.AreEqual(0, nop.Count);

			// This is proxied
			advised.ReturnsThis();
			Assert.AreEqual(1, nop.Count);

			// Only setter is advised
		    advised.SetAge(newAge);
			Assert.AreEqual(2, nop.Count);

		    Assert.AreEqual(newAge, advised.GetAge());
			Assert.AreEqual(2, nop.Count);
		}

		[Test]
		[Platform("Win")]
		public void Serialization()
		{
            IObjectFactory iof = new XmlObjectFactory(new ReadOnlyXmlTestResource("RegularExpressionSetterTests.xml", GetType()));
			IPerson p = (IPerson) iof.GetObject("SerializableSettersAdvised");
			// Interceptor behind regexp advisor
			NopInterceptor nop = (NopInterceptor) iof.GetObject("NopInterceptor");
			Assert.AreEqual(0, nop.Count);

			int newAge = 12;
			// Not advised
			Assert.AreEqual(0, p.GetAge());
			Assert.AreEqual(0, nop.Count);

			// This is proxied
			p.SetAge(newAge);
			Assert.AreEqual(1, nop.Count);
			p.SetAge(newAge);
			Assert.AreEqual(newAge, p.GetAge());
			// Only setter fired
			Assert.AreEqual(2, nop.Count);

			// Serialize and continue...

#if !NETCOREAPP // deep chains for Type serialization problems, not worth the effort at the moment
			p = (IPerson) SerializationTestUtils.SerializeAndDeserialize(p);
			Assert.AreEqual(newAge, p.GetAge());
#endif
			// Remembers count, but we need to get a new reference to nop...
			nop = (SerializableNopInterceptor) ((IAdvised) p).Advisors[0].Advice;
			Assert.AreEqual(2, nop.Count);
			Assert.AreEqual("SerializableSettersAdvised", p.GetName());
			p.SetAge(newAge + 1);
			Assert.AreEqual(3, nop.Count);
			Assert.AreEqual(newAge + 1, p.GetAge());
		}
	}
}
