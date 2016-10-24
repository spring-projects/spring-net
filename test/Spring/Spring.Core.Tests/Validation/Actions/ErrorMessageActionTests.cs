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

using NUnit.Framework;

using Spring.Context.Support;
using Spring.Expressions;

namespace Spring.Validation.Actions
{
    /// <summary>
    /// Unit tests for the ErrorMessageAction class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public class ErrorMessageActionTests
    {
        [Test]
        public void WithNullMesageId()
        {
            Assert.Throws<ArgumentNullException>(() => new ErrorMessageAction(null, "errors"));    
        }

        [Test]
        public void WithEmptyMesageId()
        {
            Assert.Throws<ArgumentNullException>(() => new ErrorMessageAction("", "errors"));    
        }

        [Test]
        public void WithWhitespaceMesageId()
        {
            Assert.Throws<ArgumentNullException>(() => new ErrorMessageAction("\t   ", "errors"));    
        }

        [Test]
        public void WithNullProviders()
        {
            Assert.Throws<ArgumentException>(() => new ErrorMessageAction("error", null));    
        }

        [Test]
        public void WithEmptyProviders()
        {
            Assert.Throws<ArgumentException>(() => new ErrorMessageAction("error", new string[0]));    
        }

        [Test]
        public void WhenValid()
        {
            Inventor context = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            IValidationErrors errors = new ValidationErrors();
            ErrorMessageAction action = new ErrorMessageAction("error", "errors");
            
            action.Execute(true, context, null, errors);
            Assert.IsTrue(errors.IsEmpty);
        }

        [Test]
        public void WhenInvalid()
        {
            Inventor context = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            IValidationErrors errors = new ValidationErrors();

            ErrorMessageAction action = new ErrorMessageAction("{0}, {1}", "errors");
            action.Parameters = new IExpression[] {Expression.Parse("Name"), Expression.Parse("Nationality")};
            
            action.Execute(false, context, null, errors);
            Assert.IsFalse(errors.IsEmpty);
            Assert.AreEqual(1, errors.GetErrors("errors").Count);
            Assert.AreEqual(context.Name + ", " + context.Nationality, errors.GetResolvedErrors("errors", new NullMessageSource())[0]);
        }

        [Test]
        public void WhenActionIsNotExecutedBecauseWhenExpressionReturnsFalse()
        {
            Inventor context = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            IValidationErrors errors = new ValidationErrors();

            ErrorMessageAction action = new ErrorMessageAction("{0}, {1}", "errors");
            action.When = Expression.Parse("false");
            action.Execute(false, context, null, errors);
            Assert.IsTrue(errors.IsEmpty);
        }

    }
}