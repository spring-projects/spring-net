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

using System.Collections;
using System.Diagnostics;
using System.EnterpriseServices;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Services;
using NUnit.Framework;
using Spring.Collections;
using Spring.Context;
using Spring.Context.Support;
using Spring.Core;
using Spring.Core.TypeResolution;
using Spring.Expressions.Processors;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Threading;
using Spring.Util;

namespace Spring.Expressions;

/// <summary>
/// This class contains tests for ExpressionEvaluator.
/// </summary>
/// <author>Aleksandar Seovic</author>
[TestFixture]
public sealed class ExpressionEvaluatorTests
{
    public class AsyncTestExpressionEvaluation : AsyncTestTask
    {
        private IExpression exp;
        private object rootContext;
        private object expected;
        private IDictionary<string, object> variables;

        public AsyncTestExpressionEvaluation(int iterations, IExpression exp, object rootContext, object expected,
            IDictionary<string, object> variables)
            : base(iterations)
        {
            this.exp = exp;
            this.rootContext = rootContext;
            this.expected = expected;
            this.variables = variables;
        }

        public override void DoExecute()
        {
            object result = exp.GetValue(rootContext, variables);
            Assert.That(result, Is.EqualTo(expected));
        }
    }

    private Inventor tesla;
    private Inventor pupin;
    private Society ieee;

    /// <summary>
    /// The setup logic executed before the execution of each individual test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        ContextRegistry.Clear();
        tesla = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
        tesla.Inventions = new string[] { "Telephone repeater", "Rotating magnetic field principle", "Polyphase alternating-current system", "Induction motor", "Alternating-current power transmission", "Tesla coil transformer", "Wireless communication", "Radio", "Fluorescent lights" };
        tesla.PlaceOfBirth.City = "Smiljan";

        pupin = new Inventor("Mihajlo Pupin", new DateTime(1854, 10, 9), "Serbian");
        pupin.Inventions =
            new string[] { "Long distance telephony & telegraphy", "Secondary X-Ray radiation", "Sonar" };
        pupin.PlaceOfBirth.City = "Idvor";
        pupin.PlaceOfBirth.Country = "Serbia";

        ieee = new Society();
        ieee.Members.Add(tesla);
        ieee.Members.Add(pupin);
        ieee.Officers["president"] = pupin;
        ieee.Officers["advisors"] = new Inventor[] { tesla, pupin };
        // not historically accurate, but I need an array in the map ;-)

        TypeRegistry.RegisterType("Society", typeof(Society));
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        //DynamicCodeManager.SaveAssembly();
    }

    /// <summary>
    /// GetObjectData() is not overridden on purpose !!!
    /// </summary>
    [Serializable]
    private class SerializationTestExpression : BaseNode
    {
        private int testValue = 0;

        public int TestValue
        {
            get { return testValue; }
        }

        public SerializationTestExpression(int testValue)
        {
            this.testValue = testValue;
        }

        protected SerializationTestExpression(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected override object Get(object context, EvaluationContext evalContext)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Tests serialization + deserialization of all BaseNode derived types
    /// </summary>
    [Test]
    public void AllExpressionNodeTypesAreSerializable()
    {
        Type[] possibleTypes = typeof(BaseNode).Assembly.GetTypes();

        BinaryFormatter formatter = new BinaryFormatter();

        // look for all BaseNode derived types defined in assembly Spring.Core
        for (int i = 0; i < possibleTypes.Length; i++)
        {
            Type t = possibleTypes[i];
            if (t != typeof(BaseNode)
                && typeof(BaseNode).IsAssignableFrom(t)
                && (!t.IsAbstract)
               )
            {
                //Console.WriteLine("testing " + t.FullName);

                // create using for public default ctor
                IExpression exp = (IExpression) Activator.CreateInstance(t, true);
                // serialize and deserialize it
                exp = SerializeDeserializeExpression(exp);
                exp = SerializeDeserializeExpressionUsingSoap(exp);
            }
        }
    }

    /// <summary>
    /// <see cref="SpringAST"/> implements <see cref="ISerializable"/>.
    /// Thus members in derived classes won't get automatically serialized.
    /// </summary>
    [Test]
    public void MembersDontGetSerializedByDefault()
    {
        SerializationTestExpression exp = new SerializationTestExpression(5);
        Assert.That(exp.TestValue, Is.EqualTo(5));
        SerializationTestExpression exp2 = (SerializationTestExpression) SerializeDeserializeExpression(exp);
        Assert.That(exp2.TestValue, Is.EqualTo(0));
    }

    /// <summary>
    /// This test ensures, that the default node-type is serializable.
    /// </summary>
    /// <remarks>
    /// date() is parsed into DateLiteralNode( down:&lt;default node type&gt; ).
    /// </remarks>
    [Test]
    public void ExpressionDateLiteralNodeMaintainsStateAfterSerialization()
    {
        IExpression exp = Expression.Parse("date('08-24-1974', 'MM-dd-yyyy')");

        Assert.That(exp.GetValue(null), Is.EqualTo(new DateTime(1974, 8, 24)));

        exp = SerializeDeserializeExpression(exp);

        Assert.That(exp.GetValue(null), Is.EqualTo(new DateTime(1974, 8, 24)));
    }

    private static IExpression SerializeDeserializeExpression(IExpression exp)
    {
        byte[] data;
        BinaryFormatter formatter = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            formatter.Serialize(ms, exp);
            ms.Flush();
            data = ms.ToArray();
        }

        using (MemoryStream ms = new MemoryStream(data))
        {
            exp = (IExpression) formatter.Deserialize(ms);
        }

        return exp;
    }

    private static IExpression SerializeDeserializeExpressionUsingSoap(IExpression exp)
    {
        string xml;
        SoapFormatter formatter = new SoapFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            formatter.Serialize(ms, exp);
            ms.Position = 0;
            byte[] b = new byte[ms.Length];
            ms.Read(b, 0, (int) ms.Length);
            xml = Encoding.ASCII.GetString(b, 0, b.Length);
        }

        using (StringReader sr = new StringReader(xml))
        {
            byte[] b = Encoding.ASCII.GetBytes(xml);
            Stream stream = new MemoryStream(b);
            exp = (IExpression) formatter.Deserialize(stream);
        }

        return exp;
    }

    [Test]
    public void TestConstantRead()
    {
        object value = ExpressionEvaluator.GetValue(null, "Society.ByteConst == 1");
        Assert.That(value, Is.EqualTo(true));
    }

    [Test]
    public void TestBitwiseXOR()
    {
        object value = ExpressionEvaluator.GetValue(null, "'123' + 1");
        Assert.That(value, Is.EqualTo("1231"));
    }

    [Test]
    public void TestMixedAddition()
    {
        object value = ExpressionEvaluator.GetValue(null, "'123' + 1");
        Assert.That(value, Is.EqualTo("1231"));
    }

    [Test(Description = "SPRNET-1507 - Test 1")]
    public void TestExpandoObject()
    {
        dynamic dynamicObject = new System.Dynamic.ExpandoObject();
        //add property at run-time
        dynamicObject.IssueId = "1507";

        object value = ExpressionEvaluator.GetValue(dynamicObject, "IssueId");
        Assert.That(value, Is.EqualTo("1507"));
    }

    [Test(Description = "SPRNET-1507 - Test 2")]
    public void TestExpandoObjectWithNotExistedProperty()
    {
        try
        {
            dynamic dynamicObject = new System.Dynamic.ExpandoObject();

            ExpressionEvaluator.GetValue(dynamicObject, "PropertyName");
            Assert.Fail();
        }
        catch (InvalidPropertyException ex)
        {
            Assert.That(
                ex.Message,
                Is.EqualTo("'PropertyName' node cannot be resolved for the specified context [System.Dynamic.ExpandoObject]."));
        }
    }

    [Test(Description = "SPRNET-944")]
    public void DateTests()
    {
        string dateLiteral = (string) ExpressionEvaluator.GetValue(null, "'date'");
        Assert.That(dateLiteral, Is.EqualTo("date"));
    }

    [Test(Description = "http://jira.springframework.org/browse/SPRNET-944")]
    public void TestDateVariableExpression()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["date"] = "2008-05-15";
        object value = ExpressionEvaluator.GetValue(null, "#date", vars);
        Assert.That(value, Is.EqualTo("2008-05-15"));
    }

    [Test(Description = "http://jira.springframework.org/browse/SPRNET-1155")]
    public void TestDateVariableExpressionCamelCased()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["Date"] = "2008-05-15";
        object value = ExpressionEvaluator.GetValue(null, "#Date", vars);
        Assert.That(value, Is.EqualTo("2008-05-15"));
    }

    [Test]
    public void ThrowsSyntaxErrorException()
    {
        try
        {
            ExpressionEvaluator.GetValue(null, "'date"); // unclose string literal
            Assert.Fail();
        }
        catch (SyntaxErrorException ex)
        {
            Assert.That(ex.Message, Is.EqualTo("Syntax Error on line 1, column 6: expecting ''', found '<EOF>' in expression" + Environment.NewLine + "''date'"));
        }
    }

    /// <summary>
    /// Should throw exception for null root object
    /// </summary>
    [Test]
    public void NullRoot()
    {
        Assert.Throws<NullValueInNestedPathException>(() => ExpressionEvaluator.GetValue(null, "dummy.expression"));
    }

    /// <summary>
    /// Should throw exception for null root object
    /// </summary>
    [Test]
    public void TryingToSetTheValueOfNonSettableNode()
    {
        Assert.Throws<NotSupportedException>(() => ExpressionEvaluator.SetValue(null, "10", 5));
    }

    /// <summary>
    /// Should return root itself for empty expression
    /// </summary>
    [Test]
    public void GetNullOrEmptyExpression()
    {
        DateTime now = DateTime.Now;
        Assert.That(now, Is.EqualTo(ExpressionEvaluator.GetValue(now, null)));
        Assert.That(now, Is.EqualTo(ExpressionEvaluator.GetValue(now, "")));
    }

    /// <summary>
    /// Should fail when setting value for the empty expression
    /// </summary>
    [Test]
    public void SetNullOrEmptyExpression()
    {
        Assert.Throws<NotSupportedException>(() => ExpressionEvaluator.SetValue("xyz", null, "abc"));
    }

    /// <summary>
    /// Tests null literal.
    /// </summary>
    [Test]
    public void TestNullLiteral()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "null"), Is.Null);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'xyz' == null"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null != 'xyz'"), Is.True);
    }

    [Test]
    public void TestUnicode()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "'\u6f22\u5b57'"), Is.EqualTo("\u6f22\u5b57"));
    }

    /// <summary>
    /// Tests string literals.
    /// </summary>
    [Test]
    public void TestStringLiterals()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "'literal string'"), Is.EqualTo("literal string"));
        Assert.That(ExpressionEvaluator.GetValue(null, "'literal ''string'"), Is.EqualTo("literal 'string"));
        Assert.That(ExpressionEvaluator.GetValue(null, "''"), Is.Empty);
        Assert.That(ExpressionEvaluator.GetValue(null, "'escaped \t string \n'"), Is.EqualTo("escaped \t string \n"));
        //Debug.Write(ExpressionEvaluator.GetValue(null, "'escaped\tstring\nsecond line\n\nfourth line'"));
    }

    /// <summary>
    /// Tests integer literals.
    /// </summary>
    [Test]
    public void TestIntLiterals()
    {
        object int32 = ExpressionEvaluator.GetValue(null, Int32.MaxValue.ToString());
        Assert.That(int32, Is.EqualTo(Int32.MaxValue));
        Assert.That(int32 is Int32, Is.True);
        Assert.That(ExpressionEvaluator.GetValue(null, "0x20"), Is.EqualTo(32));

        Assert.That(ExpressionEvaluator.GetValue(null, Int64.MaxValue.ToString() + ".ToString()"), Is.EqualTo(Int64.MaxValue.ToString()));
        Assert.That(ExpressionEvaluator.GetValue(null, "long.MaxValue.ToString()"), Is.EqualTo(Int64.MaxValue.ToString()));

        object int64 = ExpressionEvaluator.GetValue(null, Int64.MaxValue.ToString());
        Assert.That(int64, Is.EqualTo(Int64.MaxValue));
        Assert.That(int64 is Int64, Is.True);
    }

    /// <summary>
    /// Tests hexadecimal integer literals.
    /// </summary>
    [Test]
    public void TestHexLiterals()
    {
        IExpression exp = Expression.Parse("0x20");
        Assert.That(exp.GetValue(), Is.EqualTo(32));
        Assert.That(exp.GetValue(), Is.EqualTo(32));
        Assert.That(ExpressionEvaluator.GetValue(null, "0xFF"), Is.EqualTo(255));
        Assert.That(ExpressionEvaluator.GetValue(null, "0x7FFFFFFF"), Is.EqualTo(Int32.MaxValue));
        Assert.That(ExpressionEvaluator.GetValue(null, "0x7FFFFFFFFFFFFFFF"), Is.EqualTo(Int64.MaxValue));
        Assert.That(ExpressionEvaluator.GetValue(null, "0x80000000"), Is.EqualTo(Int32.MinValue));
        Assert.That(ExpressionEvaluator.GetValue(null, "0x8000000000000000"), Is.EqualTo(Int64.MinValue));
    }

    /// <summary>
    /// Tests real literals.
    /// </summary>
    [Test]
    public void TestRealLiterals()
    {
        IExpression exp = Expression.Parse("3.402823E+38");
        exp.GetValue();
        object s = exp.GetValue();
        object d = ExpressionEvaluator.GetValue(null, "1.797693E+308");
        object dec = ExpressionEvaluator.GetValue(null, "1000.00m");

        Assert.That(s is Double, Is.True);
        Assert.That(d is Double, Is.True);
        Assert.That(dec is Decimal, Is.True);

        Assert.That(s, Is.EqualTo(3.402823E+38));
        Assert.That(d, Is.EqualTo(1.797693E+308));

        Assert.That(ExpressionEvaluator.GetValue(null, "5.25f"), Is.EqualTo(5.25F));
        Assert.That(ExpressionEvaluator.GetValue(null, "0.75D"), Is.EqualTo(0.75d));

        Assert.That((bool) ExpressionEvaluator.GetValue(null, "1000 == 1e3 and 1e+4 != 1000"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "100 < 1000.00m and 10000.00 > 1000"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "100 < 1000.00 and 10000.00m > 1e2"), Is.True);
    }

    /// <summary>
    /// Tests boolean literals.
    /// </summary>
    [Test]
    public void TestBooleanLiterals()
    {
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false"), Is.False);
    }

    /// <summary>
    /// Tests date literals.
    /// </summary>
    [Test]
    public void TestDateLiterals()
    {
        IExpression exp = Expression.Parse("date('1974/08/24')");
        Assert.That(exp.GetValue(), Is.EqualTo(new DateTime(1974, 8, 24)));
        Assert.That(exp.GetValue(), Is.EqualTo(new DateTime(1974, 8, 24)));
        Assert.That(ExpressionEvaluator.GetValue(null, "date('1974-08-24')"), Is.EqualTo(new DateTime(1974, 8, 24)));
        Assert.That(ExpressionEvaluator.GetValue(null, "date('08-24-1974', 'MM-dd-yyyy')"), Is.EqualTo(new DateTime(1974, 8, 24)));
        Assert.That(ExpressionEvaluator.GetValue(null, "date('08/24/1974', 'MM/dd/yyyy')"), Is.EqualTo(new DateTime(1974, 8, 24)));
        Assert.That(ExpressionEvaluator.GetValue(null, "date('1974-08-24 12:35:06Z', 'u')"), Is.EqualTo(new DateTime(1974, 8, 24, 12, 35, 6)));
        Assert.That(ExpressionEvaluator.GetValue(null, "date('1974/08/24').Year"), Is.EqualTo(1974));
        Assert.That(ExpressionEvaluator.GetValue(null, "date('1974/08/24').AddYears(31).Year"), Is.EqualTo(2005));
    }

    /// <summary>
    /// Tests simple property and field accessors and mutators
    /// </summary>
    [Test]
    public void TestSimplePropertyAccess()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "DateTime.Today"), Is.EqualTo(DateTime.Today));
        Assert.That(ExpressionEvaluator.GetValue(tesla, "Name"), Is.EqualTo("Nikola Tesla"));
        Assert.That(ExpressionEvaluator.GetValue(pupin, "PlaceOfBirth.City"), Is.EqualTo("Idvor"));
        ExpressionEvaluator.SetValue(tesla, "PlaceOfBirth.Country", "Croatia");
        Assert.That(ExpressionEvaluator.GetValue(tesla, "PlaceOfBirth.Country"), Is.EqualTo("Croatia"));
        ExpressionEvaluator.SetValue(pupin, "Name", "Michael Pupin");
        Assert.That(ExpressionEvaluator.GetValue(pupin, "Name"), Is.EqualTo("Michael Pupin"));
        Assert.That(ExpressionEvaluator.GetValue(tesla, "DOB"), Is.EqualTo(new DateTime(1856, 7, 9)));
        Assert.That(ExpressionEvaluator.GetValue(tesla, "DOB.Year"), Is.EqualTo(1856));
    }

    /// <summary>
    /// Tests that simple property and field accessors and mutators are case-insensitive.
    /// </summary>
    [Test]
    public void SimplePropertyAccessIsCaseInsensitive()
    {
        Assert.That(ExpressionEvaluator.GetValue(tesla, "nAme"), Is.EqualTo("Nikola Tesla"));
        Assert.That(ExpressionEvaluator.GetValue(pupin, "Placeofbirth.city"), Is.EqualTo("Idvor"));
        ExpressionEvaluator.SetValue(tesla, "PlaceOfBirth.CountRY", "Croatia");
        Assert.That(ExpressionEvaluator.GetValue(tesla, "Placeofbirth.COUNtry"), Is.EqualTo("Croatia"));
        ExpressionEvaluator.SetValue(pupin, "NAME", "Michael Pupin");
        Assert.That(ExpressionEvaluator.GetValue(pupin, "name"), Is.EqualTo("Michael Pupin"));
        Assert.That(ExpressionEvaluator.GetValue(tesla, "dob"), Is.EqualTo(new DateTime(1856, 7, 9)));
        Assert.That(ExpressionEvaluator.GetValue(tesla, "DOb.YEar"), Is.EqualTo(1856));
    }

    /// <summary>
    /// Tests setting and getting shadowed properties
    /// </summary>
    [Test]
    public void TestShadowedPropertyAccess()
    {
        ShadowingTestsMostSpezializedClass o;

        // test read
        o = new ShadowingTestsMostSpezializedClass();
        o.SomeValue = "SomeString";
        Assert.That(ExpressionEvaluator.GetValue(o, "SomeValue"), Is.EqualTo("SomeString"));

        // test write
        o = new ShadowingTestsMostSpezializedClass();
        ExpressionEvaluator.SetValue(o, "SomeValue", "SomeOtherString");
        Assert.That(o.SomeValue, Is.EqualTo("SomeOtherString"));

        // test readonly shadowed
        o = new ShadowingTestsMostSpezializedClass();
        ((ShadowingTestsBaseClass) o).ReadonlyShadowedValue = "SomeString1";
        Assert.That(ExpressionEvaluator.GetValue(o, "ReadonlyShadowedValue"), Is.EqualTo("SomeString1"));
        try
        {
            ExpressionEvaluator.SetValue(o, "ReadonlyShadowedValue", "SomeString2");
            Assert.Fail("Setting readonly property should throw NotWritablePropertyException");
        }
        catch (NotWritablePropertyException)
        {
        }

        Assert.That(ExpressionEvaluator.GetValue(o, "ReadonlyShadowedValue"), Is.EqualTo("SomeString1"));

        // test writeonly shadowed
        o = new ShadowingTestsMostSpezializedClass();
        ExpressionEvaluator.SetValue(o, "WriteonlyShadowedValue", "SomeString3");
        Assert.That(((ShadowingTestsBaseClass) o).WriteonlyShadowedValue, Is.EqualTo("SomeString3"));
        try
        {
            ExpressionEvaluator.GetValue(o, "WriteonlyShadowedValue");
            Assert.Fail("Getting writeonly property should throw NotReadablePropertyException");
        }
        catch (NotReadablePropertyException)
        {
        }
    }

    /// <summary>
    /// Tests indexed property and field accessors and mutators
    /// </summary>
    [Test]
    public void TestIndexedPropertyAccess()
    {
        TypeRegistry.RegisterType("Society", typeof(Society));

        // arrays and lists
        Assert.That(ExpressionEvaluator.GetValue(tesla, "Inventions[3]"), Is.EqualTo("Induction motor"));
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Members[0].Name"), Is.EqualTo("Nikola Tesla"));
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Members[0].Inventions[6]"), Is.EqualTo("Wireless communication"));

        // maps
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Officers['president']"), Is.EqualTo(pupin));
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Officers['president'].PlaceOfBirth.City"), Is.EqualTo("Idvor"));
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Officers['advisors'][0]"), Is.EqualTo(tesla));
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Officers['advisors'][0].Inventions[2]"),
            Is.EqualTo("Polyphase alternating-current system"));

        // maps with non-literal parameters
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["prez"] = "president";
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Officers[#prez]", vars), Is.EqualTo(pupin));

        Assert.That(ExpressionEvaluator.GetValue(ieee, "Officers[Society.President]"), Is.EqualTo(pupin));
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Officers[Society.President].PlaceOfBirth.City"), Is.EqualTo("Idvor"));
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Officers[Society.Advisors][0]"), Is.EqualTo(tesla));
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Officers[Society.Advisors][0].Inventions[2]"),
            Is.EqualTo("Polyphase alternating-current system"));

        // try to set some values
        ExpressionEvaluator.SetValue(ieee, "Officers['advisors'][0].PlaceOfBirth.Country", "Croatia");
        Assert.That(ExpressionEvaluator.GetValue(tesla, "PlaceOfBirth.Country"), Is.EqualTo("Croatia"));
        ExpressionEvaluator.SetValue(ieee, "Officers['president'].Name", "Michael Pupin");
        Assert.That(ExpressionEvaluator.GetValue(pupin, "Name"), Is.EqualTo("Michael Pupin"));
        ExpressionEvaluator.SetValue(ieee, "Officers['advisors']", new Inventor[] { pupin, tesla });
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Officers['advisors'][0]"), Is.EqualTo(pupin));
        Assert.That(ExpressionEvaluator.GetValue(ieee, "Officers['advisors'][1]"), Is.EqualTo(tesla));

        // generic indexer
        Bar bar = new Bar();
        Foo foo = new Foo();
        IExpression exp = Expression.Parse("[1]");
        Assert.That(exp.GetValue(bar), Is.EqualTo(2));
        Assert.That(exp.GetValue(bar), Is.EqualTo(2));
        Assert.That(ExpressionEvaluator.GetValue(foo, "[1, 'test']"), Is.EqualTo("test_1"));
    }

    /// <summary>
    /// Tests indexer access with invalid number of indices
    /// </summary>
    [Test]
    public void TestIndexedPropertyAccessWithInvalidNumberOfIndices()
    {
        Assert.Throws<InvalidPropertyException>(() => ExpressionEvaluator.GetValue(tesla, "Inventions[3, 2]"));
    }

    /// <summary>
    /// Tests method accessors
    /// </summary>
    [Test]
    public void TestMethodAccess()
    {
        Guid guid = Guid.NewGuid();

        TypeRegistry.RegisterType("Guid", typeof(Guid));
        Assert.That(ExpressionEvaluator.GetValue(guid, "ToString()"), Is.EqualTo(guid.ToString()));
        Assert.That(ExpressionEvaluator.GetValue(guid, "ToString('n')"), Is.EqualTo(guid.ToString("n")));
        Assert.That(ExpressionEvaluator.GetValue(null, "Guid.NewGuid().ToByteArray().Length"), Is.EqualTo(16));

        Assert.That(ExpressionEvaluator.GetValue(ieee, "Members[0].GetAge(date('2005-01-01'))"),
            Is.EqualTo(2005 - tesla.DOB.Year));
    }

    [Test]
    public void TestMethodEvaluationOnDifferentContextType()
    {
        IExpression expression = Expression.Parse("ToString('dummy', null)");
        Assert.That(expression.GetValue(0m, null), Is.EqualTo("dummy"));
        Assert.That(expression.GetValue(0, null), Is.EqualTo("dummy"));
    }

    [Test]
    public void TestMethodEvaluationOnDifferentArgumentTypes()
    {
        IExpression expression = Expression.Parse("Foo(#var1)");
        MethodInvokationCases testContext = new MethodInvokationCases();
        Dictionary<string, object> args = new Dictionary<string, object>();
        args["var1"] = "myString";
        Assert.That(expression.GetValue(testContext, args), Is.EqualTo("myString"));
        args["var1"] = 12;
        Assert.That(expression.GetValue(testContext, args), Is.EqualTo(12));
    }

    /// <summary>
    /// Tests missing method accessors
    /// </summary>
    [Test]
    public void TestMissingMethodAccess()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue("xyz", "ToStringilyLingily()"));
    }

    /// <summary>
    /// Tests projection node
    /// </summary>
    [Test]
    public void TestProjection()
    {
        IList placesOfBirth = (IList) ExpressionEvaluator.GetValue(ieee, "Members.!{PlaceOfBirth.City}");

        Assert.That(placesOfBirth.Count, Is.EqualTo(2));
        Assert.That(placesOfBirth[0], Is.EqualTo("Smiljan"));
        Assert.That(placesOfBirth[1], Is.EqualTo("Idvor"));

        IList names = (IList) ExpressionEvaluator.GetValue(ieee, "Officers['advisors'].!{Name}");
        Assert.That(names.Count, Is.EqualTo(2));
        Assert.That(names[0], Is.EqualTo("Nikola Tesla"));
        Assert.That(names[1], Is.EqualTo("Mihajlo Pupin"));
    }

    /// <summary>
    /// Tests selection node
    /// </summary>
    [Test]
    public void TestSelection()
    {
        IList memberSelection =
            (IList) ExpressionEvaluator.GetValue(ieee, "Members.?{PlaceOfBirth.City == 'Smiljan'}");

        Assert.That(memberSelection.Count, Is.EqualTo(1));
        Assert.That(((Inventor) memberSelection[0]).Name, Is.EqualTo("Nikola Tesla"));

        IList serbianOfficers =
            (IList) ExpressionEvaluator.GetValue(ieee, "Officers['advisors'].?{Nationality == 'Serbian'}");
        Assert.That(serbianOfficers.Count, Is.EqualTo(2));
        Assert.That(((Inventor) serbianOfficers[0]).Name, Is.EqualTo("Nikola Tesla"));
        Assert.That(((Inventor) serbianOfficers[1]).Name, Is.EqualTo("Mihajlo Pupin"));

        Inventor first =
            (Inventor) ExpressionEvaluator.GetValue(ieee, "Officers['advisors'].^{Nationality == 'Serbian'}");
        Assert.That(first.Name, Is.EqualTo("Nikola Tesla"));

        Inventor last =
            (Inventor) ExpressionEvaluator.GetValue(ieee, "Officers['advisors'].${Nationality == 'Serbian'}");
        Assert.That(last.Name, Is.EqualTo("Mihajlo Pupin"));
    }

    /// <summary>
    /// Tests type node
    /// </summary>
    [Test]
    public void TestTypeNode()
    {
        IExpression exp = Expression.Parse("T(DateTime)");
        exp.GetValue();
        Assert.That(exp.GetValue(), Is.EqualTo(typeof(DateTime)));

        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.DateTime)"), Is.EqualTo(typeof(DateTime)));
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.DateTime[], mscorlib)"), Is.EqualTo(typeof(DateTime[])));
        Assert.That(ExpressionEvaluator.GetValue(null, "T(Spring.Expressions.ExpressionEvaluator, Spring.Core)"), Is.EqualTo(typeof(ExpressionEvaluator)));
        Assert.That((bool) ExpressionEvaluator.GetValue(tesla, "T(System.DateTime) == DOB.GetType()"), Is.True);
    }

    /// <summary>
    /// Tests type node
    /// </summary>
    [Test]
    public void TestTypeNodeWithArrays()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.DateTime[])"), Is.EqualTo(typeof(DateTime[])));
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.DateTime[,])"), Is.EqualTo(typeof(DateTime[,])));
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.DateTime[], mscorlib)"), Is.EqualTo(typeof(DateTime[])));
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.DateTime[,], mscorlib)"), Is.EqualTo(typeof(DateTime[,])));
    }

    /// <summary>
    /// Tests type node
    /// </summary>
    [Test]
    public void TestTypeNodeWithAssemblyQualifiedName()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, string.Format("T({0})", typeof(ExpressionEvaluator).AssemblyQualifiedName)), Is.EqualTo(typeof(ExpressionEvaluator)));
    }

    /// <summary>
    /// Tests type node
    /// </summary>
    [Test]
    public void TestTypeNodeWithGenericAssemblyQualifiedName()
    {
        //            Assert.That(ExpressionEvaluator.GetValue(null, "T(System.Nullable`1[System.Int32], mscorlib)"), Is.EqualTo(typeof(int?)));
        //            Assert.That(ExpressionEvaluator.GetValue(null, "T(System.Nullable`1[[System.Int32, mscorlib]], mscorlib)"), Is.EqualTo(typeof(int?)));
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.Nullable`1[[int]], mscorlib)"), Is.EqualTo(typeof(int?)));
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.Collections.Generic.Dictionary`2[System.String,System.Boolean],mscorlib)"), Is.EqualTo(typeof(System.Collections.Generic.Dictionary<string, bool>)));
    }

    [Test]
    public void TestGenericDictionary()
    {
        ExpressionEvaluator.GetValue(null,
            "T(System.Collections.Generic.Dictionary`2[System.String,System.Boolean],mscorlib)");
    }

    /// <summary>
    /// Tests type node
    /// </summary>
    [Test]
    public void TestTypeNodeWithAliasedGenericArguments()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.Collections.Generic.Dictionary`2[string,bool],mscorlib)"), Is.EqualTo(typeof(System.Collections.Generic.Dictionary<string, bool>)));
    }

    /// <summary>
    /// Tests type node
    /// </summary>
    [Test]
    public void TestTypeNodeWithGenericAssemblyQualifiedArrayName()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.Nullable`1[[System.Int32, mscorlib]][,], mscorlib)"), Is.EqualTo(typeof(int?[,])));
    }

    /// <summary>
    /// Tests constructor node
    /// </summary>
    [Test]
    public void TestConstructor()
    {
        TypeRegistry.RegisterType(typeof(Inventor));

        IExpression exp = Expression.Parse("new System.DateTime(2004, 8, 14)");
        Assert.That(ExpressionEvaluator.GetValue(null, "new Decimal(1000)"), Is.EqualTo(1000));
        Assert.That(exp.GetValue(null), Is.EqualTo(new DateTime(2004, 8, 14)));
        Assert.That(exp.GetValue("xyz"), Is.EqualTo(new DateTime(2004, 8, 14)));
        Assert.That(ExpressionEvaluator.GetValue(null, "new DateTime(2004, 8, 14).AddDays(10).AddYears(-30)"),
            Is.EqualTo(new DateTime(1974, 8, 24)));

        // test named arguments
        Inventor ana =
            (Inventor)
            ExpressionEvaluator.GetValue(null,
                "new Inventor(Name = 'Ana Maria Seovic', DOB = date('2004-08-14'), Nationality = 'American')");
        Assert.That(ana.Name, Is.EqualTo("Ana Maria Seovic"));
        Assert.That(ana.DOB, Is.EqualTo(new DateTime(2004, 8, 14)));
        Assert.That(ana.Nationality, Is.EqualTo("American"));

        Inventor aleks =
            (Inventor)
            ExpressionEvaluator.GetValue(null,
                "new Inventor('Aleksandar Seovic', date('1974-08-24'), 'Serbian', Inventions = {'SPELL'})");
        Assert.That(aleks.Name, Is.EqualTo("Aleksandar Seovic"));
        Assert.That(aleks.DOB, Is.EqualTo(new DateTime(1974, 8, 24)));
        Assert.That(aleks.Nationality, Is.EqualTo("Serbian"));
        Assert.That(aleks.Inventions.Length, Is.EqualTo(1));
        Assert.That(aleks.Inventions[0], Is.EqualTo("SPELL"));
    }

    /// <summary>
    /// Tests missing constructor
    /// </summary>
    [Test]
    public void TestMissingConstructor()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "new Decimal('xyz')"));
    }

    /// <summary>
    /// Tests expression list node
    /// </summary>
    [Test]
    public void TestExpressionList()
    {
        TypeRegistry.RegisterType("Inventor", typeof(Inventor));
        Assert.That(ExpressionEvaluator.GetValue(ieee.Members,
                "(Add(new Inventor('Aleksandar Seovic', date('1974-08-24'), 'Serbian')); Count)"),
            Is.EqualTo(3));
        Assert.That(ExpressionEvaluator.GetValue(ieee,
                "Members.(Add(new Inventor('Ana Maria Seovic', date('2004-08-14'), 'Serbian')); RemoveAt(1); Count)"),
            Is.EqualTo(3));
        Assert.That(ExpressionEvaluator.GetValue(ieee.Members,
                "([1].PlaceOfBirth.City = 'Beograd'; [1].PlaceOfBirth.Country = 'Serbia'; [1].Name)"),
            Is.EqualTo("Aleksandar Seovic"));
        Assert.That(((Inventor) ieee.Members[1]).PlaceOfBirth.City, Is.EqualTo("Beograd"));
    }

    /// <summary>
    /// Tests assignment node
    /// </summary>
    [Test]
    public void TestAssignNode()
    {
        Inventor inventor = new Inventor();
        Assert.That(ExpressionEvaluator.GetValue(inventor, "Name = 'Aleksandar Seovic'"), Is.EqualTo("Aleksandar Seovic"));
        Assert.That(ExpressionEvaluator.GetValue(inventor, "DOB = date('1974-08-24')"),
            Is.EqualTo(new DateTime(1974, 8, 24)));
        Assert.That(ExpressionEvaluator.GetValue(inventor, "Nationality = 'Serbian'"), Is.EqualTo("Serbian"));
        Assert.That(ExpressionEvaluator.GetValue(inventor,
                "(DOB = date('2004-08-14'); Name = 'Ana Maria Seovic')"),
            Is.EqualTo("Ana Maria Seovic"));
        Assert.That(inventor.DOB, Is.EqualTo(new DateTime(2004, 8, 14)));
        ExpressionEvaluator.GetValue(ieee, "Officers['vp'] = Members[0]");
        Assert.That(((Inventor) ieee.Officers["vp"]).Name, Is.EqualTo("Nikola Tesla"));
    }

    /// <summary>
    /// Tests default node
    /// </summary>
    [Test]
    public void TestDefaultNode()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "null ?? 'default'"), Is.EqualTo("default"));
        Assert.That(ExpressionEvaluator.GetValue(null, "null ?? 2 * 2 - 3"), Is.EqualTo(1));
        Assert.That(ExpressionEvaluator.GetValue(tesla, "null ?? #root.Name"), Is.EqualTo("Nikola Tesla"));

        Assert.That(ExpressionEvaluator.GetValue(null, "'default' ?? 'xyz'"), Is.EqualTo("default"));
        Assert.That(ExpressionEvaluator.GetValue(null, "2 * 2 - 3 ?? 5"), Is.EqualTo(1));
        Assert.That(ExpressionEvaluator.GetValue(tesla, "#root.Name ?? 'Pupin'"), Is.EqualTo("Nikola Tesla"));
    }

    /// <summary>
    /// Tests variable node
    /// </summary>
    [Test]
    public void TestVariableNode()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["newName"] = "Aleksandar Seovic";
        Assert.That(ExpressionEvaluator.GetValue(null, "#newName = 'Ana Maria Seovic'", vars),
            Is.EqualTo("Ana Maria Seovic"));
        Assert.That(ExpressionEvaluator.GetValue(tesla, "Name = #newName", vars), Is.EqualTo("Ana Maria Seovic"));
        Assert.That(ExpressionEvaluator.GetValue(tesla, "(#oldName = Name; Name = 'Nikola Tesla')", vars),
            Is.EqualTo("Nikola Tesla"));
        Assert.That(((Inventor) ExpressionEvaluator.GetValue(tesla, "#this", vars)).Name, Is.EqualTo("Nikola Tesla"));
        Assert.That(ExpressionEvaluator.GetValue(tesla, "(Nationality = 'Srbin'; #this).Name", vars),
            Is.EqualTo("Nikola Tesla"));
        Assert.That(tesla.Name, Is.EqualTo("Nikola Tesla"));
        Assert.That(tesla.Nationality, Is.EqualTo("Srbin"));
        Assert.That(vars["oldName"], Is.EqualTo("Ana Maria Seovic"));
        Assert.That(ExpressionEvaluator.GetValue(tesla, "#root", vars), Is.EqualTo(tesla));
    }

    /// <summary>
    /// Try to set 'this' variable
    /// </summary>
    [Test]
    public void TryToSetThis()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.SetValue(null, "#this", "xyz"));
    }

    /// <summary>
    /// Try to set 'root' variable
    /// </summary>
    [Test]
    public void TryToSetRoot()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.SetValue(null, "#root", "xyz"));
    }

    /// <summary>
    /// Tests ternary node
    /// </summary>
    [Test]
    public void TestTernaryNode()
    {
        IExpression exp = Expression.Parse("true ? 'trueExp' : 'falseExp'");
        exp.GetValue();

        Assert.That(exp.GetValue(), Is.EqualTo("trueExp"));
        Assert.That(ExpressionEvaluator.GetValue(null, "false ? 'trueExp' : 'falseExp'"), Is.EqualTo("falseExp"));
        Assert.That(ExpressionEvaluator.GetValue(null, "(true ? 'trueExp' : 'falseExp')"), Is.EqualTo("trueExp"));
        Assert.That(ExpressionEvaluator.GetValue(null, "(false ? 'trueExp' : 'falseExp')"), Is.EqualTo("falseExp"));

        ExpressionEvaluator.SetValue(ieee, "Name", "IEEE");
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["queryName"] = "Nikola Tesla";
        string expression =
            @"IsMember(#queryName)
                    ? #queryName + ' is a member of the ' + Name + ' Society'
                    : #queryName + ' is not a member of ' + Name + ' Society'";
        Assert.That(ExpressionEvaluator.GetValue(ieee, expression, vars),
            Is.EqualTo("Nikola Tesla is a member of the IEEE Society"));
    }

    /// <summary>
    /// Tests logical OR operator
    /// </summary>
    [Test]
    public void TestLogicalOrOperator()
    {
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true or true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false or false"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true or false"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false or true"), Is.True);
        string expression = @"IsMember('Nikola Tesla') or IsMember('Albert Einstien')";
        Assert.That((bool) ExpressionEvaluator.GetValue(ieee, expression), Is.True);
    }

    /// <summary>
    /// Tests bitwise OR operator
    /// </summary>
    [Test]
    public void TestBitwiseOrOperator()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "1 or 2"), Is.EqualTo(1 | 2));
        Assert.That(ExpressionEvaluator.GetValue(null, "1 or -2"), Is.EqualTo(1 | -2));
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.Text.RegularExpressions.RegexOptions).IgnoreCase or T(System.Text.RegularExpressions.RegexOptions).Compiled"), Is.EqualTo(RegexOptions.IgnoreCase | RegexOptions.Compiled));
    }

    /// <summary>
    /// Tests logical AND operator
    /// </summary>
    [Test]
    public void TestLogicalAndOperator()
    {
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true and true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false and false"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true and false"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false and true"), Is.False);
        string expression = @"IsMember('Nikola Tesla') and IsMember('Mihajlo Pupin')";
        Assert.That((bool) ExpressionEvaluator.GetValue(ieee, expression), Is.True);
    }

    /// <summary>
    /// Tests bitwise OR operator
    /// </summary>
    [Test]
    public void TestBitwiseAndOperator()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "1 and 3"), Is.EqualTo(1 & 3));
        Assert.That(ExpressionEvaluator.GetValue(null, "1 and -1"), Is.EqualTo(1 & -1));
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["ALL"] = (RegexOptions) 0xFFFF;
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.Text.RegularExpressions.RegexOptions).IgnoreCase and #ALL", vars), Is.EqualTo(RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Tests logical NOT operator
    /// </summary>
    [Test]
    public void TestLogicalNotOperator()
    {
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!true"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!false"), Is.True);
        string expression = @"IsMember('Nikola Tesla') and !IsMember('Mihajlo Pupin')";
        Assert.That((bool) ExpressionEvaluator.GetValue(ieee, expression), Is.False);
        Assert.That(ExpressionEvaluator.GetValue(null, "!T(System.Text.RegularExpressions.RegexOptions).Compiled"), Is.EqualTo(~RegexOptions.Compiled));
    }

    /// <summary>
    /// Tests bitwise OR operator
    /// </summary>
    [Test]
    public void TestXorOperator()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "1 xor 3"), Is.EqualTo(1 ^ 3));
        Assert.That(ExpressionEvaluator.GetValue(null, "1 xor -1"), Is.EqualTo(1 ^ -1));
        Assert.That(ExpressionEvaluator.GetValue(null, "true xor false"), Is.EqualTo(true ^ false));
        Assert.That(ExpressionEvaluator.GetValue(null, "true xor true"), Is.EqualTo(true ^ true));
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.Text.RegularExpressions.RegexOptions).IgnoreCase xor T(System.Text.RegularExpressions.RegexOptions).Compiled"), Is.EqualTo(RegexOptions.IgnoreCase ^ RegexOptions.Compiled));
    }

    /// <summary>
    /// Tests logical operator presedance
    /// </summary>
    [Test]
    public void TestLogicalOperatorPresedance()
    {
        // NOT over AND
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!false and false"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!false and true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!true and false"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!true and true"), Is.False);

        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!(false and false)"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!(false and true)"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!(true and false)"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!(true and true)"), Is.False);

        // NOT over OR
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!false or false"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!false or true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!true or false"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!true or true"), Is.True);

        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!(false or false)"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!(false or true)"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!(true or false)"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!(true or true)"), Is.False);

        // AND over OR
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false and false or false"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false and false or true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false and true or false"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false and true or true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true and false or false"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true and false or true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true and true or false"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true and true or true"), Is.True);

        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false and (false or false)"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false and (false or true)"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false and (true or false)"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false and (true or true)"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true and (false or false)"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true and (false or true)"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true and (true or false)"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true and (true or true)"), Is.True);
    }

    /// <summary>
    /// Tests equality operator.
    /// </summary>
    [Test]
    public void TestEqualityOperator()
    {
        // Null
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null == null"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null == 'xyz'"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "123 == null"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null == 123"), Is.False);

        // Bool
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false == false"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true == true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false == true"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true == false"), Is.False);

        // Int
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2 == 2"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "-5 == -5"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2 == -5"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "-5 == 2"), Is.False);

        // String
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'test' == 'test'"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Test' == 'test'"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'test' == 'Test'"), Is.False);

        // DateTime
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "date('1974-08-24') == date('1974-08-24')"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "DateTime.Today == DateTime.Today"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "DateTime.Today == date('1974-08-24')"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "date('1974-08-24') == DateTime.Today"), Is.False);

        // Enums
        Foo foo = new Foo(FooType.One);
        TypeRegistry.RegisterType("FooType", typeof(FooType));
        Assert.That((bool) ExpressionEvaluator.GetValue(foo, "Type == FooType.One"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(foo, "Type == 'One'"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(foo, "Type == 'Two'"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(foo, "FooType.One == Type"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(foo, "'One' == Type"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(foo, "'Two' == Type"), Is.False);
    }

    /// <summary>
    /// Tests inequality operator.
    /// </summary>
    [Test]
    public void TestInqualityOperator()
    {
        // Null
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null != null"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "123 != null"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null != 'xyz'"), Is.True);

        // Bool
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false != false"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true != true"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false != true"), Is.True);

        // Int
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2 != 2.0"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "-5.0 != -5"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2.0 != -5"), Is.True);

        // String
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'test' != 'test'"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Test' != 'test'"), Is.True);

        // DateTime
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "date('1974-08-24') != date('1974-08-24')"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "DateTime.Today != DateTime.Today"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "DateTime.Today != date('1974-08-24')"), Is.True);
    }

    /// <summary>
    /// Tests less than operator.
    /// </summary>
    [Test]
    public void TestLessThanOperator()
    {
        // Null
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null < null"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "123 < null"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null < 'xyz'"), Is.True);

        // Bool
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false < true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true < true"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true < false"), Is.False);

        // Int
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2 < 2.0"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "-5.0 < 2"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2 < -5.0"), Is.False);

        // String
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'test' < 'test'"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Test' < 'test'"), Is.False);

        // DateTime
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "date('1974-08-24') < date('1974-08-24')"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "date('1974-08-24') < DateTime.Today"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "DateTime.Today < date('1974-08-24')"), Is.False);
    }

    /// <summary>
    /// Tests less than or equal operator.
    /// </summary>
    [Test]
    public void TestLessThanOrEqualOperator()
    {
        // Null
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null <= null"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "123 <= null"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null <= 'xyz'"), Is.True);

        // Bool
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false <= true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true <= true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true <= false"), Is.False);

        // Int
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2 <= 2.0"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "-5.0 <= 2"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2.0 <= -5"), Is.False);

        // String
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'test' <= 'test'"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Test' <= 'test'"), Is.False);

        // DateTime
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "date('1974-08-24') <= date('1974-08-24')"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "date('1974-08-24') <= DateTime.Today"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "DateTime.Today <= date('1974-08-24')"), Is.False);
    }

    /// <summary>
    /// Tests greater than operator.
    /// </summary>
    [Test]
    public void TestGreaterThanOperator()
    {
        // Null
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null > null"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "123 > null"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null > 'xyz'"), Is.False);

        // Bool
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false > true"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true > true"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true > false"), Is.True);

        // Int
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2 > 2.0"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "-5.0 > 2"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2 > -5.0"), Is.True);

        // String
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'test' > 'test'"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Test' > 'test'"), Is.True);

        // DateTime
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "date('1974-08-24') > date('1974-08-24')"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "date('1974-08-24') > DateTime.Today"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "DateTime.Today > date('1974-08-24')"), Is.True);
    }

    /// <summary>
    /// Tests greater than or equal operator.
    /// </summary>
    [Test]
    public void TestGreaterThanOrEqualOperator()
    {
        // Null
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null >= null"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "123 >= null"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null >= 'xyz'"), Is.False);

        // Bool
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "false >= true"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true >= true"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "true >= false"), Is.True);

        // Int
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2.0 >= 2"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "-5 >= 2.0"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "2.0 >= -5"), Is.True);

        // String
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'test' >= 'test'"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Test' >= 'test'"), Is.True);

        // DateTime
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "date('1974-08-24') >= date('1974-08-24')"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "date('1974-08-24') >= DateTime.Today"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "DateTime.Today >= date('1974-08-24')"), Is.True);
    }

    /// <summary>
    /// Tests IN operator.
    /// </summary>
    [Test]
    public void TestInOperator()
    {
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null in null"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "3 in {1, 2, 3, 4, 5}"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!(3 in {1, 2, 3, 4, 5})"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'xyz' in new string[] {'abc', 'xyz'}"), Is.True);
        Assert.That(
            (bool) ExpressionEvaluator.GetValue(null, "'xyz' in #{'abc' : 'Value 1', 'xyz' : DateTime.Today}"),
            Is.True);
    }

    /// <summary>
    /// Tests IS operator.
    /// </summary>
    [Test]
    public void TestIsOperator()
    {
        TypeRegistry.RegisterType(typeof(IList));
        TypeRegistry.RegisterType(typeof(IDictionary));

        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null is null"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "5 is null"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null is int"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "5 is int"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!(5 is int)"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "{1, 2, 3, 4, 5} is IList"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "new string[] {'abc', 'xyz'} is T(string[])"), Is.True);
        Assert.That(
            (bool) ExpressionEvaluator.GetValue(null, "#{'abc' : 'Value 1', 'xyz' : DateTime.Today} is IDictionary"),
            Is.True);
    }

    /// <summary>
    /// Tests BETWEEN operator.
    /// </summary>
    [Test]
    public void TestBetweenOperator()
    {
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "null between {1, 5}"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "0 between {1, 5}"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "1 between {1, 5}"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "3 between {1, 5}"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "5 between {1, 5}"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "6 between {1, 5}"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "!(6 between {1, 5})"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'efg' between {'abc', 'xyz'}"), Is.True);
        Assert.That(
            (bool) ExpressionEvaluator.GetValue(null, "DateTime.Today between {DateTime.Today, DateTime.Now}"),
            Is.True);
        Assert.That(
            (bool) ExpressionEvaluator.GetValue(null, "DateTime.Today between {DateTime.Now, DateTime.Now}"),
            Is.False);
    }
#if !MONO
    /// <summary>
    /// Tests LIKE operator.
    /// </summary>
    [Test]
    public void TestLikeOperator()
    {
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'A' like '?'"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Abc' like '?'"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Abc' like '[A-Z]b?'"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Abc' like '*'"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Aleksandar' like 'Aleks*'"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Ana Maria Seovic' like '*Maria*'"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'Marija Seovic' like '*Seovic'"), Is.True);
    }
#endif
    /// <summary>
    /// Tests MATCHES operator.
    /// </summary>
    [Test]
    public void TestMatchesOperator()
    {
        string emailCheck =
            @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

        Assert.That((bool) ExpressionEvaluator.GetValue(emailCheck, "'A' matches #this"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(emailCheck, "'aleks@seovic.com' matches #this"), Is.True);
        Assert.That((bool) ExpressionEvaluator.GetValue(emailCheck, "'@seovic.com' matches #this"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(emailCheck, "'seovic.com' matches #this"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(emailCheck, "'aleks' matches #this"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(emailCheck, "'aleks@' matches #this"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(emailCheck, "'aleks@seovic' matches #this"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "'5.0067' matches '^-?\\d+(\\.\\d{2})?$'"), Is.False);
        Assert.That((bool) ExpressionEvaluator.GetValue(null, @"'5.00' matches '^-?\d+(\.\d{2})?$'"), Is.True);
    }

    /// <summary>
    /// Type coercion failure.
    /// </summary>
    [Test]
    public void TestTypeCoercionForUncoercableTypes()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "'xyz' > 123"));
    }

    /// <summary>
    /// Type comparison failure.
    /// </summary>
    [Test]
    public void TestComparisonOfInstancesThatDoNotImplementIComparable()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["tesla"] = tesla;
        vars["pupin"] = pupin;
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "#tesla > #pupin", vars));
    }

    /// <summary>
    /// Tests addition operator.
    /// </summary>
    [Test]
    public void TestAddOperator()
    {
        // numbers
        Assert.That(ExpressionEvaluator.GetValue(null, "1 + 1"), Is.EqualTo(2));
        Assert.That(ExpressionEvaluator.GetValue(null, "1e0 + 1"), Is.EqualTo(2));
        Assert.That(ExpressionEvaluator.GetValue(null, "-1e0 + 1"), Is.EqualTo(0));
        Assert.That(ExpressionEvaluator.GetValue(null, "-1e0 + -1"), Is.EqualTo(-2));
        Assert.That(ExpressionEvaluator.GetValue(null, "1.0m + 1.0"),
            Is.EqualTo(Decimal.Parse("2.0", NumberFormatInfo.InvariantInfo)));
        Assert.That(ExpressionEvaluator.GetValue(null, "2.0 + 3e0 + 4"), Is.EqualTo(9));

        // strings
        Assert.That(ExpressionEvaluator.GetValue(null, "'test' + ' ' + 'string'"), Is.EqualTo("test string"));
        Assert.That(ExpressionEvaluator.GetValue(null, "'test' + ' ' + 2"), Is.EqualTo("test 2"));
        Assert.That(ExpressionEvaluator.GetValue(null, "'test' + ' ' + DateTime.Today"),
            Is.EqualTo("test " + DateTime.Today.ToString()));
        Assert.That(ExpressionEvaluator.GetValue(null, "'test' + #this"), Is.EqualTo("test")); // can concat null
        Assert.That(ExpressionEvaluator.GetValue(null, "#this+'test'"), Is.EqualTo("test")); // can concat null

        // dates
        DateTime anaDOB = new DateTime(2004, 8, 14);
        DateTime aleksDOB = new DateTime(1974, 8, 24);
        TimeSpan diff = anaDOB - aleksDOB;
        IDictionary<string, object> vars = new Dictionary<string, object>();
        vars["ts"] = diff;

        Assert.That(ExpressionEvaluator.GetValue(null, "date('1974-08-24') + #ts", vars), Is.EqualTo(anaDOB));
        Assert.That(ExpressionEvaluator.GetValue(null, "date('1974-08-24') + 5"), Is.EqualTo(new DateTime(1974, 8, 29)));
        Assert.That(ExpressionEvaluator.GetValue(null, "date('1974-08-24') + '5.0:0'"),
            Is.EqualTo(new DateTime(1974, 8, 29)));
    }

    /// <summary>
    /// Tests addition operator with invalid arguments.
    /// </summary>
    [Test]
    public void TestAddOperatorWithInvalidArguments()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "DateTime.Today + false"));
    }

    /// <summary>
    /// Tests subtraction operator.
    /// </summary>
    [Test]
    public void TestSubtractOperator()
    {
        // numbers
        Assert.That(ExpressionEvaluator.GetValue(null, "3 - 1"), Is.EqualTo(2));
        Assert.That(ExpressionEvaluator.GetValue(null, "1 - 3"), Is.EqualTo(-2));
        Assert.That(ExpressionEvaluator.GetValue(null, "1 - -3"), Is.EqualTo(4));
        Assert.That(ExpressionEvaluator.GetValue(null, "1000.00m - 1e4"),
            Is.EqualTo(Decimal.Parse("-9000.00", NumberFormatInfo.InvariantInfo)));
        Assert.That(ExpressionEvaluator.GetValue(null, "2.0 - 3e0 - 4"), Is.EqualTo(-5));

        // dates
        DateTime anaDOB = new DateTime(2004, 8, 14);
        DateTime aleksDOB = new DateTime(1974, 8, 24);
        TimeSpan diff = anaDOB - aleksDOB;
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["ts"] = diff;

        Assert.That(ExpressionEvaluator.GetValue(null, "date('2004-08-14') - #ts", vars), Is.EqualTo(aleksDOB));
        Assert.That(ExpressionEvaluator.GetValue(null, "date('2004-08-14') - date('1974-08-24')"), Is.EqualTo(diff));
        Assert.That(ExpressionEvaluator.GetValue(null, "date('1974-08-24') - 5"), Is.EqualTo(new DateTime(1974, 8, 19)));
        Assert.That(ExpressionEvaluator.GetValue(null, "date('1974-08-24') - '5.0:0'"),
            Is.EqualTo(new DateTime(1974, 8, 19)));
    }

    /// <summary>
    /// Tests subtraction operator with invalid arguments.
    /// </summary>
    [Test]
    public void TestSubtractOperatorWithInvalidArguments()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "DateTime.Today - false"));
    }

    /// <summary>
    /// Tests multiplication operator.
    /// </summary>
    [Test]
    public void TestMultiplyOperator()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "2 * 2"), Is.EqualTo(4));
        Assert.That(ExpressionEvaluator.GetValue(null, "2 * -3"), Is.EqualTo(-6));
        Assert.That(ExpressionEvaluator.GetValue(null, "-2 * -3"), Is.EqualTo(6));
        Assert.That(ExpressionEvaluator.GetValue(null, "1000.00m * 1e3"),
            Is.EqualTo(Decimal.Parse("1000000.00", NumberFormatInfo.InvariantInfo)));
        Assert.That(ExpressionEvaluator.GetValue(null, "2.0 * 3e0 * 4"), Is.EqualTo(24));
    }

    /// <summary>
    /// Tests multiplication operator with invalid arguments.
    /// </summary>
    [Test]
    public void TestMultiplyOperatorWithInvalidArguments()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "DateTime.Today * false"));
    }

    /// <summary>
    /// Tests division operator.
    /// </summary>
    [Test]
    public void TestDivideOperator()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "2 / 2"), Is.EqualTo(1));
        Assert.That(ExpressionEvaluator.GetValue(null, "6 / -3"), Is.EqualTo(-2));
        Assert.That(ExpressionEvaluator.GetValue(null, "-6 / -3"), Is.EqualTo(2));
        Assert.That(ExpressionEvaluator.GetValue(null, "1000.00m / 1e3"),
            Is.EqualTo(Decimal.Parse("1.00", NumberFormatInfo.InvariantInfo)));
        Assert.That(ExpressionEvaluator.GetValue(null, "8.0 / 4e0 / 2"), Is.EqualTo(1));
    }

    /// <summary>
    /// Tests division operator with invalid arguments.
    /// </summary>
    [Test]
    public void TestDivideOperatorWithInvalidArguments()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "DateTime.Today / false"));
    }

    /// <summary>
    /// Tests modulus operator.
    /// </summary>
    [Test]
    public void TestModulusOperator()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "2 % 2"), Is.EqualTo(0));
        Assert.That(ExpressionEvaluator.GetValue(null, "6 % -4"), Is.EqualTo(2));
        Assert.That(ExpressionEvaluator.GetValue(null, "-6 % -5"), Is.EqualTo(-1));
        Assert.That(ExpressionEvaluator.GetValue(null, "1005.00m % 1e3"),
            Is.EqualTo(Decimal.Parse("5.00", NumberFormatInfo.InvariantInfo)));
        Assert.That(ExpressionEvaluator.GetValue(null, "8.0 % 5e0 % 2"), Is.EqualTo(1));
    }

    /// <summary>
    /// Tests modulus operator with invalid arguments.
    /// </summary>
    [Test]
    public void TestModulusOperatorWithInvalidArguments()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "DateTime.Today % false"));
    }

    /// <summary>
    /// Tests power operator.
    /// </summary>
    [Test]
    public void TestPowerOperator()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "2 ^ +3"), Is.EqualTo(8));
        Assert.That(ExpressionEvaluator.GetValue(null, "-2 ^ 4"), Is.EqualTo(16));
        Assert.That(ExpressionEvaluator.GetValue(null, "-2 ^ 5"), Is.EqualTo(-32));
        Assert.That(ExpressionEvaluator.GetValue(null, "4 ^ -2"), Is.EqualTo(.0625));
        Assert.That(ExpressionEvaluator.GetValue(null, "+4 ^ .5"), Is.EqualTo(2));
    }

    /// <summary>
    /// Tests power operator with invalid arguments.
    /// </summary>
    [Test]
    public void TestPowerOperatorWithInvalidArguments()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "DateTime.Today ^ false"));
    }

    /// <summary>
    /// Tests unary minus operator with invalid argument.
    /// </summary>
    [Test]
    public void TestUnaryMinusOperatorWithInvalidArguments()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "-false"));
    }

    /// <summary>
    /// Tests unary plus operator with invalid argument.
    /// </summary>
    [Test]
    public void TestUnaryPlusOperatorWithInvalidArguments()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "+false"));
    }

    /// <summary>
    /// Tests operator precedence.
    /// </summary>
    [Test]
    public void TestOperatorPrecedence()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "1+2-3*8/2/2"), Is.EqualTo(-3));
        Assert.That(ExpressionEvaluator.GetValue(null, "1+2-3*8^2/2/2"), Is.EqualTo(-45));
        Assert.That(ExpressionEvaluator.GetValue(null, "1+2-3*8/2^2/2"), Is.EqualTo(0));
        Assert.That(ExpressionEvaluator.GetValue(null, "1+(2-3*8)/2.0/2"), Is.EqualTo(-4.5));
    }

    /// <summary>
    /// Tests Spring reference when reference to a non-existant object is specified.
    /// </summary>
    [Test]
    public void TestReferenceForNonExistantObject()
    {
        ContextRegistry.RegisterContext(new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml"));
        Assert.Throws<NoSuchObjectDefinitionException>(() => ExpressionEvaluator.GetValue(null, "@(dummyRef)"));
    }

    /// <summary>
    /// Tests Spring reference.
    /// </summary>
    [Test]
    public void TestReference()
    {
        ContextRegistry.RegisterContext(
            new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml"));
        Assert.That(ExpressionEvaluator.GetValue(null, "@(goran)").GetType(), Is.EqualTo(typeof(TestObject)));

        IApplicationContext ctx =
            new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml");
        ctx.Name = "myContext";
        ContextRegistry.RegisterContext(ctx);

        Assert.That(ExpressionEvaluator.GetValue(null, "@(myContext:goran)").GetType(),
            Is.EqualTo(typeof(TestObject)));

        // string literals allowed for contextname
        Assert.That(ExpressionEvaluator.GetValue(null, "@('myContext':goran)").GetType(),
            Is.EqualTo(typeof(TestObject)));
    }

    /// <summary>
    /// Since Expression-References require the context to be added to ContextRegistry,
    /// they work only if the objectdefinition's "lazy-init" is true.
    /// </summary>
    [Test]
    public void TestReferenceByExpression()
    {
        //TODO: write a test showing that expressions don't work without "lazy-init":
        /*
          <!-- fails to init context -->
          <object id="testObjectContainer"
                  type="Spring.Expressions.TestObjectContainer, Spring.Core.Tests">
                    <property name="TestObject" expression="@(goran)" />
          </object>
         */

        ContextRegistry.RegisterContext(
            new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml"));

        TestObject testObject = ExpressionEvaluator.GetValue(null, "@(goran)") as TestObject;

        TestObjectContainer testObjectContainer =
            ExpressionEvaluator.GetValue(null, "@(testObjectContainer_lazy)") as TestObjectContainer;

        Assert.That(testObject, Is.Not.Null);
        Assert.That(testObjectContainer.TestObject, Is.SameAs(testObject));
    }

    /// <summary>
    /// Ensure context-names my contain dots and slashes
    /// </summary>
    [Test]
    public void TestQualifiedNameMayContainDotsAndSlashes()
    {
        IApplicationContext ctx =
            new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml");
        ctx.Name = @"my.Context/bla\";
        ContextRegistry.RegisterContext(ctx);

        Assert.That(ExpressionEvaluator.GetValue(null, @"@(my.Context/bla\:goran)").GetType(),
            Is.EqualTo(typeof(TestObject)));
//            Assert.That(//                            ExpressionEvaluator.GetValue(null, "@(my\\.Context:goran)").GetType(), Is.EqualTo(typeof(TestObject)));
    }

    /// <summary>
    /// Tests attribute expression.
    /// </summary>
    [Test]
    public void TestAttribute()
    {
        TypeRegistry.RegisterType("WebMethod", typeof(WebMethodAttribute));
        TypeRegistry.RegisterType("TransactionOption", typeof(TransactionOption));

        Assert.That(ExpressionEvaluator.GetValue(null, "@[System.Serializable]"),
            Is.InstanceOf(typeof(SerializableAttribute)));
        Assert.That(ExpressionEvaluator.GetValue(null, "@[System.Serializable()]"),
            Is.InstanceOf(typeof(SerializableAttribute)));
        Assert.That(ExpressionEvaluator.GetValue(null, "@[WebMethod]"), Is.InstanceOf(typeof(WebMethodAttribute)));

        WebMethodAttribute webMethod = (WebMethodAttribute) ExpressionEvaluator.GetValue(null, "@[WebMethod(true)]");
        Assert.That(webMethod.EnableSession, Is.True);

        webMethod = (WebMethodAttribute)
            ExpressionEvaluator.GetValue(
                null,
                "@[WebMethod(false, CacheDuration = 60, Description = 'my web method', TransactionOption = TransactionOption.Required)]");
        Assert.That(webMethod.CacheDuration, Is.EqualTo(60));
        Assert.That(webMethod.Description, Is.EqualTo("my web method"));
        Assert.That(webMethod.TransactionOption, Is.EqualTo(TransactionOption.Required));
    }

    [Test]
    public void TestDelegateFunctionExpressions()
    {
        //for purposes of an example in documentation
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["sqrt"] = new DoubleFunction(Sqrt);
        double result = (double) ExpressionEvaluator.GetValue(null, "#sqrt(64)", vars);
        Assert.That(result, Is.EqualTo(8));

        vars = new Dictionary<string, object>();
        vars["max"] = new DoubleFunctionTwoArgs(Max);
        result = (double) ExpressionEvaluator.GetValue(null, "#max(5,25)", vars);
        Assert.That(result, Is.EqualTo(25));
    }

    private delegate double DoubleFunction(double arg);

    private double Sqrt(double arg)
    {
        return Math.Sqrt(arg);
    }

    private delegate double DoubleFunctionTwoArgs(double arg1, double arg2);

    private double Max(double arg1, double arg2)
    {
        return Math.Max(arg1, arg2);
    }

    /// <summary>
    /// Type lambda expressions.
    /// </summary>
    [Test]
    public void TestLambdaExpressions()
    {
        TypeRegistry.RegisterType(typeof(Math));

        // simple function
        Assert.That(ExpressionEvaluator.GetValue(null, "(#add = {|x, y| $x + $y}; #add(2, 2))", new Dictionary<string, object>()),
            Is.EqualTo(4));
        Assert.That(ExpressionEvaluator.GetValue(null, "(#max = {|x, y| $x > $y ? $x : $y }; #max(5,25))",
                new Dictionary<string, object>()),
            Is.EqualTo(25));

        // recursive function
        Assert.That(ExpressionEvaluator.GetValue(null,
                "(#fact = {|n| $n <= 1 ? 1 : $n * #fact($n-1) }; #fact(5))",
                new Dictionary<string, object>()),
            Is.EqualTo(120));

        // function invoked within projection expression
        string expr = "(#upper = {|txt| $txt.ToUpper() }; !{ #upper(Name) })";
        IList upperNames = (IList) ExpressionEvaluator.GetValue(ieee.Members, expr, new Dictionary<string, object>());
        Assert.That(upperNames[0], Is.EqualTo("NIKOLA TESLA"));
        Assert.That(upperNames[1], Is.EqualTo("MIHAJLO PUPIN"));

        // function that delegates to a function passed as a parameter
        Dictionary<string, object> vars = new Dictionary<string, object>();
        Expression.RegisterFunction("sqrt", "{|n| Math.Sqrt($n)}", vars);
        Expression.RegisterFunction("fact", "{|n| $n <= 1 ? 1 : $n * #fact($n-1)}", vars);
        string expr2 =
            @"(
                                #delegate = {|f,n| $f($n) };
                                #d = #delegate;

                                #result = { #delegate(#sqrt, 4), #d(#fact, 5), #delegate({|n| $n ^ 2 }, 5) }
                            )";
        IList results = (IList) ExpressionEvaluator.GetValue(null, expr2, vars);
        Assert.That(results[0], Is.EqualTo(2));
        Assert.That(results[1], Is.EqualTo(120));
        Assert.That(results[2], Is.EqualTo(25));

        // function assignment
        Assert.That(ExpressionEvaluator.GetValue(null,
                "(#fact = {|n| $n <= 1 ? 1 : $n * #fact($n-1) }; #f = #fact; #f(5))",
                new Dictionary<string, object>()),
            Is.EqualTo(120));
    }

    [Test]
    public void TestCountAggregator()
    {
        int[] arr = new int[] { 24, 8, 14, 8 };
        Assert.That(ExpressionEvaluator.GetValue(arr, "count()"), Is.EqualTo(4));
        Assert.That(ExpressionEvaluator.GetValue(null, "{1, 5, -3}.count()"), Is.EqualTo(3));
        Assert.That(ExpressionEvaluator.GetValue(null, "count()"), Is.EqualTo(0));
    }

    [Test]
    public void TestCustomCollectionProcessor()
    {
        // Test for the purposes of creating documentation example.
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["EvenSum"] = new IntEvenSumCollectionProcessor();
        Assert.That(ExpressionEvaluator.GetValue(null, "{1, 2, 3, 4}.EvenSum()", vars), Is.EqualTo(6));
    }

    private class IntEvenSumCollectionProcessor : ICollectionProcessor
    {
        public object Process(ICollection source, object[] args)
        {
            object total = 0d;
            foreach (object item in source)
            {
                if (item != null)
                {
                    if (NumberUtils.IsInteger(item))
                    {
                        if ((int) item % 2 == 0)
                        {
                            total = NumberUtils.Add(total, item);
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Sum can only be calculated for a collection of numeric values.");
                    }
                }
            }

            return total;
        }
    }

    [Test]
    public void TestSumAggregator()
    {
        int[] arr = new int[] { 24, 8, 14, 8 };
        Assert.That(ExpressionEvaluator.GetValue(arr, "sum()"), Is.EqualTo(54));
        Assert.That(ExpressionEvaluator.GetValue(null, "{1, 5, -3, 10}.sum()"), Is.EqualTo(13));

        object[] arr2 = new object[] { 5, 5.8, 12.2, 1 };
        object result = ExpressionEvaluator.GetValue(arr2, "sum()");
        Assert.That(result, Is.InstanceOf(typeof(double)));
        Assert.That(result, Is.EqualTo(24));
    }

    [Test]
    public void TestSumAggregatorWithNonNumber()
    {
        object[] arr = new object[] { 5, "ana", 12.2, 1 };
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(arr, "sum()"));
    }

    [Test]
    public void TestAverageAggregator()
    {
        int[] arr = new int[] { 24, 8, 16, 8 };
        Assert.That(ExpressionEvaluator.GetValue(arr, "average()"), Is.EqualTo(14));
        Assert.That(ExpressionEvaluator.GetValue(null, "{1, 5, -4, 10}.average()"), Is.EqualTo(3));
        Assert.That(ExpressionEvaluator.GetValue(null, "{1, 5, -2, 10}.average()"), Is.EqualTo(3.5));

        object[] arr2 = new object[] { 5, 5.8, 12.2, 1 };
        object result = ExpressionEvaluator.GetValue(arr2, "average()");
        Assert.That(result, Is.InstanceOf(typeof(double)));
        Assert.That(result, Is.EqualTo(6));
    }

    [Test]
    public void TestAverageAggregatorWithNonNumber()
    {
        object[] arr = new object[] { 5, "ana", 12.2, 1 };
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(arr, "average()"));
    }

    [Test]
    public void TestMinAggregator()
    {
        int[] arr = new int[] { 24, 8, 14, 8 };
        Assert.That(ExpressionEvaluator.GetValue(arr, "min()"), Is.EqualTo(8));
        Assert.That(ExpressionEvaluator.GetValue(null, "{1, 5, -3, 10}.min()"), Is.EqualTo(-3));

        object[] arr2 = new object[] { 5, 5.8, 12.2, 1 };
        object result = ExpressionEvaluator.GetValue(arr2, "min()");
        Assert.That(result, Is.InstanceOf(typeof(int)));
        Assert.That(result, Is.EqualTo(1));

        Assert.That(ExpressionEvaluator.GetValue(ObjectUtils.EmptyObjects, "min()"), Is.Null);
    }

    [Test]
    public void TestMinAggregatorWithNonComparable()
    {
        object[] arr = new object[] { new Object(), new Object() };
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(arr, "min()"));
    }

    [Test]
    public void TestMinAggregatorWithMixedTypes()
    {
        object[] arr = new object[] { 5, "ana", 12.2, 1 };
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(arr, "min()"));
    }

    [Test]
    public void TestMaxAggregator()
    {
        int[] arr = new int[] { 24, 8, 14, 8 };
        Assert.That(ExpressionEvaluator.GetValue(arr, "max()"), Is.EqualTo(24));
        Assert.That(ExpressionEvaluator.GetValue(null, "{1, 5, -3, 10}.max()"), Is.EqualTo(10));

        object[] arr2 = new object[] { 5, 5.8, 12.2, 1 };
        object result = ExpressionEvaluator.GetValue(arr2, "max()");
        Assert.That(result, Is.InstanceOf(typeof(double)));
        Assert.That(result, Is.EqualTo(12.2));

        Assert.That(ExpressionEvaluator.GetValue(ObjectUtils.EmptyObjects, "max()"), Is.Null);
    }

    [Test]
    public void TestMaxAggregatorWithNonComparable()
    {
        object[] arr = new object[] { new Object(), new Object() };
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(arr, "max()"));
    }

    [Test]
    public void TestMaxAggregatorWithMixedTypes()
    {
        object[] arr = new object[] { 5, "ana", 12.2, 1 };
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(arr, "max()"));
    }

    [Test]
    public void TestSortProcessor()
    {
        int[] arr = new int[] { 24, 8, 14, 6 };
        Assert.That(ExpressionEvaluator.GetValue(arr, "sort()"), Is.EqualTo(new int[] { 6, 8, 14, 24 }));
        Assert.That(ExpressionEvaluator.GetValue(arr, "sort(true)"), Is.EqualTo(new int[] { 6, 8, 14, 24 }));
        Assert.That(ExpressionEvaluator.GetValue(arr, "sort(false)"), Is.EqualTo(new int[] { 24, 14, 8, 6 }));

        string[] arr2 = new string[] { "abc", "xyz", "stuv", "efg", "dcb" };
        Assert.That(ExpressionEvaluator.GetValue(arr2, "sort()"),
            Is.EqualTo(new string[] { "abc", "dcb", "efg", "stuv", "xyz" }));

        DateTime[] arr3 = new DateTime[] { DateTime.Today, DateTime.MaxValue, DateTime.MinValue };
        Assert.That(ExpressionEvaluator.GetValue(arr3, "sort()"),
            Is.EqualTo(new DateTime[] { DateTime.MinValue, DateTime.Today, DateTime.MaxValue }));

        Assert.That(ExpressionEvaluator.GetValue(null, "{1.2, 5.5, -3.3}.sort()"), Is.EqualTo(new object[] { -3.3, 1.2, 5.5 }));
        Assert.That(ExpressionEvaluator.GetValue(null, "sort()"), Is.Null);

        ISet set = new ListSet(arr);
        Assert.That(ExpressionEvaluator.GetValue(set, "sort()"), Is.EqualTo(new int[] { 6, 8, 14, 24 }));
    }

    [Test(Description = "sort supports any ICollection containing elements of uniform type")]
    public void TestSortProcessorWithSimpleICollectionType()
    {
        Stack stack = new Stack(new int[] { 24, 8, 14, 6 });
        ExpressionEvaluator.GetValue(stack, "sort()");
    }

    [Test]
    public void TestNonNullProcessor()
    {
        string[] arr2 = new string[] { "abc", "xyz", null, "abc", "def", null };
        Assert.That(ExpressionEvaluator.GetValue(arr2, "nonNull()"),
            Is.EqualTo(new string[] { "abc", "xyz", "abc", "def" }));
        Assert.That(ExpressionEvaluator.GetValue(arr2, "nonNull().sort()"),
            Is.EqualTo(new string[] { "abc", "abc", "def", "xyz" }));
    }

    [Test]
    public void TestDistinctProcessor()
    {
        int[] arr = new int[] { 24, 8, 8, 6, 24, 6, 8, 6 };
        Assert.That(ExpressionEvaluator.GetValue(arr, "distinct().sort()"), Is.EqualTo(new int[] { 6, 8, 24 }));

        string[] arr2 = new string[] { "abc", "xyz", "abc", "def", null, "def" };
        Assert.That(ExpressionEvaluator.GetValue(arr2, "distinct(true).sort()"),
            Is.EqualTo(new string[] { null, "abc", "def", "xyz" }));
        Assert.That(ExpressionEvaluator.GetValue(arr2, "distinct(false).sort()"),
            Is.EqualTo(new string[] { "abc", "def", "xyz" }));
        Assert.That(ExpressionEvaluator.GetValue(arr2, "distinct().sort()"),
            Is.EqualTo(new string[] { "abc", "def", "xyz" }));
    }

    [Test]
    public void TestDistinctProcessorWithInvalidArgumentType()
    {
        int[] arr = new int[] { 24, 8, 8, 6, 24, 6, 8, 6 };
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(arr, "distinct(6)"));
    }

    [Test]
    public void TestDistinctProcessorWithInvalidNumberOfArguments()
    {
        int[] arr = new int[] { 24, 8, 8, 6, 24, 6, 8, 6 };
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(arr, "distinct(true, 4, 'xyz')"));
    }

    [Test]
    public void TestConversionProcessor()
    {
        object[] arr = new object[] { "0", 1, 1.1m, "1.1", 1.1f };
        decimal[] result = (decimal[]) ExpressionEvaluator.GetValue(arr, "convert(decimal)");
        Assert.That(result[0], Is.EqualTo(0.0m));
        Assert.That(result[1], Is.EqualTo(1.0m));
        Assert.That(result[2], Is.EqualTo(1.1m));
        Assert.That(result[3], Is.EqualTo(1.1m));
        Assert.That(result[4], Is.EqualTo(1.1m));
    }

    [Test]
    public void TestReverseProcessor()
    {
        object[] arr = new object[] { "0", 1, 2.1m, "3", 4.1f };
        object[] result = new ArrayList((ICollection) ExpressionEvaluator.GetValue(arr, "reverse()")).ToArray();
        Assert.That(result, Is.EqualTo(new object[] { 4.1f, "3", 2.1m, 1, "0" }));
    }

    /// <summary>
    /// Type SetValue.
    /// </summary>
    [Test]
    public void TestSetValue()
    {
        Dictionary<string, object> vars = new Dictionary<string, object>();
        vars["tesla"] = tesla;
        vars["pupin"] = pupin;
        ExpressionEvaluator.SetValue(null, "#tesla.Name", vars, "Tesla, Nikola");
        Assert.That(tesla.Name, Is.EqualTo("Tesla, Nikola"));
    }

    /// <summary>
    /// Tests property access with null in the path.
    /// </summary>
    [Test]
    public void TestPropertyGetWithNullInThePath()
    {
        Assert.Throws<NullValueInNestedPathException>(() => ExpressionEvaluator.GetValue(new Inventor(), "Name.Length"));
    }

    /// <summary>
    /// Tests property set with null in the path.
    /// </summary>
    [Test]
    public void TestPropertySetWithNullInThePath()
    {
        Assert.Throws<NullValueInNestedPathException>(() => ExpressionEvaluator.SetValue(new Inventor(), "Name.Length", 20));
    }

    /// <summary>
    /// Tries to set value of the PropertyOrFieldNode that represents type.
    /// </summary>
    [Test]
    public void TestTypeSet()
    {
        Assert.Throws<NotSupportedException>(() => ExpressionEvaluator.SetValue(null, "DateTime", 20));
    }

    /// <summary>
    /// Reproduce SPRNET-408.
    /// </summary>
    /// <remarks>
    /// http://opensource.atlassian.com/projects/spring/browse/SPRNET-408
    /// http://forum.springframework.net/showthread.php?t=933
    /// </remarks>
    [Test(Description = "Test to reproduce SPRNET-408")]
    public void TestNullableTypes()
    {
        Foo foo = new Foo();
        Assert.That(ExpressionEvaluator.GetValue(foo, "NullableDate"), Is.Null);
        Assert.That(ExpressionEvaluator.GetValue(foo, "NullableInt"), Is.Null);

        ExpressionEvaluator.SetValue(foo, "NullableDate", DateTime.Today);
        ExpressionEvaluator.SetValue(foo, "NullableDate", null);
        ExpressionEvaluator.SetValue(foo, "NullableDate", "2004-08-14");
        ExpressionEvaluator.SetValue(foo, "NullableDate", DateTime.Today);
        ExpressionEvaluator.SetValue(foo, "NullableInt", 1);
        ExpressionEvaluator.SetValue(foo, "NullableInt", null);
        ExpressionEvaluator.SetValue(foo, "NullableInt", "5");
        ExpressionEvaluator.SetValue(foo, "NullableInt", 1);

        Assert.That(ExpressionEvaluator.GetValue(foo, "NullableDate"), Is.InstanceOf(typeof(DateTime?)));
        Assert.That(ExpressionEvaluator.GetValue(foo, "NullableInt"), Is.InstanceOf(typeof(Int32?)));

        Assert.That(ExpressionEvaluator.GetValue(foo, "NullableDate"), Is.EqualTo(DateTime.Today));
        Assert.That(ExpressionEvaluator.GetValue(foo, "NullableInt"), Is.EqualTo(1));

        int? test = 1;
        Assert.That(ExpressionEvaluator.GetValue(test, "#root"), Is.InstanceOf(typeof(Int32?)));
        Assert.That((bool) ExpressionEvaluator.GetValue(test, "#root != null"), Is.True);
        Assert.That(ExpressionEvaluator.GetValue(test, "#root"), Is.EqualTo(1));

        test = null;
        Assert.That((bool) ExpressionEvaluator.GetValue(test, "#root == null"), Is.True);
        Assert.That(ExpressionEvaluator.GetValue(test, "#root"), Is.Null);
    }

    /// <summary>
    /// Reproduce SPRNET-462.
    /// </summary>
    /// <remarks>
    /// http://opensource.atlassian.com/projects/spring/browse/SPRNET-462
    /// http://forum.springframework.net/showthread.php?t=1515
    /// </remarks>
    [Test(Description = "Test to reproduce SPRNET-462")]
    public void TestMethodResolutionWithNullArguments()
    {
        DateTime today = DateTime.Today;
        Assert.That(ExpressionEvaluator.GetValue(today, "ToString('d')"), Is.EqualTo(today.ToString("d")));

        TypeRegistry.RegisterType(typeof(TypeRegistry));
        try
        {
            ExpressionEvaluator.GetValue(null, "TypeRegistry.RegisterType(null)");
            Assert.Fail("Should throw ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
        }

        try
        {
            ExpressionEvaluator.GetValue(null, "TypeRegistry.RegisterType(null, 'System.Object')");
            Assert.Fail("Should throw ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
        }

        try
        {
            ExpressionEvaluator.GetValue(null, "TypeRegistry.RegisterType(null, null)");
            Assert.Fail("Should throw AmbiguousMatchException");
        }
        catch (AmbiguousMatchException)
        {
        }

        try
        {
            ExpressionEvaluator.GetValue(null, "TypeRegistry.RegisterType(null, null, null)");
            Assert.Fail("Should throw ArgumentException");
        }
        catch (ArgumentException)
        {
        }

        try
        {
            ExpressionEvaluator.GetValue(null, "TypeRegistry.RegisterType(int, string)");
            Assert.Fail("Should throw ArgumentException");
        }
        catch (ArgumentException)
        {
        }
    }

    /// <summary>
    /// Reproduce SPRNET-464.
    /// </summary>
    /// <remarks>
    /// http://opensource.atlassian.com/projects/spring/browse/SPRNET-464
    /// http://forum.springframework.net/showthread.php?t=1515
    /// </remarks>
    [Test(Description = "Test to reproduce SPRNET-464")]
    public void TestMethodResolutionWithParamArray()
    {
        Foo foo = new Foo();
        Assert.That(foo.MethodWithArrayArgument(new string[] { "a", "b", "c" }), Is.EqualTo("a|b|c"));
        Assert.That(foo.MethodWithArrayArgument(new string[] { "a", null, "c" }), Is.EqualTo("a||c"));
        Assert.That(foo.MethodWithParamArray("a", "b", "c"), Is.EqualTo("a|b|c"));
        Assert.That(foo.MethodWithParamArray("a", null, "c"), Is.EqualTo("a||c"));

        Assert.That(ExpressionEvaluator.GetValue(foo, "MethodWithArrayArgument(new string[] { 'a', 'b', 'c' })"), Is.EqualTo("a|b|c"));
        Assert.That(ExpressionEvaluator.GetValue(foo, "MethodWithArrayArgument(new string[] { 'a', null, 'c' })"), Is.EqualTo("a||c"));
        Assert.That(ExpressionEvaluator.GetValue(foo, "MethodWithParamArray('a', 'b', 'c')"), Is.EqualTo("a|b|c"));
        Assert.That(ExpressionEvaluator.GetValue(foo, "MethodWithParamArray('a', null, 'c')"), Is.EqualTo("a||c"));

        Assert.That(ExpressionEvaluator.GetValue(foo, "MethodWithParamArray(false, 'a', 'b', 'c')"), Is.EqualTo("a|b|c"));
        Assert.That(ExpressionEvaluator.GetValue(foo, "MethodWithParamArray(false, 'a', null, 'c')"), Is.EqualTo("a||c"));
        Assert.That(ExpressionEvaluator.GetValue(foo, "MethodWithParamArray(true, 'a', 'b', 'c')"), Is.EqualTo("A|B|C"));
        Assert.That(ExpressionEvaluator.GetValue(foo, "MethodWithParamArray(true, 'a', null, 'c')"), Is.EqualTo("A||C"));
    }

    [Test]
    public void TestMethodResolutionResolvesToExactMatchOfArgumentTypes()
    {
        Dictionary<string, object> args = new Dictionary<string, object>();
        args["bars"] = new Bar[] { new Bar() };
        Foo foo = new Foo();

        // ensure no one changed our test class
        Assert.That(foo.MethodWithSimilarArguments(1, (Bar[]) args["bars"]), Is.EqualTo("ExactMatch"));
        Assert.That(foo.MethodWithSimilarArguments(1, (ICollection) args["bars"]), Is.EqualTo("AssignableMatch"));

        Assert.That(ExpressionEvaluator.GetValue(foo, "MethodWithSimilarArguments(1, #bars)", args), Is.EqualTo("ExactMatch"));
    }

    /// <summary>
    /// Test to show that a large number of parameters can be passed to methods
    /// </summary>
    [Test]
    public void TestMethodResolutionWithLargeNumberOfParametersDoesNotThrow()
    {
        int expectedResult = 150;
        int result = 0;

        Foo foo = new Foo();
        string expression = $"MethodWithParamArray({String.Join(", ", Enumerable.Range(0, expectedResult))})";

        Assert.DoesNotThrow(() =>
        {
            result = (int) ExpressionEvaluator.GetValue(foo, expression);
        });

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public void TestIndexerResolutionResolvesToExactMatchOfArgumentTypes()
    {
        Dictionary<string, object> args = new Dictionary<string, object>();
        args["bars"] = new Bar[] { new Bar() };
        Foo foo = new Foo();

        // ensure no one changed our test class
        Assert.That(foo[(Bar[]) args["bars"]], Is.EqualTo("ExactMatch"));
        Assert.That(foo[(ICollection) args["bars"]], Is.EqualTo("AssignableMatch"));

        Assert.That(ExpressionEvaluator.GetValue(foo, "#root[#bars]", args), Is.EqualTo("ExactMatch"));
    }

    [Test]
    public void TestCtorResolutionResolvesToExactMatchOfArgumentTypes()
    {
        TypeRegistry.RegisterType(typeof(Foo));
        Dictionary<string, object> args = new Dictionary<string, object>();
        args["bars"] = new Bar[] { new Bar() };

        // ensure no one changed our test class
        Foo foo1 = new Foo(1, (Bar[]) args["bars"]);
        try
        {
            Foo foo2 = new Foo(1, (ICollection) args["bars"]);
        }
        catch (InvalidOperationException) { }

        Assert.That(ExpressionEvaluator.GetValue(null, "new Foo(1, #bars)", args), Is.Not.Null);
    }

    [Test]
    public void TestCtorResolutionWithParamArray()
    {
        TypeRegistry.RegisterType(typeof(Foo));
        Assert.That(ExpressionEvaluator.GetValue(null, "new Foo('a', 'b', 'c')"), Is.Not.Null);
        Assert.That(ExpressionEvaluator.GetValue(null, "new Foo('a', null, 'c')"), Is.Not.Null);
        Assert.That(ExpressionEvaluator.GetValue(null, "new Foo(false, 'a', 'b', 'c')"), Is.Not.Null);
        Assert.That(ExpressionEvaluator.GetValue(null, "new Foo(false, 'a', null, 'c')"), Is.Not.Null);
    }

    /// <summary>
    /// Reproduce SPRNET-470.
    /// </summary>
    /// <remarks>
    /// http://opensource.atlassian.com/projects/spring/browse/SPRNET-470
    /// http://forum.springframework.net/showthread.php?t=1574
    /// </remarks>
    [Test(Description = "Test to reproduce SPRNET-470")]
    public void TestPropertyAccessForTypes()
    {
        Assert.That(ExpressionEvaluator.GetValue(null, "Int32.MaxValue"), Is.EqualTo(Int32.MaxValue));
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.Int32).MaxValue"), Is.EqualTo(Int32.MaxValue));
        Assert.That(ExpressionEvaluator.GetValue(null, "Int32.FullName"), Is.EqualTo(typeof(Int32).FullName));
        Assert.That(ExpressionEvaluator.GetValue(null, "T(System.Int32).FullName"), Is.EqualTo(typeof(Int32).FullName));
        Assert.That((bool) ExpressionEvaluator.GetValue(null, "Int32.IsSubclassOf(Int64)"), Is.False);

        TypeRegistry.RegisterType(typeof(FooType));
        Assert.That(ExpressionEvaluator.GetValue(null, "FooType.One"), Is.EqualTo(FooType.One));
        Assert.That(ExpressionEvaluator.GetValue(null, "FooType.FullName"), Is.EqualTo(typeof(FooType).FullName));
    }

    /// <summary>
    /// Reproduce SPRNET-450
    /// Try to set/get numeric value from enum type property/field
    /// </summary>
    /// <remarks>
    /// http://opensource.atlassian.com/projects/spring/browse/SPRNET-450
    /// http://forum.springframework.net/showthread.php?t=1353
    /// </remarks>
    [Test]
    public void TestSetEnumTypePropertyOrFieldFromNumeric()
    {
        //object val = Convert.ChangeType((Int16) 1, typeof(Int32));
        //Assert.That(val.GetType(), Is.EqualTo(typeof(Int32)));

        IExpression expField = Expression.Parse("SampleEnumField");
        IExpression expProperty = Expression.Parse("SampleEnumProperty");
        IExpression expFlagsField = Expression.Parse("SampleFlagsEnumField");
        IExpression expFlagsProperty = Expression.Parse("SampleFlagsEnumProperty");

        TestEnumTypePropertyClass o = new TestEnumTypePropertyClass();

        // test field set operations
        expField.SetValue(o, TestEnumTypePropertyClass.ESampleEnumType.Trunk);
        Assert.That(o.SampleEnumField, Is.EqualTo(TestEnumTypePropertyClass.ESampleEnumType.Trunk));

        expField.SetValue(o, (short) 1);
        Assert.That(o.SampleEnumField, Is.EqualTo(TestEnumTypePropertyClass.ESampleEnumType.Trunk));

        expField.SetValue(o, 1);
        Assert.That(o.SampleEnumField, Is.EqualTo(TestEnumTypePropertyClass.ESampleEnumType.Trunk));

        expField.SetValue(o, (long) 1);
        Assert.That(o.SampleEnumField, Is.EqualTo(TestEnumTypePropertyClass.ESampleEnumType.Trunk));

        // test property set operations
        expProperty.SetValue(o, TestEnumTypePropertyClass.ESampleEnumType.Trunk);
        Assert.That(o.SampleEnumField, Is.EqualTo(TestEnumTypePropertyClass.ESampleEnumType.Trunk));

        expProperty.SetValue(o, (short) 1);
        Assert.That(o.SampleEnumField, Is.EqualTo(TestEnumTypePropertyClass.ESampleEnumType.Trunk));

        expProperty.SetValue(o, 1);
        Assert.That(o.SampleEnumField, Is.EqualTo(TestEnumTypePropertyClass.ESampleEnumType.Trunk));

        expProperty.SetValue(o, (long) 1);
        Assert.That(o.SampleEnumField, Is.EqualTo(TestEnumTypePropertyClass.ESampleEnumType.Trunk));

        expProperty.SetValue(o, "Trunk");
        Assert.That(o.SampleEnumField, Is.EqualTo(TestEnumTypePropertyClass.ESampleEnumType.Trunk));

        try
        {
            expProperty.SetValue(o, 1.0);
            Assert.Fail("should throw");
        }
        catch (TypeMismatchException e)
        {
            Assert.That(e.Message.StartsWith("Cannot convert property value of type [System.Double]"), Is.True);
        }

        try
        {
            expProperty.SetValue(o, ((float) 1.0));
            Assert.Fail("should throw");
        }
        catch (TypeMismatchException e)
        {
            Assert.That(e.Message.StartsWith("Cannot convert property value of type [System.Single]"), Is.True);
        }

        // test get operations
        object val = expField.GetValue(o);
        Assert.That(val.GetType(), Is.EqualTo(typeof(TestEnumTypePropertyClass.ESampleEnumType)));
        Assert.That(val, Is.EqualTo(TestEnumTypePropertyClass.ESampleEnumType.Trunk));

        val = expProperty.GetValue(o);
        Assert.That(val.GetType(), Is.EqualTo(typeof(TestEnumTypePropertyClass.ESampleEnumType)));
        Assert.That(val, Is.EqualTo(TestEnumTypePropertyClass.ESampleEnumType.Trunk));

        // test bitwise combined enum set
        try
        {
            // not allowed since -1 is not defined in enum
            expField.SetValue(o, -1);
            Assert.Fail("should throw");
        }
        catch (TypeMismatchException ex)
        {
            Assert.That(
                ex.Message.StartsWith("Cannot convert property value of type [System.Int32] to required type"),
                Is.True);
        }

        try
        {
            // not allowed since -1 is not a representation of any bitwise combination of the enum's values.
            expFlagsField.SetValue(o, -1);
            Assert.Fail("should throw");
        }
        catch (TypeMismatchException ex)
        {
            Assert.That(
                ex.Message.StartsWith("Cannot convert property value of type [System.Int32] to required type"),
                Is.True);
        }

        expFlagsField.SetValue(o,
            TestEnumTypePropertyClass.ESampleFlagsEnumType.SOME |
            TestEnumTypePropertyClass.ESampleFlagsEnumType.SOMEOTHER);
        Assert.That(
            o.SampleFlagsEnumField, Is.EqualTo(TestEnumTypePropertyClass.ESampleFlagsEnumType.SOME |
            TestEnumTypePropertyClass.ESampleFlagsEnumType.SOMEOTHER));
    }

    /// <summary>
    /// Test to reproduce SPRNET-342, provided on the forum.
    /// </summary>
    /// <remarks>
    /// http://opensource.atlassian.com/projects/spring/browse/SPRNET-342
    /// http://forum.springframework.net/showthread.php?t=614
    /// </remarks>
    [Test]
    public void ForumTestThread614()
    {
        TypeRegistry.RegisterType(typeof(Sample));

        IExpression e = Expression.Parse("new Sample(O, T, H)");

        Sample d1 = new Sample("A", "B", "C");
        Sample d2 = new Sample("A", "B", "Z");

        Sample tmp1 = (Sample) e.GetValue(d1);
        Assert.That(tmp1.O, Is.EqualTo("A"));
        Assert.That(tmp1.T, Is.EqualTo("B"));
        Assert.That(tmp1.H, Is.EqualTo("C"));

        Sample tmp2 = (Sample) e.GetValue(d2);
        Assert.That(tmp2.O, Is.EqualTo("A"));
        Assert.That(tmp2.T, Is.EqualTo("B"));
        Assert.That(tmp2.H, Is.EqualTo("Z"));
    }

    /// <summary>
    /// More "to the point" test to reproduce SPRNET-342
    /// </summary>
    /// <remarks>
    /// http://opensource.atlassian.com/projects/spring/browse/SPRNET-342
    /// http://forum.springframework.net/showthread.php?t=614
    /// </remarks>
    [Test]
    public void RootContextChangeTest()
    {
        IExpression e = Expression.Parse("#root");

        Assert.That(e.GetValue("RootA"), Is.EqualTo("RootA"));
        Assert.That(e.GetValue("RootB"), Is.EqualTo("RootB"));
    }

    /// <summary>
    /// Hopefully detects threading issues during Expression evaluation
    /// </summary>
    /// <remarks>
    /// A single expression instance may be evaluated against different contexts on different threads.
    /// </remarks>
    [Test]
    public void ExpressionEvaluationIsThreadSafe()
    {
        IExpression exp = Expression.Parse("PlaceOfBirth.Country");

        Inventor seovic = new Inventor("Aleksandar Seovic", new DateTime(1974, 08, 24), "Serbian");

        AsyncTestTask t1 = new AsyncTestExpressionEvaluation(2000, exp, tesla, tesla.PlaceOfBirth.Country, null).Start();
        AsyncTestTask t2 = new AsyncTestExpressionEvaluation(2000, exp, pupin, pupin.PlaceOfBirth.Country, null).Start();
        AsyncTestTask t3 = new AsyncTestExpressionEvaluation(2000, exp, seovic, seovic.PlaceOfBirth.Country, null).Start();

        IExpression exp2 = Expression.Parse("(#fact = {|n| $n <= 1 ? 1 : $n * #fact($n-1) }; #fact(#root))");

        AsyncTestTask t4 = new AsyncTestExpressionEvaluation(2000, exp2, 5, 120, new Dictionary<string, object>()).Start();
        AsyncTestTask t5 = new AsyncTestExpressionEvaluation(2000, exp2, 6, 720, new Dictionary<string, object>()).Start();

        t1.AssertNoException();
        t2.AssertNoException();
        t3.AssertNoException();
        t4.AssertNoException();
        t5.AssertNoException();
    }

    /// <summary>
    /// Hopefully detects threading issues during Expression parsing
    /// </summary>
    [Test]
    public void ExpressionParserIsThreadSafe()
    {
        AsyncTestTask t1 = new AsyncTestMethod(200, new ThreadStart(TestOperatorPrecedence)).Start();
        AsyncTestTask t2 = new AsyncTestMethod(200, new ThreadStart(TestOperatorPrecedence)).Start();
        AsyncTestTask t3 = new AsyncTestMethod(200, new ThreadStart(TestOperatorPrecedence)).Start();

        t1.AssertNoException();
        t2.AssertNoException();
        t3.AssertNoException();
    }

    /// <summary>
    /// Checks if Expression Language array initializers work properly.
    /// </summary>
    [Test]
    public void TestArrayConstructor()
    {
        object obj = ExpressionEvaluator.GetValue(null, "new int[] {3, 4, 5, 6}");
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj, Is.InstanceOf(typeof(int[])));
        int[] intarray = (int[]) obj;
        Assert.That(intarray.Length, Is.EqualTo(4));
        for (int i = 0; i < intarray.Length; i++)
        {
            Assert.That(intarray[i], Is.EqualTo(i + 3));
        }

        obj = ExpressionEvaluator.GetValue(null, "new long[5]");
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj, Is.InstanceOf(typeof(long[])));
        long[] longarray = obj as long[];
        Assert.That(longarray.Length, Is.EqualTo(5));
        for (int i = 0; i < longarray.Length; i++)
        {
            Assert.That(longarray[i], Is.EqualTo(0));
        }

        obj = ExpressionEvaluator.GetValue(null, "new double[4, 5]");
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj, Is.InstanceOf(typeof(double[,])));
        double[,] twodimarray = obj as double[,];
        Assert.That(twodimarray.Length, Is.EqualTo(4 * 5));
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Assert.That(twodimarray[i, j], Is.EqualTo(0.0));
            }
        }

        obj = ExpressionEvaluator.GetValue(null, "new int[Int32.Parse('11')]");
        Assert.That(obj, Is.Not.Null);
        Assert.That(obj, Is.InstanceOf(typeof(int[])));
        intarray = obj as int[];
        Assert.That(intarray.Length, Is.EqualTo(11));
    }

    [Test]
    public void TestMethodArgumentNodesResolveAgainstThisContext()
    {
        IExpression exp;

        // case #root == #this - ToString() will be applied to #this
        exp = Expression.Parse("long.Parse(ToString())");
        object result = exp.GetValue(100);
        Assert.That(result, Is.EqualTo((long) 100));

        // case #root != #this in Projection - ToString() will be applied to #this
        exp = Expression.Parse("(ToString(); #noop ={|val| $val}; !{#noop(ToString()) } )");
        result = exp.GetValue(new int[] { 100, 200 }, new Dictionary<string, object>());
        Assert.That(result, Is.EqualTo(new string[] { "100", "200" }));

        // case #root != #this in Selection - ToString() will be applied to #this
        exp = Expression.Parse("(#noop ={|val| $val}; ?{#noop(ToString()=='100')} )");
        result = exp.GetValue(new int[] { 100, 200 }, new Dictionary<string, object>());
        IList list = new ArrayList();
        list.Add(100);
        Assert.That(result, Is.EqualTo(list));
    }

    [Test]
    public void TestAccessVisibility()
    {
        AccessVisibilityCases cases = new AccessVisibilityCases();

        try
        {
            ExpressionEvaluator.SetValue(cases, "_privateReadonlyField", "notsoreadonly");
            Assert.Fail("writing to readonly field should throw " + typeof(NotWritablePropertyException).FullName);
        }
        catch (NotWritablePropertyException) { }

        try
        {
            ExpressionEvaluator.SetValue(cases, "PrivateReadonlyProperty", "notsoreadonly");
            Assert.Fail("writing to readonly field should throw " + typeof(NotWritablePropertyException).FullName);
        }
        catch (NotWritablePropertyException) { }

        Assert.That(ExpressionEvaluator.GetValue(cases, "_privateField"), Is.EqualTo("_privateField"));
        Assert.That(ExpressionEvaluator.GetValue(cases, "_protectedField"), Is.EqualTo("_protectedField"));
        Assert.That(ExpressionEvaluator.GetValue(cases, "_publicField"), Is.EqualTo("_publicField"));

        Assert.That(ExpressionEvaluator.GetValue(cases, "PrivateProperty"), Is.EqualTo("PrivateProperty"));
        Assert.That(ExpressionEvaluator.GetValue(cases, "ProtectedProperty"), Is.EqualTo("ProtectedProperty"));
        Assert.That(ExpressionEvaluator.GetValue(cases, "PublicProperty"), Is.EqualTo("PublicProperty"));

        Assert.That(ExpressionEvaluator.GetValue(cases, "#root[1]"), Is.EqualTo("PrivateIndexer"));
        Assert.That(ExpressionEvaluator.GetValue(cases, "#root[1.0]"), Is.EqualTo("ProtectedIndexer"));
        Assert.That(ExpressionEvaluator.GetValue(cases, "#root['']"), Is.EqualTo("PublicIndexer"));

        Assert.That(ExpressionEvaluator.GetValue(cases, "GetPrivateMethod()"), Is.EqualTo("PrivateMethod"));
        Assert.That(ExpressionEvaluator.GetValue(cases, "GetProtectedMethod()"), Is.EqualTo("ProtectedMethod"));
        Assert.That(ExpressionEvaluator.GetValue(cases, "GetPublicMethod()"), Is.EqualTo("PublicMethod"));
    }

    internal class AccessVisibilityCases
    {
        private readonly string _privateReadonlyField = "_privateReadonlyField";
        private string _privateField = "_privateField";
        protected string _protectedField = "_protectedField";
        public string _publicField = "_publicField";

        public AccessVisibilityCases()
        {
            Assert.That(_privateReadonlyField != string.Empty, Is.True);
            Assert.That(_privateField != string.Empty, Is.True);
        }

        private string PrivateReadonlyProperty { get { return "PrivateReadonlyProperty"; } }
        private string PrivateProperty { get { return "PrivateProperty"; } }
        protected string ProtectedProperty { get { return "ProtectedProperty"; } }
        public string PublicProperty { get { return "PublicProperty"; } }

        private string this[int intindex] { get { return "PrivateIndexer"; } }
        protected string this[double strindex] { get { return "ProtectedIndexer"; } }
        public string this[string strindex] { get { return "PublicIndexer"; } }

        private string GetPrivateMethod() { return "PrivateMethod"; }
        protected string GetProtectedMethod() { return "ProtectedMethod"; }
        public string GetPublicMethod() { return "PublicMethod"; }
    }

    class MethodInvokationCases
    {
        public string Foo(string stringArg) { return stringArg; }
        public int Foo(int intArg) { return intArg; }
    }

    [Test]
    public void TestUnionOperator()
    {
        object o = ExpressionEvaluator.GetValue(null, "{1,2,3} + {3,4,5}");
        Assert.That(o, Is.InstanceOf(typeof(ISet)));
        ISet union = (ISet) o;
        Assert.That(union.Count, Is.EqualTo(5));
        Assert.That(union.Contains(1), Is.True);
        Assert.That(union.Contains(3), Is.True);
        Assert.That(union.Contains(5), Is.True);

        o = ExpressionEvaluator.GetValue(null, "{1,2,3} + {3,4,5} + {'ivan', 'gox', 'damjao', 5}");
        Assert.That(o, Is.InstanceOf(typeof(ISet)));
        union = (ISet) o;
        Assert.That(union.Count, Is.EqualTo(8));
        Assert.That(union.Contains(1), Is.True);
        Assert.That(union.Contains("ivan"), Is.True);

        ISet testset = new ListSet();
        testset.AddAll(new int[] { 1, 2, 3, 5, 8 });
        o = ExpressionEvaluator.GetValue(testset, "#this + {1, 2, 13, 15}");
        Assert.That(o, Is.InstanceOf(typeof(ISet)));
        union = (ISet) o;
        Assert.That(union.Count, Is.EqualTo(7));
        Assert.That(union.Contains(1), Is.True);
        Assert.That(union.Contains(15), Is.True);

        o = ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} + #{1:'ivan', 5:'five'}");
        Assert.That(o, Is.InstanceOf(typeof(IDictionary)));
        IDictionary result = (IDictionary) o;
        Assert.That(result.Count, Is.EqualTo(4));
        Assert.That(result[1], Is.EqualTo("one"));
        Assert.That(result[5], Is.EqualTo("five"));
    }

    [Test]
    public void TestUnionOperatorBad()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} + {1, 5}"));
    }

    [Test]
    public void TestIntersectionOperator()
    {
        object o = ExpressionEvaluator.GetValue(null, "{111, 'ivan', 23, 24} * {111, 11, 'ivan'}");
        Assert.That(o, Is.InstanceOf(typeof(ISet)));
        ISet intersection = (ISet) o;
        Assert.That(intersection.Count, Is.EqualTo(2));
        Assert.That(intersection.Contains(111), Is.True);
        Assert.That(intersection.Contains("ivan"), Is.True);

        o = ExpressionEvaluator.GetValue(null, "{24, 25, 'aaa' + 'bb'} * {date('2007/2/5').day * 5, 24 - 1}");
        Assert.That(o, Is.InstanceOf(typeof(ISet)));
        intersection = (ISet) o;
        Assert.That(intersection.Count, Is.EqualTo(1));
        Assert.That(intersection.Contains(25), Is.True);

        ISet testset = new ListSet();
        testset.AddAll(new int[] { 1, 2, 3, 5, 8 });
        o = ExpressionEvaluator.GetValue(testset, "#this * #{1:'one', 10:'ten'}");
        Assert.That(o, Is.InstanceOf(typeof(ISet)));
        intersection = (ISet) o;
        Assert.That(intersection.Count, Is.EqualTo(1));
        Assert.That(intersection.Contains(1), Is.True);

        o = ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} * #{1:'ivan', 5:'five'}");
        Assert.That(o, Is.InstanceOf(typeof(IDictionary)));
        IDictionary result = (IDictionary) o;
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[1], Is.EqualTo("one"));

        o = ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} * {1, 2, 5, 7}");
        Assert.That(o, Is.InstanceOf(typeof(IDictionary)));
        result = (IDictionary) o;
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[1], Is.EqualTo("one"));
        Assert.That(result[2], Is.EqualTo("two"));
    }

    [Test]
    public void TestIntersectionOperatorBad()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} * 'something'"));
    }

    [Test]
    public void TestDifferenceOperator()
    {
        object o = ExpressionEvaluator.GetValue(null, "{111, 11} - {14, 12, 11}");
        Assert.That(o, Is.InstanceOf(typeof(ISet)));
        ISet diff = (ISet) o;
        Assert.That(diff.Count, Is.EqualTo(1));
        Assert.That(diff.Contains(111), Is.True);

        o = ExpressionEvaluator.GetValue(null, "{111, 11} - {14, 12, 11} - {111}");
        Assert.That(o, Is.InstanceOf(typeof(ISet)));
        diff = (ISet) o;
        Assert.That(diff.Count, Is.EqualTo(0));

        ISet testset = new ListSet();
        testset.AddAll(new int[] { 1, 2, 3, 5, 8 });
        o = ExpressionEvaluator.GetValue(testset, "#this - #{1:'one', 10:'ten'}");
        Assert.That(o, Is.InstanceOf(typeof(ISet)));
        diff = (ISet) o;
        Assert.That(diff.Count, Is.EqualTo(4));
        Assert.That(diff.Contains(1), Is.False);

        o = ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} - #{1:'ivan', 5:'five'}");
        Assert.That(o, Is.InstanceOf(typeof(IDictionary)));
        IDictionary result = (IDictionary) o;
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result[1], Is.Null);
        Assert.That(result[3], Is.EqualTo("three"));

        o = ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} - {1, 2, 3, 5, 7}");
        Assert.That(o, Is.InstanceOf(typeof(IDictionary)));
        result = (IDictionary) o;
        Assert.That(result.Count, Is.EqualTo(0));
    }

    [Test]
    public void TestDifferenceOperatorBad()
    {
        Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} - 'something'"));
    }

    private DateTime start, stop;

    //[Test]
    public void PerformanceTests()
    {
        int n = 10000000;
        object x = "";
        IDictionary<string, object> vars = new Dictionary<string, object>();

        // tesla.PlaceOfBirth
        start = DateTime.Now;
        for (int i = 0; i < n; i++)
        {
            x = tesla.PlaceOfBirth;
        }

        stop = DateTime.Now;
        PrintTest("tesla.PlaceOfBirth (direct)", n, Elapsed);

        //            start = DateTime.Now;
        //            for (int i = 0; i < n; i++)
        //            {
        //                x = ExpressionEvaluator.GetValue(tesla, "PlaceOfBirth", vars);
        //            }
        //            stop = DateTime.Now;
        //            PrintTest("tesla.PlaceOfBirth (multi-parse)", n, Elapsed);

        start = DateTime.Now;
        IExpression exp = Expression.Parse("PlaceOfBirth");
        for (int i = 0; i < n; i++)
        {
            x = exp.GetValue(tesla, vars);
        }

        stop = DateTime.Now;
        PrintTest("tesla.PlaceOfBirth (single-parse)", n, Elapsed);

        // ieee.Officers['advisors'][0].Inventions[2]
        start = DateTime.Now;
        for (int i = 0; i < n; i++)
        {
            x = ((Inventor) ((IList) ieee.Officers["advisors"])[0]).Inventions[2];
        }

        stop = DateTime.Now;
        PrintTest("ieee.Officers['advisors'][0].Inventions[2] (direct)", n, Elapsed);

        //            start = DateTime.Now;
        //            for (int i = 0; i < n / 10; i++)
        //            {
        //                x = ExpressionEvaluator.GetValue(ieee, "Officers['advisors'][0].Inventions[2]", vars);
        //            }
        //            stop = DateTime.Now;
        //            PrintTest("ieee.Officers['advisors'][0].Inventions[2] (multi-parse)", n / 10, Elapsed);

        start = DateTime.Now;
        exp = Expression.Parse("Officers['advisors'][0].Inventions[2]");
        for (int i = 0; i < n; i++)
        {
            x = exp.GetValue(ieee, vars);
        }

        stop = DateTime.Now;
        PrintTest("ieee.Officers['advisors'][0].Inventions[2] (single-parse)", n, Elapsed);

        x.ToString();
    }

    private double Elapsed
    {
        get { return (stop.Ticks - start.Ticks) / 10000000f; }
    }

    private static void PrintTest(string name, int iterations, double duration)
    {
        Debug.WriteLine(
            String.Format("{0,-60} {1,12:#,###} {2,12:##0.000} {3,12:#,###}", name, iterations, duration,
                iterations / duration));
    }

    [Test]
    public void TestInheritedMethodInvocation()
    {
        DerivedSingleMethodTestClass testClass = new DerivedSingleMethodTestClass();
        Assert.That(ExpressionEvaluator.GetValue(testClass, "#root.GetString()"), Is.EqualTo("Hello World"));
    }

    [Test]
    public void TestStaticInheritedMethodInvocation()
    {
        Assert.That(DerivedSingleMethodTestClass.StaticMethod(), Is.EqualTo("Hello Static World from SingleMethodTestClass"));
        Assert.That(ExpressionEvaluator.GetValue(null, string.Format("T({0}).StaticMethod()", typeof(DerivedSingleMethodTestClass).FullName)), Is.EqualTo("Hello Static World from SingleMethodTestClass"));

        Assert.That(SingleMethodTestClass.ShadowedStaticMethod(), Is.EqualTo("SingleMethodTestClass.ShadowedStaticMethod"));
        Assert.That(DerivedSingleMethodTestClass.ShadowedStaticMethod(), Is.EqualTo("DerivedSingleMethodTestClass.ShadowedStaticMethod"));
        Assert.That(ExpressionEvaluator.GetValue(null, string.Format("T({0}).ShadowedStaticMethod()", typeof(SingleMethodTestClass).FullName)), Is.EqualTo("SingleMethodTestClass.ShadowedStaticMethod"));
        Assert.That(ExpressionEvaluator.GetValue(null, string.Format("T({0}).ShadowedStaticMethod()", typeof(DerivedSingleMethodTestClass).FullName)), Is.EqualTo("DerivedSingleMethodTestClass.ShadowedStaticMethod"));
    }

    private static void DumpNode(SpringAST rootNode, int level)
    {
        Trace.WriteLine(new string(' ', level) + rootNode.ToString());

        int numberOfChildren = rootNode.getNumberOfChildren();
        if (numberOfChildren > 0)
        {
            SpringAST node = rootNode.getFirstChild();
            while (node != null)
            {
                DumpNode(node, level + 2);
                node = node.getNextSibling();
            }
        }
    }
}

internal class SingleMethodTestClass
{
    protected static string GetMethodName(MethodBase method)
    {
        return method.DeclaringType.Name + "." + method.Name;
    }

    public static string StaticMethod()
    {
        return "Hello Static World from SingleMethodTestClass";
    }

    public static string ShadowedStaticMethod()
    {
        return GetMethodName(MethodInfo.GetCurrentMethod());
    }

    public string GetString()
    {
        return "Hello World";
    }
}

internal class DerivedSingleMethodTestClass : SingleMethodTestClass
{
    public new static string ShadowedStaticMethod()
    {
        return GetMethodName(MethodInfo.GetCurrentMethod());
    }
}

internal class TestObjectContainer
{
    private TestObject testObject;

    public TestObject TestObject
    {
        get { return this.testObject; }
        set { this.testObject = value; }
    }
}

internal class TestEnumTypePropertyClass
{
    [Flags]
    internal enum ESampleFlagsEnumType : int
    {
        NONE = 0,
        SOME = 1,
        SOMEOTHER = 2,
    }

    internal enum ESampleEnumType : int
    {
        Van = 0,
        Trunk = 1,
        Air = 2
    }

    public ESampleFlagsEnumType SampleFlagsEnumField;
    public ESampleEnumType SampleEnumField;

    public ESampleFlagsEnumType SampleFlagsEnumProperty
    {
        get { return SampleFlagsEnumField; }
        set { SampleFlagsEnumField = value; }
    }

    public ESampleEnumType SampleEnumProperty
    {
        get { return SampleEnumField; }
        set { SampleEnumField = value; }
    }
}

internal sealed class Bar
{
    private int[] numbers = new int[] { 1, 2, 3 };

    public int this[int index]
    {
        get { return numbers[index]; }
    }
}

internal class Foo
{
    private FooType type;
    private Nullable<DateTime> nullableDate;
    private Nullable<Int32> nullableInt;

    public Foo() : this(FooType.One)
    {
    }

    public Foo(FooType type)
    {
        this.type = type;
    }

    public Foo(params string[] values)
    {
    }

    public Foo(bool flag, params string[] values)
    {
    }

    public Foo(int flag, Bar[] bars)
    {
    }

    public Foo(int flag, ICollection bars)
    {
        throw new InvalidOperationException("should have selected ctor(int, Bar[])");
    }

    public string this[Bar[] bars]
    {
        get { return "ExactMatch"; }
    }

    public string this[ICollection bars]
    {
        get { return "AssignableMatch"; }
    }

    public object this[int foo, string key]
    {
        get { return key + "_" + foo; }
    }

    public FooType Type
    {
        get { return type; }
    }

    public DateTime? NullableDate
    {
        get { return nullableDate; }
        set { nullableDate = value; }
    }

    public int? NullableInt
    {
        get { return nullableInt; }
        set { nullableInt = value; }
    }

    public string MethodWithSimilarArguments(int flags, Bar[] bars)
    {
        return "ExactMatch";
    }

    public string MethodWithSimilarArguments(int flags, ICollection bar)
    {
        return "AssignableMatch";
    }

    public string MethodWithArrayArgument(string[] values)
    {
        return string.Join("|", values);
    }

    public string MethodWithParamArray(params string[] values)
    {
        return string.Join("|", values);
    }

    public string MethodWithParamArray(bool uppercase, params string[] values)
    {
        string ret = string.Join("|", values);
        return (uppercase ? ret.ToUpper() : ret);
    }

    public int MethodWithParamArray(params int[] values)
    {
        return values.Length;
    }
}

internal enum FooType
{
    One,
    Two,
    Three
}

internal class Sample
{
    public string O;
    public string T;
    public string H;

    public Sample(string o, string t, string h)
    {
        O = o;
        T = t;
        H = h;
    }
}

internal class ShadowingTestsBaseClass
{
    private object _someValue;
    private object _readonlyShadowedValue;
    private object _writeonlyShadowedValue;

    public object SomeValue
    {
        get { return _someValue; }
        set { _someValue = value; }
    }

    public object ReadonlyShadowedValue
    {
        get { return _readonlyShadowedValue; }
        set { _readonlyShadowedValue = value; }
    }

    public object WriteonlyShadowedValue
    {
        get { return _writeonlyShadowedValue; }
        set { _writeonlyShadowedValue = value; }
    }
}

internal class ShadowingTestsSpezializedClass : ShadowingTestsBaseClass
{
    public new string SomeValue
    {
        get { return (string) base.SomeValue; }
        set { base.SomeValue = value; }
    }

    public new string ReadonlyShadowedValue
    {
        get { return (string) base.ReadonlyShadowedValue; }
    }

    public new string WriteonlyShadowedValue
    {
        set { base.WriteonlyShadowedValue = value; }
    }
}

internal class ShadowingTestsMoreSpezializedClass : ShadowingTestsSpezializedClass
{
}

internal class ShadowingTestsMostSpezializedClass : ShadowingTestsMoreSpezializedClass
{
}