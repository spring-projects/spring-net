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
using System.EnterpriseServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Web.Services;
using NUnit.Framework;
using Spring.Core.TypeResolution;

namespace Spring.Expressions;

/// <summary>
/// Tests binary and SOAP serialization of expression nodes. These formatters exist on .NET Framework only,
/// so this fixture is compiled for net472 only.
/// </summary>
[TestFixture]
public sealed class ExpressionEvaluatorSerializationTests
{
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

    /// <summary>
    /// Tests attribute expressions against System.Web.Services and System.EnterpriseServices attributes,
    /// which exist on .NET Framework only. The framework-neutral part is in
    /// <see cref="ExpressionEvaluatorTests.TestAttribute"/>.
    /// </summary>
    [Test]
    public void TestWebServicesAttribute()
    {
        TypeRegistry.RegisterType("WebMethod", typeof(WebMethodAttribute));
        TypeRegistry.RegisterType("TransactionOption", typeof(TransactionOption));

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
}
