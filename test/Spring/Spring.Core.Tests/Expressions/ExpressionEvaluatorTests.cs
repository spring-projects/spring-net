#region License

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

#endregion

#region Imports

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.EnterpriseServices;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Services;
using NUnit.Framework;
using Spring.Collections;
using Spring.Context;
using Spring.Context.Support;
using Spring.Core;
using Spring.Core.TypeResolution;
using Spring.Expressions.Parser.antlr;
using Spring.Expressions.Parser.antlr.collections;
using Spring.Expressions.Processors;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Threading;
using Spring.Util;

#endregion

namespace Spring.Expressions
{
    /// <summary>
    /// This class contains tests for ExpressionEvaluator.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public sealed class ExpressionEvaluatorTests
    {
        #region Helper classes for threading tests

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
                Assert.AreEqual(expected, result);
            }
        }

        #endregion

        private Inventor tesla;
        private Inventor pupin;
        private Society ieee;

        #region SetUp and TearDown

        /// <summary>
        /// The setup logic executed before the execution of each individual test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            ContextRegistry.Clear();
            tesla = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            tesla.Inventions = new string[]
                {
                    "Telephone repeater", "Rotating magnetic field principle",
                    "Polyphase alternating-current system", "Induction motor",
                    "Alternating-current power transmission", "Tesla coil transformer",
                    "Wireless communication", "Radio", "Fluorescent lights"
                };
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

        #endregion

        #region Serialization Tests

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
                    IExpression exp = (IExpression)Activator.CreateInstance(t, true);
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
            Assert.AreEqual(5, exp.TestValue);
            SerializationTestExpression exp2 = (SerializationTestExpression)SerializeDeserializeExpression(exp);
            Assert.AreEqual(0, exp2.TestValue);
        }

        /// <summary>
        /// This test ensures, that the default node-type is serializable.
        /// </summary>
        /// <remarks>
        /// date() is parsed into DateLiteralNode( down:&lt;default node type&gt; ).
        /// Normally antlr.CommonAST is the default node used by antlr. To enable serialization, Spring
        /// uses a custom ASTFactory in <see cref="Expression.Parse"/>
        /// </remarks>
        [Test]
        public void ExpressionDateLiteralNodeMaintainsStateAfterSerialization()
        {
            IExpression exp = Expression.Parse("date('08-24-1974', 'MM-dd-yyyy')");

            Assert.AreEqual(new DateTime(1974, 8, 24), exp.GetValue(null));

            exp = SerializeDeserializeExpression(exp);

            Assert.AreEqual(new DateTime(1974, 8, 24), exp.GetValue(null));
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
                exp = (IExpression)formatter.Deserialize(ms);
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
                ms.Read(b, 0, (int)ms.Length);
                xml = Encoding.ASCII.GetString(b, 0, b.Length);
            }
            using (StringReader sr = new StringReader(xml))
            {
                byte[] b = Encoding.ASCII.GetBytes(xml);
                Stream stream = new MemoryStream(b);
                exp = (IExpression)formatter.Deserialize(stream);
            }
            return exp;
        }

        #endregion Serialization Tests

        [Test]
        public void TestConstantRead()
        {
            object value = ExpressionEvaluator.GetValue(null, "Society.ByteConst == 1");
            Assert.AreEqual(true, value);
        }

        [Test]
        public void TestBitwiseXOR()
        {
            object value = ExpressionEvaluator.GetValue(null, "'123' + 1");
            Assert.AreEqual("1231", value);
        }

        [Test]
        public void TestMixedAddition()
        {
            object value = ExpressionEvaluator.GetValue(null, "'123' + 1");
            Assert.AreEqual("1231", value);
        }

        [Test(Description = "SPRNET-1507 - Test 1")]
        public void TestExpandoObject()
        {
            dynamic dynamicObject = new System.Dynamic.ExpandoObject();
            //add property at run-time
            dynamicObject.IssueId = "1507";

            object value = ExpressionEvaluator.GetValue(dynamicObject, "IssueId");
            Assert.AreEqual("1507", value);
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
                Assert.AreEqual(
                    "'PropertyName' node cannot be resolved for the specified context [System.Dynamic.ExpandoObject].",
                    ex.Message);
            }
        }

        [Test(Description = "SPRNET-944")]
        public void DateTests()
        {
            string dateLiteral = (string)ExpressionEvaluator.GetValue(null, "'date'");
            Assert.AreEqual("date", dateLiteral);
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
            catch (RecognitionException ex)
            {
                Assert.AreEqual("Syntax Error on line 1, column 6: expecting ''', found '<EOF>' in expression"+Environment.NewLine+"''date'", ex.Message);
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
            Assert.AreEqual(ExpressionEvaluator.GetValue(now, null), now);
            Assert.AreEqual(ExpressionEvaluator.GetValue(now, ""), now);
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
            Assert.IsNull(ExpressionEvaluator.GetValue(null, "null"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "'xyz' == null"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "null != 'xyz'"));
        }

        [Test]
        public void TestUnicode()
        {
            Assert.AreEqual("\u6f22\u5b57", ExpressionEvaluator.GetValue(null, "'\u6f22\u5b57'"));
        }
        /// <summary>
        /// Tests string literals.
        /// </summary>
        [Test]
        public void TestStringLiterals()
        {
            Assert.AreEqual("literal string", ExpressionEvaluator.GetValue(null, "'literal string'"));
            Assert.AreEqual("literal 'string", ExpressionEvaluator.GetValue(null, "'literal ''string'"));
            Assert.AreEqual(string.Empty, ExpressionEvaluator.GetValue(null, "''"));
            Assert.AreEqual("escaped \t string \n", ExpressionEvaluator.GetValue(null, "'escaped \t string \n'"));
            //Debug.Write(ExpressionEvaluator.GetValue(null, "'escaped\tstring\nsecond line\n\nfourth line'"));
        }

        /// <summary>
        /// Tests integer literals.
        /// </summary>
        [Test]
        public void TestIntLiterals()
        {
            object int32 = ExpressionEvaluator.GetValue(null, Int32.MaxValue.ToString());
            Assert.AreEqual(int32, Int32.MaxValue);
            Assert.IsTrue(int32 is Int32);
            Assert.AreEqual(32, ExpressionEvaluator.GetValue(null, "0x20"));

            Assert.AreEqual(Int64.MaxValue.ToString(), ExpressionEvaluator.GetValue(null, Int64.MaxValue.ToString() + ".ToString()"));
            Assert.AreEqual(Int64.MaxValue.ToString(), ExpressionEvaluator.GetValue(null, "long.MaxValue.ToString()"));

            object int64 = ExpressionEvaluator.GetValue(null, Int64.MaxValue.ToString());
            Assert.AreEqual(int64, Int64.MaxValue);
            Assert.IsTrue(int64 is Int64);
        }

        /// <summary>
        /// Tests hexadecimal integer literals.
        /// </summary>
        [Test]
        public void TestHexLiterals()
        {
            IExpression exp = Expression.Parse("0x20");
            Assert.AreEqual(32, exp.GetValue());
            Assert.AreEqual(32, exp.GetValue());
            Assert.AreEqual(255, ExpressionEvaluator.GetValue(null, "0xFF"));
            Assert.AreEqual(Int32.MaxValue, ExpressionEvaluator.GetValue(null, "0x7FFFFFFF"));
            Assert.AreEqual(Int64.MaxValue, ExpressionEvaluator.GetValue(null, "0x7FFFFFFFFFFFFFFF"));
            Assert.AreEqual(Int32.MinValue, ExpressionEvaluator.GetValue(null, "0x80000000"));
            Assert.AreEqual(Int64.MinValue, ExpressionEvaluator.GetValue(null, "0x8000000000000000"));
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


            Assert.IsTrue(s is Double);
            Assert.IsTrue(d is Double);
            Assert.IsTrue(dec is Decimal);

            Assert.AreEqual(s, 3.402823E+38);
            Assert.AreEqual(d, 1.797693E+308);

            Assert.AreEqual(5.25F, ExpressionEvaluator.GetValue(null, "5.25f"));
            Assert.AreEqual(0.75d, ExpressionEvaluator.GetValue(null, "0.75D"));

            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "1000 == 1e3 and 1e+4 != 1000"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "100 < 1000.00m and 10000.00 > 1000"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "100 < 1000.00 and 10000.00m > 1e2"));
        }

        /// <summary>
        /// Tests boolean literals.
        /// </summary>
        [Test]
        public void TestBooleanLiterals()
        {
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false"));
        }

        /// <summary>
        /// Tests date literals.
        /// </summary>
        [Test]
        public void TestDateLiterals()
        {
            IExpression exp = Expression.Parse("date('1974/08/24')");
            Assert.AreEqual(new DateTime(1974, 8, 24), exp.GetValue());
            Assert.AreEqual(new DateTime(1974, 8, 24), exp.GetValue());
            Assert.AreEqual(new DateTime(1974, 8, 24), ExpressionEvaluator.GetValue(null, "date('1974-08-24')"));
            Assert.AreEqual(new DateTime(1974, 8, 24), ExpressionEvaluator.GetValue(null, "date('08-24-1974', 'MM-dd-yyyy')"));
            Assert.AreEqual(new DateTime(1974, 8, 24), ExpressionEvaluator.GetValue(null, "date('08/24/1974', 'MM/dd/yyyy')"));
            Assert.AreEqual(new DateTime(1974, 8, 24, 12, 35, 6), ExpressionEvaluator.GetValue(null, "date('1974-08-24 12:35:06Z', 'u')"));
            Assert.AreEqual(1974, ExpressionEvaluator.GetValue(null, "date('1974/08/24').Year"));
            Assert.AreEqual(2005, ExpressionEvaluator.GetValue(null, "date('1974/08/24').AddYears(31).Year"));
        }

        /// <summary>
        /// Tests simple property and field accessors and mutators
        /// </summary>
        [Test]
        public void TestSimplePropertyAccess()
        {
            Assert.AreEqual(DateTime.Today, ExpressionEvaluator.GetValue(null, "DateTime.Today"));
            Assert.AreEqual("Nikola Tesla", ExpressionEvaluator.GetValue(tesla, "Name"));
            Assert.AreEqual("Idvor", ExpressionEvaluator.GetValue(pupin, "PlaceOfBirth.City"));
            ExpressionEvaluator.SetValue(tesla, "PlaceOfBirth.Country", "Croatia");
            Assert.AreEqual("Croatia", ExpressionEvaluator.GetValue(tesla, "PlaceOfBirth.Country"));
            ExpressionEvaluator.SetValue(pupin, "Name", "Michael Pupin");
            Assert.AreEqual("Michael Pupin", ExpressionEvaluator.GetValue(pupin, "Name"));
            Assert.AreEqual(new DateTime(1856, 7, 9), ExpressionEvaluator.GetValue(tesla, "DOB"));
            Assert.AreEqual(1856, ExpressionEvaluator.GetValue(tesla, "DOB.Year"));
        }

        /// <summary>
        /// Tests that simple property and field accessors and mutators are case-insensitive.
        /// </summary>
        [Test]
        public void SimplePropertyAccessIsCaseInsensitive()
        {
            Assert.AreEqual("Nikola Tesla", ExpressionEvaluator.GetValue(tesla, "nAme"));
            Assert.AreEqual("Idvor", ExpressionEvaluator.GetValue(pupin, "Placeofbirth.city"));
            ExpressionEvaluator.SetValue(tesla, "PlaceOfBirth.CountRY", "Croatia");
            Assert.AreEqual("Croatia", ExpressionEvaluator.GetValue(tesla, "Placeofbirth.COUNtry"));
            ExpressionEvaluator.SetValue(pupin, "NAME", "Michael Pupin");
            Assert.AreEqual("Michael Pupin", ExpressionEvaluator.GetValue(pupin, "name"));
            Assert.AreEqual(new DateTime(1856, 7, 9), ExpressionEvaluator.GetValue(tesla, "dob"));
            Assert.AreEqual(1856, ExpressionEvaluator.GetValue(tesla, "DOb.YEar"));
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
            Assert.AreEqual("SomeString", ExpressionEvaluator.GetValue(o, "SomeValue"));

            // test write
            o = new ShadowingTestsMostSpezializedClass();
            ExpressionEvaluator.SetValue(o, "SomeValue", "SomeOtherString");
            Assert.AreEqual("SomeOtherString", o.SomeValue);

            // test readonly shadowed
            o = new ShadowingTestsMostSpezializedClass();
            ((ShadowingTestsBaseClass)o).ReadonlyShadowedValue = "SomeString1";
            Assert.AreEqual("SomeString1", ExpressionEvaluator.GetValue(o, "ReadonlyShadowedValue"));
            try
            {
                ExpressionEvaluator.SetValue(o, "ReadonlyShadowedValue", "SomeString2");
                Assert.Fail("Setting readonly property should throw NotWritablePropertyException");
            }
            catch (NotWritablePropertyException)
            { }
            Assert.AreEqual("SomeString1", ExpressionEvaluator.GetValue(o, "ReadonlyShadowedValue"));

            // test writeonly shadowed
            o = new ShadowingTestsMostSpezializedClass();
            ExpressionEvaluator.SetValue(o, "WriteonlyShadowedValue", "SomeString3");
            Assert.AreEqual("SomeString3", ((ShadowingTestsBaseClass)o).WriteonlyShadowedValue);
            try
            {
                ExpressionEvaluator.GetValue(o, "WriteonlyShadowedValue");
                Assert.Fail("Getting writeonly property should throw NotReadablePropertyException");
            }
            catch (NotReadablePropertyException)
            { }
        }

        /// <summary>
        /// Tests indexed property and field accessors and mutators
        /// </summary>
        [Test]
        public void TestIndexedPropertyAccess()
        {
            TypeRegistry.RegisterType("Society", typeof(Society));

            // arrays and lists
            Assert.AreEqual("Induction motor", ExpressionEvaluator.GetValue(tesla, "Inventions[3]"));
            Assert.AreEqual("Nikola Tesla", ExpressionEvaluator.GetValue(ieee, "Members[0].Name"));
            Assert.AreEqual("Wireless communication", ExpressionEvaluator.GetValue(ieee, "Members[0].Inventions[6]"));

            // maps
            Assert.AreEqual(pupin, ExpressionEvaluator.GetValue(ieee, "Officers['president']"));
            Assert.AreEqual("Idvor", ExpressionEvaluator.GetValue(ieee, "Officers['president'].PlaceOfBirth.City"));
            Assert.AreEqual(tesla, ExpressionEvaluator.GetValue(ieee, "Officers['advisors'][0]"));
            Assert.AreEqual("Polyphase alternating-current system",
                            ExpressionEvaluator.GetValue(ieee, "Officers['advisors'][0].Inventions[2]"));

            // maps with non-literal parameters
            Dictionary<string, object> vars = new Dictionary<string, object>();
            vars["prez"] = "president";
            Assert.AreEqual(pupin, ExpressionEvaluator.GetValue(ieee, "Officers[#prez]", vars));

            Assert.AreEqual(pupin, ExpressionEvaluator.GetValue(ieee, "Officers[Society.President]"));
            Assert.AreEqual("Idvor", ExpressionEvaluator.GetValue(ieee, "Officers[Society.President].PlaceOfBirth.City"));
            Assert.AreEqual(tesla, ExpressionEvaluator.GetValue(ieee, "Officers[Society.Advisors][0]"));
            Assert.AreEqual("Polyphase alternating-current system",
                            ExpressionEvaluator.GetValue(ieee, "Officers[Society.Advisors][0].Inventions[2]"));

            // try to set some values
            ExpressionEvaluator.SetValue(ieee, "Officers['advisors'][0].PlaceOfBirth.Country", "Croatia");
            Assert.AreEqual("Croatia", ExpressionEvaluator.GetValue(tesla, "PlaceOfBirth.Country"));
            ExpressionEvaluator.SetValue(ieee, "Officers['president'].Name", "Michael Pupin");
            Assert.AreEqual("Michael Pupin", ExpressionEvaluator.GetValue(pupin, "Name"));
            ExpressionEvaluator.SetValue(ieee, "Officers['advisors']", new Inventor[] { pupin, tesla });
            Assert.AreEqual(pupin, ExpressionEvaluator.GetValue(ieee, "Officers['advisors'][0]"));
            Assert.AreEqual(tesla, ExpressionEvaluator.GetValue(ieee, "Officers['advisors'][1]"));

            // generic indexer
            Bar bar = new Bar();
            Foo foo = new Foo();
            IExpression exp = Expression.Parse("[1]");
            Assert.AreEqual(2, exp.GetValue(bar));
            Assert.AreEqual(2, exp.GetValue(bar));
            Assert.AreEqual("test_1", ExpressionEvaluator.GetValue(foo, "[1, 'test']"));
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
            Assert.AreEqual(guid.ToString(), ExpressionEvaluator.GetValue(guid, "ToString()"));
            Assert.AreEqual(guid.ToString("n"), ExpressionEvaluator.GetValue(guid, "ToString('n')"));
            Assert.AreEqual(16, ExpressionEvaluator.GetValue(null, "Guid.NewGuid().ToByteArray().Length"));

            Assert.AreEqual(2005 - tesla.DOB.Year,
                            ExpressionEvaluator.GetValue(ieee, "Members[0].GetAge(date('2005-01-01'))"));
        }

        [Test]
        public void TestMethodEvaluationOnDifferentContextType()
        {
            IExpression expression = Expression.Parse("ToString('dummy', null)");
            Assert.AreEqual("dummy", expression.GetValue(0m, null));
            Assert.AreEqual("dummy", expression.GetValue(0, null));
        }

        [Test]
        public void TestMethodEvaluationOnDifferentArgumentTypes()
        {
            IExpression expression = Expression.Parse("Foo(#var1)");
            MethodInvokationCases testContext = new MethodInvokationCases();
            Dictionary<string, object> args = new Dictionary<string, object>();
            args["var1"] = "myString";
            Assert.AreEqual("myString", expression.GetValue(testContext, args));
            args["var1"] = 12;
            Assert.AreEqual(12, expression.GetValue(testContext, args));
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
            IList placesOfBirth = (IList)ExpressionEvaluator.GetValue(ieee, "Members.!{PlaceOfBirth.City}");

            Assert.AreEqual(2, placesOfBirth.Count);
            Assert.AreEqual("Smiljan", placesOfBirth[0]);
            Assert.AreEqual("Idvor", placesOfBirth[1]);

            IList names = (IList)ExpressionEvaluator.GetValue(ieee, "Officers['advisors'].!{Name}");
            Assert.AreEqual(2, names.Count);
            Assert.AreEqual("Nikola Tesla", names[0]);
            Assert.AreEqual("Mihajlo Pupin", names[1]);
        }

        /// <summary>
        /// Tests selection node
        /// </summary>
        [Test]
        public void TestSelection()
        {
            IList memberSelection =
                (IList)ExpressionEvaluator.GetValue(ieee, "Members.?{PlaceOfBirth.City == 'Smiljan'}");

            Assert.AreEqual(1, memberSelection.Count);
            Assert.AreEqual("Nikola Tesla", ((Inventor)memberSelection[0]).Name);

            IList serbianOfficers =
                (IList)ExpressionEvaluator.GetValue(ieee, "Officers['advisors'].?{Nationality == 'Serbian'}");
            Assert.AreEqual(2, serbianOfficers.Count);
            Assert.AreEqual("Nikola Tesla", ((Inventor)serbianOfficers[0]).Name);
            Assert.AreEqual("Mihajlo Pupin", ((Inventor)serbianOfficers[1]).Name);

            Inventor first =
                (Inventor)ExpressionEvaluator.GetValue(ieee, "Officers['advisors'].^{Nationality == 'Serbian'}");
            Assert.AreEqual("Nikola Tesla", first.Name);

            Inventor last =
                (Inventor)ExpressionEvaluator.GetValue(ieee, "Officers['advisors'].${Nationality == 'Serbian'}");
            Assert.AreEqual("Mihajlo Pupin", last.Name);
        }

        /// <summary>
        /// Tests type node
        /// </summary>
        [Test]
        public void TestTypeNode()
        {
            IExpression exp = Expression.Parse("T(DateTime)");
            exp.GetValue();
            Assert.AreEqual(typeof(DateTime), exp.GetValue());

            Assert.AreEqual(typeof(DateTime), ExpressionEvaluator.GetValue(null, "T(System.DateTime)"));
            Assert.AreEqual(typeof(DateTime[]), ExpressionEvaluator.GetValue(null, "T(System.DateTime[], mscorlib)"));
            Assert.AreEqual(typeof(ExpressionEvaluator), ExpressionEvaluator.GetValue(null, "T(Spring.Expressions.ExpressionEvaluator, Spring.Core)"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(tesla, "T(System.DateTime) == DOB.GetType()"));
        }

        /// <summary>
        /// Tests type node
        /// </summary>
        [Test]
        public void TestTypeNodeWithArrays()
        {
            Assert.AreEqual(typeof(DateTime[]), ExpressionEvaluator.GetValue(null, "T(System.DateTime[])"));
            Assert.AreEqual(typeof(DateTime[,]), ExpressionEvaluator.GetValue(null, "T(System.DateTime[,])"));
            Assert.AreEqual(typeof(DateTime[]), ExpressionEvaluator.GetValue(null, "T(System.DateTime[], mscorlib)"));
            Assert.AreEqual(typeof(DateTime[,]), ExpressionEvaluator.GetValue(null, "T(System.DateTime[,], mscorlib)"));
        }

        /// <summary>
        /// Tests type node
        /// </summary>
        [Test]
        public void TestTypeNodeWithAssemblyQualifiedName()
        {
            Assert.AreEqual(typeof(ExpressionEvaluator), ExpressionEvaluator.GetValue(null, string.Format("T({0})", typeof(ExpressionEvaluator).AssemblyQualifiedName)));
        }

        /// <summary>
        /// Tests type node
        /// </summary>
        [Test]
        public void TestTypeNodeWithGenericAssemblyQualifiedName()
        {
//            Assert.AreEqual(typeof(int?), ExpressionEvaluator.GetValue(null, "T(System.Nullable`1[System.Int32], mscorlib)"));
//            Assert.AreEqual(typeof(int?), ExpressionEvaluator.GetValue(null, "T(System.Nullable`1[[System.Int32, mscorlib]], mscorlib)"));
            Assert.AreEqual(typeof(int?), ExpressionEvaluator.GetValue(null, "T(System.Nullable`1[[int]], mscorlib)"));
            Assert.AreEqual(typeof(System.Collections.Generic.Dictionary<string, bool>), ExpressionEvaluator.GetValue(null, "T(System.Collections.Generic.Dictionary`2[System.String,System.Boolean],mscorlib)"));
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
            Assert.AreEqual(typeof(System.Collections.Generic.Dictionary<string, bool>), ExpressionEvaluator.GetValue(null, "T(System.Collections.Generic.Dictionary`2[string,bool],mscorlib)"));
        }

        /// <summary>
        /// Tests type node
        /// </summary>
        [Test]
        public void TestTypeNodeWithGenericAssemblyQualifiedArrayName()
        {
            Assert.AreEqual(typeof(int?[,]), ExpressionEvaluator.GetValue(null, "T(System.Nullable`1[[System.Int32, mscorlib]][,], mscorlib)"));
        }

        /// <summary>
        /// Tests constructor node
        /// </summary>
        [Test]
        public void TestConstructor()
        {
            TypeRegistry.RegisterType(typeof(Inventor));

            IExpression exp = Expression.Parse("new System.DateTime(2004, 8, 14)");
            Assert.AreEqual(1000, ExpressionEvaluator.GetValue(null, "new Decimal(1000)"));
            Assert.AreEqual(new DateTime(2004, 8, 14), exp.GetValue(null));
            Assert.AreEqual(new DateTime(2004, 8, 14), exp.GetValue("xyz"));
            Assert.AreEqual(new DateTime(1974, 8, 24),
                            ExpressionEvaluator.GetValue(null, "new DateTime(2004, 8, 14).AddDays(10).AddYears(-30)"));

            // test named arguments
            Inventor ana =
                (Inventor)
                ExpressionEvaluator.GetValue(null,
                                             "new Inventor(Name = 'Ana Maria Seovic', DOB = date('2004-08-14'), Nationality = 'American')");
            Assert.AreEqual("Ana Maria Seovic", ana.Name);
            Assert.AreEqual(new DateTime(2004, 8, 14), ana.DOB);
            Assert.AreEqual("American", ana.Nationality);

            Inventor aleks =
                (Inventor)
                ExpressionEvaluator.GetValue(null,
                                             "new Inventor('Aleksandar Seovic', date('1974-08-24'), 'Serbian', Inventions = {'SPELL'})");
            Assert.AreEqual("Aleksandar Seovic", aleks.Name);
            Assert.AreEqual(new DateTime(1974, 8, 24), aleks.DOB);
            Assert.AreEqual("Serbian", aleks.Nationality);
            Assert.AreEqual(1, aleks.Inventions.Length);
            Assert.AreEqual("SPELL", aleks.Inventions[0]);
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
            Assert.AreEqual(3,
                            ExpressionEvaluator.GetValue(ieee.Members,
                                                         "(Add(new Inventor('Aleksandar Seovic', date('1974-08-24'), 'Serbian')); Count)"));
            Assert.AreEqual(3,
                            ExpressionEvaluator.GetValue(ieee,
                                                         "Members.(Add(new Inventor('Ana Maria Seovic', date('2004-08-14'), 'Serbian')); RemoveAt(1); Count)"));
            Assert.AreEqual("Aleksandar Seovic",
                            ExpressionEvaluator.GetValue(ieee.Members,
                                                         "([1].PlaceOfBirth.City = 'Beograd'; [1].PlaceOfBirth.Country = 'Serbia'; [1].Name)"));
            Assert.AreEqual("Beograd", ((Inventor)ieee.Members[1]).PlaceOfBirth.City);
        }

        /// <summary>
        /// Tests assignment node
        /// </summary>
        [Test]
        public void TestAssignNode()
        {
            Inventor inventor = new Inventor();
            Assert.AreEqual("Aleksandar Seovic", ExpressionEvaluator.GetValue(inventor, "Name = 'Aleksandar Seovic'"));
            Assert.AreEqual(new DateTime(1974, 8, 24),
                            ExpressionEvaluator.GetValue(inventor, "DOB = date('1974-08-24')"));
            Assert.AreEqual("Serbian", ExpressionEvaluator.GetValue(inventor, "Nationality = 'Serbian'"));
            Assert.AreEqual("Ana Maria Seovic",
                            ExpressionEvaluator.GetValue(inventor,
                                                         "(DOB = date('2004-08-14'); Name = 'Ana Maria Seovic')"));
            Assert.AreEqual(new DateTime(2004, 8, 14), inventor.DOB);
            ExpressionEvaluator.GetValue(ieee, "Officers['vp'] = Members[0]");
            Assert.AreEqual("Nikola Tesla", ((Inventor)ieee.Officers["vp"]).Name);
        }

        /// <summary>
        /// Tests default node
        /// </summary>
        [Test]
        public void TestDefaultNode()
        {
            Assert.AreEqual("default", ExpressionEvaluator.GetValue(null, "null ?? 'default'"));
            Assert.AreEqual(1, ExpressionEvaluator.GetValue(null, "null ?? 2 * 2 - 3"));
            Assert.AreEqual("Nikola Tesla", ExpressionEvaluator.GetValue(tesla, "null ?? #root.Name"));

            Assert.AreEqual("default", ExpressionEvaluator.GetValue(null, "'default' ?? 'xyz'"));
            Assert.AreEqual(1, ExpressionEvaluator.GetValue(null, "2 * 2 - 3 ?? 5"));
            Assert.AreEqual("Nikola Tesla", ExpressionEvaluator.GetValue(tesla, "#root.Name ?? 'Pupin'"));
        }

        /// <summary>
        /// Tests variable node
        /// </summary>
        [Test]
        public void TestVariableNode()
        {
            Dictionary<string, object> vars = new Dictionary<string, object>();
            vars["newName"] = "Aleksandar Seovic";
            Assert.AreEqual("Ana Maria Seovic",
                            ExpressionEvaluator.GetValue(null, "#newName = 'Ana Maria Seovic'", vars));
            Assert.AreEqual("Ana Maria Seovic", ExpressionEvaluator.GetValue(tesla, "Name = #newName", vars));
            Assert.AreEqual("Nikola Tesla",
                            ExpressionEvaluator.GetValue(tesla, "(#oldName = Name; Name = 'Nikola Tesla')", vars));
            Assert.AreEqual("Nikola Tesla", ((Inventor)ExpressionEvaluator.GetValue(tesla, "#this", vars)).Name);
            Assert.AreEqual("Nikola Tesla",
                            ExpressionEvaluator.GetValue(tesla, "(Nationality = 'Srbin'; #this).Name", vars));
            Assert.AreEqual("Nikola Tesla", tesla.Name);
            Assert.AreEqual("Srbin", tesla.Nationality);
            Assert.AreEqual("Ana Maria Seovic", vars["oldName"]);
            Assert.AreEqual(tesla, ExpressionEvaluator.GetValue(tesla, "#root", vars));
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

            Assert.AreEqual("trueExp", exp.GetValue());
            Assert.AreEqual("falseExp", ExpressionEvaluator.GetValue(null, "false ? 'trueExp' : 'falseExp'"));
            Assert.AreEqual("trueExp", ExpressionEvaluator.GetValue(null, "(true ? 'trueExp' : 'falseExp')"));
            Assert.AreEqual("falseExp", ExpressionEvaluator.GetValue(null, "(false ? 'trueExp' : 'falseExp')"));

            ExpressionEvaluator.SetValue(ieee, "Name", "IEEE");
            Dictionary<string, object> vars = new Dictionary<string, object>();
            vars["queryName"] = "Nikola Tesla";
            string expression =
                @"IsMember(#queryName)
                    ? #queryName + ' is a member of the ' + Name + ' Society'
                    : #queryName + ' is not a member of ' + Name + ' Society'";
            Assert.AreEqual("Nikola Tesla is a member of the IEEE Society",
                            ExpressionEvaluator.GetValue(ieee, expression, vars));
        }

        /// <summary>
        /// Tests logical OR operator
        /// </summary>
        [Test]
        public void TestLogicalOrOperator()
        {
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true or true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false or false"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true or false"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "false or true"));
            string expression = @"IsMember('Nikola Tesla') or IsMember('Albert Einstien')";
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(ieee, expression));
        }

        /// <summary>
        /// Tests bitwise OR operator
        /// </summary>
        [Test]
        public void TestBitwiseOrOperator()
        {
            Assert.AreEqual( 1 | 2, ExpressionEvaluator.GetValue(null, "1 or 2"));
            Assert.AreEqual( 1 | -2, ExpressionEvaluator.GetValue(null, "1 or -2"));
            Assert.AreEqual(RegexOptions.IgnoreCase | RegexOptions.Compiled, ExpressionEvaluator.GetValue(null, "T(System.Text.RegularExpressions.RegexOptions).IgnoreCase or T(System.Text.RegularExpressions.RegexOptions).Compiled"));
        }

        /// <summary>
        /// Tests logical AND operator
        /// </summary>
        [Test]
        public void TestLogicalAndOperator()
        {
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true and true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false and false"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "true and false"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false and true"));
            string expression = @"IsMember('Nikola Tesla') and IsMember('Mihajlo Pupin')";
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(ieee, expression));
        }

        /// <summary>
        /// Tests bitwise OR operator
        /// </summary>
        [Test]
        public void TestBitwiseAndOperator()
        {
            Assert.AreEqual(1 & 3, ExpressionEvaluator.GetValue(null, "1 and 3"));
            Assert.AreEqual(1 & -1, ExpressionEvaluator.GetValue(null, "1 and -1"));
            Dictionary<string, object> vars = new Dictionary<string, object>();
            vars["ALL"] = (RegexOptions) 0xFFFF;
            Assert.AreEqual(RegexOptions.IgnoreCase, ExpressionEvaluator.GetValue(null, "T(System.Text.RegularExpressions.RegexOptions).IgnoreCase and #ALL", vars));
        }

        /// <summary>
        /// Tests logical NOT operator
        /// </summary>
        [Test]
        public void TestLogicalNotOperator()
        {
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "!true"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "!false"));
            string expression = @"IsMember('Nikola Tesla') and !IsMember('Mihajlo Pupin')";
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(ieee, expression));
            Assert.AreEqual( ~RegexOptions.Compiled, ExpressionEvaluator.GetValue(null, "!T(System.Text.RegularExpressions.RegexOptions).Compiled"));
        }

        /// <summary>
        /// Tests bitwise OR operator
        /// </summary>
        [Test]
        public void TestXorOperator()
        {
            Assert.AreEqual(1 ^ 3, ExpressionEvaluator.GetValue(null, "1 xor 3"));
            Assert.AreEqual(1 ^ -1, ExpressionEvaluator.GetValue(null, "1 xor -1"));
            Assert.AreEqual(true ^ false, ExpressionEvaluator.GetValue(null, "true xor false"));
            Assert.AreEqual(true ^ true, ExpressionEvaluator.GetValue(null, "true xor true"));
            Assert.AreEqual(RegexOptions.IgnoreCase ^ RegexOptions.Compiled, ExpressionEvaluator.GetValue(null, "T(System.Text.RegularExpressions.RegexOptions).IgnoreCase xor T(System.Text.RegularExpressions.RegexOptions).Compiled"));
        }

        /// <summary>
        /// Tests logical operator presedance
        /// </summary>
        [Test]
        public void TestLogicalOperatorPresedance()
        {
            // NOT over AND
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "!false and false"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "!false and true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "!true and false"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "!true and true"));

            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "!(false and false)"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "!(false and true)"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "!(true and false)"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "!(true and true)"));

            // NOT over OR
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "!false or false"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "!false or true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "!true or false"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "!true or true"));

            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "!(false or false)"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "!(false or true)"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "!(true or false)"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "!(true or true)"));

            // AND over OR
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false and false or false"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "false and false or true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false and true or false"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "false and true or true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "true and false or false"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true and false or true"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true and true or false"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true and true or true"));

            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false and (false or false)"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false and (false or true)"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false and (true or false)"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false and (true or true)"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "true and (false or false)"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true and (false or true)"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true and (true or false)"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true and (true or true)"));
        }

        /// <summary>
        /// Tests equality operator.
        /// </summary>
        [Test]
        public void TestEqualityOperator()
        {
            // Null
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "null == null"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "null == 'xyz'"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "123 == null"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "null == 123"));

            // Bool
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "false == false"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true == true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false == true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "true == false"));

            // Int
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "2 == 2"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "-5 == -5"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "2 == -5"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "-5 == 2"));

            // String
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'test' == 'test'"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "'Test' == 'test'"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "'test' == 'Test'"));

            // DateTime
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "date('1974-08-24') == date('1974-08-24')"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "DateTime.Today == DateTime.Today"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "DateTime.Today == date('1974-08-24')"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "date('1974-08-24') == DateTime.Today"));

            // Enums
            Foo foo = new Foo(FooType.One);
            TypeRegistry.RegisterType("FooType", typeof(FooType));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(foo, "Type == FooType.One"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(foo, "Type == 'One'"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(foo, "Type == 'Two'"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(foo, "FooType.One == Type"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(foo, "'One' == Type"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(foo, "'Two' == Type"));
        }

        /// <summary>
        /// Tests inequality operator.
        /// </summary>
        [Test]
        public void TestInqualityOperator()
        {
            // Null
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "null != null"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "123 != null"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "null != 'xyz'"));

            // Bool
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false != false"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "true != true"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "false != true"));

            // Int
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "2 != 2.0"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "-5.0 != -5"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "2.0 != -5"));

            // String
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "'test' != 'test'"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'Test' != 'test'"));

            // DateTime
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "date('1974-08-24') != date('1974-08-24')"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "DateTime.Today != DateTime.Today"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "DateTime.Today != date('1974-08-24')"));
        }

        /// <summary>
        /// Tests less than operator.
        /// </summary>
        [Test]
        public void TestLessThanOperator()
        {
            // Null
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "null < null"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "123 < null"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "null < 'xyz'"));

            // Bool
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "false < true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "true < true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "true < false"));

            // Int
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "2 < 2.0"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "-5.0 < 2"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "2 < -5.0"));

            // String
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "'test' < 'test'"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "'Test' < 'test'"));

            // DateTime
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "date('1974-08-24') < date('1974-08-24')"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "date('1974-08-24') < DateTime.Today"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "DateTime.Today < date('1974-08-24')"));
        }

        /// <summary>
        /// Tests less than or equal operator.
        /// </summary>
        [Test]
        public void TestLessThanOrEqualOperator()
        {
            // Null
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "null <= null"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "123 <= null"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "null <= 'xyz'"));

            // Bool
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "false <= true"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true <= true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "true <= false"));

            // Int
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "2 <= 2.0"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "-5.0 <= 2"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "2.0 <= -5"));

            // String
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'test' <= 'test'"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "'Test' <= 'test'"));

            // DateTime
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "date('1974-08-24') <= date('1974-08-24')"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "date('1974-08-24') <= DateTime.Today"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "DateTime.Today <= date('1974-08-24')"));
        }

        /// <summary>
        /// Tests greater than operator.
        /// </summary>
        [Test]
        public void TestGreaterThanOperator()
        {
            // Null
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "null > null"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "123 > null"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "null > 'xyz'"));

            // Bool
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false > true"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "true > true"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true > false"));

            // Int
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "2 > 2.0"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "-5.0 > 2"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "2 > -5.0"));

            // String
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "'test' > 'test'"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'Test' > 'test'"));

            // DateTime
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "date('1974-08-24') > date('1974-08-24')"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "date('1974-08-24') > DateTime.Today"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "DateTime.Today > date('1974-08-24')"));
        }

        /// <summary>
        /// Tests greater than or equal operator.
        /// </summary>
        [Test]
        public void TestGreaterThanOrEqualOperator()
        {
            // Null
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "null >= null"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "123 >= null"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "null >= 'xyz'"));

            // Bool
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "false >= true"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true >= true"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "true >= false"));

            // Int
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "2.0 >= 2"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "-5 >= 2.0"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "2.0 >= -5"));

            // String
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'test' >= 'test'"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'Test' >= 'test'"));

            // DateTime
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "date('1974-08-24') >= date('1974-08-24')"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "date('1974-08-24') >= DateTime.Today"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "DateTime.Today >= date('1974-08-24')"));
        }

        /// <summary>
        /// Tests IN operator.
        /// </summary>
        [Test]
        public void TestInOperator()
        {
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "null in null"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "3 in {1, 2, 3, 4, 5}"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "!(3 in {1, 2, 3, 4, 5})"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'xyz' in new string[] {'abc', 'xyz'}"));
            Assert.IsTrue(
                (bool)ExpressionEvaluator.GetValue(null, "'xyz' in #{'abc' : 'Value 1', 'xyz' : DateTime.Today}"));
        }

        /// <summary>
        /// Tests IS operator.
        /// </summary>
        [Test]
        public void TestIsOperator()
        {
            TypeRegistry.RegisterType(typeof(IList));
            TypeRegistry.RegisterType(typeof(IDictionary));

            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "null is null"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "5 is null"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "null is int"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "5 is int"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "!(5 is int)"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "{1, 2, 3, 4, 5} is IList"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "new string[] {'abc', 'xyz'} is T(string[])"));
            Assert.IsTrue(
                (bool)ExpressionEvaluator.GetValue(null, "#{'abc' : 'Value 1', 'xyz' : DateTime.Today} is IDictionary"));
        }

        /// <summary>
        /// Tests BETWEEN operator.
        /// </summary>
        [Test]
        public void TestBetweenOperator()
        {
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "null between {1, 5}"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "0 between {1, 5}"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "1 between {1, 5}"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "3 between {1, 5}"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "5 between {1, 5}"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "6 between {1, 5}"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "!(6 between {1, 5})"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'efg' between {'abc', 'xyz'}"));
            Assert.IsTrue(
                (bool)ExpressionEvaluator.GetValue(null, "DateTime.Today between {DateTime.Today, DateTime.Now}"));
            Assert.IsFalse(
                (bool)ExpressionEvaluator.GetValue(null, "DateTime.Today between {DateTime.Now, DateTime.Now}"));
        }
#if !MONO
        /// <summary>
        /// Tests LIKE operator.
        /// </summary>
        [Test]
        public void TestLikeOperator()
        {
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'A' like '?'"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "'Abc' like '?'"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'Abc' like '[A-Z]b?'"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'Abc' like '*'"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'Aleksandar' like 'Aleks*'"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'Ana Maria Seovic' like '*Maria*'"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, "'Marija Seovic' like '*Seovic'"));
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

            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(emailCheck, "'A' matches #this"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(emailCheck, "'aleks@seovic.com' matches #this"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(emailCheck, "'@seovic.com' matches #this"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(emailCheck, "'seovic.com' matches #this"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(emailCheck, "'aleks' matches #this"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(emailCheck, "'aleks@' matches #this"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(emailCheck, "'aleks@seovic' matches #this"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "'5.0067' matches '^-?\\d+(\\.\\d{2})?$'"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(null, @"'5.00' matches '^-?\d+(\.\d{2})?$'"));
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
            Assert.AreEqual(2, ExpressionEvaluator.GetValue(null, "1 + 1"));
            Assert.AreEqual(2, ExpressionEvaluator.GetValue(null, "1e0 + 1"));
            Assert.AreEqual(0, ExpressionEvaluator.GetValue(null, "-1e0 + 1"));
            Assert.AreEqual(-2, ExpressionEvaluator.GetValue(null, "-1e0 + -1"));
            Assert.AreEqual(Decimal.Parse("2.0", NumberFormatInfo.InvariantInfo),
                            ExpressionEvaluator.GetValue(null, "1.0m + 1.0"));
            Assert.AreEqual(9, ExpressionEvaluator.GetValue(null, "2.0 + 3e0 + 4"));

            // strings
            Assert.AreEqual("test string", ExpressionEvaluator.GetValue(null, "'test' + ' ' + 'string'"));
            Assert.AreEqual("test 2", ExpressionEvaluator.GetValue(null, "'test' + ' ' + 2"));
            Assert.AreEqual("test " + DateTime.Today.ToString(),
                            ExpressionEvaluator.GetValue(null, "'test' + ' ' + DateTime.Today"));
            Assert.AreEqual("test", ExpressionEvaluator.GetValue(null, "'test' + #this")); // can concat null
            Assert.AreEqual("test", ExpressionEvaluator.GetValue(null, "#this+'test'")); // can concat null

            // dates
            DateTime anaDOB = new DateTime(2004, 8, 14);
            DateTime aleksDOB = new DateTime(1974, 8, 24);
            TimeSpan diff = anaDOB - aleksDOB;
            IDictionary<string, object> vars = new Dictionary<string, object>();
            vars["ts"] = diff;

            Assert.AreEqual(anaDOB, ExpressionEvaluator.GetValue(null, "date('1974-08-24') + #ts", vars));
            Assert.AreEqual(new DateTime(1974, 8, 29), ExpressionEvaluator.GetValue(null, "date('1974-08-24') + 5"));
            Assert.AreEqual(new DateTime(1974, 8, 29),
                            ExpressionEvaluator.GetValue(null, "date('1974-08-24') + '5.0:0'"));
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
            Assert.AreEqual(2, ExpressionEvaluator.GetValue(null, "3 - 1"));
            Assert.AreEqual(-2, ExpressionEvaluator.GetValue(null, "1 - 3"));
            Assert.AreEqual(4, ExpressionEvaluator.GetValue(null, "1 - -3"));
            Assert.AreEqual(Decimal.Parse("-9000.00", NumberFormatInfo.InvariantInfo),
                            ExpressionEvaluator.GetValue(null, "1000.00m - 1e4"));
            Assert.AreEqual(-5, ExpressionEvaluator.GetValue(null, "2.0 - 3e0 - 4"));

            // dates
            DateTime anaDOB = new DateTime(2004, 8, 14);
            DateTime aleksDOB = new DateTime(1974, 8, 24);
            TimeSpan diff = anaDOB - aleksDOB;
            Dictionary<string, object> vars = new Dictionary<string, object>();
            vars["ts"] = diff;

            Assert.AreEqual(aleksDOB, ExpressionEvaluator.GetValue(null, "date('2004-08-14') - #ts", vars));
            Assert.AreEqual(diff, ExpressionEvaluator.GetValue(null, "date('2004-08-14') - date('1974-08-24')"));
            Assert.AreEqual(new DateTime(1974, 8, 19), ExpressionEvaluator.GetValue(null, "date('1974-08-24') - 5"));
            Assert.AreEqual(new DateTime(1974, 8, 19),
                            ExpressionEvaluator.GetValue(null, "date('1974-08-24') - '5.0:0'"));
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
            Assert.AreEqual(4, ExpressionEvaluator.GetValue(null, "2 * 2"));
            Assert.AreEqual(-6, ExpressionEvaluator.GetValue(null, "2 * -3"));
            Assert.AreEqual(6, ExpressionEvaluator.GetValue(null, "-2 * -3"));
            Assert.AreEqual(Decimal.Parse("1000000.00", NumberFormatInfo.InvariantInfo),
                            ExpressionEvaluator.GetValue(null, "1000.00m * 1e3"));
            Assert.AreEqual(24, ExpressionEvaluator.GetValue(null, "2.0 * 3e0 * 4"));
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
            Assert.AreEqual(1, ExpressionEvaluator.GetValue(null, "2 / 2"));
            Assert.AreEqual(-2, ExpressionEvaluator.GetValue(null, "6 / -3"));
            Assert.AreEqual(2, ExpressionEvaluator.GetValue(null, "-6 / -3"));
            Assert.AreEqual(Decimal.Parse("1.00", NumberFormatInfo.InvariantInfo),
                            ExpressionEvaluator.GetValue(null, "1000.00m / 1e3"));
            Assert.AreEqual(1, ExpressionEvaluator.GetValue(null, "8.0 / 4e0 / 2"));
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
            Assert.AreEqual(0, ExpressionEvaluator.GetValue(null, "2 % 2"));
            Assert.AreEqual(2, ExpressionEvaluator.GetValue(null, "6 % -4"));
            Assert.AreEqual(-1, ExpressionEvaluator.GetValue(null, "-6 % -5"));
            Assert.AreEqual(Decimal.Parse("5.00", NumberFormatInfo.InvariantInfo),
                            ExpressionEvaluator.GetValue(null, "1005.00m % 1e3"));
            Assert.AreEqual(1, ExpressionEvaluator.GetValue(null, "8.0 % 5e0 % 2"));
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
            Assert.AreEqual(8, ExpressionEvaluator.GetValue(null, "2 ^ +3"));
            Assert.AreEqual(16, ExpressionEvaluator.GetValue(null, "-2 ^ 4"));
            Assert.AreEqual(-32, ExpressionEvaluator.GetValue(null, "-2 ^ 5"));
            Assert.AreEqual(.0625, ExpressionEvaluator.GetValue(null, "4 ^ -2"));
            Assert.AreEqual(2, ExpressionEvaluator.GetValue(null, "+4 ^ .5"));
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
            Assert.AreEqual(-3, ExpressionEvaluator.GetValue(null, "1+2-3*8/2/2"));
            Assert.AreEqual(-45, ExpressionEvaluator.GetValue(null, "1+2-3*8^2/2/2"));
            Assert.AreEqual(0, ExpressionEvaluator.GetValue(null, "1+2-3*8/2^2/2"));
            Assert.AreEqual(-4.5, ExpressionEvaluator.GetValue(null, "1+(2-3*8)/2.0/2"));
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
            Assert.AreEqual(typeof(TestObject), ExpressionEvaluator.GetValue(null, "@(goran)").GetType());

            IApplicationContext ctx =
                new XmlApplicationContext(false, "assembly://Spring.Core.Tests/Spring.Context.Support/objects.xml");
            ctx.Name = "myContext";
            ContextRegistry.RegisterContext(ctx);

            Assert.AreEqual(typeof(TestObject),
                            ExpressionEvaluator.GetValue(null, "@(myContext:goran)").GetType());

            // string literals allowed for contextname
            Assert.AreEqual(typeof(TestObject),
                            ExpressionEvaluator.GetValue(null, "@('myContext':goran)").GetType());
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

            Assert.IsNotNull(testObject);
            Assert.AreSame(testObject, testObjectContainer.TestObject);
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

            Assert.AreEqual(typeof(TestObject),
                            ExpressionEvaluator.GetValue(null, @"@(my.Context/bla\:goran)").GetType());
//            Assert.AreEqual(typeof(TestObject),
//                            ExpressionEvaluator.GetValue(null, "@(my\\.Context:goran)").GetType());
        }

        /// <summary>
        /// Tests attribute expression.
        /// </summary>
        [Test]
        public void TestAttribute()
        {
            TypeRegistry.RegisterType("WebMethod", typeof(WebMethodAttribute));
            TypeRegistry.RegisterType("TransactionOption", typeof(TransactionOption));

            Assert.IsInstanceOf(typeof(SerializableAttribute),
                                    ExpressionEvaluator.GetValue(null, "@[System.Serializable]"));
            Assert.IsInstanceOf(typeof(SerializableAttribute),
                                    ExpressionEvaluator.GetValue(null, "@[System.Serializable()]"));
            Assert.IsInstanceOf(typeof(WebMethodAttribute), ExpressionEvaluator.GetValue(null, "@[WebMethod]"));

            WebMethodAttribute webMethod = (WebMethodAttribute)ExpressionEvaluator.GetValue(null, "@[WebMethod(true)]");
            Assert.IsTrue(webMethod.EnableSession);

            webMethod = (WebMethodAttribute)
                        ExpressionEvaluator.GetValue(
                            null,
                            "@[WebMethod(false, CacheDuration = 60, Description = 'my web method', TransactionOption = TransactionOption.Required)]");
            Assert.AreEqual(60, webMethod.CacheDuration);
            Assert.AreEqual("my web method", webMethod.Description);
            Assert.AreEqual(TransactionOption.Required, webMethod.TransactionOption);
        }

        [Test]
        public void TestDelegateFunctionExpressions()
        {
            //for purposes of an example in documentation
            Dictionary<string, object> vars = new Dictionary<string, object>();
            vars["sqrt"] = new DoubleFunction(Sqrt);
            double result = (double)ExpressionEvaluator.GetValue(null, "#sqrt(64)", vars);
            Assert.AreEqual(8, result);

            vars = new Dictionary<string, object>();
            vars["max"] = new DoubleFunctionTwoArgs(Max);
            result = (double) ExpressionEvaluator.GetValue(null, "#max(5,25)", vars);
            Assert.AreEqual(25, result);


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
            Assert.AreEqual(4,
                            ExpressionEvaluator.GetValue(null, "(#add = {|x, y| $x + $y}; #add(2, 2))", new Dictionary<string, object>()));
            Assert.AreEqual(25,
                            ExpressionEvaluator.GetValue(null, "(#max = {|x, y| $x > $y ? $x : $y }; #max(5,25))",
                                                         new Dictionary<string, object>()));

            // recursive function
            Assert.AreEqual(120,
                            ExpressionEvaluator.GetValue(null,
                                                         "(#fact = {|n| $n <= 1 ? 1 : $n * #fact($n-1) }; #fact(5))",
                                                         new Dictionary<string, object>()));

            // function invoked within projection expression
            string expr = "(#upper = {|txt| $txt.ToUpper() }; !{ #upper(Name) })";
            IList upperNames = (IList)ExpressionEvaluator.GetValue(ieee.Members, expr, new Dictionary<string, object>());
            Assert.AreEqual("NIKOLA TESLA", upperNames[0]);
            Assert.AreEqual("MIHAJLO PUPIN", upperNames[1]);

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
            IList results = (IList)ExpressionEvaluator.GetValue(null, expr2, vars);
            Assert.AreEqual(2, results[0]);
            Assert.AreEqual(120, results[1]);
            Assert.AreEqual(25, results[2]);

            // function assignment
            Assert.AreEqual(120,
                            ExpressionEvaluator.GetValue(null,
                                                         "(#fact = {|n| $n <= 1 ? 1 : $n * #fact($n-1) }; #f = #fact; #f(5))",
                                                         new Dictionary<string, object>()));
        }

        #region Collection Processor and Aggregator tests

        [Test]
        public void TestCountAggregator()
        {
            int[] arr = new int[] { 24, 8, 14, 8 };
            Assert.AreEqual(4, ExpressionEvaluator.GetValue(arr, "count()"));
            Assert.AreEqual(3, ExpressionEvaluator.GetValue(null, "{1, 5, -3}.count()"));
            Assert.AreEqual(0, ExpressionEvaluator.GetValue(null, "count()"));
        }

        [Test]
        public void TestCustomCollectionProcessor()
        {
            // Test for the purposes of creating documentation example.
            Dictionary<string, object> vars = new Dictionary<string, object>();
            vars["EvenSum"] = new IntEvenSumCollectionProcessor();
            Assert.AreEqual(6, ExpressionEvaluator.GetValue(null, "{1, 2, 3, 4}.EvenSum()", vars));

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
                            if ((int)item % 2 == 0)
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
            Assert.AreEqual(54, ExpressionEvaluator.GetValue(arr, "sum()"));
            Assert.AreEqual(13, ExpressionEvaluator.GetValue(null, "{1, 5, -3, 10}.sum()"));

            object[] arr2 = new object[] { 5, 5.8, 12.2, 1 };
            object result = ExpressionEvaluator.GetValue(arr2, "sum()");
            Assert.IsInstanceOf(typeof(double), result);
            Assert.AreEqual(24, result);
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
            Assert.AreEqual(14, ExpressionEvaluator.GetValue(arr, "average()"));
            Assert.AreEqual(3, ExpressionEvaluator.GetValue(null, "{1, 5, -4, 10}.average()"));
            Assert.AreEqual(3.5, ExpressionEvaluator.GetValue(null, "{1, 5, -2, 10}.average()"));

            object[] arr2 = new object[] { 5, 5.8, 12.2, 1 };
            object result = ExpressionEvaluator.GetValue(arr2, "average()");
            Assert.IsInstanceOf(typeof(double), result);
            Assert.AreEqual(6, result);
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
            Assert.AreEqual(8, ExpressionEvaluator.GetValue(arr, "min()"));
            Assert.AreEqual(-3, ExpressionEvaluator.GetValue(null, "{1, 5, -3, 10}.min()"));

            object[] arr2 = new object[] { 5, 5.8, 12.2, 1 };
            object result = ExpressionEvaluator.GetValue(arr2, "min()");
            Assert.IsInstanceOf(typeof(int), result);
            Assert.AreEqual(1, result);

            Assert.IsNull(ExpressionEvaluator.GetValue(ObjectUtils.EmptyObjects, "min()"));
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
            Assert.AreEqual(24, ExpressionEvaluator.GetValue(arr, "max()"));
            Assert.AreEqual(10, ExpressionEvaluator.GetValue(null, "{1, 5, -3, 10}.max()"));

            object[] arr2 = new object[] { 5, 5.8, 12.2, 1 };
            object result = ExpressionEvaluator.GetValue(arr2, "max()");
            Assert.IsInstanceOf(typeof(double), result);
            Assert.AreEqual(12.2, result);

            Assert.IsNull(ExpressionEvaluator.GetValue(ObjectUtils.EmptyObjects, "max()"));
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
            Assert.AreEqual(new int[] { 6, 8, 14, 24 }, ExpressionEvaluator.GetValue(arr, "sort()"));
            Assert.AreEqual(new int[] { 6, 8, 14, 24 }, ExpressionEvaluator.GetValue(arr, "sort(true)"));
            Assert.AreEqual(new int[] { 24, 14, 8, 6 }, ExpressionEvaluator.GetValue(arr, "sort(false)"));

            string[] arr2 = new string[] { "abc", "xyz", "stuv", "efg", "dcb" };
            Assert.AreEqual(new string[] { "abc", "dcb", "efg", "stuv", "xyz" },
                            ExpressionEvaluator.GetValue(arr2, "sort()"));

            DateTime[] arr3 = new DateTime[] { DateTime.Today, DateTime.MaxValue, DateTime.MinValue };
            Assert.AreEqual(new DateTime[] { DateTime.MinValue, DateTime.Today, DateTime.MaxValue },
                            ExpressionEvaluator.GetValue(arr3, "sort()"));

            Assert.AreEqual(new object[] { -3.3, 1.2, 5.5 }, ExpressionEvaluator.GetValue(null, "{1.2, 5.5, -3.3}.sort()"));
            Assert.IsNull(ExpressionEvaluator.GetValue(null, "sort()"));

            ISet set = new ListSet(arr);
            Assert.AreEqual(new int[] { 6, 8, 14, 24 }, ExpressionEvaluator.GetValue(set, "sort()"));
        }

        [Test(Description="sort supports any ICollection containing elements of uniform type")]
        public void TestSortProcessorWithSimpleICollectionType()
        {
            Stack stack = new Stack(new int[] { 24, 8, 14, 6 });
            ExpressionEvaluator.GetValue(stack, "sort()");
        }

        [Test]
        public void TestNonNullProcessor()
        {
            string[] arr2 = new string[] { "abc", "xyz", null, "abc", "def", null };
            Assert.AreEqual(new string[] { "abc", "xyz", "abc", "def" },
                            ExpressionEvaluator.GetValue(arr2, "nonNull()"));
            Assert.AreEqual(new string[] { "abc", "abc", "def", "xyz" },
                            ExpressionEvaluator.GetValue(arr2, "nonNull().sort()"));
        }

        [Test]
        public void TestDistinctProcessor()
        {
            int[] arr = new int[] { 24, 8, 8, 6, 24, 6, 8, 6 };
            Assert.AreEqual(new int[] { 6, 8, 24 }, ExpressionEvaluator.GetValue(arr, "distinct().sort()"));

            string[] arr2 = new string[] { "abc", "xyz", "abc", "def", null, "def" };
            Assert.AreEqual(new string[] { null, "abc", "def", "xyz" },
                            ExpressionEvaluator.GetValue(arr2, "distinct(true).sort()"));
            Assert.AreEqual(new string[] { "abc", "def", "xyz" },
                            ExpressionEvaluator.GetValue(arr2, "distinct(false).sort()"));
            Assert.AreEqual(new string[] { "abc", "def", "xyz" },
                            ExpressionEvaluator.GetValue(arr2, "distinct().sort()"));
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
            Assert.AreEqual( 0.0m, result[0] );
            Assert.AreEqual(1.0m, result[1]);
            Assert.AreEqual(1.1m, result[2]);
            Assert.AreEqual(1.1m, result[3]);
            Assert.AreEqual(1.1m, result[4]);
        }

        [Test]
        public void TestReverseProcessor()
        {
            object[] arr = new object[] { "0", 1, 2.1m, "3", 4.1f };
            object[] result = new ArrayList( (ICollection) ExpressionEvaluator.GetValue(arr, "reverse()") ).ToArray();
            Assert.AreEqual(new object[] { 4.1f, "3", 2.1m, 1, "0" }, result);
        }

        #endregion

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
            Assert.AreEqual("Tesla, Nikola", tesla.Name);
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
            Assert.IsNull(ExpressionEvaluator.GetValue(foo, "NullableDate"));
            Assert.IsNull(ExpressionEvaluator.GetValue(foo, "NullableInt"));

            ExpressionEvaluator.SetValue(foo, "NullableDate", DateTime.Today);
            ExpressionEvaluator.SetValue(foo, "NullableDate", null);
            ExpressionEvaluator.SetValue(foo, "NullableDate", "2004-08-14");
            ExpressionEvaluator.SetValue(foo, "NullableDate", DateTime.Today);
            ExpressionEvaluator.SetValue(foo, "NullableInt", 1);
            ExpressionEvaluator.SetValue(foo, "NullableInt", null);
            ExpressionEvaluator.SetValue(foo, "NullableInt", "5");
            ExpressionEvaluator.SetValue(foo, "NullableInt", 1);

            Assert.IsInstanceOf(typeof(DateTime?), ExpressionEvaluator.GetValue(foo, "NullableDate"));
            Assert.IsInstanceOf(typeof(Int32?), ExpressionEvaluator.GetValue(foo, "NullableInt"));

            Assert.AreEqual(DateTime.Today, ExpressionEvaluator.GetValue(foo, "NullableDate"));
            Assert.AreEqual(1, ExpressionEvaluator.GetValue(foo, "NullableInt"));

            int? test = 1;
            Assert.IsInstanceOf(typeof(Int32?), ExpressionEvaluator.GetValue(test, "#root"));
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(test, "#root != null"));
            Assert.AreEqual(1, ExpressionEvaluator.GetValue(test, "#root"));

            test = null;
            Assert.IsTrue((bool)ExpressionEvaluator.GetValue(test, "#root == null"));
            Assert.IsNull(ExpressionEvaluator.GetValue(test, "#root"));
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
            Assert.AreEqual(today.ToString("d"), ExpressionEvaluator.GetValue(today, "ToString('d')"));

            TypeRegistry.RegisterType(typeof(TypeRegistry));
            try
            {
                ExpressionEvaluator.GetValue(null, "TypeRegistry.RegisterType(null)");
                Assert.Fail("Should throw ArgumentNullException");
            }
            catch (ArgumentNullException)
            { }

            try
            {
                ExpressionEvaluator.GetValue(null, "TypeRegistry.RegisterType(null, 'System.Object')");
                Assert.Fail("Should throw ArgumentNullException");
            }
            catch (ArgumentNullException)
            { }

            try
            {
                ExpressionEvaluator.GetValue(null, "TypeRegistry.RegisterType(null, null)");
                Assert.Fail("Should throw AmbiguousMatchException");
            }
            catch (AmbiguousMatchException)
            { }

            try
            {
                ExpressionEvaluator.GetValue(null, "TypeRegistry.RegisterType(null, null, null)");
                Assert.Fail("Should throw ArgumentException");
            }
            catch (ArgumentException)
            { }

            try
            {
                ExpressionEvaluator.GetValue(null, "TypeRegistry.RegisterType(int, string)");
                Assert.Fail("Should throw ArgumentException");
            }
            catch (ArgumentException)
            { }
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
            Assert.AreEqual("a|b|c", foo.MethodWithArrayArgument(new string[] { "a", "b", "c" }));
            Assert.AreEqual("a||c", foo.MethodWithArrayArgument(new string[] { "a", null, "c" }));
            Assert.AreEqual("a|b|c", foo.MethodWithParamArray("a", "b", "c"));
            Assert.AreEqual("a||c", foo.MethodWithParamArray("a", null, "c"));

            Assert.AreEqual("a|b|c", ExpressionEvaluator.GetValue(foo, "MethodWithArrayArgument(new string[] { 'a', 'b', 'c' })"));
            Assert.AreEqual("a||c", ExpressionEvaluator.GetValue(foo, "MethodWithArrayArgument(new string[] { 'a', null, 'c' })"));
            Assert.AreEqual("a|b|c", ExpressionEvaluator.GetValue(foo, "MethodWithParamArray('a', 'b', 'c')"));
            Assert.AreEqual("a||c", ExpressionEvaluator.GetValue(foo, "MethodWithParamArray('a', null, 'c')"));

            Assert.AreEqual("a|b|c", ExpressionEvaluator.GetValue(foo, "MethodWithParamArray(false, 'a', 'b', 'c')"));
            Assert.AreEqual("a||c", ExpressionEvaluator.GetValue(foo, "MethodWithParamArray(false, 'a', null, 'c')"));
            Assert.AreEqual("A|B|C", ExpressionEvaluator.GetValue(foo, "MethodWithParamArray(true, 'a', 'b', 'c')"));
            Assert.AreEqual("A||C", ExpressionEvaluator.GetValue(foo, "MethodWithParamArray(true, 'a', null, 'c')"));
        }

        [Test]
        public void TestMethodResolutionResolvesToExactMatchOfArgumentTypes()
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args["bars"] = new Bar[] { new Bar() };
            Foo foo = new Foo();

            // ensure no one changed our test class
            Assert.AreEqual("ExactMatch", foo.MethodWithSimilarArguments(1, (Bar[])args["bars"]));
            Assert.AreEqual("AssignableMatch", foo.MethodWithSimilarArguments(1, (ICollection)args["bars"]));

            Assert.AreEqual("ExactMatch", ExpressionEvaluator.GetValue(foo, "MethodWithSimilarArguments(1, #bars)", args));
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
                result = (int)ExpressionEvaluator.GetValue(foo, expression);
            });

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void TestIndexerResolutionResolvesToExactMatchOfArgumentTypes()
        {
            Dictionary<string, object> args = new Dictionary<string, object>();
            args["bars"] = new Bar[] { new Bar() };
            Foo foo = new Foo();

            // ensure no one changed our test class
            Assert.AreEqual("ExactMatch", foo[(Bar[])args["bars"]]);
            Assert.AreEqual("AssignableMatch", foo[(ICollection)args["bars"]]);

            Assert.AreEqual("ExactMatch", ExpressionEvaluator.GetValue(foo, "#root[#bars]", args));
        }


        [Test]
        public void TestCtorResolutionResolvesToExactMatchOfArgumentTypes()
        {
            TypeRegistry.RegisterType(typeof(Foo));
            Dictionary<string, object> args = new Dictionary<string, object>();
            args["bars"] = new Bar[] { new Bar() };

            // ensure no one changed our test class
            Foo foo1 = new Foo(1, (Bar[])args["bars"]);
            try
            {
                Foo foo2 = new Foo(1, (ICollection)args["bars"]);
            }
            catch (InvalidOperationException) { }

            Assert.IsNotNull(ExpressionEvaluator.GetValue(null, "new Foo(1, #bars)", args));
        }

        [Test]
        public void TestCtorResolutionWithParamArray()
        {
            TypeRegistry.RegisterType(typeof(Foo));
            Assert.IsNotNull(ExpressionEvaluator.GetValue(null, "new Foo('a', 'b', 'c')"));
            Assert.IsNotNull(ExpressionEvaluator.GetValue(null, "new Foo('a', null, 'c')"));
            Assert.IsNotNull(ExpressionEvaluator.GetValue(null, "new Foo(false, 'a', 'b', 'c')"));
            Assert.IsNotNull(ExpressionEvaluator.GetValue(null, "new Foo(false, 'a', null, 'c')"));
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
            Assert.AreEqual(Int32.MaxValue, ExpressionEvaluator.GetValue(null, "Int32.MaxValue"));
            Assert.AreEqual(Int32.MaxValue, ExpressionEvaluator.GetValue(null, "T(System.Int32).MaxValue"));
            Assert.AreEqual(typeof(Int32).FullName, ExpressionEvaluator.GetValue(null, "Int32.FullName"));
            Assert.AreEqual(typeof(Int32).FullName, ExpressionEvaluator.GetValue(null, "T(System.Int32).FullName"));
            Assert.IsFalse((bool)ExpressionEvaluator.GetValue(null, "Int32.IsSubclassOf(Int64)"));

            TypeRegistry.RegisterType(typeof(FooType));
            Assert.AreEqual(FooType.One, ExpressionEvaluator.GetValue(null, "FooType.One"));
            Assert.AreEqual(typeof(FooType).FullName, ExpressionEvaluator.GetValue(null, "FooType.FullName"));
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
            //Assert.AreEqual( typeof(Int32), val.GetType() );

            IExpression expField = Expression.Parse("SampleEnumField");
            IExpression expProperty = Expression.Parse("SampleEnumProperty");
            IExpression expFlagsField = Expression.Parse("SampleFlagsEnumField");
            IExpression expFlagsProperty = Expression.Parse("SampleFlagsEnumProperty");

            TestEnumTypePropertyClass o = new TestEnumTypePropertyClass();

            // test field set operations
            expField.SetValue(o, TestEnumTypePropertyClass.ESampleEnumType.Trunk);
            Assert.AreEqual(TestEnumTypePropertyClass.ESampleEnumType.Trunk, o.SampleEnumField);

            expField.SetValue(o, (short) 1);
            Assert.AreEqual(TestEnumTypePropertyClass.ESampleEnumType.Trunk, o.SampleEnumField);

            expField.SetValue(o, 1);
            Assert.AreEqual(TestEnumTypePropertyClass.ESampleEnumType.Trunk, o.SampleEnumField);

            expField.SetValue(o, (long) 1);
            Assert.AreEqual(TestEnumTypePropertyClass.ESampleEnumType.Trunk, o.SampleEnumField);

            // test property set operations
            expProperty.SetValue(o, TestEnumTypePropertyClass.ESampleEnumType.Trunk);
            Assert.AreEqual(TestEnumTypePropertyClass.ESampleEnumType.Trunk, o.SampleEnumField);

            expProperty.SetValue(o, (short) 1);
            Assert.AreEqual(TestEnumTypePropertyClass.ESampleEnumType.Trunk, o.SampleEnumField);

            expProperty.SetValue(o, 1);
            Assert.AreEqual(TestEnumTypePropertyClass.ESampleEnumType.Trunk, o.SampleEnumField);

            expProperty.SetValue(o, (long) 1);
            Assert.AreEqual(TestEnumTypePropertyClass.ESampleEnumType.Trunk, o.SampleEnumField);

            expProperty.SetValue(o, "Trunk");
            Assert.AreEqual(TestEnumTypePropertyClass.ESampleEnumType.Trunk, o.SampleEnumField);

            try
            {
                expProperty.SetValue(o, 1.0);
                Assert.Fail("should throw");
            }
            catch (TypeMismatchException e)
            {
                Assert.IsTrue(e.Message.StartsWith("Cannot convert property value of type [System.Double]"));
            }

            try
            {
                expProperty.SetValue(o, ((float)1.0));
                Assert.Fail("should throw");
            }
            catch (TypeMismatchException e)
            {
                Assert.IsTrue(e.Message.StartsWith("Cannot convert property value of type [System.Single]"));
            }

            // test get operations
            object val = expField.GetValue(o);
            Assert.AreEqual(typeof(TestEnumTypePropertyClass.ESampleEnumType), val.GetType());
            Assert.AreEqual(TestEnumTypePropertyClass.ESampleEnumType.Trunk, val);

            val = expProperty.GetValue(o);
            Assert.AreEqual(typeof(TestEnumTypePropertyClass.ESampleEnumType), val.GetType());
            Assert.AreEqual(TestEnumTypePropertyClass.ESampleEnumType.Trunk, val);

            // test bitwise combined enum set
            try
            {
                // not allowed since -1 is not defined in enum
                expField.SetValue(o, -1);
                Assert.Fail("should throw");
            }
            catch (TypeMismatchException ex)
            {
                Assert.IsTrue(
                    ex.Message.StartsWith("Cannot convert property value of type [System.Int32] to required type"));
            }

            try
            {
                // not allowed since -1 is not a representation of any bitwise combination of the enum's values.
                expFlagsField.SetValue(o, -1);
                Assert.Fail("should throw");
            }
            catch (TypeMismatchException ex)
            {
                Assert.IsTrue(
                    ex.Message.StartsWith("Cannot convert property value of type [System.Int32] to required type"));
            }

            expFlagsField.SetValue(o,
                                   TestEnumTypePropertyClass.ESampleFlagsEnumType.SOME |
                                   TestEnumTypePropertyClass.ESampleFlagsEnumType.SOMEOTHER);
            Assert.AreEqual(
                TestEnumTypePropertyClass.ESampleFlagsEnumType.SOME |
                TestEnumTypePropertyClass.ESampleFlagsEnumType.SOMEOTHER, o.SampleFlagsEnumField);
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

            Sample tmp1 = (Sample)e.GetValue(d1);
            Assert.AreEqual("A", tmp1.O);
            Assert.AreEqual("B", tmp1.T);
            Assert.AreEqual("C", tmp1.H);

            Sample tmp2 = (Sample)e.GetValue(d2);
            Assert.AreEqual("A", tmp2.O);
            Assert.AreEqual("B", tmp2.T);
            Assert.AreEqual("Z", tmp2.H);
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

            Assert.AreEqual("RootA", e.GetValue("RootA"));
            Assert.AreEqual("RootB", e.GetValue("RootB"));
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
            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(int[]), obj);
            int[] intarray = (int[])obj;
            Assert.AreEqual(4, intarray.Length);
            for (int i = 0; i < intarray.Length; i++)
            {
                Assert.AreEqual(i + 3, intarray[i]);
            }

            obj = ExpressionEvaluator.GetValue(null, "new long[5]");
            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(long[]), obj);
            long[] longarray = obj as long[];
            Assert.AreEqual(5, longarray.Length);
            for (int i = 0; i < longarray.Length; i++)
            {
                Assert.AreEqual(0, longarray[i]);
            }

            obj = ExpressionEvaluator.GetValue(null, "new double[4, 5]");
            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(double[,]), obj);
            double[,] twodimarray = obj as double[,];
            Assert.AreEqual(4 * 5, twodimarray.Length);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Assert.AreEqual(0.0, twodimarray[i, j]);
                }
            }

            obj = ExpressionEvaluator.GetValue(null, "new int[Int32.Parse('11')]");
            Assert.IsNotNull(obj);
            Assert.IsInstanceOf(typeof(int[]), obj);
            intarray = obj as int[];
            Assert.AreEqual(11, intarray.Length);
        }

        [Test]
        public void TestMethodArgumentNodesResolveAgainstThisContext()
        {
            IExpression exp;

            // case #root == #this - ToString() will be applied to #this
            exp = Expression.Parse("long.Parse(ToString())");
            object result = exp.GetValue(100);
            Assert.AreEqual((long)100, result);

            // case #root != #this in Projection - ToString() will be applied to #this
            exp = Expression.Parse("(ToString(); #noop ={|val| $val}; !{#noop(ToString()) } )");
            result = exp.GetValue(new int[] { 100, 200 }, new Dictionary<string, object>());
            Assert.AreEqual(new string[] { "100", "200" }, result);

            // case #root != #this in Selection - ToString() will be applied to #this
            exp = Expression.Parse("(#noop ={|val| $val}; ?{#noop(ToString()=='100')} )");
            result = exp.GetValue(new int[] { 100, 200 }, new Dictionary<string, object>());
            IList list = new ArrayList();
            list.Add(100);
            Assert.AreEqual(list, result);
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

            Assert.AreEqual("_privateField", ExpressionEvaluator.GetValue(cases, "_privateField"));
            Assert.AreEqual("_protectedField", ExpressionEvaluator.GetValue(cases, "_protectedField"));
            Assert.AreEqual("_publicField", ExpressionEvaluator.GetValue(cases, "_publicField"));

            Assert.AreEqual("PrivateProperty", ExpressionEvaluator.GetValue(cases, "PrivateProperty"));
            Assert.AreEqual("ProtectedProperty", ExpressionEvaluator.GetValue(cases, "ProtectedProperty"));
            Assert.AreEqual("PublicProperty", ExpressionEvaluator.GetValue(cases, "PublicProperty"));

            Assert.AreEqual("PrivateIndexer", ExpressionEvaluator.GetValue(cases, "#root[1]"));
            Assert.AreEqual("ProtectedIndexer", ExpressionEvaluator.GetValue(cases, "#root[1.0]"));
            Assert.AreEqual("PublicIndexer", ExpressionEvaluator.GetValue(cases, "#root['']"));

            Assert.AreEqual("PrivateMethod", ExpressionEvaluator.GetValue(cases, "GetPrivateMethod()"));
            Assert.AreEqual("ProtectedMethod", ExpressionEvaluator.GetValue(cases, "GetProtectedMethod()"));
            Assert.AreEqual("PublicMethod", ExpressionEvaluator.GetValue(cases, "GetPublicMethod()"));
        }

        #region TestAccessVisibility Classes

        internal class AccessVisibilityCases
        {
            private readonly string _privateReadonlyField = "_privateReadonlyField";
            private string _privateField = "_privateField";
            protected string _protectedField = "_protectedField";
            public string _publicField = "_publicField";


            public AccessVisibilityCases()
            {
                Assert.IsTrue(_privateReadonlyField != string.Empty);
                Assert.IsTrue(_privateField != string.Empty);
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

        #endregion

        #region TestMethodInvocation Classes

        class MethodInvokationCases
        {
            public string Foo(string stringArg) { return stringArg; }
            public int Foo(int intArg) { return intArg; }
        }

        #endregion

        #region Set operations tests

        [Test]
        public void TestUnionOperator()
        {
            object o = ExpressionEvaluator.GetValue(null, "{1,2,3} + {3,4,5}");
            Assert.IsInstanceOf(typeof(ISet), o);
            ISet union = (ISet)o;
            Assert.AreEqual(5, union.Count);
            Assert.IsTrue(union.Contains(1));
            Assert.IsTrue(union.Contains(3));
            Assert.IsTrue(union.Contains(5));

            o = ExpressionEvaluator.GetValue(null, "{1,2,3} + {3,4,5} + {'ivan', 'gox', 'damjao', 5}");
            Assert.IsInstanceOf(typeof(ISet), o);
            union = (ISet)o;
            Assert.AreEqual(8, union.Count);
            Assert.IsTrue(union.Contains(1));
            Assert.IsTrue(union.Contains("ivan"));

            ISet testset = new ListSet();
            testset.AddAll(new int[] { 1, 2, 3, 5, 8 });
            o = ExpressionEvaluator.GetValue(testset, "#this + {1, 2, 13, 15}");
            Assert.IsInstanceOf(typeof(ISet), o);
            union = (ISet)o;
            Assert.AreEqual(7, union.Count);
            Assert.IsTrue(union.Contains(1));
            Assert.IsTrue(union.Contains(15));

            o = ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} + #{1:'ivan', 5:'five'}");
            Assert.IsInstanceOf(typeof(IDictionary), o);
            IDictionary result = (IDictionary)o;
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual("one", result[1]);
            Assert.AreEqual("five", result[5]);
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
            Assert.IsInstanceOf(typeof(ISet), o);
            ISet intersection = (ISet)o;
            Assert.AreEqual(2, intersection.Count);
            Assert.IsTrue(intersection.Contains(111));
            Assert.IsTrue(intersection.Contains("ivan"));

            o = ExpressionEvaluator.GetValue(null, "{24, 25, 'aaa' + 'bb'} * {date('2007/2/5').day * 5, 24 - 1}");
            Assert.IsInstanceOf(typeof(ISet), o);
            intersection = (ISet)o;
            Assert.AreEqual(1, intersection.Count);
            Assert.IsTrue(intersection.Contains(25));

            ISet testset = new ListSet();
            testset.AddAll(new int[] { 1, 2, 3, 5, 8 });
            o = ExpressionEvaluator.GetValue(testset, "#this * #{1:'one', 10:'ten'}");
            Assert.IsInstanceOf(typeof(ISet), o);
            intersection = (ISet)o;
            Assert.AreEqual(1, intersection.Count);
            Assert.IsTrue(intersection.Contains(1));

            o = ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} * #{1:'ivan', 5:'five'}");
            Assert.IsInstanceOf(typeof(IDictionary), o);
            IDictionary result = (IDictionary)o;
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("one", result[1]);

            o = ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} * {1, 2, 5, 7}");
            Assert.IsInstanceOf(typeof(IDictionary), o);
            result = (IDictionary)o;
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("one", result[1]);
            Assert.AreEqual("two", result[2]);
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
            Assert.IsInstanceOf(typeof(ISet), o);
            ISet diff = (ISet)o;
            Assert.AreEqual(1, diff.Count);
            Assert.IsTrue(diff.Contains(111));

            o = ExpressionEvaluator.GetValue(null, "{111, 11} - {14, 12, 11} - {111}");
            Assert.IsInstanceOf(typeof(ISet), o);
            diff = (ISet)o;
            Assert.AreEqual(0, diff.Count);

            ISet testset = new ListSet();
            testset.AddAll(new int[] { 1, 2, 3, 5, 8 });
            o = ExpressionEvaluator.GetValue(testset, "#this - #{1:'one', 10:'ten'}");
            Assert.IsInstanceOf(typeof(ISet), o);
            diff = (ISet)o;
            Assert.AreEqual(4, diff.Count);
            Assert.IsFalse(diff.Contains(1));

            o = ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} - #{1:'ivan', 5:'five'}");
            Assert.IsInstanceOf(typeof(IDictionary), o);
            IDictionary result = (IDictionary)o;
            Assert.AreEqual(2, result.Count);
            Assert.IsNull(result[1]);
            Assert.AreEqual("three", result[3]);

            o = ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} - {1, 2, 3, 5, 7}");
            Assert.IsInstanceOf(typeof(IDictionary), o);
            result = (IDictionary)o;
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void TestDifferenceOperatorBad()
        {
            Assert.Throws<ArgumentException>(() => ExpressionEvaluator.GetValue(null, "#{1:'one', 2:'two', 3:'three'} - 'something'"));
        }

        #endregion

        #region Performance tests

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
                x = ((Inventor)((IList)ieee.Officers["advisors"])[0]).Inventions[2];
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

        #endregion

        #region Method Inheritance tests

        [Test]
        public void TestInheritedMethodInvocation()
        {
            DerivedSingleMethodTestClass testClass = new DerivedSingleMethodTestClass();
            Assert.AreEqual("Hello World", ExpressionEvaluator.GetValue(testClass, "#root.GetString()"));
        }

        [Test]
        public void TestStaticInheritedMethodInvocation()
        {
            Assert.AreEqual("Hello Static World from SingleMethodTestClass", DerivedSingleMethodTestClass.StaticMethod());
            Assert.AreEqual("Hello Static World from SingleMethodTestClass", ExpressionEvaluator.GetValue(null, string.Format("T({0}).StaticMethod()", typeof(DerivedSingleMethodTestClass).FullName)));

            Assert.AreEqual("SingleMethodTestClass.ShadowedStaticMethod", SingleMethodTestClass.ShadowedStaticMethod());
            Assert.AreEqual("DerivedSingleMethodTestClass.ShadowedStaticMethod", DerivedSingleMethodTestClass.ShadowedStaticMethod());
            Assert.AreEqual("SingleMethodTestClass.ShadowedStaticMethod", ExpressionEvaluator.GetValue(null, string.Format("T({0}).ShadowedStaticMethod()", typeof(SingleMethodTestClass).FullName)));
            Assert.AreEqual("DerivedSingleMethodTestClass.ShadowedStaticMethod", ExpressionEvaluator.GetValue(null, string.Format("T({0}).ShadowedStaticMethod()", typeof(DerivedSingleMethodTestClass).FullName)));
        }

        #endregion

        private static void DumpNode(AST rootNode, int level)
        {
            Trace.WriteLine(new string(' ', level) + rootNode.ToString());

            int numberOfChildren = rootNode.getNumberOfChildren();
            if (numberOfChildren > 0)
            {
                AST node = rootNode.getFirstChild();
                while (node != null)
                {
                    DumpNode(node, level + 2);
                    node = node.getNextSibling();
                }
            }
        }
    }

    #region Helper classes

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

    #endregion

    #region Shadowing Test Helper Classes

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
            get { return (string)base.SomeValue; }
            set { base.SomeValue = value; }
        }

        public new string ReadonlyShadowedValue
        {
            get { return (string)base.ReadonlyShadowedValue; }
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

    #endregion // Shadowing Test Helper Classes


}
