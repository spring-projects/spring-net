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
using Spring.Expressions;
using Spring.Objects.Factory.Support;

namespace Spring.Validation;

/// <summary>
/// Unit tests for ValidatorReference class.
/// </summary>
/// <author>Aleksandar Seovic</author>
[TestFixture]
public class ValidatorReferenceTests
{
    [Test]
    public void TrueValidatorReference()
    {
        StaticListableObjectFactory factory = new StaticListableObjectFactory();
        factory.AddObject("validator", new TrueValidator());

        ValidatorReference v = new ValidatorReference("true");
        v.ObjectFactory = factory;
        v.Name = "validator";

        IValidationErrors errors = new ValidationErrors();

        Assert.IsTrue(v.Validate(null, null, errors));
        Assert.IsTrue(v.Validate(null, errors));
    }

    [Test]
    public void FalseValidatorReference()
    {
        StaticListableObjectFactory factory = new StaticListableObjectFactory();
        factory.AddObject("validator", new FalseValidator());

        ValidatorReference v = new ValidatorReference();
        v.ObjectFactory = factory;
        v.Name = "validator";

        IValidationErrors errors = new ValidationErrors();
        Assert.IsFalse(v.Validate(null, null, errors));
        Assert.IsFalse(v.Validate(null, errors));
    }

    [Test]
    public void FalseValidatorReferenceNotEvaluatedBecauseWhenExpressionReturnsFalse()
    {
        StaticListableObjectFactory factory = new StaticListableObjectFactory();
        factory.AddObject("validator", new FalseValidator());

        ValidatorReference v = new ValidatorReference("false");
        v.ObjectFactory = factory;
        v.Name = "validator";

        IValidationErrors errors = new ValidationErrors();
        Assert.IsTrue(v.Validate(null, null, errors));
        Assert.IsTrue(v.Validate(null, errors));
    }

    [Test]
    public void ContextNarrowing()
    {
        Inventor context = new Inventor("Nikola Tesla", new DateTime(1856, 7, 9), "Serbian");

        ConditionValidator cv1 = new ConditionValidator("DOB.Year == 1856", null);
        ConditionValidator cv2 = new ConditionValidator("Year == 1856", null);

        StaticListableObjectFactory factory = new StaticListableObjectFactory();
        factory.AddObject("cv1", cv1);
        factory.AddObject("cv2", cv2);

        ValidatorReference v1 = new ValidatorReference();
        v1.ObjectFactory = factory;
        v1.Name = "cv1";

        IValidationErrors errors = new ValidationErrors();

        Assert.IsTrue(v1.Validate(context, null, errors));
        Assert.IsTrue(v1.Validate(context, errors));

        ValidatorReference v2 = new ValidatorReference();
        v2.ObjectFactory = factory;
        v2.Name = "cv2";
        v2.Context = Expression.Parse("DOB");

        Assert.IsTrue(v2.Validate(context, null, errors));
        Assert.IsTrue(v2.Validate(context, errors));

        ValidatorReference v3 = new ValidatorReference("false");
        v3.ObjectFactory = factory;
        v3.Name = "cv2";
        v3.Context = Expression.Parse("DOB");

        Assert.IsTrue(v3.Validate(null, errors));
    }
}
