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

using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

#endregion

namespace Spring.Expressions
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class FunctionNodeTests
    {
        [Test]
        public void ExecutesLambdaFunction()
        {
            Dictionary<string, object> vars = new Dictionary<string, object>();
            Expression.RegisterFunction("ident", "{|n| $n}", vars);

            FunctionNode fn = new FunctionNode();
            fn.Text = "ident";
            StringLiteralNode str = new StringLiteralNode();
            str.Text = "theValue";
            fn.addChild(str);

            IExpression exp = fn;
            Assert.AreEqual(str.Text, exp.GetValue(null, vars));
        }

        [Test]
        public void ExecutesDelegate()
        {
            Dictionary<string, object> vars = new Dictionary<string, object>();
            vars["concat"] = new TestCallback(Concat);

            FunctionNode fn = new FunctionNode();
            fn.Text = "concat";
            StringLiteralNode str = new StringLiteralNode();
            str.Text = "theValue";
            fn.addChild(str);
            StringLiteralNode str2 = new StringLiteralNode();
            str2.Text = "theValue";
            fn.addChild(str2);

            IExpression exp = fn;
            Assert.AreEqual(string.Format("{0},{1},{2}", this.GetHashCode(), str.Text, str2.Text), exp.GetValue(null, vars));
        }

        private delegate string TestCallback(string arg1, string arg2);

        private string Concat(string arg1, string arg2)
        {
            return string.Format("{0},{1},{2}", this.GetHashCode(), arg1, arg2);
        }

        [Category("Performance")]
        [Test, Explicit]
        public void ExecutesDelegatePerformance()
        {
            Dictionary<string, object> vars = new Dictionary<string, object>(5);
            WaitCallback noop = delegate (object arg)
                                      {
                                          // noop
                                      };
            vars["noop"] = noop;

            FunctionNode fn = new FunctionNode();
            fn.Text = "noop";
            StringLiteralNode str = new StringLiteralNode();
            str.Text = "theArg";
            fn.addChild(str);

            int ITERATIONS = 10000000;

            StopWatch watch = new StopWatch();
            using (watch.Start("Duration Direct: {0}"))
            {
                for (int i = 0; i < ITERATIONS; i++)
                {
                    ((WaitCallback)vars["noop"])(str.getText());
                }
            }

            using (watch.Start("Duration SpEL: {0}"))
            {
                for (int i = 0; i < ITERATIONS; i++)
                {
                    fn.GetValue(null, vars);
                }
            }
        }
    }
}