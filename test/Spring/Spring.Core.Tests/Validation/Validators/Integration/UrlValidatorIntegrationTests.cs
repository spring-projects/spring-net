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

using System.IO;
using System.Text;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;

#endregion

namespace Spring.Validation.Validators
{
    /// <summary>
    /// UrlValidatorIntegration integration tests.
    /// </summary>
    /// <author>Goran Milosavljevic</author>
    [TestFixture]
    public class UrlValidatorIntegrationTests
    {
        [Test]
        public void ISBNValidatorTests()
        {
            const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
            <objects xmlns='http://www.springframework.net' xmlns:v='http://www.springframework.net/validation'>
                <v:validator id='urlValidator' test='#this' type='Spring.Validation.Validators.UrlValidator, Spring.Core'/>
            </objects>";
            
            MemoryStream stream = new MemoryStream(new UTF8Encoding().GetBytes(xml));
            IResource resource = new InputStreamResource(stream, "urlValidator");

            XmlObjectFactory objectFactory = new XmlObjectFactory(resource, null);
            object obj = objectFactory.GetObject("urlValidator");
            
            Assert.IsTrue(obj is IValidator);
            IValidator validator = obj as IValidator;
            Assert.IsTrue(validator.Validate("http://www.springframework.net", new ValidationErrors()));
        }
    }
}
