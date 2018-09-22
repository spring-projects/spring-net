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

using System;
using System.Reflection;

using FakeItEasy;

using NUnit.Framework;
using Spring.Context;
using Spring.Validation;
using Spring.Validation.Actions;

namespace Spring.Aspects.Validation
{
    /// <summary>
    /// Unit tests for the CacheParameterAdvice class.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [TestFixture]
    public sealed class ParameterValidationAdviceTests
    {
        private IApplicationContext mockContext;
        private ParameterValidationAdvice advice;
        private RequiredValidator requiredValidator;

        [SetUp]
        public void SetUp()
        {
            mockContext = A.Fake<IApplicationContext>();

            advice = new ParameterValidationAdvice();
            advice.ApplicationContext = mockContext;

            requiredValidator = new RequiredValidator();
            requiredValidator.Actions.Add(new ErrorMessageAction("error.required", "errors"));
        }

        [Test]
        public void TestValidArgument()
        {
            MethodInfo method = typeof(ValidationTarget).GetMethod("Save");
            Inventor inventor = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");
            ValidationTarget target = new ValidationTarget();
            object[] args = new object[] {inventor};

            ExpectValidatorRetrieval("required", requiredValidator);

            advice.Before(method, args, target);
            method.Invoke(target, args);
            Assert.AreEqual("NIKOLA TESLA", inventor.Name);
        }

        [Test]
        public void TestInvalidArgument()
        {
            MethodInfo method = typeof(ValidationTarget).GetMethod("Save");

            ExpectValidatorRetrieval("required", requiredValidator);

            Assert.Throws<ValidationException>(() => advice.Before(method, new object[] {null}, new ValidationTarget()));
        }

        private void ExpectValidatorRetrieval(string validatorName, IValidator validator)
        {
            A.CallTo(() => mockContext.GetObject(validatorName)).Returns(validator).Once();
        }
    }

    #region Inner Class : ValidationTarget

    public interface IValidationTarget
    {
        void Save(Inventor inventor);
    }

    public sealed class ValidationTarget : IValidationTarget
    {
        public void Save([Validated("required")] Inventor inventor)
        {
            inventor.Name = inventor.Name.ToUpper();
        }
    }

    #endregion
}