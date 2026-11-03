/*
 * Copyright � 2002-2011 the original author or authors.
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

using System.Xml;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Xml;
using Spring.Validation.Actions;
using Spring.Validation.Config;

namespace Spring.Validation;

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
        Assert.That(defs.Count, Is.EqualTo(9));

        IObjectDefinition def = registry.GetObjectDefinition("destinationAirportValidator");
        Assert.That(def.IsSingleton, Is.True);
        Assert.That(def.IsLazyInit, Is.True);
        Assert.That(typeof(IValidator).IsAssignableFrom(def.ObjectType), Is.True);

        PropertyValue fastValidateProperty = def.PropertyValues.GetPropertyValue("FastValidate");
        Assert.That(fastValidateProperty, Is.Not.Null);
        Assert.That(fastValidateProperty.Value, Is.EqualTo("true"));

        PropertyValue validatorsProperty = def.PropertyValues.GetPropertyValue("Validators");
        Assert.That(validatorsProperty, Is.Not.Null);
        object validatorsObject = validatorsProperty.Value;
        Assert.That(validatorsObject.GetType(), Is.EqualTo(typeof(ManagedList)));
        ManagedList validators = (ManagedList) validatorsObject;
        Assert.That(validators.Count, Is.EqualTo(4));

        def = (IObjectDefinition) validators[3];
        Assert.That(def.IsSingleton, Is.True);
        Assert.That(def.IsLazyInit, Is.True);
        Assert.That(def.ObjectType, Is.EqualTo(typeof(RegularExpressionValidator)));
        Assert.That(def.PropertyValues.GetPropertyValue("Expression").Value, Is.EqualTo("[A-Z]*"));

        def = registry.GetObjectDefinition("simpleAirportValidator");
        Assert.That(def.IsSingleton, Is.True);
        Assert.That(def.IsLazyInit, Is.True);
        Assert.That(typeof(IValidator).IsAssignableFrom(def.ObjectType), Is.True);
        PropertyValue actionsProperty = def.PropertyValues.GetPropertyValue("Actions");
        Assert.That(actionsProperty, Is.Not.Null);
        object actionsObject = actionsProperty.Value;
        Assert.That(actionsObject.GetType(), Is.EqualTo(typeof(ManagedList)));
        ManagedList actions = (ManagedList) actionsObject;
        Assert.That(actions.Count, Is.EqualTo(1));

        IObjectDefinition exceptionActionDefinition = (IObjectDefinition) actions[0];
        Assert.That(exceptionActionDefinition.ObjectType, Is.EqualTo(typeof(ExceptionAction)));
        Assert.That(exceptionActionDefinition.ConstructorArgumentValues.GenericArgumentValues[0].ToString(), Is.EqualTo("'new System.InvalidOperationException(\"invalid\")' []"));
        //

        def = registry.GetObjectDefinition("airportCodeValidator");
        Assert.That(def.IsSingleton, Is.True);
        Assert.That(def.IsLazyInit, Is.True);
        Assert.That(typeof(IValidator).IsAssignableFrom(def.ObjectType), Is.True);
        actionsProperty = def.PropertyValues.GetPropertyValue("Actions");
        Assert.That(actionsProperty, Is.Not.Null);
        actionsObject = actionsProperty.Value;
        Assert.That(actionsObject.GetType(), Is.EqualTo(typeof(ManagedList)));
        actions = (ManagedList) actionsObject;
        Assert.That(actions.Count, Is.EqualTo(4));

        IObjectDefinition messageDefinition = (IObjectDefinition) actions[1];
        Assert.That(messageDefinition.ObjectType, Is.EqualTo(typeof(ErrorMessageAction)));

        IObjectDefinition actionDefinition = (IObjectDefinition) actions[2];
        Assert.That(actionDefinition.ObjectType, Is.EqualTo(typeof(ExpressionAction)));
        Assert.That(actionDefinition.PropertyValues.GetPropertyValue("Valid").Value, Is.EqualTo("#now = DateTime.Now"));

        def = registry.GetObjectDefinition("regex");
        Assert.That(def.IsSingleton, Is.True);
        Assert.That(def.IsLazyInit, Is.True);
        Assert.That(typeof(IValidator).IsAssignableFrom(def.ObjectType), Is.True);
        PropertyValue expressionProperty = def.PropertyValues.GetPropertyValue("Expression");
        Assert.That(expressionProperty, Is.Not.Null);
        Assert.That(expressionProperty.Value, Is.EqualTo("RegExp"));
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
