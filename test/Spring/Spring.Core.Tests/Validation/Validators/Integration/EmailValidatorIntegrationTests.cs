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

using System.Text;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;

namespace Spring.Validation.Validators;

/// <summary>
/// EmailValidatorIntegration integration tests.
/// </summary>
/// <author>Goran Milosavljevic</author>
[TestFixture]
public class EmailValidatorIntegrationTests
{
    [Test]
    public void EmailValidatorTests()
    {
        const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
            <objects xmlns='http://www.springframework.net' xmlns:v='http://www.springframework.net/validation'>
                <v:validator id='emailValidator' test='#this' type='Spring.Validation.Validators.EmailValidator, Spring.Core'/>
            </objects>";

        MemoryStream stream = new MemoryStream(new UTF8Encoding().GetBytes(xml));
        IResource resource = new InputStreamResource(stream, "emailValidator");

        XmlObjectFactory objectFactory = new XmlObjectFactory(resource, null);
        object obj = objectFactory.GetObject("emailValidator");

        Assert.IsTrue(obj is IValidator);
        IValidator validator = obj as IValidator;
        Assert.IsTrue(validator.Validate("goran@goran.com", new ValidationErrors()));
    }
}