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
using System;
using System.Text.RegularExpressions;
using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;
using Spring.Util;

#endregion

namespace Spring.Aop.Support
{
	/// <summary>
	/// Unit tests for the SdkRegularExpressionMethodPointcut class.
	/// </summary>
	/// <author>Dmitriy Kopylenko</author>
	/// <author>Rick Evans (.NET)</author>
	[TestFixture]
	public sealed class SdkRegularExpressionMethodPointcutTests : AbstractRegularExpressionMethodPointcutTests
	{
        /// <summary>
        /// The setup logic executed before the execution of this test fixture.
        /// </summary>
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            // enable (null appender) logging, to ensure that the logging code is exercised
            LogManager.Adapter = new NoOpLoggerFactoryAdapter(); 
        }

		/// <summary>
		/// Returns the method pointcut implementation to be tested.
		/// </summary>
		/// <returns>The implementation.</returns>
		protected override AbstractRegularExpressionMethodPointcut GetRegexpMethodPointcut() 
		{
			return new SdkRegularExpressionMethodPointcut();
		}

	    [Test]
	    public void InstantiationViaSerialization()
	    {
	    	SdkRegularExpressionMethodPointcut initial = new SdkRegularExpressionMethodPointcut();
            initial.Pattern = "Foo";
	    	SdkRegularExpressionMethodPointcut pcut = (SdkRegularExpressionMethodPointcut) SerializationTestUtils.SerializeAndDeserialize(initial);
            Assert.IsNotNull(pcut, "Deserialized instance must (obviously) not be null.");
            Assert.AreEqual(initial.Pattern, pcut.Pattern, "Pattern property not deserialized correctly.");
        }

        /// <summary>
        /// This exercises the logger after deserialization.
        /// </summary>
        [Test]
        public void TryMatchesAfterSerialization()
        {
        	SdkRegularExpressionMethodPointcut initial = new SdkRegularExpressionMethodPointcut();
            initial.Pattern = "Foo";
        	SdkRegularExpressionMethodPointcut pcut = (SdkRegularExpressionMethodPointcut) SerializationTestUtils.SerializeAndDeserialize(initial);
            Assert.IsNotNull(pcut, "Deserialized instance must (obviously) not be null.");
            Type type = GetType();
            bool isMatch = pcut.Matches(type.GetMethod("ForMatchingPurposesOnly"), type);
            Assert.IsFalse(isMatch, "Whoops, should not be matching here at all.");
        }

        public void ForMatchingPurposesOnly ()
        {
        }
	    
	    [Test]
        public void MixedPatternsAndDefaultOptions()
	    {
            Type type = GetType();
            SdkRegularExpressionMethodPointcut pcut = new SdkRegularExpressionMethodPointcut();

	        pcut.DefaultOptions = RegexOptions.None;
            pcut.Patterns = new object[] {"forMatching*", new Regex("xyz*", RegexOptions.Compiled)};
            Assert.IsFalse(pcut.Matches(type.GetMethod("ForMatchingPurposesOnly"), type));
            
	        pcut.DefaultOptions = RegexOptions.IgnoreCase;
            pcut.Patterns = new object[] { "forMatching*", new Regex("xyz*", RegexOptions.Compiled) };
            Assert.IsTrue(pcut.Matches(type.GetMethod("ForMatchingPurposesOnly"), type));

	        pcut.DefaultOptions = RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace;
            pcut.Patterns = new object[] { "for matc \nhing*", new Regex("xyz*", RegexOptions.Compiled) };
            Assert.IsTrue(pcut.Matches(type.GetMethod("ForMatchingPurposesOnly"), type));
        }

	    [Test]
	    public void SetPatternToNull()
	    {
        	SdkRegularExpressionMethodPointcut pcut = new SdkRegularExpressionMethodPointcut();
            Assert.Throws<ArgumentNullException>(() => pcut.Pattern = null);
        }

        [Test]
        public void SetPatternsPluralToNull()
        {
        	SdkRegularExpressionMethodPointcut pcut = new SdkRegularExpressionMethodPointcut();
            Assert.Throws<ArgumentNullException>(() => pcut.Patterns = null);
        }

        [Test]
        public void SetPatternsPluralToStringArrayWithNullValue()
        {
        	SdkRegularExpressionMethodPointcut pcut = new SdkRegularExpressionMethodPointcut();
            Assert.Throws<ArgumentNullException>(() => pcut.Patterns = new string[] { null });
        }

	    [Test]
	    public void InstantiationWithSuppliedPattern()
	    {
	    	SdkRegularExpressionMethodPointcut pcut = new SdkRegularExpressionMethodPointcut("Foo");
            Assert.AreEqual("Foo", pcut.Pattern, "Pattern supplied via the ctor was not set.");
	    }
	}
}
