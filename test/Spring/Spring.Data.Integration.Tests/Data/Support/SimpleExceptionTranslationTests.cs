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

using System.Data;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Common;
using Spring.Data.Core;

namespace Spring.Data.Support;

[TestFixture]
public class SimpleExceptionTranslationTests
{
    private IDbProvider dbProvider;
    private IAdoOperations adoOperations;

    /// <summary>
    /// The shared ILog instance for this class (and derived classes).
    /// </summary>
    protected static readonly ILog log = LogManager.GetLogger<SimpleExceptionTranslationTests>();

    [SetUp]
    public void CreateAdoTemplate()
    {
        IApplicationContext ctx =
            new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/adoTemplateTests.xml");
        Assert.IsNotNull(ctx);
        dbProvider = ctx["DbProvider"] as IDbProvider;
        Assert.IsNotNull(dbProvider);
        adoOperations = new AdoTemplate(dbProvider);
    }

    [Test]
    public void ExecuteNonQueryText()
    {
        string badSql = "insert into TestObjects(Age, Name) VALS (33, 'foo')";
        try
        {
            adoOperations.ExecuteNonQuery(CommandType.Text, badSql);
        }
        catch (BadSqlGrammarException e)
        {
            log.LogError(e, "caught correct exception");
        }
        catch (Exception e)
        {
            log.LogError(e, "caught incorrect exception ");

            Assert.Fail("did not throw exception of type BadSqlGrammerException");
        }
    }
}
