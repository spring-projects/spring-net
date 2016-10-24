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
    /// Unit tests for the SafeField class. SafeField must pass the same tests 
    /// as DynamicField plus tests for accessing private members.
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class SafeFieldTests : DynamicFieldTests
    {
        protected override IDynamicField Create(FieldInfo field)
        {
            return new SafeField(field);
        }

        private static FieldInfo IField(Type type, string fieldName)
        {
            return type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        private static FieldInfo SField(Type t, string fieldName)
        {
            return t.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        [Test]
        public void ThrowsOnNullField()
        {
            Assert.Throws<ArgumentNullException>(() => new SafeField(null));
        }

        [Test]
        public void TestStaticMembersOfStruct()
        {
            TestStaticMembersOf(typeof(MyStructWithPrivateFields), MyStructWithPrivateFields.GetStaticReadonlyRefValue());
        }

        [Test]
        public void TestStaticMembersOfClass()
        {
            TestStaticMembersOf(typeof(MyClassWithPrivateFields), MyClassWithPrivateFields.GetStaticReadonlyRefValue());
        }

        [Test]
        public void TestSetIncompatibleType()
        {
            IDynamicField inventorPlace = Create( typeof( Inventor ).GetField( "pob", BINDANY ) );
            try { inventorPlace.SetValue( new Inventor(), new object() ); Assert.Fail(); }
            catch (InvalidCastException) { }
            try { inventorPlace.SetValue( new Inventor(), new DateTime() ); Assert.Fail(); }
            catch (InvalidCastException) { }

            IDynamicField inventorDOB = Create( typeof( Inventor ).GetField( "dob", BINDANY ) );
            try { inventorDOB.SetValue( new Inventor(), 2 ); Assert.Fail(); }
            catch (InvalidCastException) { }
            try { inventorDOB.SetValue( new Inventor(), new Place() ); Assert.Fail(); }
            catch (InvalidCastException) { }                       
        }

        private void TestStaticMembersOf(Type type, object expectedRoRefValue)
        {
            // ro const int
            SafeField constField = new SafeField(SField(type, "constantValue"));
            Assert.AreEqual(5, constField.GetValue(null));
            try { constField.SetValue(null, 3); }
            catch (InvalidOperationException) { }

            // ro static readonly int
            SafeField roVtField = new SafeField(SField(type, "staticReadonlyVTValue"));
            Assert.AreEqual(11, roVtField.GetValue(null));
            try { roVtField.SetValue(null, 10); }
            catch (InvalidOperationException) { }

            // ro static readonly object
            SafeField roRefField = new SafeField(SField(type, "staticReadonlyRefValue"));
            Assert.AreSame(expectedRoRefValue, roRefField.GetValue(null));
            try { roRefField.SetValue(null, new object()); }
            catch (InvalidOperationException) { }

            // rw static int
            SafeField vtField = new SafeField(SField(type, "staticVTValue"));
            vtField.SetValue(null, 10);
            Assert.AreEqual(10, vtField.GetValue(null));

            // rw static object
            SafeField refField = new SafeField(SField(type, "staticRefValue"));
            object o = new object();
            refField.SetValue(null, o);
            Assert.AreSame(o, refField.GetValue(null));
        }

        [Test]
        public void TestInstanceMembersOfStruct()
        {
            object testref1 = new object();
            object testref2 = new object();
            MyStructWithPrivateFields myStruct;

            // ro readonly int
            myStruct = new MyStructWithPrivateFields(123, testref1, 456, testref2);
            SafeField instanceReadonlyVtField = new SafeField(IField(myStruct.GetType(), "instanceReadonlyVTValue"));
            Assert.AreEqual(123, instanceReadonlyVtField.GetValue(myStruct));
            try { instanceReadonlyVtField.SetValue(myStruct, 10); }
            catch (InvalidOperationException) { }

            // ro readonly object
            myStruct = new MyStructWithPrivateFields(123, testref1, 456, testref2);
            SafeField instanceReadonlyRefField = new SafeField(IField(myStruct.GetType(), "instanceReadonlyRefValue"));
            Assert.AreSame(testref1, instanceReadonlyRefField.GetValue(myStruct));
            try { instanceReadonlyRefField.SetValue(myStruct, this); }
            catch (InvalidOperationException) { }

            // ro int
            myStruct = new MyStructWithPrivateFields(123, testref1, 456, testref2);
            SafeField instanceVtField = new SafeField(IField(myStruct.GetType(), "instanceVTValue"));
            Assert.AreEqual(456, instanceVtField.GetValue(myStruct));
            try { instanceVtField.SetValue(myStruct, 10); }
            catch (InvalidOperationException) { }

            // ro object
            myStruct = new MyStructWithPrivateFields(123, testref1, 456, testref2);
            SafeField instanceRefField = new SafeField(IField(myStruct.GetType(), "instanceRefValue"));
            Assert.AreSame(testref2, instanceRefField.GetValue(myStruct));
            try { instanceRefField.SetValue(myStruct, 10); }
            catch (InvalidOperationException) { }
        }

        [Test]
        public void TestInstanceMembersOfClass()
        {
            object testref1 = new object();
            object testref2 = new object();
            MyClassWithPrivateFields myClass;

            // ro readonly int
            myClass = new MyClassWithPrivateFields(123, testref1, 456, testref2);
            SafeField instanceReadonlyVtField = new SafeField(IField(myClass.GetType(), "instanceReadonlyVTValue"));
            Assert.AreEqual(123, instanceReadonlyVtField.GetValue(myClass));
            try { instanceReadonlyVtField.SetValue(myClass, 10); }
            catch (InvalidOperationException) { }

            // ro readonly object
            myClass = new MyClassWithPrivateFields(123, testref1, 456, testref2);
            SafeField instanceReadonlyRefField = new SafeField(IField(myClass.GetType(), "instanceReadonlyRefValue"));
            Assert.AreSame(testref1, instanceReadonlyRefField.GetValue(myClass));
            try { instanceReadonlyRefField.SetValue(myClass, this); }
            catch (InvalidOperationException) { }

            // rw int
            myClass = new MyClassWithPrivateFields(123, testref1, 456, testref2);
            SafeField instanceVtField = new SafeField(IField(myClass.GetType(), "instanceVTValue"));
            Assert.AreEqual(456, instanceVtField.GetValue(myClass));
            instanceVtField.SetValue(myClass, 9182);
            Assert.AreEqual(9182, instanceVtField.GetValue(myClass));

            // rw object
            myClass = new MyClassWithPrivateFields(123, testref1, 456, testref2);
            SafeField instanceRefField = new SafeField(IField(myClass.GetType(), "instanceRefValue"));
            Assert.AreSame(testref2, instanceRefField.GetValue(myClass));
            instanceRefField.SetValue(myClass, testref1);
            Assert.AreSame(testref1, instanceRefField.GetValue(myClass));
        }

        [Test, Explicit]
        public void SafeFieldPerformanceTests()
        {
            int runs = 10000000;
            object myClass = new MyClassWithPrivateFields(123, new object(), 456, new object());
            FieldInfo fieldClassRefValue = IField(myClass.GetType(), "instanceRefValue");
            FieldInfo fieldClassVtValue = IField(myClass.GetType(), "instanceVTValue");

            StopWatch stopWatch = new StopWatch();
            using (stopWatch.Start("Duration Class Set/Get field value: {0}"))
            {
                for(int i=0;i<runs;i++)
                {
                    SafeField instanceRefField = new SafeField(fieldClassRefValue);
                    SafeField instanceVtField = new SafeField(fieldClassVtValue);
            
                    int res = (int) instanceVtField.GetValue( myClass );
                    object ores = instanceRefField.GetValue( myClass );
                }
            }

            object myStruct = new MyStructWithPrivateFields(123, new object(), 456, new object());
            FieldInfo fieldStructRefValue = IField(myStruct.GetType(), "instanceRefValue");
            FieldInfo fieldStructVtValue = IField(myStruct.GetType(), "instanceVTValue");
            
            using (stopWatch.Start("Duration Struct Set/Get field value: {0}"))
            {
                for(int i=0;i<runs;i++)
                {
                    SafeField instanceRefField = new SafeField(fieldStructRefValue);
                    SafeField instanceVtField = new SafeField(fieldStructVtValue);

                    int res = (int) instanceVtField.GetValue( myStruct );
                    object ores = instanceRefField.GetValue( myStruct );
                }
            }

            /* on my machine prints
                
             with System.Reflection.Emit.DynamicMethod generated code:
                Duration Class Set/Get field value: 00:00:03.2031250
                Duration Struct Set/Get field value: 00:00:03.5625000
             
             with standard reflection:
                Duration Class Set/Get field value: 00:00:45.4218750
                Duration Struct Set/Get field value: 00:00:44.5312500
             
             */
        }
    }

    #region Test Classes

    public struct MyStructWithPrivateFields
    {
        // static part
        private static int staticVTValue;
        private static object staticRefValue;
        private static readonly int staticReadonlyVTValue = 11;
        private static readonly object staticReadonlyRefValue = new object();
        private const int constantValue = 5;

        public static object GetStaticReadonlyRefValue() { return staticReadonlyRefValue; }
        public static int GetStaticVTValue() { return staticVTValue; }
        public static object GetStaticRefValue() { return staticRefValue; }

        static MyStructWithPrivateFields()
        {
            staticVTValue = staticReadonlyVTValue + constantValue + 123; // make compiler happy and avoid "unused" warnings
            staticRefValue = new object();
        }

        // instance part
        private int instanceVTValue;
        private readonly int instanceReadonlyVTValue;
        private object instanceRefValue;
        private readonly object instanceReadonlyRefValue;

        public MyStructWithPrivateFields(int roVtValue, object roRefValue, int vtValue, object refValue)
        {
            instanceReadonlyVTValue = roVtValue;
            instanceReadonlyRefValue = roRefValue;
            instanceVTValue = vtValue;
            instanceRefValue = refValue;
        }

        public void SetInstanceVTValue(int val) { instanceVTValue = val; }
        public void SetInstanceRefValue(object val) { instanceRefValue = val; }

    }

    public class MyClassWithPrivateFields
    {
        // static part
        private static int staticVTValue;
        private static object staticRefValue;
        private static readonly int staticReadonlyVTValue = 11;
        private static readonly object staticReadonlyRefValue = new object();
        private const int constantValue = 5;

        public static object GetStaticReadonlyRefValue() { return staticReadonlyRefValue; }
        public static int GetStaticVTValue() { return staticVTValue; }
        public static object GetStaticRefValue() { return staticRefValue; }

        static MyClassWithPrivateFields()
        {
            staticVTValue = staticReadonlyVTValue + constantValue + 654; // make compiler happy and avoid "unused" warnings
            staticRefValue = new object();
        }

        // instance part
        private int instanceVTValue;
        private readonly int instanceReadonlyVTValue;
        private object instanceRefValue;
        private readonly object instanceReadonlyRefValue;

        public MyClassWithPrivateFields(int roVtValue, object roRefValue, int vtValue, object refValue)
        {
            instanceReadonlyVTValue = roVtValue;
            instanceReadonlyRefValue = roRefValue;
            instanceVTValue = vtValue;
            instanceRefValue = refValue;
        }

        public void SetInstanceVTValue(int val) { instanceVTValue = val; }
        public void SetInstanceRefValue(object val) { instanceRefValue = val; }
    }

    #endregion
}
