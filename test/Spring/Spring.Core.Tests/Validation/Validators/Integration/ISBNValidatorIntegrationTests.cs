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

#region Imports

using System.Text;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Validation.Validators;

/// <summary>
/// ISBNValidatorIntegration integration tests.
/// </summary>
/// <author>Goran Milosavljevic</author>
[TestFixture]
public class ISBNValidatorIntegrationTests
{
    [Test]
    public void ISBNValidatorTests()
    {
        const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
            <objects xmlns='http://www.springframework.net' xmlns:v='http://www.springframework.net/validation'>
                <v:validator id='isbnValidator' test='#this' type='Spring.Validation.Validators.ISBNValidator, Spring.Core'>
                    <v:message id='error.airportCode.dummy' providers='summary' when='false'/>
                </v:validator>
            </objects>";

        MemoryStream stream = new MemoryStream(new UTF8Encoding().GetBytes(xml));
        IResource resource = new InputStreamResource(stream, "isbnValidator");

        XmlObjectFactory objectFactory = new XmlObjectFactory(resource, null);
        object obj = objectFactory.GetObject("isbnValidator");

        Assert.IsTrue(obj is IValidator);
        IValidator validator = obj as IValidator;
        Assert.IsTrue(validator.Validate("978-1-905158-79-9", new ValidationErrors()));
    }
}
