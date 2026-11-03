/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Reflection;
using NUnit.Framework;

namespace Spring.Util;

/// <summary>
/// Unit tests for the DelegateInfo class.
/// </summary>
/// <author>Rick Evans</author>
[TestFixture]
public sealed class DelegateInfoTests
{
    [Test]
    public void Instantiation()
    {
        new DelegateInfo(typeof(EventHandler));
    }

    [Test]
    public void InstantiationWithBadType()
    {
        Assert.Throws<ArgumentException>(() => new DelegateInfo(typeof(string)));
    }

    [Test]
    public void IsDelegateWithBadType()
    {
        Assert.That(DelegateInfo.IsDelegate(typeof(string)), Is.False);
    }

    [Test]
    public void IsDelegateWithNullType()
    {
        Assert.That(DelegateInfo.IsDelegate(null), Is.False);
    }

    [Test]
    public void IsDelegate()
    {
        Assert.That(DelegateInfo.IsDelegate(typeof(EventHandler)), Is.True);
    }

    [Test]
    public void GetReturnType()
    {
        DelegateInfo del = new DelegateInfo(typeof(EventHandler));
        Assert.That(del.GetReturnType(), Is.EqualTo(typeof(void)));
    }

    [Test]
    public void GetReturnTypeWithNonVoidReturningDelegate()
    {
        DelegateInfo del = new DelegateInfo(typeof(FooHandler));
        Assert.That(del.GetReturnType(), Is.EqualTo(typeof(string)));
    }

    [Test]
    public void GetParameterTypesWithNoArgHandler()
    {
        DelegateInfo del = new DelegateInfo(typeof(NoParametersHandler));
        Type[] types = del.GetParameterTypes();
        Assert.That(types, Is.Not.Null);
        Assert.That(types.Length, Is.EqualTo(0));
    }

    [Test]
    public void GetParameterTypes()
    {
        DelegateInfo del = new DelegateInfo(typeof(FooHandler));
        Type[] types = del.GetParameterTypes();
        Assert.That(types, Is.Not.Null);
        Assert.That(types.Length, Is.EqualTo(3));
    }

    [Test]
    public void IsSignatureCompatibleWithBadMethods()
    {
        DelegateInfo del = new DelegateInfo(typeof(FooHandler));

        // null method...
        MethodInfo method = GetType().GetMethod("SeaBass said that? Well, if that guy over there is SeaBass...");
        Assert.That(del.IsSignatureCompatible(method), Is.False);

        // method that doesn;t have the required number of parameters...
        method = GetType().GetMethod("OddNumberOfParametersForFooHandler");
        Assert.That(del.IsSignatureCompatible(method), Is.False);

        // method that doesn't have the required number of parameters...
        method = GetType().GetMethod("IncompatibleParametersForFooHandler");
        Assert.That(del.IsSignatureCompatible(method), Is.False);
    }

    public string OddNumberOfParametersForFooHandler(string bar, int x)
    {
        return string.Empty;
    }

    public string IncompatibleParametersForFooHandler(string bar, int x, string nope)
    {
        return string.Empty;
    }

    [Test]
    public void IsSignatureCompatibleWithInnerClassStaticMethod()
    {
        DelegateInfo del = new DelegateInfo(typeof(FooHandler));
        MethodInfo method =
            typeof(Yossarian).GetMethod(
                "DoFoo",
                BindingFlags.Public | BindingFlags.Static);
        Assert.That(del.IsSignatureCompatible(method), Is.True);
    }

    [Test]
    public void IsSignatureCompatible()
    {
        DelegateInfo del = new DelegateInfo(typeof(FooHandler));
        MethodInfo method = GetType().GetMethod("DoFoo");
        Assert.That(del.IsSignatureCompatible(method), Is.False);
    }

    [Test]
    public void IsSignatureCompatibleStaticVersion()
    {
        EventInfo evt = typeof(Assembly).GetEvent("ModuleResolve");

        MethodInfo rubbishMethod = GetType().GetMethod("MethodSignatureIsCompatibleStatic");
        Assert.That(DelegateInfo.IsSignatureCompatible(evt, rubbishMethod), Is.False);

        MethodInfo compatibleMethod = typeof(Yossarian).GetMethod("Resolve");
        Assert.That(DelegateInfo.IsSignatureCompatible(evt, compatibleMethod), Is.True);

        Assert.That(DelegateInfo.IsSignatureCompatible(null, compatibleMethod), Is.False);
        Assert.That(DelegateInfo.IsSignatureCompatible(evt, null), Is.False);
    }

    private string DoFoo(string bar, int x, EventArgs e)
    {
        return bar;
    }

    internal sealed class Yossarian
    {
        public static string DoFoo(string bar, int x, EventArgs e)
        {
            return bar;
        }

        public Module Resolve(object sender, ResolveEventArgs e)
        {
            return null;
        }
    }
}

internal delegate string FooHandler(string bar, int x, EventArgs e);

internal delegate void NoParametersHandler();
