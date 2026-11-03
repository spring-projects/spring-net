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

using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Attributes;

[assembly: InternalsVisibleTo("ReflectionUtils.IsTypeVisible.AssemblyTestName")]

namespace Spring.Util;

/// <summary>
/// Unit tests for the ReflectionUtils class.
/// </summary>
[TestFixture]
public sealed class ReflectionUtilsTests
{
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
            ((IMapsInterfaceMethodInterface) this).SomeMethodB();
            object o = ((IMapsInterfaceMethodInterface) this).SomeProperty;
        }

        public void SomeMethodA()
        {
            MethodAInfo = (MethodInfo) MethodInfo.GetCurrentMethod();
        }

        void IMapsInterfaceMethodInterface.SomeMethodB()
        {
            MethodBInfo = (MethodInfo) MethodInfo.GetCurrentMethod();
        }

        object IMapsInterfaceMethodInterface.SomeProperty
        {
            get
            {
                PropGetterInfo = (MethodInfo) MethodInfo.GetCurrentMethod();
                return null;
            }
        }
    }

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
            Assert.Fail($"Expected '{toSearch}' contained in source, but not found. Source was {source}");
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
            Assert.That(ex.ParamName, Is.EqualTo("methodInfo"));
        }

        try
        {
            method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary((MethodInfo) MethodInfo.GetCurrentMethod(), null);
            Assert.Fail();
        }
        catch (ArgumentNullException ex)
        {
            Assert.That(ex.ParamName, Is.EqualTo("implementingType"));
        }

        try
        {
            // unrelated types
            method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary((MethodInfo) MethodInfo.GetCurrentMethod(), typeof(MapsInterfaceMethodClass));
            Assert.Fail();
        }
        catch (ArgumentException ex)
        {
            Assert.That(ex.Message, Is.EqualTo("methodInfo and implementingType are unrelated"));
        }

        method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary(typeof(MapsInterfaceMethodClass).GetMethod("SomeMethodA"), typeof(MapsInterfaceMethodClass));
        Assert.That(method, Is.SameAs(instance.MethodAInfo));
        method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary(typeof(IMapsInterfaceMethodInterface).GetMethod("SomeMethodA"), typeof(MapsInterfaceMethodClass));
        Assert.That(method, Is.SameAs(instance.MethodAInfo));
        method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary(typeof(IMapsInterfaceMethodInterface).GetMethod("SomeMethodB"), typeof(MapsInterfaceMethodClass));
        Assert.That(method, Is.SameAs(instance.MethodBInfo));
        method = ReflectionUtils.MapInterfaceMethodToImplementationIfNecessary(typeof(IMapsInterfaceMethodInterface).GetProperty("SomeProperty").GetGetMethod(), typeof(MapsInterfaceMethodClass));
        Assert.That(method, Is.SameAs(instance.PropGetterInfo));
    }

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

    [Test(Description = "http://jira.springframework.org/browse/SPRNET-992")]
    public void ShouldPickDefaultConstructorWithoutArgs()
    {
        object[] args = new object[] { };
        ConstructorInfo best = ReflectionUtils.GetConstructorByArgumentValues(typeof(Foo).GetConstructors(), null);
        Foo foo = (Foo) best.Invoke(args);

        Assert.That(foo.a, Is.Empty);
        Assert.That(foo.b, Is.EqualTo(-1));
        Assert.That(foo.c, Is.EqualTo('0'));
    }

    [Test(Description = "http://jira.springframework.org/browse/SPRNET-992")]
    public void ShouldPickDefaultConstructor()
    {
        object[] args = new object[] { };
        ConstructorInfo best = ReflectionUtils.GetConstructorByArgumentValues(typeof(Foo).GetConstructors(), args);
        Foo foo = (Foo) best.Invoke(args);

        Assert.That(foo.a, Is.Empty);
        Assert.That(foo.b, Is.EqualTo(-1));
        Assert.That(foo.c, Is.EqualTo('0'));
    }

    [Test(Description = "http://jira.springframework.org/browse/SPRNET-992")]
    public void ShouldPickSingleArgConstructor()
    {
        object[] args = new object[] { "b" };
        ConstructorInfo best = ReflectionUtils.GetConstructorByArgumentValues(typeof(Foo).GetConstructors(), args);
        Foo foo = (Foo) best.Invoke(args);

        Assert.That(foo.a, Is.EqualTo("b"));
        Assert.That(foo.b, Is.EqualTo(-1));
        Assert.That(foo.c, Is.EqualTo('0'));
    }

    [Test]
    public void GetParameterTypes()
    {
        Type[] expectedParameterTypes = new Type[] { typeof(string) };
        MethodInfo method = typeof(ReflectionUtilsObject).GetMethod("BadSpanglish");
        Type[] parameterTypes = ReflectionUtils.GetParameterTypes(method);
        Assert.That(ArrayUtils.AreEqual(expectedParameterTypes, parameterTypes), Is.True);
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
        Assert.That(foundMethods.Length, Is.EqualTo(clonesMethods.Length));
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
        Assert.That(foundMethods.Length, Is.EqualTo(clonesMethods.Length));
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
        Assert.That(foundMethods.Length, Is.EqualTo(clonesMethods.Length));
    }

    [Test]
    public void ParameterTypesMatch()
    {
        MethodInfo method = typeof(ReflectionUtilsObject).GetMethod("Spanglish");
        Type[] types = new Type[] { typeof(string), typeof(object[]) };
        Assert.That(ReflectionUtils.ParameterTypesMatch(method, types), Is.True);
    }

    [Test]
    public void ParameterTypesDontMatchWithNonMatchingArgs()
    {
        Type[] types = new Type[] { typeof(string), typeof(object[]) };

        MethodInfo method = typeof(ReflectionUtilsObject).GetMethod("BadSpanglish");
        Assert.That(ReflectionUtils.ParameterTypesMatch(method, types), Is.False);
        method = typeof(ReflectionUtilsObject).GetMethod("WickedSpanglish");
        Assert.That(ReflectionUtils.ParameterTypesMatch(method, types), Is.False);
    }

    [Test]
    public void GetDefaultValue()
    {
        Assert.That(ReflectionUtils.GetDefaultValue(GetType()), Is.Null);
        Assert.That(ReflectionUtils.GetDefaultValue(typeof(Cuts)), Is.EqualTo(Cuts.Superficial));
        Assert.That(ReflectionUtils.GetDefaultValue(typeof(bool)), Is.EqualTo(false));
        Assert.That(ReflectionUtils.GetDefaultValue(typeof(DateTime)), Is.EqualTo(DateTime.MinValue));
        Assert.That(ReflectionUtils.GetDefaultValue(typeof(char)), Is.EqualTo(Char.MinValue));
        Assert.That(ReflectionUtils.GetDefaultValue(typeof(long)), Is.EqualTo(0));
        Assert.That(ReflectionUtils.GetDefaultValue(typeof(int)), Is.EqualTo(0));
        Assert.That(ReflectionUtils.GetDefaultValue(typeof(short)), Is.EqualTo(0));
    }

    [Test]
    public void PropertyIsIndexer()
    {
        Assert.That(ReflectionUtils.PropertyIsIndexer("Item", typeof(TestObject)), Is.True);
        Assert.That(ReflectionUtils.PropertyIsIndexer("Item", typeof(SideEffectObject)), Is.False);
        Assert.That(ReflectionUtils.PropertyIsIndexer("Count", typeof(SideEffectObject)), Is.False);
        Assert.That(ReflectionUtils.PropertyIsIndexer("MyItem", typeof(ObjectWithNonDefaultIndexerName)), Is.True);
    }

    [Test]
    public void MethodIsOnOneOfTheseInterfaces()
    {
        MethodInfo method = typeof(ReflectionUtilsObject).GetMethod("Spanglish");
        Assert.That(ReflectionUtils.MethodIsOnOneOfTheseInterfaces(method, new Type[] { typeof(IFoo) }), Is.True);
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
        Assert.That(ReflectionUtils.MethodIsOnOneOfTheseInterfaces(method, null), Is.False);
    }

    [Test]
    public void MethodIsOnOneOfTheseInterfacesMultiple()
    {
        MethodInfo method = typeof(RequiredTestObject).GetMethod("set_ObjectFactory");
        Assert.That(method, Is.Not.Null, "Could not get setter property for ObjectFactory");
        Assert.That(ReflectionUtils.MethodIsOnOneOfTheseInterfaces(method, new Type[] { typeof(IObjectNameAware), typeof(IObjectFactoryAware) }), Is.True);
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
        Type[] pTypes = (Type[]) list.ToArray(typeof(Type));
        string actual = ReflectionUtils.GetSignature(method.DeclaringType, method.Name, pTypes);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void ToInterfaceArrayFromType()
    {
        Type[] expected = new Type[] { typeof(IFoo), typeof(IBar) };
        IList<Type> actual = ReflectionUtils.ToInterfaceArray(typeof(IBar));
        Assert.That(actual.Count, Is.EqualTo(expected.Length));
        Assert.That(actual[0], Is.EqualTo(expected[0]));
        Assert.That(actual[1], Is.EqualTo(expected[1]));
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
        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public void GetMethodWithExplicitInterfaceMethod()
    {
        MethodInfo actual = ReflectionUtils.GetMethod(
            typeof(ExplicitFoo),
            "Spring.Util.IFoo.Spanglish",
            new Type[] { typeof(string), typeof(object[]) });
        Assert.That(actual, Is.Not.Null);
    }

    [Test]
    public void GetMethodIsCaseInsensitive()
    {
        MethodInfo actual = ReflectionUtils.GetMethod(
            typeof(ReflectionUtilsObject),
            "spAngLISh",
            new Type[] { typeof(string), typeof(object[]) });
        Assert.That(actual, Is.Not.Null, "ReflectionUtils.GetMethod would appear to be case sensitive.");
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
        Assert.That(builder, Is.Not.Null);

        builder = ReflectionUtils.CreateCustomAttribute(typeof(MyCustomAttribute), "Rick");
        Assert.That(builder, Is.Not.Null);

        builder = ReflectionUtils.CreateCustomAttribute(typeof(MyCustomAttribute), "Rick");
        Assert.That(builder, Is.Not.Null);

        builder = ReflectionUtils.CreateCustomAttribute(new MyCustomAttribute("Rick"));
        Assert.That(builder, Is.Not.Null);

        builder = ReflectionUtils.CreateCustomAttribute(typeof(MyCustomAttribute), new MyCustomAttribute("Rick"));
        Assert.That(builder, Is.Not.Null);

        builder = ReflectionUtils.CreateCustomAttribute(typeof(MyCustomAttribute), new object[] { "Rick" }, new MyCustomAttribute("Evans"));
        Assert.That(builder, Is.Not.Null);

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
            Assert.That(builder, Is.Not.Null);
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
            Assert.That(builder, Is.Not.Null);
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
            Assert.That(builder, Is.Not.Null);
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
            Assert.That(builder, Is.Not.Null);
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
            Assert.That(builder, Is.Not.Null);
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
            Assert.That(builder, Is.Not.Null);
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
        Assert.That(builder, Is.Not.Null);

        AnotherCustomAttribute att
            = (AnotherCustomAttribute)CheckForPresenceOfCustomAttribute(builder, typeof(AnotherCustomAttribute));
        Assert.That(att.Name, Is.Null);
        Assert.That(att.Age, Is.EqualTo(0));
        Assert.That(att.HasSwallowedExplosives, Is.False);
    }

    [Test]
    public void CreateCustomAttributeFromSourceAttribute()
    {
        CustomAttributeBuilder builder = null;
        AnotherCustomAttribute source = new AnotherCustomAttribute("Rick", 30, true);
        builder = ReflectionUtils.CreateCustomAttribute(source);
        Assert.That(builder, Is.Not.Null);

        AnotherCustomAttribute att
            = (AnotherCustomAttribute)CheckForPresenceOfCustomAttribute(builder, typeof(AnotherCustomAttribute));
        Assert.That(att.Name, Is.EqualTo(source.Name));
        Assert.That(att.Age, Is.EqualTo(source.Age));
        Assert.That(att.HasSwallowedExplosives, Is.EqualTo(source.HasSwallowedExplosives));
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
        Assert.That(builder, Is.Not.Null);

        AnotherCustomAttribute att
            = (AnotherCustomAttribute)CheckForPresenceOfCustomAttribute(builder, typeof(AnotherCustomAttribute));
        Assert.That(att.Name, Is.EqualTo(expectedName));
        Assert.That(att.Age, Is.EqualTo(expectedAge));
        Assert.That(att.HasSwallowedExplosives, Is.True);
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
        Assert.That(builder, Is.Not.Null);

        AnotherCustomAttribute att
            = (AnotherCustomAttribute)CheckForPresenceOfCustomAttribute(builder, typeof(AnotherCustomAttribute));
        Assert.That(att.Name, Is.EqualTo(expectedName));
        Assert.That(att.Age, Is.EqualTo(expectedAge));
        Assert.That(att.HasSwallowedExplosives, Is.False);
    }
#endif

    [Test]
    public void HasAtLeastOneMethodWithName()
    {
        Type testType = typeof(ExtendedReflectionUtilsObject);
        // declared method...
        Assert.That(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "Declared"), Is.True);
        // case insensitive method...
        Assert.That(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "deCLAReD"), Is.True);
        // superclass method...
        Assert.That(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "Spanglish"), Is.True);
        // static method...
        Assert.That(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "Static"), Is.True);
        // protected method...
        Assert.That(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "Protected"), Is.True);
        // non existent method...
        Assert.That(ReflectionUtils.HasAtLeastOneMethodWithName(testType, "Sponglish"), Is.False);
        // null type...
        Assert.That(ReflectionUtils.HasAtLeastOneMethodWithName(null, "Spanglish"), Is.False);
        // null method name...
        Assert.That(ReflectionUtils.HasAtLeastOneMethodWithName(testType, null), Is.False);
        // empty method name...
        Assert.That(ReflectionUtils.HasAtLeastOneMethodWithName(testType, ""), Is.False);
        // all nulls...
        Assert.That(ReflectionUtils.HasAtLeastOneMethodWithName(null, null), Is.False);
    }

    [Test]
    public void GetTypeOfOrTypeWithNull()
    {
        Assert.Throws<NullReferenceException>(() => ReflectionUtils.TypeOfOrType(null));
    }

    [Test]
    public void GetTypeOfOrType()
    {
        Assert.That(ReflectionUtils.TypeOfOrType(typeof(string)), Is.EqualTo(typeof(string)));
    }

    [Test]
    public void GetTypeOfOrTypeWithNonNullType()
    {
        Assert.That(ReflectionUtils.TypeOfOrType(string.Empty), Is.EqualTo(typeof(string)));
    }

    [Test]
    public void GetTypes()
    {
        Type[] actual = ReflectionUtils.GetTypes(
            new object[] { 1, "I've never been to Taco Bell (sighs).", new ReflectionUtilsObject() });
        Type[] expected = new Type[] { typeof(int), typeof(string), typeof(ReflectionUtilsObject), };
        Assert.That(ArrayUtils.AreEqual(expected, actual), Is.True, "The ReflectionUtils.GetTypes method did not return a correct Type [] array. Yep, that's as helpful as it gets.");
    }

    [Test]
    public void GetTypesWithEmptyArrayArgument()
    {
        Type[] actual = ReflectionUtils.GetTypes(new object[] { });
        Assert.That(actual, Is.Not.Null, "The ReflectionUtils.GetTypes method returned null when given an empty array. Must return an empty Type [] array.");
        Assert.That(actual.Length == 0, Is.True, "The ReflectionUtils.GetTypes method returned a Type [] that had some elements in it when given an empty array. Must return an empty Type [] array.");
    }

    [Test]
    public void GetTypesWithNullArrayArgument()
    {
        Type[] actual = ReflectionUtils.GetTypes(null);
        Assert.That(actual, Is.Not.Null, "The ReflectionUtils.GetTypes method returned null when given null. Must return an empty Type [] array.");
        Assert.That(actual.Length == 0, Is.True, "The ReflectionUtils.GetTypes method returned a Type [] that had some elements in it when given null. Must return an empty Type [] array.");
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
        Assert.That(types, Is.Not.Null);
        Assert.That(types.Length, Is.EqualTo(2));
        // first method parameter is byRef, so type name must end in '&'
        Assert.That(types[0].FullName, Is.EqualTo("System.Int32&"));
        Assert.That(types[1].FullName, Is.EqualTo("System.Int32"));
    }

    [Test]
    public void GetMethodByArgumentValuesCanResolveWhenAmbiguousMatchIsOnlyDifferentiatedByParams()
    {
        GetMethodByArgumentValuesTarget.DummyArgumentType[] typedArg = new GetMethodByArgumentValuesTarget.DummyArgumentType[] { };
        GetMethodByArgumentValuesTarget foo = new GetMethodByArgumentValuesTarget(1, typedArg);

        Type type = typeof(GetMethodByArgumentValuesTarget);
        MethodInfo[] candidateMethods = new MethodInfo[] { type.GetMethod("ParamOverloadedMethod", new Type[] { typeof(string), typeof(string), typeof(string) }), type.GetMethod("ParamOverloadedMethod", new Type[] { typeof(string), typeof(string), typeof(bool) }), type.GetMethod("ParamOverloadedMethod", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(object[]) }) };

        // ensure no one changed our test class
        Assert.That(candidateMethods[0], Is.Not.Null);
        Assert.That(candidateMethods[1], Is.Not.Null);
        Assert.That(candidateMethods[2], Is.Not.Null);
        Assert.That(foo.ParamOverloadedMethod(string.Empty, string.Empty, string.Empty), Is.EqualTo("ThreeStringsOverload"));
        Assert.That(foo.ParamOverloadedMethod(string.Empty, string.Empty, default(bool)), Is.EqualTo("TwoStringsAndABoolOverload"));
        Assert.That(foo.ParamOverloadedMethod(string.Empty, string.Empty, string.Empty, string.Empty, typedArg), Is.EqualTo("FourStringsAndAParamsCollectionOverload"));

        MethodInfo resolvedMethod = ReflectionUtils.GetMethodByArgumentValues(candidateMethods, new object[] { string.Empty, string.Empty, string.Empty, string.Empty, typedArg });
        Assert.That(resolvedMethod, Is.SameAs(candidateMethods[2]));
    }

    [Test]
    public void GetMethodByArgumentValuesResolvesToExactMatchIfAvailable()
    {
        GetMethodByArgumentValuesTarget.DummyArgumentType[] typedArg = new GetMethodByArgumentValuesTarget.DummyArgumentType[] { };
        GetMethodByArgumentValuesTarget foo = new GetMethodByArgumentValuesTarget(1, typedArg);

        Type type = typeof(GetMethodByArgumentValuesTarget);
        MethodInfo[] candidateMethods = new MethodInfo[] { type.GetMethod("MethodWithSimilarArguments", new Type[] { typeof(int), typeof(ICollection) }), type.GetMethod("MethodWithSimilarArguments", new Type[] { typeof(int), typeof(GetMethodByArgumentValuesTarget.DummyArgumentType[]) }), type.GetMethod("MethodWithSimilarArguments", new Type[] { typeof(int), typeof(object[]) }) };

        // ensure no one changed our test class
        Assert.That(candidateMethods[0], Is.Not.Null);
        Assert.That(candidateMethods[1], Is.Not.Null);
        Assert.That(candidateMethods[2], Is.Not.Null);
        Assert.That(foo.MethodWithSimilarArguments(1, new object()), Is.EqualTo("ParamArrayMatch"));
        Assert.That(foo.MethodWithSimilarArguments(1, typedArg), Is.EqualTo("ExactMatch"));
        Assert.That(foo.MethodWithSimilarArguments(1, (ICollection) typedArg), Is.EqualTo("AssignableMatch"));

        MethodInfo resolvedMethod = ReflectionUtils.GetMethodByArgumentValues(candidateMethods, new object[] { 1, typedArg });
        Assert.That(resolvedMethod, Is.SameAs(candidateMethods[1]));
    }

    [Test]
    public void GetMethodByArgumentValuesMatchesNullableArgs()
    {
        GetMethodByArgumentValuesTarget.DummyArgumentType[] typedArg = new GetMethodByArgumentValuesTarget.DummyArgumentType[] { };
        GetMethodByArgumentValuesTarget foo = new GetMethodByArgumentValuesTarget(1, typedArg);

        Type type = typeof(GetMethodByArgumentValuesTarget);
        MethodInfo[] candidateMethods = new MethodInfo[] { type.GetMethod("MethodWithSimilarArguments", new Type[] { typeof(int), typeof(ICollection) }), type.GetMethod("MethodWithSimilarArguments", new Type[] { typeof(int), typeof(GetMethodByArgumentValuesTarget.DummyArgumentType[]) }), type.GetMethod("MethodWithSimilarArguments", new Type[] { typeof(int), typeof(object[]) }), type.GetMethod("MethodWithNullableArgument", new Type[] { typeof(int?) }) };

        // ensure no one changed our test class
        Assert.That(candidateMethods[0], Is.Not.Null);
        Assert.That(candidateMethods[1], Is.Not.Null);
        Assert.That(candidateMethods[2], Is.Not.Null);
        Assert.That(candidateMethods[3], Is.Not.Null);
        Assert.That(foo.MethodWithSimilarArguments(1, new object()), Is.EqualTo("ParamArrayMatch"));
        Assert.That(foo.MethodWithSimilarArguments(1, typedArg), Is.EqualTo("ExactMatch"));
        Assert.That(foo.MethodWithSimilarArguments(1, (ICollection) typedArg), Is.EqualTo("AssignableMatch"));
        Assert.That(foo.MethodWithNullableArgument(null), Is.EqualTo("NullableArgumentMatch"));

        MethodInfo resolvedMethod = ReflectionUtils.GetMethodByArgumentValues(candidateMethods, new object[] { null });
        Assert.That(resolvedMethod, Is.SameAs(candidateMethods[3]));
    }

    [Test]
    public void GetConstructorByArgumentValuesResolvesToExactMatchIfAvailable()
    {
        GetMethodByArgumentValuesTarget.DummyArgumentType[] typedArg = new GetMethodByArgumentValuesTarget.DummyArgumentType[] { };
        Type type = typeof(GetMethodByArgumentValuesTarget);
        ConstructorInfo[] candidateConstructors = new ConstructorInfo[] { type.GetConstructor(new Type[] { typeof(int), typeof(ICollection) }), type.GetConstructor(new Type[] { typeof(int), typeof(GetMethodByArgumentValuesTarget.DummyArgumentType[]) }), type.GetConstructor(new Type[] { typeof(int), typeof(object[]) }) };

        // ensure noone changed our test class
        Assert.That(candidateConstructors[0], Is.Not.Null);
        Assert.That(candidateConstructors[1], Is.Not.Null);
        Assert.That(candidateConstructors[2], Is.Not.Null);
        Assert.That(new GetMethodByArgumentValuesTarget(1, new object()).SelectedConstructor, Is.EqualTo("ParamArrayMatch"));
        Assert.That(new GetMethodByArgumentValuesTarget(1, typedArg).SelectedConstructor, Is.EqualTo("ExactMatch"));
        Assert.That(new GetMethodByArgumentValuesTarget(1, (ICollection) typedArg).SelectedConstructor, Is.EqualTo("AssignableMatch"));

        ConstructorInfo resolvedConstructor = ReflectionUtils.GetConstructorByArgumentValues(candidateConstructors, new object[] { 1, typedArg });
        Assert.That(resolvedConstructor, Is.SameAs(candidateConstructors[1]));
    }

    [Test]
    public void GetInterfaces()
    {
        Assert.That(
            ReflectionUtils.GetInterfaces(typeof(TestObject)).Length,
            Is.EqualTo(typeof(TestObject).GetInterfaces().Length));

        Assert.That(ReflectionUtils.GetInterfaces(typeof(IInterface1)).Length, Is.EqualTo(1));
        Assert.That(ReflectionUtils.GetInterfaces(typeof(IInterface2)).Length, Is.EqualTo(4));
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

    [Test]
    public void IsTypeVisibleWithInternalType()
    {
        Type type = typeof(InternalType);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.False);
    }

    [Test]
    public void IsTypeVisibleWithPublicNestedTypeOnInternalType()
    {
        Type type = typeof(InternalType.PublicNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.False);
    }

    [Test]
    public void IsTypeVisibleWithInternalNestedTypeOnInternalType()
    {
        Type type = typeof(InternalType.InternalNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.False);
    }

    [Test]
    public void IsTypeVisibleWithProtectedInternalNestedTypeOnInternalType()
    {
        Type type = typeof(InternalType.ProtectedInternalNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.False);
    }

    [Test]
    public void IsTypeVisibleWithProtectedNestedTypeOnInternalType()
    {
        Type type = typeof(InternalType).GetNestedType("ProtectedNestedType", BindingFlags.NonPublic);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.False);
    }

    [Test]
    public void IsTypeVisibleWithPrivateNestedTypeOnInternalType()
    {
        Type type = typeof(InternalType).GetNestedType("PrivateNestedType", BindingFlags.NonPublic);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.False);
    }

    [Test]
    public void IsTypeVisibleWithPublicType()
    {
        Type type = typeof(PublicType);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.True);
    }

    [Test]
    public void IsTypeVisibleWithPublicNestedTypeOnPublicType()
    {
        Type type = typeof(PublicType.PublicNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.True);
    }

    [Test]
    public void IsTypeVisibleWithInternalNestedTypeOnPublicType()
    {
        Type type = typeof(PublicType.InternalNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.False);
    }

    [Test]
    public void IsTypeVisibleWithProtectedInternalNestedTypeOnPublicType()
    {
        Type type = typeof(PublicType.ProtectedInternalNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.False);
    }

    [Test]
    public void IsTypeVisibleWithProtectedNestedTypeOnPublicType()
    {
        Type type = typeof(PublicType).GetNestedType("ProtectedNestedType", BindingFlags.NonPublic);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.False);
    }

    [Test]
    public void IsTypeVisibleWithPrivateNestedTypeOnPublicType()
    {
        Type type = typeof(PublicType).GetNestedType("PrivateNestedType", BindingFlags.NonPublic);
        Assert.That(ReflectionUtils.IsTypeVisible(type), Is.False);
    }

    private static readonly string FRIENDLY_ASSEMBLY_NAME = "ReflectionUtils.IsTypeVisible.AssemblyTestName";

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithInternalType()
    {
        Type type = typeof(InternalType);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.True);
    }

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithPublicNestedTypeOnInternalType()
    {
        Type type = typeof(InternalType.PublicNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.True);
    }

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithInternalNestedTypeOnInternalType()
    {
        Type type = typeof(InternalType.InternalNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.True);
    }

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithProtectedInternalNestedTypeOnInternalType()
    {
        Type type = typeof(InternalType.ProtectedInternalNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.True);
    }

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithProtectedNestedTypeOnInternalType()
    {
        Type type = typeof(InternalType).GetNestedType("ProtectedNestedType", BindingFlags.NonPublic);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.False);
    }

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithPrivateNestedTypeOnInternalType()
    {
        Type type = typeof(InternalType).GetNestedType("PrivateNestedType", BindingFlags.NonPublic);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.False);
    }

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithPublicType()
    {
        Type type = typeof(PublicType);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.True);
    }

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithPublicNestedTypeOnPublicType()
    {
        Type type = typeof(PublicType.PublicNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.True);
    }

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithInternalNestedTypeOnPublicType()
    {
        Type type = typeof(PublicType.InternalNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.True);
    }

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithProtectedInternalNestedTypeOnPublicType()
    {
        Type type = typeof(PublicType.ProtectedInternalNestedType);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.True);
    }

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithProtectedNestedTypeOnPublicType()
    {
        Type type = typeof(PublicType).GetNestedType("ProtectedNestedType", BindingFlags.NonPublic);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.False);
    }

    [Test]
    public void IsTypeVisibleFromFriendlyAssemblyWithPrivateNestedTypeOnPublicType()
    {
        Type type = typeof(PublicType).GetNestedType("PrivateNestedType", BindingFlags.NonPublic);
        Assert.That(ReflectionUtils.IsTypeVisible(type, FRIENDLY_ASSEMBLY_NAME), Is.False);
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
        Assert.That(attrs.Count, Is.EqualTo(1));
#else
        Assert.That(attrs.Count, Is.EqualTo(2));
#endif
    }

    [Test]
    public void GetCustomAttributesOnMethod()
    {
        IList attrs = ReflectionUtils.GetCustomAttributes(typeof(ClassWithAttributes).GetMethod("MethodWithAttributes"));

#if NETCOREAPP
        Assert.That(attrs.Count, Is.EqualTo(1));
#else
        Assert.That(attrs.Count, Is.EqualTo(2));
#endif
    }

    [Test]
    public void GetExplicitBaseExceptionWithNoInnerException()
    {
        Exception appEx = new ApplicationException();
        Exception ex = ReflectionUtils.GetExplicitBaseException(appEx);

        Assert.That(appEx, Is.EqualTo(ex));
    }

    [Test]
    public void GetExplicitBaseExceptionWithInnerException()
    {
        Exception dbzEx = new DivideByZeroException();
        Exception appEx = new ApplicationException("Test message", dbzEx);
        Exception ex = ReflectionUtils.GetExplicitBaseException(appEx);

        Assert.That(dbzEx, Is.EqualTo(ex));
    }

    [Test]
    public void GetExplicitBaseExceptionWithInnerExceptions()
    {
        Exception dbzEx = new DivideByZeroException();
        Exception sEx = new SystemException("Test message", dbzEx);
        Exception appEx = new ApplicationException("Test message", sEx);
        Exception ex = ReflectionUtils.GetExplicitBaseException(appEx);

        Assert.That(dbzEx, Is.EqualTo(ex));
    }

    [Test]
    public void GetExplicitBaseExceptionWithNullReferenceExceptionAsRootException()
    {
        Exception nrEx = new NullReferenceException();
        Exception appEx = new ApplicationException("Test message", nrEx);
        Exception ex = ReflectionUtils.GetExplicitBaseException(appEx);

        Assert.That(appEx, Is.EqualTo(ex));
    }

    [Test]
    public void CanGetFriendlyNamesForGenericTypes()
    {
        Type t = typeof(System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, int>>);

        Assert.That(ReflectionUtils.GetTypeFriendlyName(t), Is.EqualTo("System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, int>>"));
    }

    public int Add(ref int one, int two)
    {
        return one + two;
    }

#if !NETCOREAPP
    private Attribute CheckForPresenceOfCustomAttribute(
        CustomAttributeBuilder attBuilder, Type attType)
    {
        Attribute[] atts = ApplyAndGetCustomAttributes(attBuilder);
        Assert.That(atts, Is.Not.Null);
        Assert.That(atts.Length == 1, Is.True);
        Attribute att = atts[0];
        Assert.That(att, Is.Not.Null);
        Assert.That(att.GetType(), Is.EqualTo(attType), "Wrong Attribute applied to the class.");
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
}

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
    public class DummyArgumentType
    {
    }

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