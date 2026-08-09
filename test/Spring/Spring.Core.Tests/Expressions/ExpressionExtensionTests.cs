/*
 * Copyright © 2002-2011 the original author or authors.
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
using Spring.Core.TypeResolution;
using Spring.Expressions.Processors;

namespace Spring.Expressions;

/// <summary>
/// Tests the behavior of user-registrable extension methods and extension properties.
/// </summary>
[TestFixture]
public class ExpressionExtensionTests
{
    private const string SpelExtensionLambda = "{|self, arg| $self.InternalValue + $arg}";
    private const string DoubledLambda = "{|self| $self.InternalValue * 2}";
    private const string FooTypeAlias = "ExtensionTestFoo";

    [Test]
    public void LambdaExtensionMethodIsInvoked()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("SpelExtension", SpelExtensionLambda, vars);

        Assert.AreEqual(42, ExpressionEvaluator.GetValue(null, "#foo.SpelExtension(1)", vars));
    }

    [Test]
    public void DelegateExtensionMethodIsInvoked()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod("SpelExtension", (Foo self, int arg) => self.InternalValue + arg, vars);

        Assert.AreEqual(42, ExpressionEvaluator.GetValue(null, "#foo.SpelExtension(1)", vars));
    }

    [Test]
    public void ExtensionMethodWithoutArguments()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("Doubled", DoubledLambda, vars);

        Assert.AreEqual(82, ExpressionEvaluator.GetValue(null, "#foo.Doubled()", vars));
    }

    [Test]
    public void ExtensionMethodOnIntermediateExpressionValue()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();
        Holder holder = new Holder();
        holder.Child = CreateFoo();
        vars["holder"] = holder;
        Expression.RegisterExtensionMethod<Foo>("SpelExtension", SpelExtensionLambda, vars);

        Assert.AreEqual(42, ExpressionEvaluator.GetValue(null, "#holder.Child.SpelExtension(1)", vars));
    }

    [Test]
    public void ExtensionPropertyIsResolved()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionProperty<Foo>("Doubled", DoubledLambda, vars);

        Assert.AreEqual(82, ExpressionEvaluator.GetValue(null, "#foo.Doubled", vars));
    }

    [Test]
    public void DelegateExtensionPropertyIsResolved()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionProperty("Doubled", (Foo self) => self.InternalValue * 2, vars);

        Assert.AreEqual(82, ExpressionEvaluator.GetValue(null, "#foo.Doubled", vars));
    }

    [Test]
    public void RealMethodIsNotShadowedByExtension()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("ToString", "{|self| 'fake'}", vars);

        Assert.AreEqual("real", ExpressionEvaluator.GetValue(null, "#foo.ToString()", vars));
    }

    [Test]
    public void RealPropertyIsNotShadowedByExtension()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionProperty<Foo>("InternalValue", "{|self| 999}", vars);

        Assert.AreEqual(41, ExpressionEvaluator.GetValue(null, "#foo.InternalValue", vars));
    }

    [Test]
    public void LegacyMethodCallProcessorStillWins()
    {
        Dictionary<string, object> vars = CreateVariables();
        vars["Clash"] = new MyTestMethodCallProcessor();
        Expression.RegisterExtensionMethod<Foo>("Clash", "{|self| 'extension'}", vars);

        Assert.AreEqual("processor", ExpressionEvaluator.GetValue(null, "#foo.Clash()", vars));
    }

    [Test]
    public void UnregisteredMethodStillThrowsIdenticalArgumentException()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("SpelExtension", SpelExtensionLambda, vars);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "#foo.NoSuchMethod()", vars));
        Assert.AreEqual(MissingMethodMessage("NoSuchMethod"), ex.Message);
    }

    [Test]
    public void UnregisteredMethodWithoutRegistryStillThrows()
    {
        Foo foo = CreateFoo();
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["foo"] = foo;

        ArgumentException ex = Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "#foo.NoSuchMethod()", vars));
        Assert.AreEqual(MissingMethodMessage("NoSuchMethod"), ex.Message);

        IExpression expression = Expression.Parse("NoSuchMethod()");
        ArgumentException nullVarsEx = Assert.Throws<ArgumentException>(() => expression.GetValue(foo, null));
        Assert.AreEqual(MissingMethodMessage("NoSuchMethod"), nullVarsEx.Message);
    }

    [Test]
    public void UnregisteredPropertyStillThrowsInvalidPropertyException()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionProperty<Foo>("Doubled", DoubledLambda, vars);

        InvalidPropertyException ex = Assert.Throws<InvalidPropertyException>(() => ExpressionEvaluator.GetValue(null, "#foo.NoSuchProp", vars));
        Assert.IsNotNull(ex);
        Assert.IsTrue(ex.Message.Contains("NoSuchProp"), ex.Message);
    }

    [Test]
    public void ExtensionMethodDoesNotLeakAcrossVariableDictionaries()
    {
        Foo foo = CreateFoo();
        IExpression expression = Expression.Parse("#foo.SpelExtension(1)");

        Dictionary<string, object> varsA = new Dictionary<string, object>();
        varsA["foo"] = foo;
        Expression.RegisterExtensionMethod<Foo>("SpelExtension", SpelExtensionLambda, varsA);

        Dictionary<string, object> varsB = new Dictionary<string, object>();
        varsB["foo"] = foo;

        Assert.AreEqual(42, expression.GetValue(null, varsA));
        Assert.Throws<ArgumentException>(() => expression.GetValue(null, varsB));
        Assert.AreEqual(42, expression.GetValue(null, varsA));
    }

    [Test]
    public void ExtensionPropertyDoesNotLeakAcrossVariableDictionaries()
    {
        Foo foo = CreateFoo();
        IExpression expression = Expression.Parse("#foo.Doubled");

        Dictionary<string, object> varsA = new Dictionary<string, object>();
        varsA["foo"] = foo;
        Expression.RegisterExtensionProperty<Foo>("Doubled", DoubledLambda, varsA);

        Dictionary<string, object> varsB = new Dictionary<string, object>();
        varsB["foo"] = foo;

        Assert.AreEqual(82, expression.GetValue(null, varsA));
        Assert.Throws<InvalidPropertyException>(() => expression.GetValue(null, varsB));
        Assert.AreEqual(82, expression.GetValue(null, varsA));
    }

    [Test]
    public void ExtensionIsIgnoredWhenVariablesAreNull()
    {
        Foo foo = CreateFoo();

        IExpression methodExpression = Expression.Parse("SpelExtension(1)");
        ArgumentException ex = Assert.Throws<ArgumentException>(() => methodExpression.GetValue(foo, null));
        Assert.AreEqual(MissingMethodMessage("SpelExtension"), ex.Message);

        IExpression propertyExpression = Expression.Parse("Doubled");
        Assert.Throws<InvalidPropertyException>(() => propertyExpression.GetValue(foo, null));
    }

    [Test]
    public void ExtensionIsNotResolvedForTypeContext()
    {
        TypeRegistry.RegisterType(FooTypeAlias, typeof(Foo));

        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("SpelExtension", SpelExtensionLambda, vars);

        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "ExtensionTestFoo.SpelExtension(1)", vars));
    }

    [Test]
    public void ExtensionRegisteredForSystemTypeAppliesToTypeContext()
    {
        TypeRegistry.RegisterType(FooTypeAlias, typeof(Foo));

        Dictionary<string, object> vars = new Dictionary<string, object>();
        Expression.RegisterExtensionMethod<Type>("Describe", "{|self| $self.Name}", vars);

        Assert.AreEqual("Foo", ExpressionEvaluator.GetValue(null, "ExtensionTestFoo.Describe()", vars));
    }

    [Test]
    public void ExtensionRegisteredOnBaseClassAppliesToDerivedType()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();
        Bar bar = new Bar();
        bar.InternalValue = 41;
        vars["bar"] = bar;
        Expression.RegisterExtensionMethod<Foo>("SpelExtension", SpelExtensionLambda, vars);

        Assert.AreEqual(42, ExpressionEvaluator.GetValue(null, "#bar.SpelExtension(1)", vars));
    }

    [Test]
    public void ExtensionRegisteredOnInterfaceAppliesToImplementation()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();
        Baz baz = new Baz();
        baz.InternalValue = 41;
        vars["baz"] = baz;
        Expression.RegisterExtensionMethod(typeof(IHasValue), "SpelExtension", SpelExtensionLambda, vars);

        Assert.AreEqual(42, ExpressionEvaluator.GetValue(null, "#baz.SpelExtension(1)", vars));
    }

    [Test]
    public void MostDerivedRegistrationWins()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();
        Bar bar = new Bar();
        bar.InternalValue = 41;
        vars["bar"] = bar;
        Expression.RegisterExtensionMethod<Foo>("Which", "{|self| 'base'}", vars);
        Expression.RegisterExtensionMethod<Bar>("Which", "{|self| 'derived'}", vars);

        Assert.AreEqual("derived", ExpressionEvaluator.GetValue(null, "#bar.Which()", vars));
    }

    [Test]
    public void ExtensionLookupIsCaseInsensitive()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("SpelExtension", SpelExtensionLambda, vars);

        Assert.AreEqual(42, ExpressionEvaluator.GetValue(null, "#foo.spelextension(1)", vars));
    }

    [Test]
    public void ExtensionLambdaResolvesUnqualifiedMembersAgainstReceiver()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("Bumped", "{|self| InternalValue + 1}", vars);

        Assert.AreEqual(42, ExpressionEvaluator.GetValue(null, "#foo.Bumped()", vars));
    }

    [Test]
    public void ExtensionLambdaSeesGlobalFunctionsAndVariables()
    {
        Dictionary<string, object> vars = CreateVariables();
        vars["offset"] = 1;
        Expression.RegisterFunction("triple", "{|n| $n * 3}", vars);
        Expression.RegisterExtensionMethod<Foo>("Tripled", "{|self| #triple($self.InternalValue) + #offset}", vars);

        Assert.AreEqual(124, ExpressionEvaluator.GetValue(null, "#foo.Tripled()", vars));
    }

    [Test]
    public void ExtensionLambdaReceivesLambdaArgumentUnevaluated()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("Apply", "{|self, f| $f($self.InternalValue)}", vars);

        Assert.AreEqual(82, ExpressionEvaluator.GetValue(null, "#foo.Apply({|n| $n * 2})", vars));
    }

    [Test]
    public void ExtensionMethodWithWrongArgumentCountThrowsArgumentException()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("SpelExtension", SpelExtensionLambda, vars);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "#foo.SpelExtension()", vars));
        Assert.AreEqual(ArityMessage("SpelExtension", typeof(Foo), 2, 1), ex.Message);
    }

    [Test]
    public void ExtensionDelegateWithWrongArgumentCountThrowsArgumentException()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod("SpelExtension", (Foo self, int arg) => self.InternalValue + arg, vars);

        ArgumentException ex = Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "#foo.SpelExtension()", vars));
        Assert.AreEqual(ArityMessage("SpelExtension", typeof(Foo), 2, 1), ex.Message);
    }

    [Test]
    public void RegisteringDelegateWithoutParametersThrows()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();

        Assert.Catch<ArgumentException>(() => Expression.RegisterExtensionMethod<Foo>("X", (Delegate) new Func<int>(() => 42), vars));
    }

    [Test]
    public void RegisteringDelegateWithIncompatibleReceiverThrows()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();

        Assert.Catch<ArgumentException>(() => Expression.RegisterExtensionMethod(typeof(Foo), "X", new Func<string, int>(s => 1), vars));
    }

    [Test]
    public void RegisteringPropertyLambdaWithWrongArityThrows()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();

        Assert.Catch<ArgumentException>(() => Expression.RegisterExtensionProperty<Foo>("P", "{|a, b| 1}", vars));
    }

    [Test]
    public void RegisterExtensionMethodValidatesArguments()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();

        Assert.Catch<ArgumentException>(() => Expression.RegisterExtensionMethod<Foo>(null, DoubledLambda, vars));
        Assert.Catch<ArgumentException>(() => Expression.RegisterExtensionMethod<Foo>(string.Empty, DoubledLambda, vars));
        Assert.Catch<ArgumentException>(() => Expression.RegisterExtensionMethod<Foo>("X", (string) null, vars));
        Assert.Catch<ArgumentException>(() => Expression.RegisterExtensionMethod<Foo>("X", DoubledLambda, null));
        Assert.Catch<ArgumentException>(() => Expression.RegisterExtensionMethod(null, "X", DoubledLambda, vars));
    }

    [Test]
    public void InvalidLambdaExpressionThrowsSyntaxErrorException()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();

        // Spring.Expressions.SyntaxErrorException is internal to Spring.Core, so it cannot be
        // named here - assert on the concrete runtime type instead.
        Exception ex = Assert.Catch(() => Expression.RegisterExtensionMethod<Foo>("X", "{|self ", vars));
        Assert.AreEqual("Spring.Expressions.SyntaxErrorException", ex.GetType().FullName);
    }

    [Test]
    public void SettingExtensionPropertyThrowsNotWritablePropertyException()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionProperty<Foo>("Doubled", DoubledLambda, vars);

        Assert.Throws<NotWritablePropertyException>(() => ExpressionEvaluator.SetValue(null, "#foo.Doubled", vars, 1));
    }

    [Test]
    public void ReRegisteringReplacesPreviousExtension()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("V", "{|self| 1}", vars);
        Expression.RegisterExtensionMethod<Foo>("V", "{|self| 2}", vars);

        Assert.AreEqual(2, ExpressionEvaluator.GetValue(null, "#foo.V()", vars));
    }

    [Test]
    public void RegistryIsStoredUnderReservedVariableName()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("SpelExtension", SpelExtensionLambda, vars);

        Assert.AreEqual(2, vars.Count);

        int reservedKeys = 0;
        foreach (string key in vars.Keys)
        {
            if (key.StartsWith(Expression.ReservedVariableNames.RESERVEDPREFIX, StringComparison.Ordinal))
            {
                reservedKeys++;
            }
        }

        Assert.AreEqual(1, reservedKeys);
        Assert.AreEqual(41, ExpressionEvaluator.GetValue(null, "#foo.InternalValue", vars));
    }

    [Test]
    public void ConcurrentEvaluationOfSharedExpressionIsSafe()
    {
        Dictionary<string, object> vars = CreateVariables();
        Expression.RegisterExtensionMethod<Foo>("SpelExtension", SpelExtensionLambda, vars);

        IExpression expression = Expression.Parse("#foo.SpelExtension(1)");
        int[] failures = new int[1];

        Parallel.For(0, 1000, delegate(int i)
        {
            object result = expression.GetValue(null, vars);
            if (!Equals(42, result))
            {
                Interlocked.Increment(ref failures[0]);
            }
        });

        Assert.AreEqual(0, failures[0]);
    }

    private static Foo CreateFoo()
    {
        Foo foo = new Foo();
        foo.InternalValue = 41;
        return foo;
    }

    private static Dictionary<string, object> CreateVariables()
    {
        Dictionary<string, object> variables = new Dictionary<string, object>();
        variables["foo"] = CreateFoo();
        return variables;
    }

    private static string MissingMethodMessage(string methodName)
    {
        return string.Format("Method '{0}' with the specified number and types of arguments does not exist.", methodName);
    }

    private static string ArityMessage(string extensionName, Type targetType, int expected, int actual)
    {
        return string.Format("Extension '{0}' registered for type '{1}' expects {2} argument(s) including the receiver, but was called with {3}.", extensionName, targetType.FullName, expected, actual);
    }

    private interface IHasValue
    {
        int InternalValue { get; }
    }

    private class Foo : IHasValue
    {
        public int InternalValue { get; set; }

        public override string ToString()
        {
            return "real";
        }
    }

    private class Bar : Foo
    {
    }

    private class Baz : IHasValue
    {
        public int InternalValue { get; set; }
    }

    private class Holder
    {
        public Foo Child { get; set; }
    }

    private class MyTestMethodCallProcessor : IMethodCallProcessor
    {
        public object Process(object context, object[] args)
        {
            return "processor";
        }
    }
}
