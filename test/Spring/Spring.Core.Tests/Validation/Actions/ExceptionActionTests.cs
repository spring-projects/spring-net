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
    /// Tests for the ExceptionAction class.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class ExceptionActionTests
    {

        [Test]
        public void WhenInvalidThrowDefaultException()
        {
            Inventor context = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            Dictionary<string, object> vars = new Dictionary<string, object>();
            ExceptionAction action = new ExceptionAction();
            try
            {
                action.Execute(false, context, vars, null);
                Assert.Fail("Should have thrown exception");
            }
            catch (ValidationException)
            {

            }            
        }

        [Test]
        public void WhenInvalidThrowCustomException()
        {
            Inventor context = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            Dictionary<string, object> vars = new Dictionary<string, object>();
            ExceptionAction action = new ExceptionAction("new System.InvalidOperationException('invalid')");
            try
            {
                action.Execute(false, context, vars, null);
                Assert.Fail("Should have thrown exception");
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual("invalid", e.Message);
            }
        }

        [Test]
        public void WhenInvalidThrowCustomExceptionUsingSetter()
        {
            Inventor context = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            Dictionary<string, object> vars = new Dictionary<string, object>();
            ExceptionAction action = new ExceptionAction();
            IExpression expression = Expression.Parse("new System.InvalidOperationException('invalid')");
            action.ThrowsExpression = expression;
            try
            {
                action.Execute(false, context, vars, null);
                Assert.Fail("Should have thrown exception");
            }
            catch (InvalidOperationException e)
            {
                Assert.AreEqual("invalid", e.Message);
            }
        }


        [Test]
        public void WhenValid()
        {
            Inventor context = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            Dictionary<string, object> vars = new Dictionary<string, object>();
            ExceptionAction action = new ExceptionAction();
            try
            {
                action.Execute(true, context, vars, null);                
            }
            catch (Exception)
            {
                Assert.Fail("Should not have thrown exception");
            }   
        }

    }
}