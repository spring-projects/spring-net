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
using System.Collections;
using NUnit.Framework;

using Spring.Globalization;

#endregion

namespace Spring.Expressions.Processors
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class ConversionProcessorTests
    {
        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            CultureTestScope.Set();
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            CultureTestScope.Reset();
        }

        [Test]
        public void RequiresTypeArgument()
        {
            ConversionProcessor cp = new ConversionProcessor();
            try
            {
                cp.Process(new object[] {0, 1}, null);
                Assert.Fail("should throw");
            }
            catch (ArgumentNullException)
            {                
            }
        }

        [Test]
        public void ReturnsListAsIsIfNullOrEmpty()
        {
            ConversionProcessor cp = new ConversionProcessor();
            object result;
            result = cp.Process(null, new object[] { typeof(int) });
            Assert.IsNull(result);
            ICollection input = new object[] {};
            result = cp.Process(input, new object[] { typeof(int) });
            Assert.AreEqual( input, result );
        }

        [Test]
        public void ReturnsTypedArray()
        {
            ConversionProcessor cp = new ConversionProcessor();
            object result = cp.Process( new object[] { 0, 1 }, new object[] { typeof(int) });
            Assert.IsTrue( result is int[] );
        }

        [Test]
        public void UsesTypeConverterRegistryForConversion()
        {
            ConversionProcessor cp = new ConversionProcessor();
            ICollection result = (ICollection) cp.Process(new object[] { "0", 1, 1.1m, "1.1", 1.1f }, new object[] { typeof(decimal) });
            decimal sum = 0;
            foreach (decimal element in result) sum += element;
            Assert.AreEqual( 4.3f, sum );
        }
    }
}