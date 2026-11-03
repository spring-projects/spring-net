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

using NUnit.Framework;

namespace Spring.Expressions;

/// <summary>
/// Unit tests for the OpAND class.
/// </summary>
/// <author>Erich Eichinger</author>
[TestFixture]
public class OpANDTests
{
    [Test]
    public void AndsNumbers()
    {
        OpAND band = new OpAND(new IntLiteralNode("2"), new IntLiteralNode("3"));
        Assert.That(band.GetValue(null, null), Is.EqualTo(2 & 3));
    }

    [Test]
    public void AndsBooleans()
    {
        OpAND band1 = new OpAND(new BooleanLiteralNode("true"), new BooleanLiteralNode("true"));
        Assert.That(band1.GetValue(null, null), Is.EqualTo(true));

        OpAND band2 = new OpAND(new BooleanLiteralNode("true"), new BooleanLiteralNode("false"));
        Assert.That(band2.GetValue(null, null), Is.EqualTo(false));
    }

    [Test(Description = "SPRNET-1381")]
    public void TestShortcircuitAndOperator()
    {
        object boolean = ExpressionEvaluator.GetValue(new Inventor(), "Name != null and Name.Length == 0");
        Assert.That(boolean, Is.EqualTo(false));
    }
}
