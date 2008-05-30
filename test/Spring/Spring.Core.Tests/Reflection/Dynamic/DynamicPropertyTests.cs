#region License

/*
 * Copyright 2004 the original author or authors.
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
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Spring.Context.Support;

#endregion

namespace Spring.Reflection.Dynamic
{
	/// <summary>
    /// Unit tests for the DynamicProperty class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: DynamicPropertyTests.cs,v 1.3 2008/05/17 11:05:27 oakinger Exp $</version>
	[TestFixture]
    public class DynamicPropertyTests : BasePropertyTests
    {
       protected override IDynamicProperty Create(PropertyInfo property)
        {
            return DynamicProperty.Create(property);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNonReadableProperties()
        {
            IDynamicProperty nonReadableProperty = 
                Create(typeof(ClassWithNonReadableProperty).GetProperty("MyProperty"));
            nonReadableProperty.GetValue(null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNonWritableInstanceProperty()
        {
            IDynamicProperty nonWritableProperty =
                Create(typeof(Inventor).GetProperty("PlaceOfBirth"));
            nonWritableProperty.SetValue(null, null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestAttemptingToSetPropertyOfValueTypeInstance()
        {
            MyStruct myYearHolder = new MyStruct();
            IDynamicProperty year = Create(typeof(MyStruct).GetProperty("Year"));
            year.SetValue(myYearHolder, 2004);
        }
        
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNonWritableStaticProperty()
        {
            IDynamicProperty nonWritableProperty =
                Create(typeof(DateTime).GetProperty("Today"));
            nonWritableProperty.SetValue(null, null);
        }	    
    }
}
