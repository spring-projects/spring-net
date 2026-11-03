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
using System.Runtime.Serialization;
using System.Security.Policy;
using NUnit.Framework;
using Spring.Objects;

namespace Spring.Util;

/// <summary>
/// Unit tests for the ObjectUtils class.
/// </summary>
/// <author>Rick Evans (.NET)</author>
[TestFixture]
public class ObjectUtilsTests
{
    [Test]
    public void NullSafeEqualsWithBothNull()
    {
        string first = null;
        string second = null;
        Assert.That(ObjectUtils.NullSafeEquals(first, second), Is.True);
    }

    [Test]
    public void NullSafeEqualsWithFirstNull()
    {
        string first = null;
        string second = "";
        Assert.That(ObjectUtils.NullSafeEquals(first, second), Is.False);
    }

    [Test]
    public void NullSafeEqualsWithSecondNull()
    {
        string first = "";
        string second = null;
        Assert.That(ObjectUtils.NullSafeEquals(first, second), Is.False);
    }

    [Test]
    public void NullSafeEqualsBothEquals()
    {
        string first = "this is it";
        string second = "this is it";
        Assert.That(ObjectUtils.NullSafeEquals(first, second), Is.True);
    }

    [Test]
    public void NullSafeEqualsNotEqual()
    {
        string first = "this is it";
        int second = 12;
        Assert.That(ObjectUtils.NullSafeEquals(first, second), Is.False);
    }

    [Test]
    public void IsAssignableAndNotTransparentProxyWithProxy()
    {
        AppDomain domain = null;
        try
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Environment.CurrentDirectory;
            domain = AppDomain.CreateDomain("Spring", new Evidence(AppDomain.CurrentDomain.Evidence), setup);
            object foo = domain.CreateInstanceAndUnwrap(GetType().Assembly.FullName, typeof(Foo).FullName);
            // the instance is definitely assignable to the supplied interface type...
            bool isAssignable = ObjectUtils.IsAssignableAndNotTransparentProxy(typeof(IFoo), foo);
            Assert.That(isAssignable, Is.False, "Proxied instance was not recognized as such.");
        }
        finally
        {
            AppDomain.Unload(domain);
        }
    }

    [Test]
    public void InstantiateTypeWithNullType()
    {
        Assert.Throws<ArgumentNullException>(() => ObjectUtils.InstantiateType(null));
    }

    [Test]
    public void InstantiateType()
    {
        object foo = ObjectUtils.InstantiateType(typeof(TestObject));
        Assert.That(foo, Is.Not.Null, "Failed to instantiate an instance of a valid Type.");
        Assert.That(foo is TestObject, Is.True, "The instantiated instance was not an instance of the type that was passed in.");
    }

    [Test]
    public void InstantiateTypeThrowingWithinPublicConstructor()
    {
        try
        {
            ObjectUtils.InstantiateType(typeof(ThrowingWithinConstructor));
            Assert.Fail();
        }
        catch (FatalReflectionException ex)
        {
            // no nasty "TargetInvocationException" is in between!
            Assert.That(ex.InnerException.GetType(), Is.EqualTo(typeof(ThrowingWithinConstructorException)));
        }
    }

    [Test]
    public void InstantiateTypeWithOpenGenericType()
    {
        Assert.Throws<FatalReflectionException>(() => ObjectUtils.InstantiateType(typeof(Dictionary<,>)));
    }

    [Test]
    public void InstantiateGenericTypeWithArguments()
    {
//            ObjectUtils.InstantiateType(typeof(Dictionary<string, int>), new object[] { new object() } );
    }

    [Test]
    public void InstantiateTypeWithAbstractType()
    {
        Assert.Throws<FatalReflectionException>(() => ObjectUtils.InstantiateType(typeof(AbstractType)));
    }

    [Test]
    public void InstantiateTypeWithInterfaceType()
    {
        Assert.Throws<FatalReflectionException>(() => ObjectUtils.InstantiateType(typeof(IList)));
    }

    [Test]
    public void InstantiateTypeWithTypeExposingNoZeroArgCtor()
    {
        Assert.Throws<FatalReflectionException>(() => ObjectUtils.InstantiateType(typeof(NoZeroArgConstructorType)));
    }

    [Test]
    public void InstantiateTypeWithPrivateCtor()
    {
        ConstructorInfo ctor = typeof(OnlyPrivateCtor).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(string) },
            null);
        object foo = ObjectUtils.InstantiateType(ctor, new object[] { "Chungking Express" });
        Assert.That(foo, Is.Not.Null, "Failed to instantiate an instance of a valid Type.");
        Assert.That(foo is OnlyPrivateCtor, Is.True, "The instantiated instance was not an instance of the type that was passed in.");
        Assert.That(((OnlyPrivateCtor) foo).Name, Is.EqualTo("Chungking Express"));
    }

    [Test]
    public void InstantiateTypeWithNullCtor()
    {
        Assert.Throws<ArgumentNullException>(() => ObjectUtils.InstantiateType(typeof(IList).GetConstructor(Type.EmptyTypes), new object[] { }));
    }

    [Test]
    public void InstantiateTypeWithCtorWithNoArgs()
    {
        Type type = typeof(TestObject);
        ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
        object foo = ObjectUtils.InstantiateType(ctor, ObjectUtils.EmptyObjects);
        Assert.That(foo, Is.Not.Null, "Failed to instantiate an instance of a valid Type.");
        Assert.That(foo is TestObject, Is.True, "The instantiated instance was not an instance of the Type that was passed in.");
    }

    [Test]
    public void InstantiateTypeWithCtorArgs()
    {
        Type type = typeof(TestObject);
        ConstructorInfo ctor = type.GetConstructor(new Type[] { typeof(string), typeof(int) });
        object foo = ObjectUtils.InstantiateType(ctor, new object[] { "Yakov Petrovich Golyadkin", 39 });
        Assert.That(foo, Is.Not.Null, "Failed to instantiate an instance of a valid Type.");
        Assert.That(foo is TestObject, Is.True, "The instantiated instance was not an instance of the Type that was passed in.");
        TestObject obj = foo as TestObject;
        Assert.That(obj.Name, Is.EqualTo("Yakov Petrovich Golyadkin"));
        Assert.That(obj.Age, Is.EqualTo(39));
    }

    [Test]
    public void InstantiateTypeWithBadCtorArgs()
    {
        Type type = typeof(TestObject);
        ConstructorInfo ctor = type.GetConstructor(new Type[] { typeof(string), typeof(int) });
        try
        {
            ObjectUtils.InstantiateType(ctor, new object[] { 39, "Yakov Petrovich Golyadkin" });
            Assert.Fail("Should throw an error");
        }
        catch
        {
            // ok...
        }
    }

    [Test]
    public void IsSimpleProperty()
    {
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(string)), Is.True);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(long)), Is.True);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(bool)), Is.True);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(int)), Is.True);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(float)), Is.True);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(ushort)), Is.True);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(double)), Is.True);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(ulong)), Is.True);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(char)), Is.True);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(uint)), Is.True);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(string[])), Is.True);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(Type)), Is.True);

        Assert.That(ObjectUtils.IsSimpleProperty(typeof(TestObject)), Is.False);
        Assert.That(ObjectUtils.IsSimpleProperty(typeof(IList[])), Is.False);
    }

    [Test]
    public void EnumerateFirstElement()
    {
        string expected = "Hiya";
        IList list = new string[] { expected, "Aw!", "Man!" };
        IEnumerator enumerator = list.GetEnumerator();
        object actual = ObjectUtils.EnumerateFirstElement(enumerator);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void EnumerateElementAtIndex()
    {
        string expected = "Mmm...";
        IList list = new string[] { "Aw!", "Man!", expected };
        IEnumerator enumerator = list.GetEnumerator();
        object actual = ObjectUtils.EnumerateElementAtIndex(enumerator, 2);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void EnumerateElementAtIndexViaIEnumerable()
    {
        string expected = "Mmm...";
        IList list = new string[] { "Aw!", "Man!", expected };
        object actual = ObjectUtils.EnumerateElementAtIndex(list, 2);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void EnumerateElementAtOutOfRangeIndex()
    {
        string expected = "Mmm...";
        IList list = new string[] { "Aw!", "Man!", expected };
        IEnumerator enumerator = list.GetEnumerator();
        Assert.Throws<ArgumentOutOfRangeException>(() => ObjectUtils.EnumerateElementAtIndex(enumerator, 12));
    }

    [Test]
    public void EnumerateElementAtOutOfRangeIndexViaIEnumerable()
    {
        string expected = "Mmm...";
        IList list = new string[] { "Aw!", "Man!", expected };
        Assert.Throws<ArgumentOutOfRangeException>(() => ObjectUtils.EnumerateElementAtIndex(list, 12));
    }

    [Test]
    public void EnumerateElementAtNegativeIndex()
    {
        string expected = "Mmm...";
        IList list = new string[] { "Aw!", "Man!", expected };
        IEnumerator enumerator = list.GetEnumerator();
        Assert.Throws<ArgumentOutOfRangeException>(() => ObjectUtils.EnumerateElementAtIndex(enumerator, -10));
    }

    [Test]
    public void EnumerateElementAtNegativeIndexViaIEnumerable()
    {
        string expected = "Mmm...";
        IList list = new string[] { "Aw!", "Man!", expected };
        Assert.Throws<ArgumentOutOfRangeException>(() => ObjectUtils.EnumerateElementAtIndex(list, -10));
    }

    private interface IFoo
    {
    }

    private sealed class Foo : MarshalByRefObject, IFoo
    {
    }

    /// <summary>
    /// A class that doesn't have a parameterless constructor.
    /// </summary>
    private class NoZeroArgConstructorType
    {
        /// <summary>
        /// Creates a new instance of the NoZeroArgConstructorType class.
        /// </summary>
        /// <param name="foo">A spurious argument (ignored).</param>
        public NoZeroArgConstructorType(string foo)
        {
        }
    }

    /// <summary>
    /// An abstract class. Doh!
    /// </summary>
    private abstract class AbstractType
    {
        /// <summary>
        /// Creates a new instance of the AbstractType class.
        /// </summary>
        public AbstractType()
        {
        }
    }

    private class OnlyPrivateCtor
    {
        private OnlyPrivateCtor(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _name;
    }

    [Serializable]
    private class ThrowingWithinConstructorException : TestException
    {
        public ThrowingWithinConstructorException()
        {
        }

        public ThrowingWithinConstructorException(string message) : base(message)
        {
        }

        public ThrowingWithinConstructorException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ThrowingWithinConstructorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class ThrowingWithinConstructor
    {
        public ThrowingWithinConstructor()
        {
            throw new ThrowingWithinConstructorException();
        }
    }
}
