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

using NUnit.Framework;
using Spring.Core;

namespace Spring.Objects.Support;

/// <summary>
/// Unit tests for the MethodInvoker class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class MethodInvokerTests
{
    [Test]
    public void Instantiation()
    {
        MethodInvoker vkr = new MethodInvoker();
        Assert.That(vkr.Arguments, Is.Not.Null);
    }

    [Test]
    public void SettingNamedArgumentsToNullJustClearsOutAnyNamedArguments()
    {
        MethodInvoker vkr = new MethodInvoker();
        vkr.AddNamedArgument("age", 10);
        vkr.NamedArguments = null;
        Assert.That(vkr.NamedArguments, Is.Not.Null);
        Assert.That(vkr.NamedArguments.Count, Is.EqualTo(0));
    }

    [Test]
    public void PrepareWithOnlyTargetMethodSet()
    {
        MethodInvoker vkr = new MethodInvoker();
        vkr.TargetMethod = "Foo";
        Assert.Throws<ArgumentException>(() => vkr.Prepare(), "One of either the 'TargetType' or 'TargetObject' properties is required.");
    }

    [Test]
    public void ArgumentsProperty()
    {
        MethodInvoker vkr = new MethodInvoker();
        vkr.Arguments = null;
        Assert.That(vkr.Arguments, Is.Not.Null); // should always be the empty object array, never null
        Assert.That(vkr.Arguments.Length, Is.EqualTo(0));
        vkr.Arguments = new string[] { "Chank Pop" };
        Assert.That(vkr.Arguments.Length, Is.EqualTo(1));
    }

    [Test]
    public void InvokeWithStaticMethod()
    {
        MethodInvoker mi = new MethodInvoker();
        mi.TargetType = typeof(Int32);
        mi.TargetMethod = "Parse";
        mi.Arguments = new object[] { "27" };
        mi.Prepare();
        object actual = mi.Invoke();
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.GetType(), Is.EqualTo(typeof(int)));
        Assert.That((int) actual, Is.EqualTo(27));
    }

    [Test]
    public void InvokeWithGenericStaticMethod()
    {
        MethodInvoker mi = new MethodInvoker();
        mi.TargetType = typeof(Activator);
        mi.TargetMethod = "CreateInstance<Spring.Objects.TestObject>";
        mi.Prepare();
        object actual = mi.Invoke();
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.GetType(), Is.EqualTo(typeof(TestObject)));
    }

    [Test]
    public void InvokeWithOKArguments()
    {
        Foo foo = new Foo();
        foo.Age = 88;
        MethodInvoker vkr = new MethodInvoker();
        vkr.TargetObject = foo;
        vkr.TargetMethod = "GrowOlder";
        vkr.Arguments = new object[] { 10 };
        vkr.Prepare();
        object actual = vkr.Invoke();
        Assert.That(actual, Is.EqualTo(98));
    }

    [Test]
    public void InvokeWithOKArgumentsAndMixedCaseMethodName()
    {
        Foo foo = new Foo();
        foo.Age = 88;
        MethodInvoker vkr = new MethodInvoker();
        vkr.TargetObject = foo;
        vkr.TargetMethod = "growolder";
        vkr.Arguments = new object[] { 10 };
        vkr.Prepare();
        object actual = vkr.Invoke();
        Assert.That(actual, Is.EqualTo(98));
    }

    [Test]
    public void InvokeWithNamedArgument()
    {
        Foo foo = new Foo();
        foo.Age = 88;
        MethodInvoker vkr = new MethodInvoker();
        vkr.TargetObject = foo;
        vkr.TargetMethod = "growolder";
        vkr.AddNamedArgument("years", 10);
        vkr.Prepare();
        object actual = vkr.Invoke();
        Assert.That(actual, Is.EqualTo(98));
    }

    [Test]
    public void InvokeWithMixOfNamedAndPlainVanillaArguments()
    {
        int maximumAge = 95;
        Foo foo = new Foo();
        foo.Age = 88;
        MethodInvoker vkr = new MethodInvoker();
        vkr.TargetObject = foo;
        vkr.TargetMethod = "GrowOlderUntilMaximumAgeReached";
        vkr.AddNamedArgument("years", 10);
        vkr.Arguments = new object[] { maximumAge };
        vkr.Prepare();
        object actual = vkr.Invoke();
        Assert.That(actual, Is.EqualTo(maximumAge));
    }

    [Test]
    public void InvokeWithMixOfNamedAndPlainVanillaArgumentsOfDifferingTypes()
    {
        int maximumAge = 95;
        string expectedStatus = "Old Age Pensioner";
        Foo foo = new Foo();
        foo.Age = 88;
        MethodInvoker vkr = new MethodInvoker();
        vkr.TargetObject = foo;
        vkr.TargetMethod = "GrowOlderUntilMaximumAgeReachedAndSetStatusName";
        vkr.AddNamedArgument("years", 10);
        vkr.AddNamedArgument("status", expectedStatus);
        vkr.Arguments = new object[] { maximumAge };
        vkr.Prepare();
        object actual = vkr.Invoke();
        Assert.That(actual, Is.EqualTo(maximumAge));
        Assert.That(foo.Status, Is.EqualTo(expectedStatus));
    }

    [Test]
    public void InvokeWithNamedArgumentThatDoesNotExist()
    {
        Foo foo = new Foo();
        foo.Age = 88;
        MethodInvoker vkr = new MethodInvoker();
        vkr.TargetObject = foo;
        vkr.TargetMethod = "growolder";
        vkr.AddNamedArgument("southpaw", 10);
        Assert.Throws<ArgumentException>(() => vkr.Prepare(), "The named argument 'southpaw' could not be found on the 'GrowOlder' method of class [Spring.Objects.Support.MethodInvokerTests+Foo].");
    }

    /// <summary>
    /// Tests CLS case insensitivity compliance...
    /// </summary>
    [Test]
    public void InvokeWithWeIRdLyCasedNamedArgument()
    {
        Foo foo = new Foo();
        foo.Age = 88;
        MethodInvoker vkr = new MethodInvoker();
        vkr.TargetObject = foo;
        vkr.TargetMethod = "gROwOldeR";
        vkr.AddNamedArgument("YEarS", 10);
        vkr.Prepare();
        object actual = vkr.Invoke();
        Assert.That(actual, Is.EqualTo(98));
    }

    [Test]
    public void InvokeWithArgumentOfWrongType()
    {
        Foo foo = new Foo();
        foo.Age = 88;
        MethodInvoker vkr = new MethodInvoker();
        vkr.TargetObject = foo;
        vkr.TargetMethod = "growolder";
        vkr.AddNamedArgument("years", "Bingo");
        vkr.Prepare();
        Assert.Throws<MethodInvocationException>(() => vkr.Invoke(), "At least one of the arguments passed to this MethodInvoker was incompatible with the signature of the invoked method.");
    }

    [Test]
    public void NamedArgumentsOverwriteEachOther()
    {
        Foo foo = new Foo();
        foo.Age = 88;
        MethodInvoker vkr = new MethodInvoker();
        vkr.TargetObject = foo;
        vkr.TargetMethod = "growolder";
        vkr.AddNamedArgument("years", 10);
        // this second setting must overwrite the first...
        vkr.AddNamedArgument("years", 200);
        vkr.Prepare();
        object actual = vkr.Invoke();
        Assert.That(98.Equals(actual), Is.False, "The first named argument setter is sticking; must allow itslf to be overwritten.");
        Assert.That(actual, Is.EqualTo(288), "The second named argument was not applied (it must be).");
    }

    [Test]
    public void PreparedArgumentsIsNeverNull()
    {
        MyMethodInvoker vkr = new MyMethodInvoker();
        Assert.That(vkr.GetPreparedArguments(),
            Is.Not.Null,
            "PreparedArguments is null even before Prepare() is called; must NEVER be null.");
        vkr.NullOutPreparedArguments();
        Assert.That(vkr.GetPreparedArguments(),
            Is.Not.Null,
            "PreparedArguments should revert to the empty object[] when set to null; must NEVER be null.");
    }

    private sealed class Foo
    {
        public Foo()
        {
            Age = 0;
            Status = "Baby";
        }

        public int GrowOlder()
        {
            return GrowOlder(1);
        }

        public int GrowOlder(int years)
        {
            _age += years;
            return _age;
        }

        public int GrowOlderUntilMaximumAgeReached(int years, int maximumAge)
        {
            _age += years;
            if (_age > maximumAge)
            {
                _age = maximumAge;
            }

            return _age;
        }

        public int GrowOlderUntilMaximumAgeReachedAndSetStatusName(
            int years, int maximumAge, string status)
        {
            Status = status;
            return GrowOlderUntilMaximumAgeReached(years, maximumAge);
        }

        private int _age;
        private string _status;

        public int Age
        {
            get { return _age; }
            set { _age = value; }
        }

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }
    }

    private sealed class MyMethodInvoker : MethodInvoker
    {
        public MyMethodInvoker()
        {
        }

        public void NullOutPreparedArguments()
        {
            PreparedArguments = null;
        }

        public object[] GetPreparedArguments()
        {
            return PreparedArguments;
        }
    }
}
