#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using System.Xml;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
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
    public sealed class ValidationConfigParserTests
    {
        [Test]
        public void WhenConfigFileIsValid()
        {
            const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:v='http://www.springframework.net/validation'>
    <v:group id='destinationAirportValidator'>
        <v:ref name='airportCodeValidator' context='ReturningFrom.AirportCode'/>
        <v:ref name='airportCodeValidator'/>
	    <v:condition test='ReturningFrom.AirportCode != StartingFrom.AirportCode'>
            <v:message id='error.destinationAirport.sameAsDeparture' providers='summary'/>
        </v:condition>
	    <v:validator type='Spring.Validation.RegularExpressionValidator, Spring.Core' test='ReturningFrom.AirportCode' when='true'>
            <v:property name='Expression' value='[A-Z]*'/>
            <v:message id='error.destinationAirport.invalidFormat' providers='summary'/>
        </v:validator>
    </v:group>

    <v:required id='airportCodeValidator' test='#this'>
        <v:message id='error.airportCode.dummy' providers='summary' when='false'/>
        <v:message id='error.airportCode.required' providers='summary'>
            <v:param value='#this.Abc'/>
            <v:param value='#this.Xyz'/>
        </v:message>
        <v:action type='Spring.Validation.Actions.ExpressionAction, Spring.Core' when='true'>
            <v:property name='Valid' value='#now = DateTime.Now'/>
        </v:action>
        <v:action type='Spring.Validation.Actions.ExpressionAction, Spring.Core'/>
    </v:required>

    <object id='myObject' type='DateTime'/>

</objects>
";
            XmlDocument doc = new XmlDocument();

            AssemblyResource validationSchema = new AssemblyResource("assembly://Spring.Core/Spring.Validation.Config/spring-validation-1.1.xsd");
            AssemblyResource objectsSchema = new AssemblyResource("assembly://Spring.Core/Spring.Objects.Factory.Xml/spring-objects-1.1.xsd");

#if !NET_2_0
            XmlValidatingReader validatingReader = new XmlValidatingReader(xml, XmlNodeType.Document, null);
            validatingReader.ValidationType = ValidationType.Schema;
            validatingReader.Schemas.Add("http://www.springframework.net", new XmlTextReader(objectsSchema.InputStream));
            validatingReader.Schemas.Add("http://www.springframework.net/validation", new XmlTextReader(validationSchema.InputStream));
#else
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add("http://www.springframework.net", new XmlTextReader(objectsSchema.InputStream));
            settings.Schemas.Add("http://www.springframework.net/validation", new XmlTextReader(validationSchema.InputStream));
            settings.ValidationType = ValidationType.Schema;
            XmlReader validatingReader = XmlReader.Create(new StringReader(xml), settings);
#endif
            doc.Load(validatingReader);
            
            MockObjectDefinitionRegistry registry = new MockObjectDefinitionRegistry();
            IObjectDefinitionDocumentReader reader = new DefaultObjectDefinitionDocumentReader();

            XmlReaderContext readerContext = new XmlReaderContext(null, new XmlObjectDefinitionReader(registry));
            ObjectDefinitionParserHelper helper = new ObjectDefinitionParserHelper(readerContext);
            helper.InitDefaults(doc.DocumentElement);
            ParserContext parserContext = new ParserContext(helper.ReaderContext, helper);

            ValidationNamespaceParser parser = new ValidationNamespaceParser();
            foreach (XmlElement element in doc.DocumentElement.ChildNodes)
            {
                if (element.NamespaceURI == "http://www.springframework.net/validation")
                {
                    parser.ParseElement(element, parserContext);
                }
            }
            IObjectDefinition[] defs = registry.GetObjectDefinitions();
            Assert.AreEqual(2, defs.Length);

            IObjectDefinition def = registry.GetObjectDefinition("destinationAirportValidator");
            Assert.IsTrue(def.IsSingleton);
            Assert.IsTrue(def.IsLazyInit);
            Assert.IsTrue(typeof(IValidator).IsAssignableFrom(def.ObjectType));
            PropertyValue validatorsProperty = def.PropertyValues.GetPropertyValue("Validators");
            Assert.IsNotNull(validatorsProperty);
            object validatorsObject = validatorsProperty.Value;
            Assert.AreEqual(typeof(ManagedList), validatorsObject.GetType());
            ManagedList validators = (ManagedList) validatorsObject;
            Assert.AreEqual(4, validators.Count);

            def = (IObjectDefinition) validators[3];
            Assert.IsTrue(def.IsSingleton);
            Assert.IsTrue(def.IsLazyInit);
            Assert.AreEqual(typeof(RegularExpressionValidator), def.ObjectType);
            Assert.AreEqual("[A-Z]*", def.PropertyValues.GetPropertyValue("Expression").Value);

            def = registry.GetObjectDefinition("airportCodeValidator");
            Assert.IsTrue(def.IsSingleton);
            Assert.IsTrue(def.IsLazyInit);
            Assert.IsTrue(typeof(IValidator).IsAssignableFrom(def.ObjectType));
            PropertyValue actionsProperty = def.PropertyValues.GetPropertyValue("Actions");
            Assert.IsNotNull(actionsProperty);
            object actionsObject = actionsProperty.Value;
            Assert.AreEqual(typeof(ManagedList), actionsObject.GetType());
            ManagedList actions = (ManagedList) actionsObject;
            Assert.AreEqual(4, actions.Count);

            IObjectDefinition messageDefinition = (IObjectDefinition) actions[1];
            Assert.AreEqual(typeof(ErrorMessageAction), messageDefinition.ObjectType);

            IObjectDefinition actionDefinition = (IObjectDefinition) actions[2];
            Assert.AreEqual(typeof(ExpressionAction), actionDefinition.ObjectType);
            Assert.AreEqual("#now = DateTime.Now", actionDefinition.PropertyValues.GetPropertyValue("Valid").Value);
        }

        [Test]
        [ExpectedException(typeof(ObjectDefinitionStoreException))]
        public void WhenConfigFileIsNotValid()
        {
            const string xml = @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net' xmlns:v='http://www.springframework.net/validation'>
    <v:required test='#this'>
        <v:message id='error.airportCode.required' providers='summary'/>
        <v:action type='Spring.Validation.Actions.ExpressionAction, Spring.Core' when='true'/>
    </v:required>
</objects>
";
            XmlDocument doc = new XmlDocument();

            AssemblyResource validationSchema = new AssemblyResource("assembly://Spring.Core/Spring.Validation.Config/spring-validation-1.1.xsd");
            AssemblyResource objectsSchema = new AssemblyResource("assembly://Spring.Core/Spring.Objects.Factory.Xml/spring-objects-1.1.xsd");

#if !NET_2_0
            XmlValidatingReader validatingReader = new XmlValidatingReader(xml, XmlNodeType.Document, null);
            validatingReader.ValidationType = ValidationType.Schema;
            validatingReader.Schemas.Add("http://www.springframework.net", new XmlTextReader(objectsSchema.InputStream));
            validatingReader.Schemas.Add("http://www.springframework.net/validation", new XmlTextReader(validationSchema.InputStream));
#else
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Schemas.Add("http://www.springframework.net", new XmlTextReader(objectsSchema.InputStream));
            settings.Schemas.Add("http://www.springframework.net/validation", new XmlTextReader(validationSchema.InputStream));
            settings.ValidationType = ValidationType.Schema;
            XmlReader validatingReader = XmlReader.Create(new StringReader(xml), settings);
#endif
            doc.Load(validatingReader);

            MockObjectDefinitionRegistry registry = new MockObjectDefinitionRegistry();
            IObjectDefinitionDocumentReader reader = new DefaultObjectDefinitionDocumentReader();

            XmlReaderContext readerContext = new XmlReaderContext(null, new XmlObjectDefinitionReader(registry));
            ObjectDefinitionParserHelper helper = new ObjectDefinitionParserHelper(readerContext);
            helper.InitDefaults(doc.DocumentElement);
            ParserContext parserContext = new ParserContext(helper.ReaderContext, helper);

            ValidationNamespaceParser parser = new ValidationNamespaceParser();
            foreach (XmlElement element in doc.DocumentElement.ChildNodes)
            {
                if (element.NamespaceURI == "http://www.springframework.net/validation")
                {
                    parser.ParseElement(element, parserContext);
                }
            }
        }       
    }
}