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
using System.Reflection;

using NUnit.Framework;

#endregion

namespace Spring.Reflection.Dynamic
{
    /// <summary>
    /// Unit tests for the DynamicProperty class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public class DynamicPropertyTests : BasePropertyTests
    {
        private const BindingFlags BINDANY = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        protected override IDynamicProperty Create(PropertyInfo property)
        {
            return DynamicProperty.Create(property);
        }

        [Test]
        public void TestNonReadableProperties()
        {
            IDynamicProperty nonReadableProperty = Create(typeof(ClassWithNonReadableProperty).GetProperty("MyProperty"));
            Assert.Throws<InvalidOperationException>(() => nonReadableProperty.GetValue(null));
        }

        [Test]
        public void TestNonWritableInstanceProperty()
        {
            IDynamicProperty nonWritableProperty = Create(typeof(Inventor).GetProperty("PlaceOfBirth"));
            Assert.Throws<InvalidOperationException>(() => nonWritableProperty.SetValue(null, null));
        }

        [Test]
        public void TestAttemptingToSetPropertyOfValueTypeInstance()
        {
            MyStruct myYearHolder = new MyStruct();
            IDynamicProperty year = Create(typeof(MyStruct).GetProperty("Year"));
            Assert.Throws<InvalidOperationException>(() => year.SetValue(myYearHolder, 2004));
        }

        [Test]
        public void TestNonWritableStaticProperty()
        {
            IDynamicProperty nonWritableProperty = Create(typeof(DateTime).GetProperty("Today"));
            Assert.Throws<InvalidOperationException>(() => nonWritableProperty.SetValue(null, null));
        }

        [Test]
        public void TestSetIncompatibleType()
        {
            IDynamicProperty inventorPlace = Create(typeof(Inventor).GetProperty("POB"));
            try
            {
                inventorPlace.SetValue(new Inventor(), new object());
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
            }
            try
            {
                inventorPlace.SetValue(new Inventor(), new DateTime());
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
            }

            IDynamicProperty inventorDOB = Create(typeof(Inventor).GetProperty("DOB"));
            try
            {
                inventorDOB.SetValue(new Inventor(), 2);
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
            }
            try
            {
                inventorDOB.SetValue(new Inventor(), new Place());
                Assert.Fail();
            }
            catch (InvalidCastException)
            {
            }
        }
    }
}