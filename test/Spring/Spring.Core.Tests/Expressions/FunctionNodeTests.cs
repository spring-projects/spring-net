#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
using NUnit.Framework;

#endregion

namespace Spring.Expressions
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    /// <version>$Id: FunctionNodeTests.cs,v 1.1 2008/03/20 23:58:16 oakinger Exp $</version>
    [TestFixture]
    public class FunctionNodeTests
    {
        [Test]
        public void ExecutesLambdaFunction()
        {
            Hashtable vars = new Hashtable();
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
            Hashtable vars = new Hashtable();
            vars["ident"] = new IdentityCallback(Identity);

            FunctionNode fn = new FunctionNode();
            fn.Text = "ident";
            StringLiteralNode str = new StringLiteralNode();
            str.Text = "theValue";
            fn.addChild(str);

            IExpression exp = fn;
            Assert.AreEqual(str.Text, exp.GetValue(null, vars));
        }

        private delegate object IdentityCallback(object arg);
        private object Identity(object arg)
        {
            return arg;
        }
    }
}