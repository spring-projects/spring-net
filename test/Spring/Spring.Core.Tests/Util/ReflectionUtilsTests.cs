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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using NUnit.Framework;

using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Attributes;

[assembly: InternalsVisibleTo("ReflectionUtils.IsTypeVisible.AssemblyTestName")]

namespace Spring.Util
{
    /// <summary>
    /// Unit tests for the ReflectionUtils class.
    /// </summary>
    [TestFixture]
    public sealed class ReflectionUtilsTests
    {
        #region MapsInterfaceMethodsToImplementation utility classes

        interface IMapsInterfaceMethodInterface
        {
            void SomeMethodA();
            void SomeMethodB();
            object SomeProperty { get; }
        }

        public class MapsInterfaceMethodClass : IMapsInterfaceMethodInterface
        {
            public MethodInfo MethodAInfo;
            public MethodInfo MethodBInfo;
            public MethodInfo PropGetterInfo;

            public MapsInterfaceMethodClass()
            {
                SomeMethodA();
                ((IMapsInterfaceMethodInterface)this).SomeMethodB();
                object o = ((IMapsInterfaceMethodInterface)this).SomeProperty;
            }

            public void SomeMethodA()
            {
                MethodAInfo = (MethodInfo)MethodInfo.GetCurrentMethod();
            }

            void IMapsInterfaceMethodInterface.SomeMethodB()
            {
                MethodBInfo = (MethodInfo)MethodInfo.GetCurrentMethod();
            }

            object IMapsInterfaceMethodInterface.SomeProperty
            {
                get
                {
                    PropGetterInfo = (MethodInfo)MethodInfo.GetCurrentMethod();
                    return null;
                }
            }
        }

        #endregion

        private class DummyException : ApplicationException
        {
            public DummyException()
                : base("dummy message")
            {
            }
        }

        public static void ThrowDummyException()
        {
            throw new DummyException();
        }

        public delegate void VoidAction();

        [Test]
        public void UnwrapsTargetInvocationException()
        {
            if (SystemUtils.MonoRuntime)
            {
#if DEBUG
                // TODO (EE): find solution for Mono
                return;
#endif
            }

            MethodInfo mi = new VoidAction(ThrowDummyException).Method;
            try
            {
                try
                {
                    mi.Invoke(null, null);
                    Assert.Fail();
                }
                catch (TargetInvocationException tie)
                {
                    //                    Console.WriteLine(tie);
                    throw ReflectionUtils.UnwrapTargetInvocationException(tie);
                }
                Assert.Fail();
            }
            catch (DummyException e)
            {
                //                Console.WriteLine(e);
                string[] stackFrames = e.StackTrace.Split('\n');
#if !MONO
                // TODO: mono includes the invoke() call in inner stackframe does not include the outer stackframes - either remove or document it
                string firstFrameMethodName = mi.DeclaringType.FullName + "." + mi.Name;
                AssertStringContains(firstFrameMethodName, stackFrames[0]);
                string lastFrameMethodName = MethodBase.GetCurrentMethod().DeclaringType.FullName + "." + MethodBase.GetCurrentMethod().Name;
                AssertStringContains(lastFrameMethodName, stackFrames[stackFrames.Length - 1]);

#endif
            }
        }

        private void AssertStringContains(string toSearch, string source)
        {
            if (source.IndexOf(toSearch) == -1)
            {
                Assert.Fail("Expected '{0}' contained in source, but not found. Source was {1}", toSearch, source);
            }
        }

        [Test]
        public void MapsInterfaceMethodsToImplementation()
        {
            MapsInterfaceMethodClass instance = new MapsInterfaceMethodClass();
            MethodInfo method = null;

            try
            {
                method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary(null, typeof(MapsInterfaceMethodClass));
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("methodInfo", ex.ParamName);
            }

            try
            {
                method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary((MethodInfo)MethodInfo.GetCurrentMethod(), null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual("implementingType", ex.ParamName);
            }

            try
            {
                // unrelated types
                method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary((MethodInfo)MethodInfo.GetCurrentMethod(), typeof(MapsInterfaceMethodClass));
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("methodInfo and implementingType are unrelated", ex.Message);
            }

            method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary(typeof(MapsInterfaceMethodClass).GetMethod("SomeMethodA"), typeof(MapsInterfaceMethodClass));
            Assert.AreSame(instance.MethodAInfo, method);
            method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary(typeof(IMapsInterfaceMethodInterface).GetMethod("SomeMethodA"), typeof(MapsInterfaceMethodClass));
            Assert.AreSame(instance.MethodAInfo, method);
            method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary(typeof(IMapsInterfaceMethodInterface).GetMethod("SomeMethodB"), typeof(MapsInterfaceMethodClass));
            Assert.AreSame(instance.MethodBInfo, method);
            method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary(typeof(IMapsInterfaceMethodInterface).GetProperty("SomeProperty").GetGetMethod(), typeof(MapsInterfaceMethodClass));
            Assert.AreSame(instance.PropGetterInfo, method);
        }

        #region Helper class for http://jira.springframework.org/browse/SPRNET-992 tests

        public class Foo
        {
            public readonly string a = "";
            public readonly int b = -1;
            public readonly char c = '0';

            public Foo(string a, int b, char c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            public Foo(string a)
            {
                this.a = a;
            }

            public Foo()
            {
            }

            public void MethodWithNullableIntegerArg(int? nullableInteger)
            {
            }
        }

        #endregion

        [Test(Description = "http://jira.springframework.org/browse/SPRNET-992")]
        public void ShouldPickDefaultConstructorWithoutArgs()
        {
            object[] args = new object[] { };
            ConstructorInfo best = ReflectionUtils.GetConstructorByArgumentValues(typeof(Foo).GetConstructors(), null);
            Foo foo = (Foo)best.Invoke(args);

            Assert.AreEqual("", foo.a);
            Assert.AreEqual(-1, foo.b);
            Assert.AreEqual('0', foo.c);
        }

        [Test(Description = "http://jira.springframework.org/browse/SPRNET-992")]
        public void ShouldPickDefaultConstructor()
        {
            object[] args = new object[] { };
            ConstructorInfo best = ReflectionUtils.GetConstructorByArgumentValues(typeof(Foo).GetConstructors(), args);
            Foo foo = (Foo)best.Invoke(args);

            Assert.AreEqual("", foo.a);
            Assert.AreEqual(-1, foo.b);
            Assert.AreEqual('0', foo.c);
        }

        [Test(Description = "http://jira.springframework.org/browse/SPRNET-992")]
        public void ShouldPickSingleArgConstructor()
        {
            object[] args = new object[] { "b" };
            ConstructorInfo best = ReflectionUtils.GetConstructorByArgumentValues(typeof(Foo).GetConstructors(), args);
            Foo foo = (Foo)best.Invoke(args);

            Assert.AreEqual("b", foo.a);
            Assert.AreEqual(-1, foo.b);
            Assert.AreEqual('0', foo.c);
        }

        [Test]
        public void GetParameterTypes()
        {
            Type[] expectedParameterTypes = new Type[] { typeof(string) };
            MethodInfo method = typeof(ReflectionUtilsObject).GetMethod("BadSpanglish");
            Type[] parameterTypes = ReflectionUtils.GetParameterTypes(method);
            Assert.IsTrue(ArrayUtils.AreEqual(expectedParameterTypes, parameterTypes));
        }

        [Test]
        public void GetParameterTypesWithNullMethodInfo()
        {
            Assert.Throws<ArgumentNullException>(() => ReflectionUtils.GetParameterTypes((MethodInfo) null));
        }

        [Test]
        public void GetParameterTypesWithNullParametersArgs()
        {
            Assert.Throws<ArgumentNullException>(() => ReflectionUtils.GetParameterTypes((ParameterInfo[]) null));
        }

        [Test]
        public void GetMatchingMethodsWithNullTypeToFindOn()
        {
            Assert.Throws<ArgumentNullException>(() => ReflectionUtils.GetMatchingMethods(null, new MethodInfo[] { }, true));
        }

        [Test]
        public void GetMatchingMethodsWithNullMethodsToFind()
        {
            Assert.Throws<ArgumentNullException>(() => ReflectionUtils.GetMatchingMethods(GetType(), null, true));
        }

        [Test]
        public void GetMatchingMethodsWithPerfectMatch()
        {
            MethodInfo[] clonesMethods = typeof(ReflectionUtilsObjectClone).GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            MethodInfo[] foundMethods = ReflectionUtils.GetMatchingMethods(typeof(ReflectionUtilsObject), clonesMethods, true);
            Assert.AreEqual(clonesMethods.Length, foundMethods.Length);
        }

        [Test]
        public void GetMatchingMethodsWithBadMatchStrict()
        {
            // lets include a protected method that ain't declared on the ReflectionUtilsObject class...
            MethodInfo[] clonesMethods = typeof(ReflectionUtilsObjectClone).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            Assert.Throws<Exception>(() => ReflectionUtils.GetMatchingMethods(typeof(ReflectionUtilsObject), clonesMethods, true));
        }

        [Test]
        public void GetMatchingMethodsWithBadMatchNotStrict()
        {
            // lets include a protected method that ain't declared on the ReflectionUtilsObject class...
            MethodInfo[] clonesMethods = typeof(ReflectionUtilsObjectClone).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            MethodInfo[] foundMethods = ReflectionUtils.GetMatchingMethods(typeof(ReflectionUtilsObject), clonesMethods, false);
            // obviously is not strict, 'cos we got here without throwing an exception...
            Assert.AreEqual(clonesMethods.Length, foundMethods.Length);
        }

        [Test]
        public void GetMatchingMethodsWithBadReturnTypeMatchStrict()
        {
            // lets include a method that return type is different...
            MethodInfo[] clonesMethods = typeof(ReflectionUtilsObjectBadClone).GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            Assert.Throws<Exception>(() => ReflectionUtils.GetMatchingMethods(typeof(ReflectionUtilsObject), clonesMethods, true));
        }

        [Test]
        public void GetMatchingMethodsWithBadReturnTypeMatchNotStrict()
        {
            // lets include a method that return type is different...
            MethodInfo[] clonesMethods = typeof(ReflectionUtilsObjectBadClone).GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
            MethodInfo[] foundMethods = ReflectionUtils.GetMatchingMethods(typeof(ReflectionUtilsObject), clonesMethods, false);
            // obviously is not strict, 'cos we got here without throwing an exception...
            Assert.AreEqual(clonesMethods.Length, foundMethods.Length);
        }

        [Test]
        public void ParameterTypesMatch()
        {
            MethodInfo method = typeof(ReflectionUtilsObject).GetMethod("Spanglish");
            Type[] types = new Type[] { typeof(string), typeof(object[]) };
            Assert.IsTrue(ReflectionUtils.ParameterTypesMatch(method, types));
        }

        [Test]
        public void ParameterTypesDontMatchWithNonMatchingArgs()
        {
            Type[] types = new Type[] { typeof(string), typeof(object[]) };

            MethodInfo method = typeof(ReflectionUtilsObject).GetMethod("BadSpanglish");
            Assert.IsFalse(ReflectionUtils.ParameterTypesMatch(method, types));
            method = typeof(ReflectionUtilsObject).GetMethod("WickedSpanglish");
            Assert.IsFalse(ReflectionUtils.ParameterTypesMatch(method, types));
        }

        [Test]
        public void GetDefaultValue()
        {
            Assert.IsNull(ReflectionUtils.GetDefaultValue(GetType()));
            Assert.AreEqual(Cuts.Superficial, ReflectionUtils.GetDefaultValue(typeof(Cuts)));
            Assert.AreEqual(false, ReflectionUtils.GetDefaultValue(typeof(bool)));
            Assert.AreEqual(DateTime.MinValue, ReflectionUtils.GetDefaultValue(typeof(DateTime)));
            Assert.AreEqual(Char.MinValue, ReflectionUtils.GetDefaultValue(typeof(char)));
            Assert.AreEqual(0, ReflectionUtils.GetDefaultValue(typeof(long)));
            Assert.AreEqual(0, ReflectionUtils.GetDefaultValue(typeof(int)));
            Assert.AreEqual(0, ReflectionUtils.GetDefaultValue(typeof(short)));
        }

        [Test]
        public void PropertyIsIndexer()
        {
            Assert.IsTrue(ReflectionUtils.PropertyIsIndexer("Item", typeof(TestObject)));
            Assert.IsFalse(ReflectionUtils.PropertyIsIndexer("Item", typeof(SideEffectObject)));
            Assert.IsFalse(ReflectionUtils.PropertyIsIndexer("Count", typeof(SideEffectObject)));
            Assert.IsTrue(ReflectionUtils.PropertyIsIndexer("MyItem", typeof(ObjectWithNonDefaultIndexerName)));
        }

        [Test]
        public void MethodIsOnOneOfTheseInterfaces()
        {
            MethodInfo method = typeof(ReflectionUtilsObject).GetMethod("Spanglish");
            Assert.IsTrue(ReflectionUtils.MethodIsOnOneOfTheseInterfaces(method, new Type[] { typeof(IFoo) }));
        }

        [Test]
        public void MethodIsOnOneOfTheseInterfacesWithNonInterfaceType()
        {
            MethodInfo method = typeof(ReflectionUtilsObject).GetMethod("Spanglish");
            Assert.Throws<ArgumentException>(() => ReflectionUtils.MethodIsOnOneOfTheseInterfaces(method, new Type[] { GetType() }));
        }

        [Test]
        public void MethodIsOnOneOfTheseInterfacesWithNullMethod()
        {
            MethodInfo method = null;
            Assert.Throws<ArgumentNullException>(() => ReflectionUtils.MethodIsOnOneOfTheseInterfaces(method, new Type[] { GetType() }));
        }

        [Test]
        public void MethodIsOnOneOfTheseInterfacesWithNullTypes()
        {
            MethodInfo method = typeof(ReflectionUtilsObject).GetMethod("Spanglish");
            Assert.IsFalse(ReflectionUtils.MethodIsOnOneOfTheseInterfaces(method, null));
        }

        [Test]
        public void MethodIsOnOneOfTheseInterfacesMultiple()
        {
            MethodInfo method = typeof(RequiredTestObject).GetMethod("set_ObjectFactory");
            Assert.IsNotNull(method, "Could not get setter property for ObjectFactory");
            Assert.IsTrue(ReflectionUtils.MethodIsOnOneOfTheseInterfaces(method, new Type[] { typeof(IObjectNameAware), typeof(IObjectFactoryAware) }));
        }


        [Test]
        public void ParameterTypesMatchWithNullArgs()
        {
            Assert.Throws<ArgumentNullException>(() => ReflectionUtils.ParameterTypesMatch(null, null));
        }

        [Test]
        public void GetSignature()
        {
            MethodInfo method = typeof(ReflectionUtilsObject).GetMethod("Spanglish");
            ArrayList list = new ArrayList();
            foreach (ParameterInfo p in method.GetParameters())
            {
                list.Add(p.ParameterType);
            }
            string expected = "Spring.Util.ReflectionUtilsObject::Spanglish(System.String,System.Object[])";
            Type[] pTypes = (Type[])list.ToArray(typeof(Type));
            string actual = ReflectionUtils.GetSignature(method.DeclaringType, method.Name, pTypes);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ToInterfaceArrayFromType()
        {
            Type[] expected = new Type[] { typeof(IFoo), typeof(IBar) };
            IList<Type> actual = ReflectionUtils.ToInterfaceArray(typeof(IBar));
            Assert.AreEqual(expected.Length, actual.Count);
            Assert.AreEqual(expected[0], actual[0]);
            Assert.AreEqual(expected[1], actual[1]);
        }

        [Test]
        public void ToInterfaceArrayFromTypeWithNonInterface()
        {
            Assert.Throws<ArgumentException>(() => ReflectionUtils.ToInterfaceArray(typeof(ExplicitFoo)));
        }

        [Test]
        public void GetMethod()
        {
            MethodInfo actual = ReflectionUtils.GetMethod(
                typeof(ReflectionUtilsObject),
                "Spanglish",
                new Type[] { typeof(string), typeof(object[]) });
            Assert.IsNotNull(actual);
        }

        [Test]
        public void GetMethodWithExplicitInterfaceMethod()
        {
            MethodInfo actual = ReflectionUtils.GetMethod(
                typeof(ExplicitFoo),
                "Spring.Util.IFoo.Spanglish",
                new Type[] { typeof(string), typeof(object[]) });
            Assert.IsNotNull(actual);
        }

        [Test]
        public void GetMethodIsCaseInsensitive()
        {
            MethodInfo actual = ReflectionUtils.GetMethod(
                typeof(ReflectionUtilsObject),
                "spAngLISh",
                new Type[] { typeof(string), typeof(object[]) });
            Assert.IsNotNull(actual, "ReflectionUtils.GetMethod would appear to be case sensitive.");
        }

        [Test]
        public void CreateCustomAttributeForNonAttributeType()
        {
            Assert.Throws<ArgumentException>(() => ReflectionUtils.CreateCustomAttribute(GetType()), "[Spring.Util.ReflectionUtilsTests] does not derive from the [System.Attribute] class.");
        }

        [Test]
        public void CreateCustomAttributeWithNullType()
        {
            Assert.Throws<ArgumentNullException>(() => ReflectionUtils.CreateCustomAttribute((Type) null));
        }

        [Test]
        public void CreateCustomAttributeSunnyDayScenarios()
        {
            CustomAttributeBuilder builder = null;

            builder = ReflectionUtils.CreateCustomAttribute(typeof(MyCustomAttribute));
            Assert.IsNotNull(builder);

            builder = ReflectionUtils.CreateCustomAttribute(typeof(MyCustomAttribute), "Rick");
            Assert.IsNotNull(builder);

            builder = ReflectionUtils.CreateCustomAttribute(typeof(MyCustomAttribute), "Rick");
            Assert.IsNotNull(builder);

            builder = ReflectionUtils.CreateCustomAttribute(new MyCustomAttribute("Rick"));
            Assert.IsNotNull(builder);

            builder = ReflectionUtils.CreateCustomAttribute(typeof(MyCustomAttribute), new MyCustomAttribute("Rick"));
            Assert.IsNotNull(builder);

            builder = ReflectionUtils.CreateCustomAttribute(typeof(MyCustomAttribute), new object[] { "Rick" }, new MyCustomAttribute("Evans"));
            Assert.IsNotNull(builder);

            // TODO : actually emit the attribute and check it...
        }

#if !NETCOREAPP
        [Test]
        public void CreatCustomAttriubtesFromCustomAttributeData()
        {
            Type control = typeof(System.Windows.Forms.Control);
            MethodInfo mi = control.GetMethod("get_Font");
            System.Collections.Generic.IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(mi.ReturnParameter);
            CustomAttributeBuilder builder = null;
            foreach (CustomAttributeData customAttributeData in attributes)
            {
                builder = ReflectionUtils.CreateCustomAttribute(customAttributeData);
                Assert.IsNotNull(builder);
            }
        }
#endif

        [Test]
        public void CreatCustomAttriubtesFromCustomAttributeDataWithSingleEnum()
        {
            MethodInfo mi = typeof(TestClassHavingAttributeWithEnum).GetMethod("SomeMethod");
            System.Collections.Generic.IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(mi);
            CustomAttributeBuilder builder = null;
            foreach (CustomAttributeData customAttributeData in attributes)
            {
                builder = ReflectionUtils.CreateCustomAttribute(customAttributeData);
                Assert.IsNotNull(builder);
            }
        }

        [Test]
        public void CreatCustomAttriubtesFromCustomAttributeDataWithArrayOfEnumsSetOnProperty()
        {
            MethodInfo mi = typeof(TestClassHavingAttributeWithEnumArraySetOnProperty).GetMethod("SomeMethod");
            System.Collections.Generic.IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(mi);
            CustomAttributeBuilder builder = null;
            foreach (CustomAttributeData customAttributeData in attributes)
            {
                builder = ReflectionUtils.CreateCustomAttribute(customAttributeData);
                Assert.IsNotNull(builder);
            }
        }

        [Test]
        public void CreatCustomAttriubtesFromCustomAttributeDataWithArrayOfEnumsSetInConstructor()
        {
            MethodInfo mi = typeof(TestClassHavingAttributeWithEnumArraySetInConstructor).GetMethod("SomeMethod");
            System.Collections.Generic.IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(mi);
            CustomAttributeBuilder builder = null;
            foreach (CustomAttributeData customAttributeData in attributes)
            {
                builder = ReflectionUtils.CreateCustomAttribute(customAttributeData);
                Assert.IsNotNull(builder);
            }
        }

        [Test]
        public void CreatCustomAttriubtesFromCustomAttributeDataWithSimpleTypeSetInConstructor()
        {
            MethodInfo mi = typeof(TestClassHavingAttributeWithSimpleTypeSetInConstructor).GetMethod("SomeMethod");
            System.Collections.Generic.IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(mi);
            CustomAttributeBuilder builder = null;
            foreach (CustomAttributeData customAttributeData in attributes)
            {
                builder = ReflectionUtils.CreateCustomAttribute(customAttributeData);
                Assert.IsNotNull(builder);
            }
        }

        [Test]
        public void CreatCustomAttriubtesFromCustomAttributeDataWithGenericCollectionTypeSetInConstructor()
        {
            MethodInfo mi = typeof(TestClassHavingAttributeWithGenericCollectionTypeSetInConstructor).GetMethod("SomeMethod");
            System.Collections.Generic.IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(mi);
            CustomAttributeBuilder builder = null;
            foreach (CustomAttributeData customAttributeData in attributes)
            {
                builder = ReflectionUtils.CreateCustomAttribute(customAttributeData);
                Assert.IsNotNull(builder);
            }
        }

        internal interface IHaveSomeMethod
        {
            void SomeMethod();
        }

        internal class TestClassHavingAttributeWithEnum : IHaveSomeMethod
        {
            [AttributeWithEnum(SomeProperty = TheTestEnumThing.One)]
            public void SomeMethod()
            {

            }
        }

        internal class TestClassHavingAttributeWithEnumArraySetInConstructor : IHaveSomeMethod
        {

            [AttributeWithEnumArraySetInConstructor(new TheTestEnumThing[] { TheTestEnumThing.One, TheTestEnumThing.Two })]
            public void SomeMethod()
            {

            }
        }

        internal class TestClassHavingAttributeWithSimpleTypeSetInConstructor : IHaveSomeMethod
        {
            [AttributeWithType(typeof(int))]
            public void SomeMethod()
            {

            }
        }

        internal class TestClassHavingAttributeWithGenericCollectionTypeSetInConstructor : IHaveSomeMethod
        {
            [AttributeWithType(typeof(System.Collections.Generic.List<int>))]
            public void SomeMethod()
            {

            }
        }

        internal class TestClassHavingAttributeWithEnumArraySetOnProperty : IHaveSomeMethod
        {
            [AttributeWithEnumArray(SomeProperty = new TheTestEnumThing[] { TheTestEnumThing.One, TheTestEnumThing.Three })]
            public void SomeMethod()
            {

            }
        }

        internal enum TheTestEnumThing
        {
            One, Two, Three
        }

        internal class AttributeWithTypeAttribute : Attribute
        {
            public AttributeWithTypeAttribute(Type T)
            {



            }

        }


        internal class AttributeWithEnumArrayAttribute : Attribute
        {
            private TheTestEnumThing[] _someProperty;
            public TheTestEnumThing[] SomeProperty
            {
                get { return _someProperty; }
                set
                {
                    _someProperty = value;
                }
            }

        }

        internal class AttributeWithEnumArraySetInConstructorAttribute : Attribute
        {
            /// <summary>
            /// Initializes a new instance of the AttributeWithEnumArraySetInConstructorAttribute class.
            /// </summary>
            public AttributeWithEnumArraySetInConstructorAttribute(TheTestEnumThing[] things)
            {

            }

        }

        class AttributeWithEnumAttribute : Attribute
        {
            private TheTestEnumThing _someProperty;
            public TheTestEnumThing SomeProperty
            {
                get { return _someProperty; }
                set
                {
                    _someProperty = value;
                }
            }

        }

#if !NETCOREAPP
        [Test]
        public void CreateCustomAttributeUsingDefaultValuesForTheConstructor()
        {
            CustomAttributeBuilder builder = null;
            builder = ReflectionUtils.CreateCustomAttribute(typeof(AnotherCustomAttribute));
            Assert.IsNotNull(builder);

            AnotherCustomAttribute att
                = (AnotherCustomAttribute)CheckForPresenceOfCustomAttribute(builder, typeof(AnotherCustomAttribute));
            Assert.IsNull(att.Name);
            Assert.AreEqual(0, att.Age);
            Assert.IsFalse(att.HasSwallowedExplosives);
        }

        [Test]
        public void CreateCustomAttributeFromSourceAttribute()
        {
            CustomAttributeBuilder builder = null;
            AnotherCustomAttribute source = new AnotherCustomAttribute("Rick", 30, true);
            builder = ReflectionUtils.CreateCustomAttribute(source);
            Assert.IsNotNull(builder);

            AnotherCustomAttribute att
                = (AnotherCustomAttribute)CheckForPresenceOfCustomAttribute(builder, typeof(AnotherCustomAttribute));
            Assert.AreEqual(source.Name, att.Name);
            Assert.AreEqual(source.Age, att.Age);
            Assert.AreEqual(source.HasSwallowedExplosives, att.HasSwallowedExplosives);
        }

        [Test]
        public void CreateCustomAttributeUsingExplicitValuesForTheConstructor()
        {
            CustomAttributeBuilder builder = null;
            const string expectedName = "Rick";
            const int expectedAge = 30;
            builder = ReflectionUtils.CreateCustomAttribute(typeof(AnotherCustomAttribute), new object[]
				{
					expectedName, expectedAge, true
				});
            Assert.IsNotNull(builder);

            AnotherCustomAttribute att
                = (AnotherCustomAttribute)CheckForPresenceOfCustomAttribute(builder, typeof(AnotherCustomAttribute));
            Assert.AreEqual(expectedName, att.Name);
            Assert.AreEqual(expectedAge, att.Age);
            Assert.IsTrue(att.HasSwallowedExplosives);
        }

        [Test]
        public void CreateCustomAttributeUsingExplicitValuesForTheConstructorAndASourceAttribute()
        {
            CustomAttributeBuilder builder = null;
            const string expectedName = "Rick";
            const int expectedAge = 30;
            AnotherCustomAttribute source = new AnotherCustomAttribute(expectedName, expectedAge, false);
            builder = ReflectionUtils.CreateCustomAttribute(typeof(AnotherCustomAttribute), new object[]
				{
					"Hoop", 2, true
				}, source);
            Assert.IsNotNull(builder);

            AnotherCustomAttribute att
                = (AnotherCustomAttribute)CheckForPresenceOfCustomAttribute(builder, typeof(AnotherCustomAttribute));
            Assert.AreEqual(expectedName, att.Name);
            Assert.AreEqual(expectedAge, att.Age);
            Assert.IsFalse(att.HasSwallowedExplosives);
        }
#endif

        [Test]
        public void HasAtLeastOneMethodWithName()
        {
            Type testType = typeof(ExtendedReflectionUtilsObject);
            // declared method...
            Assert.IsTrue(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "Declared"));
            // case insensitive method...
            Assert.IsTrue(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "deCLAReD"));
            // superclass method...
            Assert.IsTrue(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "Spanglish"));
            // static method...
            Assert.IsTrue(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "Static"));
            // protected method...
            Assert.IsTrue(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "Protected"));
            // non existent method...
            Assert.IsFalse(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "Sponglish"));
            // null type...
            Assert.IsFalse(ReflectionUtils.HasAtLeastOneMethodWithName(null, "Spanglish"));
            // null method name...
            Assert.IsFalse(ReflectionUtils.HasAtLeastOneMethodWithName(testType, null));
            // empty method name...
            Assert.IsFalse(ReflectionUtils.HasAtLeastOneMethodWithName(testType, ""));
            // all nulls...
            Assert.IsFalse(ReflectionUtils.HasAtLeastOneMethodWithName(null, null));
        }

        [Test]
        public void GetTypeOfOrTypeWithNull()
        {
            Assert.Throws<NullReferenceException>(() => ReflectionUtils.TypeOfOrType(null));
        }

        [Test]
        public void GetTypeOfOrType()
        {
            Assert.AreEqual(typeof(string), ReflectionUtils.TypeOfOrType(typeof(string)));
        }

        [Test]
        public void GetTypeOfOrTypeWithNonNullType()
        {
            Assert.AreEqual(typeof(string), ReflectionUtils.TypeOfOrType(string.Empty));
        }

        [Test]
        public void GetTypes()
        {
            Type[] actual = ReflectionUtils.GetTypes(
                new object[] { 1, "I've never been to Taco Bell (sighs).", new ReflectionUtilsObject() });
            Type[] expected = new Type[]
				{
					typeof (int),
					typeof (string),
					typeof (ReflectionUtilsObject),
			};
            Assert.IsTrue(ArrayUtils.AreEqual(expected, actual), "The ReflectionUtils.GetTypes method did not return a correct Type [] array. Yep, that's as helpful as it gets.");
        }

        [Test]
        public void GetTypesWithEmptyArrayArgument()
        {
            Type[] actual = ReflectionUtils.GetTypes(new object[] { });
            Assert.IsNotNull(actual, "The ReflectionUtils.GetTypes method returned null when given an empty array. Must return an empty Type [] array.");
            Assert.IsTrue(actual.Length == 0, "The ReflectionUtils.GetTypes method returned a Type [] that had some elements in it when given an empty array. Must return an empty Type [] array.");
        }

        [Test]
        public void GetTypesWithNullArrayArgument()
        {
            Type[] actual = ReflectionUtils.GetTypes(null);
            Assert.IsNotNull(actual, "The ReflectionUtils.GetTypes method returned null when given null. Must return an empty Type [] array.");
            Assert.IsTrue(actual.Length == 0, "The ReflectionUtils.GetTypes method returned a Type [] that had some elements in it when given null. Must return an empty Type [] array.");
        }

        [Test]
        public void GetDefaultValueWithZeroValueEnumType()
        {
            Assert.Throws<ArgumentException>(() => ReflectionUtils.GetDefaultValue(typeof(EnumWithNoValues)));
        }

        [Test]
        public void GetParameterTypesWithMethodThatHasRefParameters()
        {
            MethodInfo method = GetType().GetMethod("Add");
            Type[] types = ReflectionUtils.GetParameterTypes(method);
            Assert.IsNotNull(types);
            Assert.AreEqual(2, types.Length);
            // first method parameter is byRef, so type name must end in '&'
            Assert.AreEqual("System.Int32&", types[0].FullName);
            Assert.AreEqual("System.Int32", types[1].FullName);
        }

        [Test]
        public void GetMethodByArgumentValuesCanResolveWhenAmbiguousMatchIsOnlyDifferentiatedByParams()
        {
            GetMethodByArgumentValuesTarget.DummyArgumentType[] typedArg = new GetMethodByArgumentValuesTarget.DummyArgumentType[] { };
            GetMethodByArgumentValuesTarget foo = new GetMethodByArgumentValuesTarget(1, typedArg);

            Type type = typeof(GetMethodByArgumentValuesTarget);
            MethodInfo[] candidateMethods = new MethodInfo[]
                {
                    type.GetMethod("ParamOverloadedMethod", new Type[] {typeof(string), typeof(string), typeof(string)})
                    ,type.GetMethod("ParamOverloadedMethod", new Type[] {typeof(string), typeof(string), typeof(bool)})
                    ,type.GetMethod("ParamOverloadedMethod", new Type[] {typeof(string), typeof(string), typeof(string), typeof(string), typeof(object[])})
                };

            // ensure no one changed our test class
            Assert.IsNotNull(candidateMethods[0]);
            Assert.IsNotNull(candidateMethods[1]);
            Assert.IsNotNull(candidateMethods[2]);
            Assert.AreEqual("ThreeStringsOverload", foo.ParamOverloadedMethod(string.Empty, string.Empty, string.Empty));
            Assert.AreEqual("TwoStringsAndABoolOverload", foo.ParamOverloadedMethod(string.Empty, string.Empty, default(bool)));
            Assert.AreEqual("FourStringsAndAParamsCollectionOverload", foo.ParamOverloadedMethod(string.Empty, string.Empty, string.Empty, string.Empty, typedArg));

            MethodInfo resolvedMethod = ReflectionUtils.GetMethodByArgumentValues(candidateMethods, new object[] { string.Empty, string.Empty, string.Empty, string.Empty, typedArg });
            Assert.AreSame(candidateMethods[2], resolvedMethod);
        }


        [Test]
        public void GetMethodByArgumentValuesResolvesToExactMatchIfAvailable()
        {
            GetMethodByArgumentValuesTarget.DummyArgumentType[] typedArg = new GetMethodByArgumentValuesTarget.DummyArgumentType[] { };
            GetMethodByArgumentValuesTarget foo = new GetMethodByArgumentValuesTarget(1, typedArg);

            Type type = typeof(GetMethodByArgumentValuesTarget);
            MethodInfo[] candidateMethods = new MethodInfo[]
                {
                    type.GetMethod("MethodWithSimilarArguments", new Type[] {typeof(int), typeof(ICollection)})
                    , type.GetMethod("MethodWithSimilarArguments", new Type[] {typeof(int), typeof(GetMethodByArgumentValuesTarget.DummyArgumentType[])})
                    , type.GetMethod("MethodWithSimilarArguments", new Type[] {typeof(int), typeof(object[])})
                };

            // ensure no one changed our test class
            Assert.IsNotNull(candidateMethods[0]);
            Assert.IsNotNull(candidateMethods[1]);
            Assert.IsNotNull(candidateMethods[2]);
            Assert.AreEqual("ParamArrayMatch", foo.MethodWithSimilarArguments(1, new object()));
            Assert.AreEqual("ExactMatch", foo.MethodWithSimilarArguments(1, typedArg));
            Assert.AreEqual("AssignableMatch", foo.MethodWithSimilarArguments(1, (ICollection)typedArg));

            MethodInfo resolvedMethod = ReflectionUtils.GetMethodByArgumentValues(candidateMethods, new object[] { 1, typedArg });
            Assert.AreSame(candidateMethods[1], resolvedMethod);
        }

        [Test]
        public void GetMethodByArgumentValuesMatchesNullableArgs()
        {
            GetMethodByArgumentValuesTarget.DummyArgumentType[] typedArg = new GetMethodByArgumentValuesTarget.DummyArgumentType[] { };
            GetMethodByArgumentValuesTarget foo = new GetMethodByArgumentValuesTarget(1, typedArg);

            Type type = typeof(GetMethodByArgumentValuesTarget);
            MethodInfo[] candidateMethods = new MethodInfo[]
                {
                    type.GetMethod("MethodWithSimilarArguments", new Type[] {typeof(int), typeof(ICollection)})
                    , type.GetMethod("MethodWithSimilarArguments", new Type[] {typeof(int), typeof(GetMethodByArgumentValuesTarget.DummyArgumentType[])})
                    , type.GetMethod("MethodWithSimilarArguments", new Type[] {typeof(int), typeof(object[])})
                    , type.GetMethod("MethodWithNullableArgument", new Type[] {typeof(int?)})
                };

            // ensure no one changed our test class
            Assert.IsNotNull(candidateMethods[0]);
            Assert.IsNotNull(candidateMethods[1]);
            Assert.IsNotNull(candidateMethods[2]);
            Assert.IsNotNull(candidateMethods[3]);
            Assert.AreEqual("ParamArrayMatch", foo.MethodWithSimilarArguments(1, new object()));
            Assert.AreEqual("ExactMatch", foo.MethodWithSimilarArguments(1, typedArg));
            Assert.AreEqual("AssignableMatch", foo.MethodWithSimilarArguments(1, (ICollection)typedArg));
            Assert.AreEqual("NullableArgumentMatch", foo.MethodWithNullableArgument(null));

            MethodInfo resolvedMethod = ReflectionUtils.GetMethodByArgumentValues(candidateMethods, new object[] { null });
            Assert.AreSame(candidateMethods[3], resolvedMethod);
        }

        [Test]
        public void GetConstructorByArgumentValuesResolvesToExactMatchIfAvailable()
        {
            GetMethodByArgumentValuesTarget.DummyArgumentType[] typedArg = new GetMethodByArgumentValuesTarget.DummyArgumentType[] { };
            Type type = typeof(GetMethodByArgumentValuesTarget);
            ConstructorInfo[] candidateConstructors = new ConstructorInfo[]
                {
                    type.GetConstructor(new Type[] {typeof(int), typeof(ICollection)})
                    , type.GetConstructor(new Type[] {typeof(int), typeof(GetMethodByArgumentValuesTarget.DummyArgumentType[])})
                    , type.GetConstructor(new Type[] {typeof(int), typeof(object[])})
                };

            // ensure noone changed our test class
            Assert.IsNotNull(candidateConstructors[0]);
            Assert.IsNotNull(candidateConstructors[1]);
            Assert.IsNotNull(candidateConstructors[2]);
            Assert.AreEqual("ParamArrayMatch", new GetMethodByArgumentValuesTarget(1, new object()).SelectedConstructor);
            Assert.AreEqual("ExactMatch", new GetMethodByArgumentValuesTarget(1, typedArg).SelectedConstructor);
            Assert.AreEqual("AssignableMatch", new GetMethodByArgumentValuesTarget(1, (ICollection)typedArg).SelectedConstructor);

            ConstructorInfo resolvedConstructor = ReflectionUtils.GetConstructorByArgumentValues(candidateConstructors, new object[] { 1, typedArg });
            Assert.AreSame(candidateConstructors[1], resolvedConstructor);
        }

        #region GetInterfaces

        [Test]
        public void GetInterfaces()
        {
            Assert.AreEqual(
                typeof(TestObject).GetInterfaces().Length,
                ReflectionUtils.GetInterfaces(typeof(TestObject)).Length);

            Assert.AreEqual(1, ReflectionUtils.GetInterfaces(typeof(IInterface1)).Length);
            Assert.AreEqual(4, ReflectionUtils.GetInterfaces(typeof(IInterface2)).Length);
        }

        public interface IInterface1
        {
        }

        public interface IInterface2 : IInterface3
        {
        }

        public interface IInterface3 : IInterface4, IInterface5
        {
        }

        public interface IInterface4
        {
        }

        public interface IInterface5
        {
        }

        #endregion

        #region IsTypeVisible Tests

        [Test]
        public void IsTypeVisibleWithInternalType()
        {
            Type type = typeof(InternalType);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type));
        }

        [Test]
        public void IsTypeVisibleWithPublicNestedTypeOnInternalType()
        {
            Type type = typeof(InternalType.PublicNestedType);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type));
        }

        [Test]
        public void IsTypeVisibleWithInternalNestedTypeOnInternalType()
        {
            Type type = typeof(InternalType.InternalNestedType);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type));
        }

        [Test]
        public void IsTypeVisibleWithProtectedInternalNestedTypeOnInternalType()
        {
            Type type = typeof(InternalType.ProtectedInternalNestedType);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type));
        }

        [Test]
        public void IsTypeVisibleWithProtectedNestedTypeOnInternalType()
        {
            Type type = typeof(InternalType).GetNestedType("ProtectedNestedType", BindingFlags.NonPublic);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type));
        }

        [Test]
        public void IsTypeVisibleWithPrivateNestedTypeOnInternalType()
        {
            Type type = typeof(InternalType).GetNestedType("PrivateNestedType", BindingFlags.NonPublic);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type));
        }

        [Test]
        public void IsTypeVisibleWithPublicType()
        {
            Type type = typeof(PublicType);
            Assert.IsTrue(ReflectionUtils.IsTypeVisible(type));
        }

        [Test]
        public void IsTypeVisibleWithPublicNestedTypeOnPublicType()
        {
            Type type = typeof(PublicType.PublicNestedType);
            Assert.IsTrue(ReflectionUtils.IsTypeVisible(type));
        }

        [Test]
        public void IsTypeVisibleWithInternalNestedTypeOnPublicType()
        {
            Type type = typeof(PublicType.InternalNestedType);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type));
        }

        [Test]
        public void IsTypeVisibleWithProtectedInternalNestedTypeOnPublicType()
        {
            Type type = typeof(PublicType.ProtectedInternalNestedType);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type));
        }

        [Test]
        public void IsTypeVisibleWithProtectedNestedTypeOnPublicType()
        {
            Type type = typeof(PublicType).GetNestedType("ProtectedNestedType", BindingFlags.NonPublic);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type));
        }

        [Test]
        public void IsTypeVisibleWithPrivateNestedTypeOnPublicType()
        {
            Type type = typeof(PublicType).GetNestedType("PrivateNestedType", BindingFlags.NonPublic);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type));
        }

        private static readonly string FRIENDLY_ASSEMBLY_NAME = "ReflectionUtils.IsTypeVisible.AssemblyTestName";

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithInternalType()
        {
            Type type = typeof(InternalType);
            Assert.IsTrue(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithPublicNestedTypeOnInternalType()
        {
            Type type = typeof(InternalType.PublicNestedType);
            Assert.IsTrue(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithInternalNestedTypeOnInternalType()
        {
            Type type = typeof(InternalType.InternalNestedType);
            Assert.IsTrue(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithProtectedInternalNestedTypeOnInternalType()
        {
            Type type = typeof(InternalType.ProtectedInternalNestedType);
            Assert.IsTrue(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithProtectedNestedTypeOnInternalType()
        {
            Type type = typeof(InternalType).GetNestedType("ProtectedNestedType", BindingFlags.NonPublic);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithPrivateNestedTypeOnInternalType()
        {
            Type type = typeof(InternalType).GetNestedType("PrivateNestedType", BindingFlags.NonPublic);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithPublicType()
        {
            Type type = typeof(PublicType);
            Assert.IsTrue(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithPublicNestedTypeOnPublicType()
        {
            Type type = typeof(PublicType.PublicNestedType);
            Assert.IsTrue(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithInternalNestedTypeOnPublicType()
        {
            Type type = typeof(PublicType.InternalNestedType);
            Assert.IsTrue(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithProtectedInternalNestedTypeOnPublicType()
        {
            Type type = typeof(PublicType.ProtectedInternalNestedType);
            Assert.IsTrue(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithProtectedNestedTypeOnPublicType()
        {
            Type type = typeof(PublicType).GetNestedType("ProtectedNestedType", BindingFlags.NonPublic);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeVisibleFromFriendlyAssemblyWithPrivateNestedTypeOnPublicType()
        {
            Type type = typeof(PublicType).GetNestedType("PrivateNestedType", BindingFlags.NonPublic);
            Assert.IsFalse(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME));
        }

        [Test]
        public void IsTypeNullable_WhenTrue()
        {
            Type type = typeof(int?);
            Assert.That(ReflectionUtils.IsNullableType(type), Is.True);
        }

        [Test]
        public void IsTypeNullable_WhenFalse()
        {
            Type type = typeof(int);
            Assert.That(ReflectionUtils.IsNullableType(type), Is.False);
        }

        [Test]
        public void GetCustomAttributesOnType()
        {
            IList attrs = ReflectionUtils.GetCustomAttributes(typeof(ClassWithAttributes));

#if NETCOREAPP
            Assert.AreEqual(1, attrs.Count);
#else
            Assert.AreEqual(2, attrs.Count);
#endif
        }

        [Test]
        public void GetCustomAttributesOnMethod()
        {
            IList attrs = ReflectionUtils.GetCustomAttributes(typeof(ClassWithAttributes).GetMethod("MethodWithAttributes"));

#if NETCOREAPP
            Assert.AreEqual(1, attrs.Count);
#else
            Assert.AreEqual(2, attrs.Count);
#endif
        }

        #endregion

        #region GetExplicitBaseException

        [Test]
        public void GetExplicitBaseExceptionWithNoInnerException()
        {
            Exception appEx = new ApplicationException();
            Exception ex = ReflectionUtils.GetExplicitBaseException(appEx);

            Assert.AreEqual(ex, appEx);
        }

        [Test]
        public void GetExplicitBaseExceptionWithInnerException()
        {
            Exception dbzEx = new DivideByZeroException();
            Exception appEx = new ApplicationException("Test message", dbzEx);
            Exception ex = ReflectionUtils.GetExplicitBaseException(appEx);

            Assert.AreEqual(ex, dbzEx);
        }

        [Test]
        public void GetExplicitBaseExceptionWithInnerExceptions()
        {
            Exception dbzEx = new DivideByZeroException();
            Exception sEx = new SystemException("Test message", dbzEx);
            Exception appEx = new ApplicationException("Test message", sEx);
            Exception ex = ReflectionUtils.GetExplicitBaseException(appEx);

            Assert.AreEqual(ex, dbzEx);
        }

        [Test]
        public void GetExplicitBaseExceptionWithNullReferenceExceptionAsRootException()
        {
            Exception nrEx = new NullReferenceException();
            Exception appEx = new ApplicationException("Test message", nrEx);
            Exception ex = ReflectionUtils.GetExplicitBaseException(appEx);

            Assert.AreEqual(ex, appEx);
        }

        [Test]
        public void CanGetFriendlyNamesForGenericTypes()
        {
            Type t = typeof(System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, int>>);

            Assert.AreEqual("System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, int>>", ReflectionUtils.GetTypeFriendlyName(t));
        }

        #endregion

        #region Helper Methods

        public int Add(ref int one, int two)
        {
            return one + two;
        }

#if !NETCOREAPP
        private Attribute CheckForPresenceOfCustomAttribute(
            CustomAttributeBuilder attBuilder, Type attType)
        {
            Attribute[] atts = ApplyAndGetCustomAttributes(attBuilder);
            Assert.IsNotNull(atts);
            Assert.IsTrue(atts.Length == 1);
            Attribute att = atts[0];
            Assert.IsNotNull(att);
            Assert.AreEqual(attType, att.GetType(), "Wrong Attribute applied to the class.");
            return att;
        }

        private static Attribute[] ApplyAndGetCustomAttributes(CustomAttributeBuilder attBuilder)
        {
            Type type = BuildTypeWithThisCustomAttribute(attBuilder);
            object[] attributes = type.GetCustomAttributes(true);
            return (Attribute[])ArrayList.Adapter(attributes).ToArray(typeof(Attribute));
        }

        private static Type BuildTypeWithThisCustomAttribute(CustomAttributeBuilder attBuilder)
        {
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "AnAssembly";
            AssemblyBuilder asmBuilder = System.Threading.Thread.GetDomain().DefineDynamicAssembly(
                assemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule("AModule");
            TypeBuilder classBuilder = modBuilder.DefineType("AClass", TypeAttributes.Public);
            classBuilder.SetCustomAttribute(attBuilder);
            return classBuilder.CreateType();
        }
#endif
        #endregion
    }

    #region Simple Helper Classes

    public class PublicType
    {
        public class PublicNestedType
        {
        }

        internal class InternalNestedType
        {
        }

        protected internal class ProtectedInternalNestedType
        {
        }

        protected class ProtectedNestedType
        {
        }

        private class PrivateNestedType
        {
        }
    }

    internal class InternalType
    {
        public class PublicNestedType
        {
        }

        internal class InternalNestedType
        {
        }

        protected internal class ProtectedInternalNestedType
        {
        }

        protected class ProtectedNestedType
        {
        }

        private class PrivateNestedType
        {
        }
    }

    internal enum EnumWithNoValues { }

    internal enum Cuts
    {
        Superficial,
        Deep,
        Severing
    }

    internal interface IBar : IFoo
    {
    }

    internal interface IFoo
    {
        bool Spanglish(string foo, object[] args);
    }

    internal sealed class ExplicitFoo : IFoo
    {
        bool IFoo.Spanglish(string foo, object[] args)
        {
            return false;
        }
    }

    internal class ReflectionUtilsObject : IComparable
    {
        public bool Spanglish(string foo, object[] args)
        {
            return false;
        }

        public bool BadSpanglish(string foo)
        {
            return false;
        }

        public bool WickedSpanglish(string foo, string bar)
        {
            return false;
        }

        /// <summary>
        /// Explicit interface implementation for ReflectionUtils.GetMethod tests
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object obj)
        {
            return 0;
        }
    }

    internal sealed class ExtendedReflectionUtilsObject : ReflectionUtilsObject
    {
        public void Declared(string name)
        {
        }

        public void Protected()
        {
        }

        public static void Static()
        {
        }
    }

    /// <summary>
    /// Exposes methods with the same names as the ReflectionUtilsObject, used in
    /// the tests for the GetMatchingMethods method.
    /// </summary>
    internal class ReflectionUtilsObjectClone
    {
        public bool Spanglish(string foo, object[] args)
        {
            return false;
        }

        public bool BadSpanglish(string foo)
        {
            return false;
        }

        public bool WickedSpanglish(string foo, string bar)
        {
            return false;
        }

        protected void Bingo()
        {
        }
    }

    /// <summary>
    /// Exposes methods with the same names as the ReflectionUtilsObject, used in
    /// the tests for the GetMatchingMethods method.
    /// </summary>
    internal class ReflectionUtilsObjectBadClone
    {
        public bool Spanglish(string foo, object[] args)
        {
            return false;
        }

        public string BadSpanglish(string foo)
        {
            return foo;
        }

        public bool WickedSpanglish(string foo, string bar)
        {
            return false;
        }
    }

    internal class GetMethodByArgumentValuesTarget
    {
        public class DummyArgumentType { }

        public string SelectedConstructor;

        public GetMethodByArgumentValuesTarget(int flag, params object[] values)
        {
            SelectedConstructor = "ParamArrayMatch";
        }

        public GetMethodByArgumentValuesTarget(int flag, DummyArgumentType[] bars)
        {
            SelectedConstructor = "ExactMatch";
        }

        public GetMethodByArgumentValuesTarget(int flag, ICollection bars)
        {
            SelectedConstructor = "AssignableMatch";
        }

        public string MethodWithSimilarArguments(int flags, params object[] args)
        {
            return "ParamArrayMatch";
        }

        public string MethodWithSimilarArguments(int flags, DummyArgumentType[] bars)
        {
            return "ExactMatch";
        }

        public string MethodWithSimilarArguments(int flags, ICollection bar)
        {
            return "AssignableMatch";
        }

        public string MethodWithNullableArgument(int? nullableInteger)
        {
            return "NullableArgumentMatch";
        }

        public string ParamOverloadedMethod(string s1, string s2, string s3)
        {
            return "ThreeStringsOverload";
        }

        public string ParamOverloadedMethod(string s1, string s2, bool b1)
        {
            return "TwoStringsAndABoolOverload";
        }

        public string ParamOverloadedMethod(string s1, string s2, string s3, string s4, params object[] args)
        {
            return "FourStringsAndAParamsCollectionOverload";
        }


    }

    public sealed class MyCustomAttribute : Attribute
    {
        public MyCustomAttribute()
        {
        }

        public MyCustomAttribute(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        private string _name;
    }

    /// <summary>
    /// Used for testing that default values are supplied for the ctor args.
    /// </summary>
    public sealed class AnotherCustomAttribute : Attribute
    {
        public AnotherCustomAttribute(string name, int age, bool hasSwallowedExplosives)
        {
            _name = name;
            _age = age;
            _hasSwallowedExplosives = hasSwallowedExplosives;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Age
        {
            get { return _age; }
            set { _age = value; }
        }

        public bool HasSwallowedExplosives
        {
            get { return _hasSwallowedExplosives; }
            set { _hasSwallowedExplosives = value; }
        }

        private string _name;
        private int _age;
        private bool _hasSwallowedExplosives;
    }

    public sealed class ObjectWithNonDefaultIndexerName
    {
        private string[] favoriteQuotes = new string[10];

        [IndexerName("MyItem")]
        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= favoriteQuotes.Length)
                {
                    throw new ArgumentException("Index out of range");
                }
                return favoriteQuotes[index];
            }

            set
            {
                if (index < 0 || index >= favoriteQuotes.Length)
                {
                    throw new ArgumentException("index is out of range.");
                }
                favoriteQuotes[index] = value;
            }

        }
    }

    [MyCustom]
    [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
    public sealed class ClassWithAttributes
    {
        [MyCustom]
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public void MethodWithAttributes()
        {
        }
    }

    #endregion
}
