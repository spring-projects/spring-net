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

using System;
using System.Collections.Generic;
using NUnit.Framework;

using Spring.Expressions;

namespace Spring.Validation.Actions
{
    /// <summary>
    /// Unit tests for the ExpressionAction class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public class ExpressionActionTests
    {
        [Test]
        public void WhenValid()
        {
            Inventor context = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            Dictionary<string, object> vars = new Dictionary<string, object>();

            ExpressionAction action = new ExpressionAction("#result = 'valid'", "#result = 'invalid'");
            action.Execute(true, context, vars, null);
            Assert.AreEqual("valid", vars["result"]);

            action = new ExpressionAction(Expression.Parse("#result = Name"), Expression.Parse("#result = Nationality"));
            action.Execute(true, context, vars, null);
            Assert.AreEqual(context.Name, vars["result"]);

            action = new ExpressionAction();
            action.Valid = Expression.Parse("#result = DOB.Year");
            action.Invalid = Expression.Parse("#result = DOB.Month");
            action.Execute(true, context, vars, null);
            Assert.AreEqual(context.DOB.Year, vars["result"]);

            vars.Clear();
            action = new ExpressionAction(null, "#result = 'invalid'");
            action.Execute(true, context, vars, null);
            Assert.IsFalse(vars.ContainsKey("result"), "Result should not exist when valid expression is null.");
        }

        [Test]
        public void WhenInvalid()
        {
            Inventor context = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            Dictionary<string, object> vars = new Dictionary<string, object>();

            ExpressionAction action = new ExpressionAction("#result = 'valid'", "#result = 'invalid'");
            action.Execute(false, context, vars, null);
            Assert.AreEqual("invalid", vars["result"]);

            action = new ExpressionAction(Expression.Parse("#result = Name"), Expression.Parse("#result = Nationality"));
            action.Execute(false, context, vars, null);
            Assert.AreEqual(context.Nationality, vars["result"]);

            action = new ExpressionAction();
            action.Valid = Expression.Parse("#result = DOB.Year");
            action.Invalid = Expression.Parse("#result = DOB.Month");
            action.Execute(false, context, vars, null);
            Assert.AreEqual(context.DOB.Month, vars["result"]);

            vars.Clear();
            action = new ExpressionAction("#result = 'valid'", null);
            action.Execute(false, context, vars, null);
            Assert.IsFalse(vars.ContainsKey("result"), "Result should not exist when invalid expression is null.");
        }

        [Test]
        public void WhenActionIsNotExecutedBecauseWhenExpressionReturnsFalse()
        {
            Inventor context = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            Dictionary<string, object> vars = new Dictionary<string, object>();

            ExpressionAction action = new ExpressionAction("#result = 'valid'", "#result = 'invalid'");
            action.When = Expression.Parse("false");
            action.Execute(true, context, vars, null);
            Assert.IsFalse(vars.ContainsKey("result"));
        }

    }
}