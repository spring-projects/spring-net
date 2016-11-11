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
using NUnit.Framework;

#endregion

namespace Spring.Expressions.Processors
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class OrderByProcessorTests
    {
        [Test]
        public void OrderByExpressionString()
        {
            IExpression exp = Expression.Parse("orderBy('ToString()')");
            object[] input = new object[] { 'b', 1, 2.0, "a" };

            Assert.AreEqual( new object[] { 1,2.0,"a",'b' }, exp.GetValue(input) );
        }

        [Test]
        public void OrderByLambdaFunction()
        {
            IExpression exp = Expression.Parse("orderBy({|a,b| $a.ToString().CompareTo($b.ToString())})");
            object[] input = new object[] { 'b', 1, 2.0, "a" };

            Assert.AreEqual(new object[] { 1, 2.0, "a", 'b' }, exp.GetValue(input));

            Dictionary<string, object> vars = new Dictionary<string, object>();
            Expression.RegisterFunction( "compare", "{|a,b| $a.ToString().CompareTo($b.ToString())}", vars);
            exp = Expression.Parse("orderBy(#compare)");
            Assert.AreEqual(new object[] { 1, 2.0, "a", 'b' }, exp.GetValue(input, vars));
        }

        [Test]
        public void OrderByDelegate()
        {
            Dictionary<string, object> vars = new Dictionary<string, object>();
            vars["compare"] = new CompareCallback(CompareObjects);

            IExpression exp = Expression.Parse("orderBy(#compare)");
            object[] input = new object[] { 'b', 1, 2.0, "a" };

            Assert.AreEqual(new object[] { 1, 2.0, "a", 'b' }, exp.GetValue(input, vars));
        }

        private delegate int CompareCallback(object x, object y);
        private int CompareObjects(object x, object y)
        {
            if (x == y) return 0;
            return x.ToString().CompareTo(""+y);
        }
    }
}