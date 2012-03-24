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
using NUnit.Framework;
using Spring.Expressions.Processors;

#endregion

namespace Spring.Expressions
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class MethodNodeTests
    {
        private class MyTestCollectionProcessor : ICollectionProcessor
        {
            public object Process(ICollection source, object[] args)
            {
                return source;
            }
        }

        [Test]
        public void CallCustomCollectionProcessor()
        {
            Dictionary<string, object> vars = new Dictionary<string, object>();
            vars["myCollProc"] = new MyTestCollectionProcessor();

            MethodNode mn = new MethodNode();
            mn.Text = "myCollProc";

            IExpression exp = mn;
            int[] input = new int[] {1, 2, 3};
            Assert.AreSame(input, exp.GetValue(input, vars));
        }

        [Test, Explicit]
        public void PerformanceOfMethodEvaluationOnDifferentContextTypes()
        {
            MethodNode mn = new MethodNode();
            mn.Text = "ToString";

            TypeNode nln = new TypeNode();
            nln.Text = "System.Globalization.CultureInfo";

            PropertyOrFieldNode pn = new PropertyOrFieldNode();
            pn.Text = "InvariantCulture";


            Expression exp = new Expression();
            exp.addChild(nln);
            exp.addChild(pn);

            StringLiteralNode sln = new StringLiteralNode();
            sln.Text = "dummy";

            mn.addChild(sln);
            mn.addChild(exp);

            IExpression mnExp = mn;
            Assert.AreEqual("dummy", mnExp.GetValue(0m, null));

            int runs = 10000000;

            StopWatch watch = new StopWatch();
            using (watch.Start("Duration: {0}"))
            {
                for (int i = 0; i < runs; i++)
                {
                    mnExp.GetValue(0m, null);
                }
            }
        }

        #region StopWatch

        private class StopWatch
        {
            private DateTime _startTime;
            private TimeSpan _elapsed;

            private class Stopper : IDisposable
            {
                private readonly StopWatch _owner;
                private readonly string _format;
                public Stopper(StopWatch owner, string format) { _owner = owner; _format = format; }
                public void Dispose() { _owner.Stop(_format); GC.SuppressFinalize(this); }
            }

            public IDisposable Start(string outputFormat)
            {
                Stopper stopper = new Stopper(this, outputFormat);
                _startTime = DateTime.Now;
                return stopper;
            }

            private void Stop(string outputFormat)
            {
                _elapsed = DateTime.Now.Subtract(_startTime);
                if (outputFormat != null)
                {
                    Console.WriteLine(outputFormat, _elapsed);
                }
            }

            public DateTime StartTime
            {
                get { return _startTime; }
            }

            public TimeSpan Elapsed
            {
                get { return _elapsed; }
            }
        }

        #endregion
    }
}