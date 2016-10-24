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

using System.Collections;
using NUnit.Framework;

#endregion

namespace Spring.Expressions
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class SelectionNodeTests
    {
        [Test]
        public void RespectsLimits()
        {
            char[] input = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n' };
            char[] result;

            result = Evaluate("?{ true }", input);
            Assert.AreEqual(input, result);
            result = Evaluate("?{ true, 5, 10 }", input);
            Assert.AreEqual( new char[] { 'f', 'g', 'h', 'i', 'j', 'k' }, result );
            result = Evaluate("?{ true, 5 }", input);
            Assert.AreEqual(new char[] { 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n' }, result);
            result = Evaluate("?{ true, 0, 10 }", input);
            Assert.AreEqual(new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k' }, result);

            result = Evaluate("?{ T(System.Convert).ToInt32(#this) % 2 == 0, 1, 3 }", input);
            Assert.AreEqual(new char[] { 'd', 'f', 'h' }, result);
        }

        private char[] Evaluate(string expression, char[] input)
        {
            IExpression expr = Expression.Parse(expression);
            ICollection collection = (ICollection)expr.GetValue(input);
            return (char[]) new ArrayList(collection).ToArray(typeof(char));
        }
    }
}