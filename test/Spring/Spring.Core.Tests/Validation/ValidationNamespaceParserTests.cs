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

#region Imports

using System.Collections.Generic;
using System.Xml;

using NUnit.Framework;

using Spring.Core.IO;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;
using Spring.Validation.Actions;
using Spring.Validation.Config;

#endregion

namespace Spring.Validation
{
    /// <summary>
    /// Unit tests for the ValidationNamespaceParser class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class ValidationNamespaceParserTests
    {
        [Test]
        public void WhenConfigFileIsValid()
        {
            XmlDocument doc = GetValidatedXmlResource("_WhenConfigFileIsValid.xml");

            MockObjectDefinitionRegistry registry = new MockObjectDefinitionRegistry();

            XmlReaderContext readerContext = new XmlReaderContext(null, new XmlObjectDefinitionReader(registry));
            ObjectDefinitionParserHelper helper = new ObjectDefinitionParserHelper(readerContext);
            helper.InitDefaults(doc.DocumentElement);
            ParserContext parserContext = new ParserContext(helper);

            ValidationNamespaceParser parser = new ValidationNamespaceParser();
            foreach (XmlElement element in doc.DocumentElement.ChildNodes)
            {
                if (element.NamespaceURI == "http://www.springframework.net/validation")
                {
                    parser.ParseElement(element, parserContext);
                }
            }
            IList<IObjectDefinition> defs = registry.GetObjectDefinitions();
            Assert.AreEqual(9, defs.Count);

            IObjectDefinition def = registry.GetObjectDefinition("destinationAirportValidator");
            Assert.IsTrue(def.IsSingleton);
            Assert.IsTrue(def.IsLazyInit);
            Assert.IsTrue(typeof(IValidator).IsAssignableFrom(def.ObjectType));
            
            PropertyValue fastValidateProperty = def.PropertyValues.GetPropertyValue("FastValidate");
            Assert.IsNotNull(fastValidateProperty);
            Assert.AreEqual("true", fastValidateProperty.Value);

            PropertyValue validatorsProperty = def.PropertyValues.GetPropertyValue("Validators");
            Assert.IsNotNull(validatorsProperty);
            object validatorsObject = validatorsProperty.Value;
            Assert.AreEqual(typeof(ManagedList), validatorsObject.GetType());
            ManagedList validators = (ManagedList)validatorsObject;
            Assert.AreEqual(4, validators.Count);

            def = (IObjectDefinition)validators[3];
            Assert.IsTrue(def.IsSingleton);
            Assert.IsTrue(def.IsLazyInit);
            Assert.AreEqual(typeof(RegularExpressionValidator), def.ObjectType);
            Assert.AreEqual("[A-Z]*", def.PropertyValues.GetPropertyValue("Expression").Value);


            def = registry.GetObjectDefinition("simpleAirportValidator");
            Assert.IsTrue(def.IsSingleton);
            Assert.IsTrue(def.IsLazyInit);
            Assert.IsTrue(typeof(IValidator).IsAssignableFrom(def.ObjectType));
            PropertyValue actionsProperty = def.PropertyValues.GetPropertyValue("Actions");
            Assert.IsNotNull(actionsProperty);
            object actionsObject = actionsProperty.Value;
            Assert.AreEqual(typeof(ManagedList), actionsObject.GetType());
            ManagedList actions = (ManagedList)actionsObject;
            Assert.AreEqual(1, actions.Count);

            IObjectDefinition exceptionActionDefinition = (IObjectDefinition)actions[0];
            Assert.AreEqual(typeof(ExceptionAction), exceptionActionDefinition.ObjectType);
            Assert.AreEqual("'new System.InvalidOperationException(\"invalid\")' []", exceptionActionDefinition.ConstructorArgumentValues.GenericArgumentValues[0].ToString());
            //
            
            def = registry.GetObjectDefinition("airportCodeValidator");
            Assert.IsTrue(def.IsSingleton);
            Assert.IsTrue(def.IsLazyInit);
            Assert.IsTrue(typeof(IValidator).IsAssignableFrom(def.ObjectType));
            actionsProperty = def.PropertyValues.GetPropertyValue("Actions");
            Assert.IsNotNull(actionsProperty);
            actionsObject = actionsProperty.Value;
            Assert.AreEqual(typeof(ManagedList), actionsObject.GetType());
            actions = (ManagedList)actionsObject;
            Assert.AreEqual(4, actions.Count);

            IObjectDefinition messageDefinition = (IObjectDefinition)actions[1];
            Assert.AreEqual(typeof(ErrorMessageAction), messageDefinition.ObjectType);

            IObjectDefinition actionDefinition = (IObjectDefinition)actions[2];
            Assert.AreEqual(typeof(ExpressionAction), actionDefinition.ObjectType);
            Assert.AreEqual("#now = DateTime.Now", actionDefinition.PropertyValues.GetPropertyValue("Valid").Value);

            def = registry.GetObjectDefinition("regex");
            Assert.IsTrue(def.IsSingleton);
            Assert.IsTrue(def.IsLazyInit);
            Assert.IsTrue(typeof(IValidator).IsAssignableFrom(def.ObjectType));
            PropertyValue expressionProperty = def.PropertyValues.GetPropertyValue("Expression");
            Assert.IsNotNull(expressionProperty);
            Assert.AreEqual("RegExp", expressionProperty.Value);
        }

        private XmlDocument GetValidatedXmlResource(string resourceExtension)
        {
            AssemblyResource validationSchema = new AssemblyResource("assembly://Spring.Core/Spring.Validation.Config/spring-validation-1.3.xsd");
            AssemblyResource objectsSchema = new AssemblyResource("assembly://Spring.Core/Spring.Objects.Factory.Xml/spring-objects-1.3.xsd");

            return TestResourceLoader.GetXmlValidated(this, resourceExtension, objectsSchema, validationSchema);
        }

        [Test]
        public void WhenConfigFileIsNotValid()
        {
//            const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
//<objects xmlns='http://www.springframework.net' xmlns:v='http://www.springframework.net/validation'>
//    <v:required test='#this'>
//        <v:message id='error.airportCode.required' providers='summary'/>
//        <v:action type='Spring.Validation.Actions.ExpressionAction, Spring.Core' when='true'/>
//    </v:required>
//</objects>
//";
            XmlDocument doc = GetValidatedXmlResource("_WhenConfigFileIsNotValid.xml");

            MockObjectDefinitionRegistry registry = new MockObjectDefinitionRegistry();

            XmlReaderContext readerContext = new XmlReaderContext(null, new XmlObjectDefinitionReader(registry));
            ObjectDefinitionParserHelper helper = new ObjectDefinitionParserHelper(readerContext);
            helper.InitDefaults(doc.DocumentElement);
            ParserContext parserContext = new ParserContext(helper);

            ValidationNamespaceParser parser = new ValidationNamespaceParser();
            Assert.Throws<ObjectDefinitionStoreException>(() =>
            {
                foreach (XmlElement element in doc.DocumentElement.ChildNodes)
                {
                    if (element.NamespaceURI == "http://www.springframework.net/validation")
                    {
                        parser.ParseElement(element, parserContext);
                    }
                }
            });
        }
    }
}