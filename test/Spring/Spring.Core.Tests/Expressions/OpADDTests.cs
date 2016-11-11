#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using NUnit.Framework;

namespace Spring.Expressions
{
    /// <summary>
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class OpADDTests
    {
        [Test]
        public void CanAddStrings()
        {
            OpADD add = new OpADD();
            add.addChild( new StringLiteralNode("20"));
            add.addChild( new StringLiteralNode("30"));
            object result = add.GetValue(null, null);
            Assert.AreEqual("2030", result);
        }
        [Test]
        public void CanAddNumbers()
        {
            OpADD add = new OpADD();
            add.addChild( new IntLiteralNode("20"));
            add.addChild( new IntLiteralNode("30"));
            object result = add.GetValue(null, null);
            Assert.AreEqual(50, result);
        }
    }
}