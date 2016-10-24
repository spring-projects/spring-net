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
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using Spring.Data.Core;
using Spring.Reflection.Dynamic;
using Spring.Transaction.Support;
using Spring.Util;

#endregion

namespace Spring.Data
{
    /// <summary>
    /// This calss contains tests for 
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture, Explicit]
    public class AdoTemplatePerformanceTests
    {
        private AdoTemplate adoTemplate;
        private string cmdText = "insert into TestObjects(Age, Name) VALUES (24, 'John')";
        private DateTime start, stop;

        [SetUp]
        public void CreateAdoTemplate()
        {
            IApplicationContext ctx =
                new XmlApplicationContext("assembly://Spring.Data.Integration.Tests/Spring.Data/adoTemplateTests.xml");
            Assert.IsNotNull(ctx);
            adoTemplate = ctx["adoTemplate"] as AdoTemplate;
            Assert.IsNotNull(adoTemplate);
            //CleanDb();
        }

        private void CleanDb()
        {
            adoTemplate.ExecuteNonQuery(CommandType.Text, "truncate table TestObjects");
        }

        [Test]
        public void PerformanceWithOneTxTests()
        {
            UseAdoTemplateApiOneTx(1000);
            CleanDb();
        }

        [Test]
        public void PerformanceTests()
        {
            for (int i = 0; i < 10; i++)
                DoTest(10000, true, 5000, true);
        }

        [Test]
        public void ObjectInstantiatonTests()
        {
            int numIterations = 1000000;


            start = DateTime.Now;
            Type t = typeof (AccountCreditDao);
            for (int i = 0; i < numIterations; i++)
            {
                //ObjectUtils.IsInstantiable(t);
                IDynamicConstructor dc = DynamicConstructor.Create(t.GetConstructor(Type.EmptyTypes));                       
                dc.Invoke(ObjectUtils.EmptyObjects);
            }
            stop = DateTime.Now;
            double timeElapsed = Elapsed;
            PrintTest("SafeConstructor", numIterations, timeElapsed);

            start = DateTime.Now;
            for (int i = 0; i < numIterations; i++)
            {
                ObjectUtils.InstantiateType(typeof (AccountCreditDao));
            }
            stop = DateTime.Now;
            timeElapsed = Elapsed;
            PrintTest("InstantiateType", numIterations, timeElapsed);

        }

        private void DoTest(int numIterations, bool standardApiFirst, int sleepTimeMs, bool oneTx)
        {
            Console.WriteLine("sleeping for " + sleepTimeMs);
            Thread.Sleep(sleepTimeMs);
            double stdTime;
            double templateTime;
            if (standardApiFirst)
            {
                if (oneTx)
                {
                    stdTime = UseStandardApiOneTx(numIterations);
                }
                else
                {
                    stdTime = UseStandardApi(numIterations);
                }

                CleanDb();
                Console.WriteLine("sleeping for " + sleepTimeMs);
                Thread.Sleep(sleepTimeMs);

                if (oneTx)
                {
                    templateTime = UseAdoTemplateApiOneTx(numIterations);
                }
                else
                {
                    templateTime = UseAdoTemplateApi(numIterations);
                }

                CleanDb();
            }
            else
            {
                if (oneTx)
                {
                    templateTime = UseAdoTemplateApiOneTx(numIterations);
                }
                else
                {
                    templateTime = UseAdoTemplateApi(numIterations);
                }

                CleanDb();
                Console.WriteLine("sleeping for " + sleepTimeMs);
                Thread.Sleep(sleepTimeMs);

                if (oneTx)
                {
                    stdTime = UseStandardApiOneTx(numIterations);
                }
                else
                {
                    stdTime = UseStandardApi(numIterations);
                }

                CleanDb();
            }
            Console.WriteLine("stdTime = " + stdTime + ", templateTime = " + templateTime);
            double percentDiff = ((2*Math.Abs(stdTime - templateTime))/(stdTime + templateTime))*100;
            Console.WriteLine("stdTime-templateTime=" + (stdTime - templateTime));
            Console.WriteLine("% diff = " + percentDiff);
        }

        private double UseAdoTemplateApiOneTx(int numIterations)
        {
            AdoPlatformTransactionManager tm = new AdoPlatformTransactionManager(adoTemplate.DbProvider);

            TransactionTemplate tt = new TransactionTemplate(tm);
            double timeElapsed = 0;
            tt.Execute(status =>
                           {
                               start = DateTime.Now;
                               for (int i = 0; i < numIterations; i++)
                               {
                                   adoTemplate.ExecuteNonQuery(CommandType.Text, cmdText);
                               }
                               stop = DateTime.Now;
                               timeElapsed = Elapsed;
                               PrintTest("AdoTemplateApi", numIterations, timeElapsed);
                               return null;
                           });
            return timeElapsed;
        }

        private double UseAdoTemplateApi(int numIterations)
        {
            start = DateTime.Now;
            for (int i = 0; i < numIterations; i++)
            {
                adoTemplate.ExecuteNonQuery(CommandType.Text, cmdText);
            }
            stop = DateTime.Now;
            double timeElapsed = Elapsed;
            PrintTest("AdoTemplateApi", numIterations, timeElapsed);
            return timeElapsed;
        }

        private double UseStandardApiOneTx(int numIterations)
        {
            start = DateTime.Now;
            using (SqlConnection connection = new SqlConnection(adoTemplate.DbProvider.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(cmdText, connection))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    for (int i = 0; i < numIterations; i++)
                    {
                        command.ExecuteNonQuery();
                    }
                    // no error handling....
                    transaction.Commit();                    
                }
            }
            stop = DateTime.Now;
            double timeElapsed = Elapsed;
            PrintTest("StandardAPI", numIterations, timeElapsed);
            return timeElapsed;
        }

        private double UseStandardApi(int numIterations)
        {
            start = DateTime.Now;
            for (int i = 0; i < numIterations; i++)
            {
                using (SqlConnection connection = new SqlConnection(adoTemplate.DbProvider.ConnectionString))
                {
                    using (SqlCommand command = new SqlCommand(cmdText, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
            }
            stop = DateTime.Now;
            double timeElapsed = Elapsed;
            PrintTest("StandardAPI", numIterations, timeElapsed);
            return timeElapsed;
        }


        private double Elapsed
        {
            get { return (stop.Ticks - start.Ticks)/10000000f; }
        }

        private static void PrintTest(string name, int iterations, double duration)
        {
            Debug.WriteLine(
                String.Format("{0,-60} {1,12:#,###} {2,12:##0.000} {3,12:#,###}", name, iterations, duration,
                              iterations/duration));
        }
    }
}
