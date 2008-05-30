#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
    /// Unit tests for the SafeProperty class. SafeProperty must pass the same tests 
    /// as DynamicField plus tests for accessing private members.
    /// </summary>
    /// <author>Erich Eichinger</author>
    /// <version>$Id: SafePropertyTests.cs,v 1.1 2008/05/17 11:05:27 oakinger Exp $</version>
    [TestFixture]
    public class SafePropertyTests : BasePropertyTests
    {
        protected override IDynamicProperty Create(PropertyInfo property)
        {
            return new SafeProperty(property);
        }

#if NET_2_0
        [Test]
        public void TestForRestrictiveSetterWithSafeWrapper()
        {
            Something something = new Something();

            IDynamicProperty third = Create(typeof(Something).GetProperty("Third"));
            third.SetValue(something, 456);
            //this should be ok, because both get and set of the "Third" property are public
            Assert.AreEqual(456, third.GetValue(something));

            IDynamicProperty second = Create(typeof(Something).GetProperty("Second"));
            Assert.AreEqual(2, second.GetValue(something));
            //this should not cause MethodAccessException because "second" is created using "CreateSafe"
            second.SetValue(something, 123);
            
            Assert.AreEqual(123, second.GetValue(something));
        }

        [Test]        
        public void TestForRestrictiveGetterWithSafeWrapper()
        {
            Something something = new Something();
            //new SafeProperty()
            IDynamicProperty third = Create(typeof(Something).GetProperty("Third"));
            //this should be ok, because both get and set of the "Third" property are public
            third.SetValue(something, 456);
            Assert.AreEqual(456, third.GetValue(something));
            
            IDynamicProperty first = Create(typeof(Something).GetProperty("First"));
            first.SetValue(something, 123);
            //this should not cause MethodAccessException, "first" is createtd using "CreateSafe"
            Assert.AreEqual(123, first.GetValue(something));
        }	
#endif
    }

    #region Test Classes

    public class MyClassWithPrivateProperties
    {
        public static object s_staticRef;
        public static int s_staticVt;
        public object _ref;
        public int _vt;

        public MyClassWithPrivateProperties(object @ref, int vt)
        {
            _ref = @ref;
            _vt = vt;
        }

        private static object StaticRef
        {
            get { return s_staticRef; }
            set { s_staticRef = value; }
        }

        private static object StaticRefRo
        {
            get { return s_staticRef; }
        }

        private static object StaticRefWo
        {
            set { s_staticRef = value; }
        }

        private static int StaticVt
        {
            get { return s_staticVt; }
            set { s_staticVt = value; }
        }

        private static int StaticVtRo
        {
            get { return s_staticVt; }
        }

        private static int StaticVtWo
        {
            set { s_staticVt = value; }
        }

        private object Ref
        {
            get { return _ref; }
            set { _ref = value; }
        }

        private object RefRo
        {
            get { return _ref; }
        }

        private object RefWo
        {
            set { _ref = value; }
        }

        private int Vt
        {
            get { return _vt; }
            set { _vt = value; }
        }

        private int VtRo
        {
            get { return _vt; }
        }

        private int VtWo
        {
            set { _vt = value; }
        }
    }


    #endregion
}