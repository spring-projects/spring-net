#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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

namespace Spring.Core.TypeConversion
{
    /// <summary>
    /// This class contains tests for TypeConversionUtils
    /// </summary>
    /// <author>Mark Pollack</author>
    /// <author>Andreas Kluth</author>
    [TestFixture]
    public class TypeConversionUtilsTests
    {
        [Test]
        public void NullAbleTest()
        {
            object o = TypeConversionUtils.ConvertValueIfNecessary(typeof(DateTime?), "", "bla");
            Assert.IsNull(o);
        }

        [Test]
        [SetCulture( "en-US" )]
        public void ConvertValueForDecimalMarkWithPointReturnsValue()
        {
          object o = TypeConversionUtils.ConvertValueIfNecessary( typeof( Double ), "1.2", "foo" );
          Assert.That( o, Is.EqualTo( 1.2 ) );
        }

        [Test]
        [SetCulture( "en-US" )]
        public void ConvertValueForDecimalMarkWithCommaFails()
        {
          TestDelegate testDelegate = () => TypeConversionUtils.ConvertValueIfNecessary( typeof( Double ), "1,2", "foo" );
          Assert.Throws<TypeMismatchException>( testDelegate );
        }

        [Test]
        [SetCulture( "nl-NL" )]
        public void ConvertValueWithDutchCultureForDecimalMarkWithPointReturnsValue()
        {
          object o = TypeConversionUtils.ConvertValueIfNecessary( typeof( Double ), "1.2", "foo" );
          Assert.That( o, Is.EqualTo( 1.2 ) );
        }

        [Test]
        [SetCulture( "nl-NL" )]
        public void ConvertValueWithDutchCultureForDecimalMarkWithCommaReturnsValue()
        {
          object o = TypeConversionUtils.ConvertValueIfNecessary( typeof( Double ), "1,2", "foo" );
          Assert.That( o, Is.EqualTo( 1.2 ) );
        }
    }
}