#region License

/*
 * Copyright 2002-2004 the original author or authors.
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

using System.Collections;
using System.Xml;

using Spring.Core.TypeResolution;
using Spring.Context.Support;
using Spring.Expressions;
using Spring.Objects;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Objects.Factory.Xml;
using Spring.Threading;
using Spring.Util;

#endregion

namespace Spring.Validation.Config
{
    /// <summary>
    /// Implementation of the custom configuration parser for validator definitions.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    /// <version>$Id: ValidationNamespaceParser.cs,v 1.2 2007/08/08 00:34:17 bbaia Exp $</version>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net/validation", 
            SchemaLocationAssemblyHint = typeof(ValidationNamespaceParser),
            SchemaLocation = "/Spring.Validation.Config/spring-validation-1.1.xsd")
    ]
    public sealed class ValidationNamespaceParser : ObjectsNamespaceParser
    {
        private const string ValidatorTypePrefix = "validator: ";

//        [ThreadStatic]
//        private int definitionCount = 0;
        private readonly string key_DefinitionCount;
        private int definitionCount
        {
            get
            {
                object tmp = LogicalThreadContext.GetData(key_DefinitionCount);
                if (tmp != null) return (int)tmp;
                LogicalThreadContext.SetData(key_DefinitionCount, 0);
                return 0;
            }
            set
            {
                LogicalThreadContext.SetData(key_DefinitionCount, value);
            }
        }
    	
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
            key_DefinitionCount = FIELDPREFIX + ".definitionCount";
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

            //TODO pass down parserContext...
            ParseAndRegisterValidator(element, parserContext.ParserHelper);

            return null;
            //return definitionCount;
        }

        /// <summary>
        /// Parses the validator definition.
        /// </summary>
        /// <param name="id">Validator's identifier.</param>
        /// <param name="element">The element to parse.</param>
        /// <param name="parserHelper">The parser helper.</param>
        /// <returns>Validator object definition.</returns>
        private IObjectDefinition ParseValidator(string id, XmlElement element, ObjectDefinitionParserHelper parserHelper)
        {
            string typeName = GetTypeName(element);
            string parent = element.GetAttribute(ObjectDefinitionConstants.ParentAttribute);
            string test = element.GetAttribute(ValidatorDefinitionConstants.TestAttribute);
            string when = element.GetAttribute(ValidatorDefinitionConstants.WhenAttribute);
            string validateAll = element.GetAttribute(ValidatorDefinitionConstants.CollectionValidateAllAttribute);
            string context = element.GetAttribute(ValidatorDefinitionConstants.CollectionContextAttribute);
            string includeElementsErrors = element.GetAttribute(ValidatorDefinitionConstants.CollectionIncludeElementsErrors);
            
            string name = "validator: " + (StringUtils.HasText(id) ? id : this.definitionCount.ToString());
            MutablePropertyValues properties = new MutablePropertyValues();
            if (StringUtils.HasText(test))
            {
                properties.Add("Test", test);
            }
            if (StringUtils.HasText(when))
            {
                properties.Add("When", when);
            }
            if (StringUtils.HasText(validateAll))
            {
                properties.Add("ValidateAll", validateAll);
            }
            if (StringUtils.HasText(validateAll))
            {
                properties.Add("Context", context);
            }
            if (StringUtils.HasText(includeElementsErrors))
            {
                properties.Add("IncludeElementErrors", includeElementsErrors);
            }
            

            ManagedList nestedValidators = new ManagedList();
            ManagedList actions = new ManagedList();
            foreach (XmlNode node in element.ChildNodes) 
            {
                XmlElement child = node as XmlElement;
                if (child != null)
                {
                    switch (child.LocalName)
                    {
                        case ValidatorDefinitionConstants.PropertyElement:
                            string propertyName = child.GetAttribute(ValidatorDefinitionConstants.PropertyNameAttribute);
                            properties.Add(propertyName, base.GetPropertyValue(child, name, parserHelper));
                            break;
                        case ValidatorDefinitionConstants.MessageElement:
                            actions.Add(ParseErrorMessageAction(child, parserHelper));
                            break;
                        case ValidatorDefinitionConstants.ActionElement:
                            actions.Add(ParseGenericAction(child, parserHelper));
                            break;
                        case ValidatorDefinitionConstants.ReferenceElement:
                            nestedValidators.Add(ParseValidatorReference(child, parserHelper));
                            break;
                        default:
                            nestedValidators.Add(ParseAndRegisterValidator(child, parserHelper));
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

            IConfigurableObjectDefinition od
                = parserHelper.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                    typeName, parent, parserHelper.ReaderContext.Reader.Domain);

            od.PropertyValues = properties;
            od.IsSingleton = true;
            od.IsLazyInit = true;

            return od;
        }

        /// <summary>
        /// Parses and potentially registers a validator.
        /// </summary>
        /// <remarks>
        /// Only validators that have <code>id</code> attribute specified are registered 
        /// as separate object definitions within application context.
        /// </remarks>
        /// <param name="element">Validator XML element.</param>
        /// <param name="parserHelper">The parser helper.</param>
        /// <returns>Validator object definition.</returns>
        private IObjectDefinition ParseAndRegisterValidator(XmlElement element, ObjectDefinitionParserHelper parserHelper)
        {
            string id = element.GetAttribute(ObjectDefinitionConstants.IdAttribute);
            IObjectDefinition validator = ParseValidator(id, element, parserHelper);
            if (StringUtils.HasText(id))
            {
                parserHelper.ReaderContext.Registry.RegisterObjectDefinition(id, validator);
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
            string typeName = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
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
        /// <param name="parserHelper">The parser helper.</param>
        /// <returns>The error message action definition.</returns>
        private static IObjectDefinition ParseErrorMessageAction(XmlElement message, ObjectDefinitionParserHelper parserHelper)
        {
            string messageId = message.GetAttribute(MessageConstants.IdAttribute);                        
            string[] providers = message.GetAttribute(MessageConstants.ProvidersAttribute).Split(',');
            ArrayList parameters = new ArrayList();

            foreach (XmlElement param in message.ChildNodes)
            {
                IExpression paramExpression = Expression.Parse(param.GetAttribute(MessageConstants.ParameterValueAttribute));
                parameters.Add(paramExpression);
            }

            string typeName = "Spring.Validation.Actions.ErrorMessageAction, Spring.Core";
            ConstructorArgumentValues ctorArgs = new ConstructorArgumentValues();
            ctorArgs.AddGenericArgumentValue(messageId);
            ctorArgs.AddGenericArgumentValue(providers);

            string when = message.GetAttribute(ValidatorDefinitionConstants.WhenAttribute);
            MutablePropertyValues properties = new MutablePropertyValues();
            if (StringUtils.HasText(when))
            {
                properties.Add("When", when);
            }
            if (parameters.Count > 0)
            {
                properties.Add("Parameters", parameters.ToArray(typeof(IExpression)));
            }
            
            IConfigurableObjectDefinition action =
                parserHelper.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(typeName, null, parserHelper.ReaderContext.Reader.Domain);
            action.ConstructorArgumentValues = ctorArgs;
            action.PropertyValues = properties;

            return action;
        }

        /// <summary>
        /// Creates a generic action based on the specified element.
        /// </summary>
        /// <param name="element">The action definition element.</param>
        /// <param name="parserHelper">The parser helper.</param>
        /// <returns>Generic validation action definition.</returns>
        private IObjectDefinition ParseGenericAction(XmlElement element, ObjectDefinitionParserHelper parserHelper)
        {
            string typeName = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
            string when = element.GetAttribute(ValidatorDefinitionConstants.WhenAttribute);
            MutablePropertyValues properties = base.GetPropertyValueSubElements("validator:action", element, parserHelper);
            if (StringUtils.HasText(when))
            {
                properties.Add("When", when);
            }

            IConfigurableObjectDefinition action =
                parserHelper.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(typeName, null, parserHelper.ReaderContext.Reader.Domain);
            action.PropertyValues = properties;

            return action;
        }

        /// <summary>
        /// Creates object definition for the validator reference.
        /// </summary>
        /// <param name="element">The action definition element.</param>
        /// <param name="parserHelper">The parser helper.</param>
        /// <returns>Generic validation action definition.</returns>
        private IObjectDefinition ParseValidatorReference(XmlElement element, ObjectDefinitionParserHelper parserHelper)
        {
            string typeName = "Spring.Validation.ValidatorReference, Spring.Core";
            string name = element.GetAttribute(ValidatorDefinitionConstants.ReferenceNameAttribute);
            string context = element.GetAttribute(ValidatorDefinitionConstants.ReferenceContextAttribute);
            
            MutablePropertyValues properties = new MutablePropertyValues();
            properties.Add("Name", name);
            if (StringUtils.HasText(context))
            {
                properties.Add("Context", context);
            }

            IConfigurableObjectDefinition reference =
                parserHelper.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(typeName, null, parserHelper.ReaderContext.Reader.Domain);
            reference.PropertyValues = properties;
            return reference;
        }
        
        #region Element & Attribute Name Constants

        private class ValidatorDefinitionConstants
        {
            public const string PropertyElement = "property";
            public const string MessageElement = "message";
            public const string ActionElement = "action";
            public const string ReferenceElement = "ref";

            public const string TypeAttribute = "type";
            public const string TestAttribute = "test";
            public const string NameAttribute = "name";
            public const string WhenAttribute = "when";

            public const string PropertyNameAttribute = "name";

            public const string ReferenceNameAttribute = "name";
            public const string ReferenceContextAttribute = "context";

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