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

using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Validation.Validators
{
    /// <summary>
    /// CreditCardValidator integration tests.
    /// </summary>
    /// <author>Goran Milosavljevic</author>
    [TestFixture]
    public class CreditCardValidatorIntegrationTests
    {
        [Test]
        public void CreditCardValidatorTests()
        {
            const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
            <objects xmlns='http://www.springframework.net' xmlns:v='http://www.springframework.net/validation'>
                <object id='ccAmex' type='Spring.Validation.Validators.Amex, Spring.Core'/>
                <v:validator id='ccValidator' test='#this' type='Spring.Validation.Validators.CreditCardValidator, Spring.Core'>
                  <v:property name='CardType' ref='ccAmex'/>
                  <v:message id='error.airportCode.dummy' providers='summary' when='false'/>
                </v:validator>
            </objects>";
            
            MemoryStream stream = new MemoryStream(new UTF8Encoding().GetBytes(xml));
            IResource resource = new InputStreamResource(stream, "ccValidator");
            
            XmlObjectFactory objectFactory = new XmlObjectFactory(resource, null);
            object obj = objectFactory.GetObject("ccValidator");
            
            Assert.IsTrue(obj is CreditCardValidator);
            CreditCardValidator validator = obj as CreditCardValidator;
            Assert.IsNotNull(validator.CardType);
            Assert.IsTrue(validator.CardType is Amex);
            Assert.IsTrue(validator.Validate("378282246310005", new ValidationErrors()));
        }

        [Test]
        public void WithNullCardType()
        {
            const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
            <objects xmlns='http://www.springframework.net' xmlns:v='http://www.springframework.net/validation'>
                <v:validator id='ccValidator' test='#this' type='Spring.Validation.Validators.CreditCardValidator, Spring.Core'>
                  <v:message id='error.airportCode.dummy' providers='summary' when='false'/>
                </v:validator>
            </objects>";

            MemoryStream stream = new MemoryStream(new UTF8Encoding().GetBytes(xml));
            IResource resource = new InputStreamResource(stream, "ccValidator");

            XmlObjectFactory objectFactory = new XmlObjectFactory(resource, null);
            object obj = objectFactory.GetObject("ccValidator");

            Assert.IsTrue(obj is CreditCardValidator);
            CreditCardValidator validator = obj as CreditCardValidator;
            Assert.IsNull(validator.CardType);
            Assert.Throws<ArgumentException>(() => validator.Validate("378282246310005", new ValidationErrors()));
        }

        [Test]
        public void ErrorTests()
        {
            const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
            <objects xmlns='http://www.springframework.net' xmlns:v='http://www.springframework.net/validation'>
                <object id='ccAmex' type='Spring.Validation.Validators.Amex, Spring.Core'/>
                <v:validator id='ccValidator' test='Creditcard' type='Spring.Validation.Validators.CreditCardValidator, Spring.Core'>
                    <v:property name='CardType' ref='ccAmex'/>
                    <v:message id='errorKey' providers='validationSummary' />
                </v:validator>
            </objects>";

            MemoryStream stream = new MemoryStream(new UTF8Encoding().GetBytes(xml));
            IResource resource = new InputStreamResource(stream, "ccValidator");

            XmlObjectFactory objectFactory = new XmlObjectFactory(resource, null);

            IValidationErrors errors = new ValidationErrors();
            Contact contact = new Contact();
            IValidator validator = (IValidator)objectFactory.GetObject("ccValidator");

            contact.Creditcard = "378282246310";
            bool result = validator.Validate(contact, errors);
           
            Assert.IsNotNull(errors.GetErrors("validationSummary"));
            Assert.IsFalse(result);
        }    
    }
}
