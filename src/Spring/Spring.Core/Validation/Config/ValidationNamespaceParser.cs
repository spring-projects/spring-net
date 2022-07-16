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

using System.Xml;

using Spring.Core.TypeResolution;
using Spring.Expressions;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Util;

namespace Spring.Validation.Config
{
    /// <summary>
    /// Implementation of the custom configuration parser for validator definitions.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/validation",
            SchemaLocationAssemblyHint = typeof(ValidationNamespaceParser),
            SchemaLocation = "/Spring.Validation.Config/spring-validation-1.3.xsd")
    ]
    public sealed class ValidationNamespaceParser : ObjectsNamespaceParser
    {
        private const string ValidatorTypePrefix = "validator: ";

        [ThreadStatic]
        private int definitionCount = 0;

        static ValidationNamespaceParser()
        {
            TypeRegistry.RegisterType(ValidatorTypePrefix + "group", typeof(ValidatorGroup));
            TypeRegistry.RegisterType(ValidatorTypePrefix + "any", typeof(AnyValidatorGroup));
            TypeRegistry.RegisterType(ValidatorTypePrefix + "exclusive", typeof(ExclusiveValidatorGroup));
            TypeRegistry.RegisterType(ValidatorTypePrefix + "collection", typeof(CollectionValidator));
            TypeRegistry.RegisterType(ValidatorTypePrefix + "required", typeof(RequiredValidator));
            TypeRegistry.RegisterType(ValidatorTypePrefix + "condition", typeof(ConditionValidator));
            TypeRegistry.RegisterType(ValidatorTypePrefix + "regex", typeof(RegularExpressionValidator));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationNamespaceParser"/> class.
        /// </summary>
        public ValidationNamespaceParser()
        {
            // generate unique key for instance field to be stored in LogicalThreadContext
            string FIELDPREFIX = typeof(ValidationNamespaceParser).FullName + base.GetHashCode();
        }


        /// <summary>
        /// Parse the specified element and register any resulting
        /// IObjectDefinitions with the IObjectDefinitionRegistry that is
        /// embedded in the supplied ParserContext.
        /// </summary>
        /// <param name="element">The element to be parsed into one or more IObjectDefinitions</param>
        /// <param name="parserContext">The object encapsulating the current state of the parsing
        /// process.</param>
        /// <returns>
        /// The primary IObjectDefinition (can be null as explained above)
        /// </returns>
        /// <remarks>
        /// Implementations should return the primary IObjectDefinition
        /// that results from the parse phase if they wish to used nested
        /// inside (for example) a <code>&lt;property&gt;</code> tag.
        /// <para>Implementations may return null if they will not
        /// be used in a nested scenario.
        /// </para>
        /// </remarks>
        public override IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            if (!element.HasAttribute("id"))
            {
                throw new ObjectDefinitionStoreException(parserContext.ReaderContext.Resource, "validator", "Top-level validator element must have an 'id' attribute defined.");
            }
            this.definitionCount = 0;

            ParseAndRegisterValidator(element, parserContext);

            return null;
            //return definitionCount;
        }

        /// <summary>
        /// Parses the validator definition.
        /// </summary>
        /// <param name="id">Validator's identifier.</param>
        /// <param name="element">The element to parse.</param>
        /// <param name="parserContext">The parser helper.</param>
        /// <returns>Validator object definition.</returns>
        private IObjectDefinition ParseValidator(string id, XmlElement element, ParserContext parserContext)
        {
            string typeName = GetTypeName(element);
            string parent = GetAttributeValue(element, ObjectDefinitionConstants.ParentAttribute);

            string name = "validator: " + (StringUtils.HasText(id) ? id : this.definitionCount.ToString());

            MutablePropertyValues properties = new MutablePropertyValues();
            IConfigurableObjectDefinition od
                = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                    typeName, parent, parserContext.ReaderContext.Reader.Domain);

            od.PropertyValues = properties;
            od.IsSingleton = true;
            od.IsLazyInit = true;

            ParseAttributeIntoProperty(element, ValidatorDefinitionConstants.TestAttribute, properties, "Test");
            ParseAttributeIntoProperty(element, ValidatorDefinitionConstants.WhenAttribute, properties, "When");
            ParseAttributeIntoProperty(element, ValidatorDefinitionConstants.GroupFastValidateAttribute, properties, "FastValidate");
            ParseAttributeIntoProperty(element, ValidatorDefinitionConstants.RegexExpressionAttribute, properties, "Expression");
            ParseAttributeIntoProperty(element, ValidatorDefinitionConstants.CollectionValidateAllAttribute, properties, "ValidateAll");
            ParseAttributeIntoProperty(element, ValidatorDefinitionConstants.CollectionContextAttribute, properties, "Context");
            ParseAttributeIntoProperty(element, ValidatorDefinitionConstants.CollectionIncludeElementsErrors, properties, "IncludeElementErrors");

// TODO: (EE) - is this a mistake to check 'validateAll' but add 'context' then?
//            if (StringUtils.HasText(validateAll))
//            {
//                properties.Add("Context", context);
//            }

            ManagedList nestedValidators = new ManagedList();
            ManagedList actions = new ManagedList();
            ParserContext childParserContext = new ParserContext(parserContext.ParserHelper, od);
            foreach (XmlNode node in element.ChildNodes)
            {
                XmlElement child = node as XmlElement;
                if (child != null)
                {
                    switch (child.LocalName)
                    {
                        case ValidatorDefinitionConstants.PropertyElement:
                            string propertyName = GetAttributeValue(child, ValidatorDefinitionConstants.PropertyNameAttribute);
                            properties.Add(propertyName, base.ParsePropertyValue(child, name, childParserContext));
                            break;
                        case ValidatorDefinitionConstants.MessageElement:
                            actions.Add(ParseErrorMessageAction(child, childParserContext));
                            break;
                        case ValidatorDefinitionConstants.ActionElement:
                            actions.Add(ParseGenericAction(child, childParserContext));
                            break;
                        case ValidatorDefinitionConstants.ExceptionElement:
                            actions.Add(ParseExceptionAction(child, childParserContext));
                            break;
                        case ValidatorDefinitionConstants.ReferenceElement:
                            nestedValidators.Add(ParseValidatorReference(child, childParserContext));
                            break;
                        default:
                            nestedValidators.Add(ParseAndRegisterValidator(child, childParserContext));
                            break;
                    }
                }
            }
            if (nestedValidators.Count > 0)
            {
                properties.Add("Validators", nestedValidators);
            }
            if (actions.Count > 0)
            {
                properties.Add("Actions", actions);
            }

            return od;
        }

        /// <summary>
        /// Parses the attribute of the given <paramref name="attName"/> from the XmlElement and, if available, adds a property of the given <paramref name="propName"/> with
        /// the parsed value.
        /// </summary>
        private void ParseAttributeIntoProperty(XmlElement element, string attName, MutablePropertyValues properties, string propName)
        {
            string test = GetAttributeValue(element, attName);
            if (StringUtils.HasText(test))
            {
                properties.Add(propName, test);
            }
        }

        /// <summary>
        /// Parses and potentially registers a validator.
        /// </summary>
        /// <remarks>
        /// Only validators that have <code>id</code> attribute specified are registered
        /// as separate object definitions within application context.
        /// </remarks>
        /// <param name="element">Validator XML element.</param>
        /// <param name="parserContext">The parser helper.</param>
        /// <returns>Validator object definition.</returns>
        private IObjectDefinition ParseAndRegisterValidator(XmlElement element, ParserContext parserContext)
        {
            string id = GetAttributeValue(element, ObjectDefinitionConstants.IdAttribute);
            IObjectDefinition validator = ParseValidator(id, element, parserContext);
            if (StringUtils.HasText(id))
            {
                parserContext.ReaderContext.Registry.RegisterObjectDefinition(id, validator);
                this.definitionCount++;
            }
            return validator;
        }

        /// <summary>
        /// Gets the name of the object type for the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The name of the object type.</returns>
        private string GetTypeName(XmlElement element)
        {
            string typeName = GetAttributeValue(element, ObjectDefinitionConstants.TypeAttribute);
            if (StringUtils.IsNullOrEmpty(typeName))
            {
                return ValidatorTypePrefix + element.LocalName;
            }
            return typeName;
        }

        /// <summary>
        /// Creates an error message action based on the specified message element.
        /// </summary>
        /// <param name="message">The message element.</param>
        /// <param name="parserContext">The parser helper.</param>
        /// <returns>The error message action definition.</returns>
        private static IObjectDefinition ParseErrorMessageAction(XmlElement message, ParserContext parserContext)
        {
            string messageId = GetAttributeValue(message, MessageConstants.IdAttribute);
            string[] providers = GetAttributeValue(message, MessageConstants.ProvidersAttribute).Split(',');
            List<IExpression> parameters = new List<IExpression>();

            foreach (XmlElement param in message.ChildNodes)
            {
                IExpression paramExpression = Expression.Parse(GetAttributeValue(param, MessageConstants.ParameterValueAttribute));
                parameters.Add(paramExpression);
            }

            string typeName = "Spring.Validation.Actions.ErrorMessageAction, Spring.Core";
            ConstructorArgumentValues ctorArgs = new ConstructorArgumentValues();
            ctorArgs.AddGenericArgumentValue(messageId);
            ctorArgs.AddGenericArgumentValue(providers);

            string when = GetAttributeValue(message, ValidatorDefinitionConstants.WhenAttribute);
            MutablePropertyValues properties = new MutablePropertyValues();
            if (StringUtils.HasText(when))
            {
                properties.Add("When", when);
            }
            if (parameters.Count > 0)
            {
                properties.Add("Parameters", parameters.ToArray());
            }

            IConfigurableObjectDefinition action =
                parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(typeName, null, parserContext.ReaderContext.Reader.Domain);
            action.ConstructorArgumentValues = ctorArgs;
            action.PropertyValues = properties;

            return action;
        }

        private IObjectDefinition ParseExceptionAction(XmlElement element, ParserContext parserContext)
        {
            string typeName = "Spring.Validation.Actions.ExceptionAction, Spring.Core";
            string throwExpression = GetAttributeValue(element, ValidatorDefinitionConstants.ThrowAttribute);


            ConstructorArgumentValues ctorArgs = new ConstructorArgumentValues();
            ctorArgs.AddGenericArgumentValue(throwExpression);

            string when = GetAttributeValue(element, ValidatorDefinitionConstants.WhenAttribute);
            MutablePropertyValues properties = new MutablePropertyValues();
            if (StringUtils.HasText(when))
            {
                properties.Add("When", when);
            }

            IConfigurableObjectDefinition action =
                parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(typeName, null, parserContext.ReaderContext.Reader.Domain);
            action.ConstructorArgumentValues = ctorArgs;
            action.PropertyValues = properties;

            return action;
        }

        /// <summary>
        /// Creates a generic action based on the specified element.
        /// </summary>
        /// <param name="element">The action definition element.</param>
        /// <param name="parserContext">The parser helper.</param>
        /// <returns>Generic validation action definition.</returns>
        private IObjectDefinition ParseGenericAction(XmlElement element, ParserContext parserContext)
        {
            string typeName = GetAttributeValue(element, ObjectDefinitionConstants.TypeAttribute);
            string when = GetAttributeValue(element, ValidatorDefinitionConstants.WhenAttribute);
            MutablePropertyValues properties = base.ParsePropertyElements("validator:action", element, parserContext);
            if (StringUtils.HasText(when))
            {
                properties.Add("When", when);
            }

            IConfigurableObjectDefinition action =
                parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(typeName, null, parserContext.ReaderContext.Reader.Domain);
            action.PropertyValues = properties;

            return action;
        }

        /// <summary>
        /// Creates object definition for the validator reference.
        /// </summary>
        /// <param name="element">The action definition element.</param>
        /// <param name="parserContext">The parser helper.</param>
        /// <returns>Generic validation action definition.</returns>
        private IObjectDefinition ParseValidatorReference(XmlElement element, ParserContext parserContext)
        {
            string typeName = "Spring.Validation.ValidatorReference, Spring.Core";
            string name = GetAttributeValue(element, ValidatorDefinitionConstants.ReferenceNameAttribute);
            string context = GetAttributeValue(element, ValidatorDefinitionConstants.ReferenceContextAttribute);
            string when = GetAttributeValue(element, ValidatorDefinitionConstants.WhenAttribute);

            MutablePropertyValues properties = new MutablePropertyValues();
            properties.Add("Name", name);
            if (StringUtils.HasText(context))
            {
                properties.Add("Context", context);
            }
            if (StringUtils.HasText(when))
            {
                properties.Add("When", when);
            }

            IConfigurableObjectDefinition reference =
                parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(typeName, null, parserContext.ReaderContext.Reader.Domain);
            reference.PropertyValues = properties;
            return reference;
        }

        #region Element & Attribute Name Constants

        private class ValidatorDefinitionConstants
        {
            public const string PropertyElement = "property";
            public const string MessageElement = "message";
            public const string ActionElement = "action";
            public const string ExceptionElement = "exception";
            public const string ReferenceElement = "ref";

            public const string TypeAttribute = "type";
            public const string TestAttribute = "test";
            public const string NameAttribute = "name";
            public const string WhenAttribute = "when";
            public const string ThrowAttribute = "throw";

            public const string PropertyNameAttribute = "name";

            public const string ReferenceNameAttribute = "name";
            public const string ReferenceContextAttribute = "context";

            public const string RegexExpressionAttribute = "expression";

            public const string GroupFastValidateAttribute = "fast-validate";

            public const string CollectionValidateAllAttribute = "validate-all";
            public const string CollectionContextAttribute = "context";
            public const string CollectionIncludeElementsErrors = "include-element-errors";
        }

        private class MessageConstants
        {
            public const string ParamElement = "param";

            public const string IdAttribute = "id";
            public const string ProvidersAttribute = "providers";
            public const string ParameterValueAttribute = "value";
        }

        #endregion
    }
}
