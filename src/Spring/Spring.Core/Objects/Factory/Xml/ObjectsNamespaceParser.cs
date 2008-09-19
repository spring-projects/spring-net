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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using Common.Logging;

using Spring.Collections;
using Spring.Core;
using Spring.Core.IO;
using Spring.Core.TypeResolution;
using Spring.Objects.Factory.Config;
using Spring.Objects.Factory.Support;
using Spring.Util;

#endregion

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Default implementation of the
    /// <see cref="INamespaceParser"/> interface.
    /// </summary>
    /// <remarks>
    /// <p>
    /// Parses object definitions according to the standard Spring.NET schema.
    /// </p>
    /// <p>
    /// This schema is <b>typically</b> located at
    /// <a href="http://www.springframework.net/xsd/spring-objects.xsd">http://www.springframework.net/xsd/spring-objects.xsd</a>.
    /// </p>
    /// </remarks>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans (.NET)</author>
    [
        NamespaceParser(
            Namespace = "http://www.springframework.net",
			SchemaLocationAssemblyHint = typeof(ObjectsNamespaceParser),
			SchemaLocation = "/Spring.Objects.Factory.Xml/spring-objects-1.1.xsd"
        )			
    ]
    public class ObjectsNamespaceParser : INamespaceParser
    {
        /// <summary>
        /// The namespace URI for the standard Spring.NET object definition schema.
        /// </summary>
        public const string Namespace = "http://www.springframework.net";

        /// <summary>
        /// The shared <see cref="Common.Logging.ILog"/> instance for this class (and derived classes).
        /// </summary>
        protected static readonly ILog log =
            LogManager.GetLogger(typeof(ObjectsNamespaceParser));

        #region IXmlObjectDefinitionParser Members

        /// <summary>
        /// Invoked by <see cref="NamespaceParserRegistry"/> after construction but before any
        /// elements have been parsed.
        /// </summary>
        /// <remarks>This is a NoOp</remarks>
        public void Init()
        {
            
        }

        #endregion


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
        public virtual IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            
            if (element.LocalName == ObjectDefinitionConstants.ImportElement)
            {
                ImportObjectDefinitionResource(element, parserContext);
            }
            else if (element.LocalName == ObjectDefinitionConstants.AliasElement)
            {
                ParseAlias(element, parserContext.ReaderContext.Registry);
            }
            else if (element.LocalName == ObjectDefinitionConstants.ObjectElement)
            {
                RegisterObjectDefinition(element, parserContext);
            }
                        
            return null;
        }


        /// <summary>
        /// Parse the specified XmlNode and decorate the supplied ObjectDefinitionHolder,
        /// returning the decorated definition.
        /// </summary>
        /// <remarks>The XmlNode may either be an XmlAttribute or an XmlElement, depending on
        /// whether a custom attribute or element is being parsed.
        /// <para>Implementations may choose to return a completely new definition,
        /// which will replace the original definition in the resulting IApplicationContext/IObjectFactory.
        /// </para>
        /// <para>The supplied ParserContext can be used to register any additional objects needed to support
        /// the main definition.</para>
        /// </remarks>
        /// <param name="node">The source element or attribute that is to be parsed.</param>
        /// <param name="definition">The current object definition.</param>
        /// <param name="parserContext">The object encapsulating the current state of the parsing
        /// process.</param>
        /// <returns>The decorated definition (to be registered in the IApplicationContext/IObjectFactory),
        /// or simply the original object definition if no decoration is required.  A null value is strickly
        /// speaking invalid, but will leniently treated like the case where the original object definition
        /// gets returned.</returns>
        public ObjectDefinitionHolder Decorate(XmlNode node, ObjectDefinitionHolder definition,
                                               ParserContext parserContext)
        {
            return null;
        }

        private void ParseAlias(XmlElement aliasElement, IObjectDefinitionRegistry registry)
        {
            string name = aliasElement.GetAttribute(ObjectDefinitionConstants.NameAttribute);
            string alias = aliasElement.GetAttribute(ObjectDefinitionConstants.AliasAttribute);            
            registry.RegisterAlias(name, alias);
        }
         

        

        /// <summary>
        /// Loads external XML object definitions from the resource described by the supplied
        /// <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The XML element describing the resource.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <exception cref="Spring.Objects.Factory.ObjectDefinitionStoreException">
        /// If the resource could not be imported.
        /// </exception>
        protected virtual void ImportObjectDefinitionResource(XmlElement resource, ParserContext parserContext)
        {
            string location = resource.GetAttribute(ObjectDefinitionConstants.ImportResourceAttribute);
            try
            {
                #region Instrumentation

                if (log.IsDebugEnabled)
                {
                    log.Debug(string.Format(
                        CultureInfo.InvariantCulture,
                        "Attempting to import object definitions from '{0}'.", location));
                }

                #endregion

                IResource importResource = parserContext.ReaderContext.Resource.CreateRelative(location);
                parserContext.ReaderContext.Reader.LoadObjectDefinitions(importResource);
            }
            catch (IOException ex)
            {
                parserContext.ReaderContext.ReportException(resource, null, string.Format(
                    CultureInfo.InvariantCulture,
                    "Invalid relative resource location '{0}' to import object definitions from.",
                    location), ex);
            }
        }
        

        /// <summary>Parses an event listener definition.</summary>
        /// <param name="name">
        /// The name associated with the object that the event handler is being defined on.
        /// </param>
        /// <param name="events">The events being populated.</param>
        /// <param name="element">
        /// The element containing the event listener definition.
        /// </param>
        /// <param name="parserContext">
        /// The namespace-aware parser.
        /// </param>
        protected virtual void ParseEventListenerDefinition(
            string name, EventValues events, XmlElement element, ParserContext parserContext)
        {
            // get an appropriate IEventHandlerValue instance based upon the
            // attribute values of the listener element...
            IEventHandlerValue myHandler = ObjectDefinitionReaderUtils.CreateEventHandlerValue(
                element.GetAttribute(ObjectDefinitionConstants.ListenerMethodAttribute),
                element.GetAttribute(ObjectDefinitionConstants.ListenerEventAttribute));

            // and then get the source of the event (another managed object instance
            // or a Type reference (i.e. a static event exposed on a class)...
            XmlElement sourceElement = this.SelectSingleNode(element, ObjectDefinitionConstants.RefElement) as XmlElement;

            XmlAttribute sourceAtt = sourceElement.Attributes[0];
            if (StringUtils.IsNullOrEmpty(sourceAtt.Value))
            {
                parserContext.ReaderContext.ReportFatalException(sourceElement, string.Format(
                    CultureInfo.InvariantCulture,
                    "The single attribute of the <{0}/> element cannot be empty. Specify the " +
                        "object id (alias) or the full, assembly qualified Type name that is the " +
                        "source of the event.",
                    ObjectDefinitionConstants.RefElement));
                return;
            }
            switch (sourceAtt.LocalName)
            {
                case ObjectDefinitionConstants.LocalRefAttribute:
                case ObjectDefinitionConstants.ObjectRefAttribute:
                    // we're wiring up to an event exposed on another managed object (instance)
                    RuntimeObjectReference ror = new RuntimeObjectReference(sourceAtt.Value);
                    myHandler.Source = ror;
                    break;
                case ObjectDefinitionConstants.TypeAttribute:
                    // we're wiring up to a static event exposed on a Type (class)
                    myHandler.Source = parserContext.ReaderContext.Reader.Domain == null ?
                        (object) sourceAtt.Value :
                        (object)TypeResolutionUtils.ResolveType(sourceAtt.Value);
                    break;
            }
            events.AddHandler(myHandler);
        }

        

        /// <summary>
        /// Parse an object definition and register it with the object factory..
        /// </summary>
        /// <param name="element">The element containing the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <seealso cref="Spring.Objects.Factory.Support.ObjectDefinitionReaderUtils.RegisterObjectDefinition"/>
        protected void RegisterObjectDefinition(XmlElement element, ParserContext parserContext)
        {
            ObjectDefinitionHolder holder = null;
            try
            {
                holder = ParseObjectDefinitionElement(element, parserContext, false);
                if (holder == null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
              throw new ObjectDefinitionStoreException(string.Format("Failed parsing object definition '{0}'", element.OuterXml), ex);
            }


            holder = parserContext.ParserHelper.DecorateObjectDefinitionIfRequired(element, holder);

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                log.Debug(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Registering object definition with id '{0}'.", holder.ObjectName));
            }

            #endregion

            ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, parserContext.ReaderContext.Registry);
        }


        /// <summary>
        /// Parse a standard object definition into a
        /// <see cref="Spring.Objects.Factory.Config.ObjectDefinitionHolder"/>,
        /// including object name and aliases.
        /// </summary>
        /// <param name="element">The element containing the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <param name="nestedDefinition">if set to <c>true</c> if we are processing an inner
        /// object definition.</param>
        /// <returns>
        /// The object (definition) wrapped within an
        /// <see cref="Spring.Objects.Factory.Config.ObjectDefinitionHolder"/>
        /// instance.
        /// </returns>
        /// <remarks>
        /// 	<p>
        /// Object elements specify their canonical name via the "id" attribute
        /// and their aliases as a delimited "name" attribute.
        /// </p>
        /// 	<p>
        /// If no "id" is specified, uses the first name in the "name" attribute
        /// as the canonical name, registering all others as aliases.
        /// </p>
        /// </remarks>
        protected ObjectDefinitionHolder ParseObjectDefinitionElement(XmlElement element, ParserContext parserContext, bool nestedDefinition)
        {
            string id = element.GetAttribute(ObjectDefinitionConstants.IdAttribute);
            string nameAttr = element.GetAttribute(ObjectDefinitionConstants.NameAttribute);
            ArrayList aliases = new ArrayList();
            if (StringUtils.HasText(nameAttr))
            {
                aliases.AddRange(GetObjectNames(nameAttr));
            }

            // if we ain't got an id, check if object is page definition or assign any existing (first) alias...
            string objectName = id;
            if (StringUtils.IsNullOrEmpty(objectName))
            {
                objectName = CalculateId(element, aliases);
            }
            

            IConfigurableObjectDefinition definition = ParseObjectDefinitionElement(element, objectName, parserContext);
            if (definition != null)
            {
                if (StringUtils.IsNullOrEmpty(objectName))
                {
                    if (nestedDefinition)
                    {
                        objectName =
                            ObjectDefinitionReaderUtils.GenerateObjectName(definition, parserContext.Registry, true);
                    }
                    else
                    {
                        objectName = ObjectDefinitionReaderUtils.GenerateObjectName(definition, parserContext.Registry);
                    }

                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(string.Format(
                                      "Neither XML '{0}' nor '{1}' specified - using object " +
                                      "class name [{2}] as the id.",
                                      id, ObjectDefinitionConstants.IdAttribute, ObjectDefinitionConstants.NameAttribute));
                    }

                    #endregion
                }
                string[] aliasesArray = (string[]) aliases.ToArray(typeof (string));
                return new ObjectDefinitionHolder(definition, objectName, aliasesArray);
            }
            return null;
        }

        /// <summary>
        /// Calculates an id for an object definition.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Called when an object definition has not been explicitly defined
        /// with an id.
        /// </p>
        /// </remarks>
        /// <param name="element">
        /// The element containing the object definition.
        /// </param>
        /// <param name="aliases">
        /// The list of names defined for the object; may be <see lang="null"/>
        /// or even empty.
        /// </param>
        /// <returns>
        /// A calculated object definition id.
        /// </returns>
        protected virtual string CalculateId(XmlElement element, ArrayList aliases)
        {
            string id = null;
            if (aliases.Count > 0)
            {
                string firstAlias = aliases[0] as string;
                aliases.RemoveAt(0);
                id = firstAlias;
            }

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                StringBuilder buffer = new StringBuilder();
                foreach (string alias in aliases)
                {
                    buffer.Append(alias).Append(",");
                }
                log.Debug(string.Format("No XML 'id' specified - using '{0}' as the id and '{1}' as aliases.",
                                        id, buffer.ToString()));
            }

            #endregion

            return id;
        }

        /// <summary>
        /// Parse a standard object definition.
        /// </summary>
        /// <param name="element">The element containing the object definition.</param>
        /// <param name="id">The id of the object definition.</param>
        /// <param name="parserContext">parsing state holder</param>
        /// <returns>The object (definition).</returns>
        protected virtual IConfigurableObjectDefinition ParseObjectDefinitionElement(
            XmlElement element, string id, ParserContext parserContext)
        {
            string typeName = null;
            try
            {
                if (element.HasAttribute(ObjectDefinitionConstants.TypeAttribute))
                {
                    typeName = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
                    if (StringUtils.IsNullOrEmpty(typeName))
                    {
                        throw new ObjectDefinitionStoreException(
                            parserContext.ReaderContext.Resource, id,
                            "The 'type' attribute does not need to be present, but if it is it must not be empty: got '" + typeName + "'.");
                    }
                }
                string parent = element.GetAttribute(ObjectDefinitionConstants.ParentAttribute);


                AbstractObjectDefinition od
                    = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                        typeName, parent, parserContext.ReaderContext.Reader.Domain);


                MutablePropertyValues pvs = ParsePropertyElements(id, element, parserContext);
                ConstructorArgumentValues arguments
                    = ParseConstructorArgSubElements(id, element, parserContext);
                EventValues events = ParseEventHandlerSubElements(id, element, parserContext);
                MethodOverrides methodOverrides = ParseMethodOverrideSubElements(id, element, parserContext);

                bool isPage = StringUtils.HasText(typeName) && typeName!= null && typeName.ToLower().EndsWith(".aspx");
                if (!isPage)
                {
                    od.ConstructorArgumentValues = arguments;
                }

                od.PropertyValues = pvs;
                od.MethodOverrides = methodOverrides;
                od.EventHandlerValues = events;
                if (element.HasAttribute(ObjectDefinitionConstants.DependsOnAttribute))
                {
                    string dependsOn = element.GetAttribute(ObjectDefinitionConstants.DependsOnAttribute);
                    od.DependsOn = GetObjectNames(dependsOn);
                }
                od.FactoryMethodName = element.GetAttribute(ObjectDefinitionConstants.FactoryMethodAttribute);
                od.FactoryObjectName = element.GetAttribute(ObjectDefinitionConstants.FactoryObjectAttribute);
                string dependencyCheck = element.GetAttribute(ObjectDefinitionConstants.DependencyCheckAttribute);
                if (ObjectDefinitionConstants.DefaultValue.Equals(dependencyCheck))
                {
                    dependencyCheck = parserContext.ParserHelper.Defaults.DependencyCheck;
                }
                od.DependencyCheck = GetDependencyCheck(dependencyCheck);
                string autowire = element.GetAttribute(ObjectDefinitionConstants.AutowireAttribute);
                if (ObjectDefinitionConstants.DefaultValue.Equals(autowire))
                {
                    autowire = parserContext.ParserHelper.Defaults.Autowire;
                }
                od.AutowireMode = GetAutowireMode(autowire);
                string initMethodName = element.GetAttribute(ObjectDefinitionConstants.InitMethodAttribute);
                if (StringUtils.HasText(initMethodName))
                {
                    od.InitMethodName = initMethodName;
                }
                string destroyMethodName = element.GetAttribute(ObjectDefinitionConstants.DestroyMethodAttribute);
                if (StringUtils.HasText(destroyMethodName))
                {
                    od.DestroyMethodName = destroyMethodName;
                }
                if (element.HasAttribute(ObjectDefinitionConstants.SingletonAttribute))
                {
                    od.IsSingleton = IsTrueStringValue(element.GetAttribute(ObjectDefinitionConstants.SingletonAttribute).ToLower(CultureInfo.CurrentCulture));
                }
                string lazyInit = element.GetAttribute(ObjectDefinitionConstants.LazyInitAttribute);
                if (ObjectDefinitionConstants.DefaultValue.Equals(lazyInit) && od.IsSingleton)
                {
                    // just apply default to singletons, as lazy-init has no meaning for prototypes...
                    lazyInit = parserContext.ParserHelper.Defaults.LazyInit;
                }
                od.IsLazyInit = IsTrueStringValue(lazyInit);

                // try to get the line info
                string resourceDescription = parserContext.ParserHelper.ReaderContext.Resource.Description;
                if (StringUtils.HasText(resourceDescription))
                {
                    int line = ConfigurationUtils.GetLineNumber(element);
                    if (line > 0)
                    {
                        resourceDescription += " line " + line;
                    }
                }
                od.ResourceDescription = resourceDescription;

                string isAbstract = element.GetAttribute(ObjectDefinitionConstants.AbstractAttribute);
                if (StringUtils.HasText(isAbstract))
                {
                    od.IsAbstract = IsTrueStringValue(isAbstract);
                }
                return od;
            }
            catch (TypeLoadException ex)
            {
                parserContext.ReaderContext.ReportException(
                    element,
                    id,
                    string.Format(
                        "Object class [{0}] not found.",
                        typeName),
                    ex);
            }
            catch (ApplicationException ex)
            {
                parserContext.ReaderContext.ReportException(element, id, string.Empty, ex);
            }
            return null;
        }

        /// <summary>
        /// Parse method override argument subelements of the given object element.
        /// </summary>
        protected MethodOverrides ParseMethodOverrideSubElements(
            string name, XmlElement element, ParserContext parserContext)
        {
            MethodOverrides overrides = new MethodOverrides();
            foreach (XmlNode node in this.SelectNodes(element, ObjectDefinitionConstants.LookupMethodElement))
            {
                ParseLookupMethodElement(name, overrides, (XmlElement)node, parserContext);
            }
            foreach (XmlNode node in this.SelectNodes(element, ObjectDefinitionConstants.ReplacedMethodElement))
            {
                ParseReplacedMethodElement(name, overrides, (XmlElement)node, parserContext);
            }
            return overrides;
        }

        /// <summary>
        /// Parse <see cref="ObjectDefinitionConstants.LookupMethodElement"/> element and add parsed element to <paramref name="overrides"/>
        /// </summary>
        protected void ParseLookupMethodElement(
            string name, MethodOverrides overrides, XmlElement element, ParserContext parserContext)
        {
            string methodName = element.GetAttribute(ObjectDefinitionConstants.LookupMethodNameAttribute);
            string targetObjectName = element.GetAttribute(ObjectDefinitionConstants.LookupMethodObjectNameAttribute);
            if (StringUtils.IsNullOrEmpty(methodName))
            {
                throw new ObjectDefinitionStoreException(
                    parserContext.ReaderContext.Resource, name,
                    string.Format("The '{0}' attribute is required for the '{1}' element.",
                                  ObjectDefinitionConstants.LookupMethodNameAttribute, ObjectDefinitionConstants.LookupMethodElement));
            }
            if (StringUtils.IsNullOrEmpty(targetObjectName))
            {
                throw new ObjectDefinitionStoreException(
                    parserContext.ReaderContext.Resource, name,
                    string.Format("The '{0}' attribute is required for the '{1}' element.",
                                  ObjectDefinitionConstants.LookupMethodObjectNameAttribute, ObjectDefinitionConstants.LookupMethodElement));
            }
            overrides.Add(new LookupMethodOverride(methodName, targetObjectName));
        }

        /// <summary>
        /// Parse <see cref="ObjectDefinitionConstants.ReplacedMethodElement"/> element and add parsed element to <paramref name="overrides"/>
        /// </summary>
        protected void ParseReplacedMethodElement(
            string name, MethodOverrides overrides, XmlElement element, ParserContext parserContext)
        {
            string methodName = element.GetAttribute(ObjectDefinitionConstants.ReplacedMethodNameAttribute);
            string targetReplacerObjectName = element.GetAttribute(ObjectDefinitionConstants.ReplacedMethodReplacerNameAttribute);
            if (StringUtils.IsNullOrEmpty(methodName))
            {
                throw new ObjectDefinitionStoreException(
                    parserContext.ReaderContext.Resource, name,
                    string.Format("The '{0}' attribute is required for the '{1}' element.",
                                  ObjectDefinitionConstants.ReplacedMethodNameAttribute, ObjectDefinitionConstants.ReplacedMethodElement));
            }
            if (StringUtils.IsNullOrEmpty(targetReplacerObjectName))
            {
                throw new ObjectDefinitionStoreException(
                    parserContext.ReaderContext.Resource, name,
                    string.Format("The '{0}' attribute is required for the '{1}' element.",
                                  ObjectDefinitionConstants.ReplacedMethodReplacerNameAttribute, ObjectDefinitionConstants.ReplacedMethodElement));
            }
            ReplacedMethodOverride theOverride = new ReplacedMethodOverride(methodName, targetReplacerObjectName);
            foreach (XmlNode node in this.SelectNodes(element, ObjectDefinitionConstants.ReplacedMethodArgumentTypeElement))
            {
                XmlElement argElement = (XmlElement) node;
                string match = argElement.GetAttribute(ObjectDefinitionConstants.ReplacedMethodArgumentTypeMatchAttribute);
                if (StringUtils.IsNullOrEmpty(match))
                {
                    throw new ObjectDefinitionStoreException(
                        parserContext.ReaderContext.Resource, name,
                        string.Format("The '{0}' attribute is required for the '{1}' element.",
                                      ObjectDefinitionConstants.ReplacedMethodArgumentTypeMatchAttribute, ObjectDefinitionConstants.ReplacedMethodArgumentTypeElement));
                }
                theOverride.AddTypeIdentifier(match);
            }
            overrides.Add(theOverride);
        }

        /// <summary>
        /// Parse constructor argument subelements of the given object element.
        /// </summary>
        protected ConstructorArgumentValues ParseConstructorArgSubElements(
            string name, XmlElement element, ParserContext parserContext)
        {
            ConstructorArgumentValues arguments = new ConstructorArgumentValues();
            foreach (XmlNode node in this.SelectNodes(element, ObjectDefinitionConstants.ConstructorArgElement))
            {
                ParseConstructorArgElement(name, arguments, (XmlElement)node, parserContext);
            }
            return arguments;
        }

        /// <summary>
        /// Parse event handler subelements of the given object element.
        /// </summary>
        protected EventValues ParseEventHandlerSubElements(
            string name, XmlElement element, ParserContext parserContext)
        {
            EventValues events = new EventValues();
            foreach (XmlNode node in this.SelectNodes(element, ObjectDefinitionConstants.ListenerElement))
            {
                ParseEventListenerDefinition(name, events, (XmlElement)node, parserContext);
            }
            return events;
        }

        /// <summary>
        /// Parse property value subelements of the given object element.
        /// </summary>
        /// <param name="name">
        /// The name of the object (definition) associated with the property element (s)
        /// </param>
        /// <param name="element">
        /// The element containing the top level object definition.
        /// </param>
        /// <param name="parserContext">
        /// The namespace-aware parser.
        /// </param>
        /// <returns>
        /// The property (s) associated with the object (definition).
        /// </returns>
        protected virtual MutablePropertyValues ParsePropertyElements(
            string name, XmlElement element, ParserContext parserContext)
        {
            MutablePropertyValues properties = new MutablePropertyValues();
            foreach (XmlNode node in this.SelectNodes(element, ObjectDefinitionConstants.PropertyElement))
            {
                ParsePropertyElement(name, properties, (XmlElement) node, parserContext);
            }
            return properties;
        }

        /// <summary>
        /// Parse a constructor-arg element.
        /// </summary>
        /// <param name="name">
        /// The name of the object (definition) associated with the ctor arg.
        /// </param>
        /// <param name="arguments">
        /// The list of constructor args associated with the object (definition).
        /// </param>
        /// <param name="element">
        /// The name of the element containing the ctor arg definition.
        /// </param>
        /// <param name="parserContext">
        /// The namespace-aware parser.
        /// </param>
        protected virtual void ParseConstructorArgElement(
            string name, ConstructorArgumentValues arguments, XmlElement element, ParserContext parserContext)
        {
            object val = ParsePropertyValue(element, name, parserContext);
            string indexAttr = element.GetAttribute(ObjectDefinitionConstants.IndexAttribute);
            string typeAttr = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
            string nameAttr = element.GetAttribute(ObjectDefinitionConstants.ArgumentNameAttribute);

            // only one of the 'index' or 'name' attributes can be present
            if (StringUtils.HasText(indexAttr)
                && StringUtils.HasText(nameAttr))
            {
                throw new ObjectDefinitionStoreException(
                    parserContext.ReaderContext.Resource, name,
                    "Only one of the 'index' or 'name' attributes can be present per constructor argument.");
            }
            if (StringUtils.HasText(indexAttr))
            {
                try
                {
                    int index = int.Parse(indexAttr, CultureInfo.CurrentCulture);
                    if (index < 0)
                    {
                        throw new ObjectDefinitionStoreException(
                            parserContext.ReaderContext.Resource, name,
                            "'index' cannot be lower than 0");
                    }
                    if (StringUtils.HasText(typeAttr))
                    {
                        arguments.AddIndexedArgumentValue(index, val, typeAttr);
                    }
                    else
                    {
                        arguments.AddIndexedArgumentValue(index, val);
                    }
                }
                catch (FormatException)
                {
                    throw new ObjectDefinitionStoreException(
                        parserContext.ReaderContext.Resource, name,
                        "Attribute 'index' of tag 'constructor-arg' must be an integer value.");
                }
            }
            else if (StringUtils.HasText(nameAttr))
            {
                if (StringUtils.HasText(typeAttr))
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Warn("The 'type' attribute is redundant when the 'name' attribute has been used on a constructor argument element.");
                    }
                }
                arguments.AddNamedArgumentValue(nameAttr, val);
            }
            else
            {
                if (StringUtils.HasText(typeAttr))
                {
                    arguments.AddGenericArgumentValue(val, typeAttr);
                }
                else
                {
                    arguments.AddGenericArgumentValue(val);
                }
            }
        }

        /// <summary>
        /// Parse a property element.
        /// </summary>
        /// <param name="name">
        /// The name of the object (definition) associated with the property.
        /// </param>
        /// <param name="properties">
        /// The list of properties associated with the object (definition).
        /// </param>
        /// <param name="element">
        /// The name of the element containing the property definition.
        /// </param>
        /// <param name="parserContext">
        /// The namespace-aware parser.
        /// </param>
        protected void ParsePropertyElement(
            string name, MutablePropertyValues properties, XmlElement element, ParserContext parserContext)
        {
            string propertyName = element.GetAttribute(ObjectDefinitionConstants.NameAttribute);
            if (StringUtils.IsNullOrEmpty(propertyName))
            {
                throw new ObjectDefinitionStoreException(
                    parserContext.ReaderContext.Resource,
                    name,
                    "The 'property' element must have a 'name' attribute");
            }
            object val = ParsePropertyValue(element, name, parserContext);
            properties.Add(new PropertyValue(propertyName, val));
        }

        /// <summary>
        /// Get the value of a property element (may be a list).</summary>
        /// <remarks>
        /// <p>
        /// Please note that even though this method is named Get<b>Property</b>Value,
        /// it is called by both the property and constructor argument element
        /// handlers.
        /// </p>
        /// </remarks>
        /// <param name="element">The property element.</param>
        /// <param name="name">
        /// The name of the object associated with the property.
        /// </param>
        /// <param name="parserContext">
        /// The namespace-aware parser.
        /// </param>
        protected virtual object ParsePropertyValue(
            XmlElement element, string name, ParserContext parserContext)
        {
            XmlAttribute inlineValueAtt = element.Attributes[ObjectDefinitionConstants.ValueAttribute];
            if (inlineValueAtt != null)
            {
                return inlineValueAtt.Value;
            }
            XmlAttribute inlineRefAtt = element.Attributes[ObjectDefinitionConstants.RefAttribute];
            if (inlineRefAtt != null)
            {
                return new RuntimeObjectReference(inlineRefAtt.Value);
            }
            XmlAttribute inlineExpressionAtt = element.Attributes[ObjectDefinitionConstants.ExpressionAttribute];
            if (inlineExpressionAtt != null)
            {
                return new ExpressionHolder(inlineExpressionAtt.Value);
            }
            
            // should only have one element child: value, ref, collection...
            XmlNodeList nodes = element.ChildNodes;
            XmlElement valueRefOrCollectionElement = null;
            for (int i = 0; i < nodes.Count; ++i)
            {
                XmlElement candidateEle = nodes.Item(i) as XmlElement;
                if (candidateEle != null)
                {
                    if (ObjectDefinitionConstants.DescriptionElement.Equals(candidateEle.Name))
                    {
                        // keep going: we don't use this value for now...
                    }
                    else
                    {
                        // child element is what we're looking for...
                        valueRefOrCollectionElement = candidateEle;
                    }
                }
            }
            if (valueRefOrCollectionElement == null)
            {
                throw new ObjectDefinitionStoreException(
                    parserContext.ReaderContext.Resource,
                    name,
                    "The '<property/>' element must have a subelement such as 'value' or 'ref'.");
            }
            return ParsePropertySubElement(valueRefOrCollectionElement, name, parserContext);
        }

        /// <summary>
        /// Parse a value, ref or collection subelement of a property element.
        /// </summary>
        /// <param name="element">
        /// Subelement of property element; we don't know which yet.
        /// </param>
        /// <param name="name">
        /// The name of the object (definition) associated with the top level property.
        /// </param>
        /// <param name="parserContext">
        /// The namespace-aware parser.
        /// </param>
        protected virtual object ParsePropertySubElement(
            XmlElement element, string name, ParserContext parserContext)
        {
            if (element.Name.Equals(ObjectDefinitionConstants.ObjectElement))
            {
                return ParseObjectDefinitionElement(element, parserContext, true);
            }
            else if (element.Name.Equals(ObjectDefinitionConstants.RefElement))
            {
                return ParseReference(element, parserContext.ParserHelper, name);
            }
            else if (element.Name.Equals(ObjectDefinitionConstants.IdRefElement))
            {
                return ParseIdReference(element, parserContext.ParserHelper, name);
            }
            else if (element.Name.Equals(ObjectDefinitionConstants.ListElement))
            {
                return ParseListElement(element, name, parserContext);
            }
            else if (element.Name.Equals(ObjectDefinitionConstants.SetElement))
            {
                return ParseSetElement(element, name, parserContext);
            }
            else if (element.Name.Equals(ObjectDefinitionConstants.DictionaryElement))
            {
                return ParseDictionaryElement(element, name, parserContext);
            }
            else if (element.Name.Equals(ObjectDefinitionConstants.NameValuesElement))
            {
                return ParseNameValueCollectionElement(element, name);
            }
            else if (element.Name.Equals(ObjectDefinitionConstants.ValueElement))
            {
                return ParseValueElement(element, name);
            }
            else if (element.Name.Equals(ObjectDefinitionConstants.ExpressionElement))
            {
                return ParseExpressionElement(element, name, parserContext);
            }
            else if (element.Name.Equals(ObjectDefinitionConstants.NullElement))
            {
                // it's a distinguished null value...
                return null;
            }
            else
            {
                // it may match another Parser
                INamespaceParser otherParser = GetParser(element.NamespaceURI);
                if (otherParser != null)
                {
                    // The other parser uses nestings tags and thus returns the definition
                    // of the parsed object.
                    return otherParser.ParseElement(element, new ParserContext(parserContext.ReaderContext, parserContext.ParserHelper));
                }
            }
            throw new ObjectDefinitionStoreException(
                parserContext.ReaderContext.Resource,
                name,
                "Unknown subelement of <property>: <" + element.Name + ">");
        }

        private static INamespaceParser GetParser(string nspace)
        {
            // finds the configuration parser for the given namespace
            try
            {
                return NamespaceParserRegistry.GetParser(nspace);
            }
            catch (Exception)
            {
                // The parser for the given namespace is not found
                return null;
            }
        }
        
        private static object ParseIdReference(XmlElement element, ObjectDefinitionParserHelper parserHelper, string name)
        {
            // a generic reference to any name of any object
            string objectRef = element.GetAttribute(ObjectDefinitionConstants.ObjectRefAttribute);
            if (StringUtils.IsNullOrEmpty(objectRef))
            {
                // a reference to the id of another object in the same XML file
                objectRef = element.GetAttribute(ObjectDefinitionConstants.LocalRefAttribute);
                if (StringUtils.IsNullOrEmpty(objectRef))
                {
                    throw new ObjectDefinitionStoreException(
                        parserHelper.ReaderContext.Resource,
                        name,
                        "Either 'object' or 'local' is required for an idref");
                }
            }
            return objectRef;
        }

        private object ParseReference(XmlElement element, ObjectDefinitionParserHelper parserHelper, string name)
        {
            // is it a generic reference to any name of any object?
            string objectRef = element.GetAttribute(ObjectDefinitionConstants.ObjectRefAttribute);
            if (StringUtils.IsNullOrEmpty(objectRef))
            {
                // is it a reference to the id of another object in the same XML file?
                objectRef = element.GetAttribute(ObjectDefinitionConstants.LocalRefAttribute);
                if (StringUtils.IsNullOrEmpty(objectRef))
                {
                    // is it a reference to the id of another object in a parent context?
                    objectRef = element.GetAttribute(ObjectDefinitionConstants.ParentAttribute);
                    if (StringUtils.IsNullOrEmpty(objectRef))
                    {
                        throw new ObjectDefinitionStoreException(
                            parserHelper.ReaderContext.Resource,
                            name,
                            "Either 'object' or 'local' is required for a reference");
                    }
                    return new RuntimeObjectReference(objectRef, true);
                }
            }
            return new RuntimeObjectReference(objectRef);
        }

        private object ParseValueElement(XmlElement element, string name)
        {
            string valueType = element.GetAttribute(ObjectDefinitionConstants.TypeAttribute);
            if (StringUtils.IsNullOrEmpty(valueType))
            {
                return ParseTextValueElement(element, name);
            }
            else
            {
                Type resolvedValueType = TypeResolutionUtils.ResolveType(valueType);
                if (resolvedValueType == typeof(string))
                {
                    return ParseTextValueElement(element, name);
                }
                else
                {
                    return new TypedStringValue(ParseTextValueElement(element, name), resolvedValueType);
                }
            }
        }

        private object ParseExpressionElement(XmlElement element, string name, ParserContext parserContext)
        {
            string expression = element.GetAttribute(ObjectDefinitionConstants.ValueAttribute);
            ExpressionHolder holder = new ExpressionHolder(expression);
            holder.Properties = ParsePropertyElements(name, element, parserContext);
            return holder;
        }

        /// <summary>
        /// Gets a list definition.
        /// </summary>
        /// <param name="element">
        /// The element describing the list definition.
        /// </param>
        /// <param name="name">
        /// The name of the object (definition) associated with the list definition.
        /// </param>
        /// <param name="parserContext">
        /// The namespace-aware parser.
        /// </param>
        /// <returns>The list definition.</returns>
        protected virtual IList ParseListElement(XmlElement element, string name, ParserContext parserContext)
        {
            ManagedList list = new ManagedList();

            string elementTypeName = element.GetAttribute("element-type");
            if (StringUtils.HasText(elementTypeName))
            {
                list.ElementTypeName = elementTypeName;
            }
            
            foreach (XmlNode node in element.ChildNodes)
            {
                XmlElement ele = node as XmlElement;
                if (ele != null)
                {
                    list.Add(ParsePropertySubElement(ele, name, parserContext));
                }
            }
            return list;
        }

        /// <summary>
        /// Gets a set definition.
        /// </summary>
        /// <param name="element">
        /// The element describing the set definition.
        /// </param>
        /// <param name="name">
        /// The name of the object (definition) associated with the set definition.
        /// </param>
        /// <param name="parserContext">
        /// The namespace-aware parser.
        /// </param>
        /// <returns>The set definition.</returns>
        protected Set ParseSetElement(XmlElement element, string name, ParserContext parserContext)
        {
            ManagedSet theSet = new ManagedSet();
            string elementTypeName = element.GetAttribute("element-type");
            if (StringUtils.HasText(elementTypeName))
            {
                theSet.ElementTypeName = elementTypeName;
            }
            foreach (XmlNode node in element.ChildNodes)
            {
                XmlElement ele = node as XmlElement;
                if (ele != null)
                {
                    object sub = ParsePropertySubElement(ele, name, parserContext);
                    theSet.Add(sub);
                }
            }
            return theSet;
        }

        /// <summary>
        /// Gets a dictionary definition.
        /// </summary>
        /// <param name="element">
        /// The element describing the dictionary definition.
        /// </param>
        /// <param name="name">
        /// The name of the object (definition) associated with the dictionary definition.
        /// </param>
        /// <param name="parserContext">
        /// The namespace-aware parser.
        /// </param>
        /// <returns>The dictionary definition.</returns>
        protected IDictionary ParseDictionaryElement(XmlElement element, string name, ParserContext parserContext)
        {
            ManagedDictionary dictionary = new ManagedDictionary();
            string keyTypeName = element.GetAttribute("key-type");
            string valueTypeName = element.GetAttribute("value-type");
            if (StringUtils.HasText(keyTypeName))
            {
                dictionary.KeyTypeName = keyTypeName;
            }
            if (StringUtils.HasText(valueTypeName))
            {
                dictionary.ValueTypeName = valueTypeName;
            }

            XmlNodeList entryElements = SelectNodes(element, ObjectDefinitionConstants.EntryElement);
            foreach (XmlElement entryEle in entryElements)
            {
                #region Key

                object key = null;

                XmlAttribute keyAtt = entryEle.Attributes[ObjectDefinitionConstants.KeyAttribute];
                if (keyAtt != null)
                {
                    key = keyAtt.Value;
                }
                else
                {
                    // ok, we're not using the 'key' attribute; lets check for the ref shortcut...
                    XmlAttribute keyRefAtt = entryEle.Attributes[ObjectDefinitionConstants.DictionaryKeyRefShortcutAttribute];
                    if (keyRefAtt != null)
                    {
                        key = new RuntimeObjectReference(keyRefAtt.Value);
                    }
                    else
                    {
                        // so check for the 'key' element...
                        XmlNode keyNode = SelectSingleNode(entryEle, ObjectDefinitionConstants.KeyElement);
                        if (keyNode == null)
                        {
                            throw new ObjectDefinitionStoreException(
                                parserContext.ReaderContext.Resource, name,
                                string.Format("One of either the '{0}' element, or the the '{1}' or '{2}' attributes " +
                                              "is required for the <{3}/> element.",
                                              ObjectDefinitionConstants.KeyElement,
                                              ObjectDefinitionConstants.KeyAttribute,
                                              ObjectDefinitionConstants.DictionaryKeyRefShortcutAttribute,
                                              ObjectDefinitionConstants.EntryElement));
                        }
                        XmlElement keyElement = (XmlElement) keyNode;
                        XmlNodeList keyNodes = keyElement.GetElementsByTagName("*");
                        if (keyNodes == null || keyNodes.Count == 0)
                        {
                            throw new ObjectDefinitionStoreException(
                                parserContext.ReaderContext.Resource, name,
                                string.Format("Malformed <{0}/> element... the value of the key must be " +
                                    "specified as a child value-style element.",
                                              ObjectDefinitionConstants.KeyElement));
                        }
                        key = ParsePropertySubElement((XmlElement)keyNodes.Item(0), name, parserContext);
                    }
                }

                #endregion

                #region Value

                XmlAttribute inlineValueAtt = entryEle.Attributes[ObjectDefinitionConstants.ValueAttribute];
                if (inlineValueAtt != null)
                {
                    // ok, we're using the value attribute shortcut...
                    dictionary[key] = inlineValueAtt.Value;
                }
                else if (entryEle.Attributes[ObjectDefinitionConstants.DictionaryValueRefShortcutAttribute] != null)
                {
                    // ok, we're using the value-ref attribute shortcut...
                    XmlAttribute inlineValueRefAtt = entryEle.Attributes[ObjectDefinitionConstants.DictionaryValueRefShortcutAttribute];
                    RuntimeObjectReference ror = new RuntimeObjectReference(inlineValueRefAtt.Value);
                    dictionary[key] = ror;
                }
                else if (entryEle.Attributes[ObjectDefinitionConstants.ExpressionAttribute] != null)
                {
                    // ok, we're using the expression attribute shortcut...
                    XmlAttribute inlineExpressionAtt = entryEle.Attributes[ObjectDefinitionConstants.ExpressionAttribute];
                    ExpressionHolder expHolder = new ExpressionHolder(inlineExpressionAtt.Value);
                    dictionary[key] = expHolder;
                }
                else
                {
                    XmlNode keyNode = SelectSingleNode(entryEle, ObjectDefinitionConstants.KeyElement);
                    if (keyNode != null)
                    {
                        entryEle.RemoveChild(keyNode);
                    }
                    // ok, we're using the original full-on value element...
                    XmlNodeList valueElements = entryEle.GetElementsByTagName("*");
                    if (valueElements == null || valueElements.Count == 0)
                    {
                        throw new ObjectDefinitionStoreException(
                            parserContext.ReaderContext.Resource, name,
                            string.Format("One of either the '{0}' or '{1}' attributes, or a value-style element " +
                                "is required for the <{2}/> element.",
                                          ObjectDefinitionConstants.ValueAttribute, ObjectDefinitionConstants.DictionaryValueRefShortcutAttribute, ObjectDefinitionConstants.EntryElement));
                    }
                    dictionary[key] = ParsePropertySubElement((XmlElement)valueElements.Item(0), name, parserContext);
                }

                #endregion
            }
            return dictionary;
        }

        /// <summary>
        /// Selects sub-elements with a given
        /// <paramref name="childElementName">name</paramref>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Uses a namespace manager if necessary.
        /// </p>
        /// </remarks>
        /// <param name="element">
        /// The element to be searched in.
        /// </param>
        /// <param name="childElementName">
        /// The name of the child nodes to look for.
        /// </param>
        /// <returns>
        /// The child <see cref="System.Xml.XmlNode"/>s of the supplied
        /// <paramref name="element"/> with the supplied
        /// <paramref name="childElementName"/>.
        /// </returns>
        protected XmlNodeList SelectNodes(XmlElement element, string childElementName)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace(GetNamespacePrefix(element), element.NamespaceURI);
            return element.SelectNodes(GetNamespacePrefix(element) + ":" + childElementName, nsManager);
        }

        /// <summary>
        /// Selects a single sub-element with a given
        /// <paramref name="childElementName">name</paramref>.
        /// </summary>
        /// <remarks>
        /// <p>
        /// Uses a namespace manager if necessary.
        /// </p>
        /// </remarks>
        /// <param name="element">
        /// The element to be searched in.
        /// </param>
        /// <param name="childElementName">
        /// The name of the child node to look for.
        /// </param>
        /// <returns>
        /// The first child <see cref="System.Xml.XmlNode"/> of the supplied
        /// <paramref name="element"/> with the supplied
        /// <paramref name="childElementName"/>.
        /// </returns>
        protected XmlNode SelectSingleNode(XmlElement element, string childElementName)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace(GetNamespacePrefix(element), element.NamespaceURI);
            return element.SelectSingleNode(GetNamespacePrefix(element) + ":" + childElementName, nsManager);
        }

        /// <summary>
        /// Gets a name value collection mapping definition.
        /// </summary>
        /// <param name="element">
        /// The element describing the name value collection mapping definition.
        /// </param>
        /// <param name="name">
        /// The name of the object (definition) associated with the
        /// name value collection mapping definition.
        /// </param>
        /// <returns>The name value collection definition.</returns>
        protected NameValueCollection ParseNameValueCollectionElement(XmlElement element, string name)
        {
            NameValueCollection nvc = new NameValueCollection();
            XmlNodeList addElements = element.GetElementsByTagName(ObjectDefinitionConstants.AddElement);
            foreach (XmlElement addElement in addElements)
            {
                string key = addElement.GetAttribute(ObjectDefinitionConstants.KeyAttribute);
                string value = addElement.GetAttribute(ObjectDefinitionConstants.ValueAttribute);
                string delimiters = addElement.GetAttribute(ObjectDefinitionConstants.DelimitersAttribute);

                if (StringUtils.HasText(delimiters))
                {
                    string[] values = value.Split(delimiters.ToCharArray());
                    foreach (string v in values)
                    {
                        nvc.Add(key, v);
                    }
                }
                else
                {
                    nvc.Add(key,value);
                }
            }
            return nvc;
        }

        /// <summary>
        /// Returns the text of the supplied <paramref name="element"/>,
        /// or the empty string value if said <paramref name="element"/> is empty.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the supplied <paramref name="element"/> is <see langword="null"/>,
        /// then the empty string value will be returned.
        /// </p>
        /// </remarks>
        protected string ParseTextValueElement(XmlElement element, string name)
        {
            if (element == null || StringUtils.IsNullOrEmpty(element.InnerText))
            {
                return String.Empty;
            }
            return element.InnerText;
        }

        /// <summary>
        /// Strips the dependency check value out of the supplied string.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the supplied <paramref name="value"/> is an invalid dependency
        /// checking mode, the invalid value will be logged and this method will
        /// return the <see cref="DependencyCheckingMode.None"/> value.
        /// No exception will be raised.
        /// </p>
        /// </remarks>
        /// <param name="value">
        /// The string containing the dependency check value.
        /// </param>
        /// <returns>The dependency check value.</returns>
        /// <seealso cref="DependencyCheckingMode"/>
        protected DependencyCheckingMode GetDependencyCheck(string value)
        {
            DependencyCheckingMode code = DependencyCheckingMode.None;
            if (StringUtils.HasText(value))
            {
                try
                {
                    code = (DependencyCheckingMode) Enum.Parse(
                        typeof(DependencyCheckingMode), value, true);
                }
                catch (ArgumentException ex)
                {
                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                            string.Format("Error while parsing dependency checking mode : '{0}' is an invalid value.",
                                          value), ex);
                    }

                    #endregion
                }
            }
            return code;
        }

        /// <summary>
        /// Strips the autowiring mode out of the supplied string.
        /// </summary>
        /// <remarks>
        /// <p>
        /// If the supplied <paramref name="value"/> is an invalid autowiring mode,
        /// the invalid value will be logged and this method will return the
        /// <see cref="AutoWiringMode.No"/> value. No exception will be raised.
        /// </p>
        /// </remarks>
        /// <param name="value">
        /// The string containing the autowiring mode definition.
        /// </param>
        /// <returns>The autowiring mode.</returns>
        /// <seealso cref="AutoWiringMode"/>
        protected AutoWiringMode GetAutowireMode(string value)
        {
            AutoWiringMode mode = AutoWiringMode.No;
            if (StringUtils.HasText(value))
            {
                try
                {
                    mode = (AutoWiringMode) Enum.Parse(
                        typeof(AutoWiringMode), value, true);
                }
                catch (ArgumentException ex)
                {
                    #region Instrumentation

                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                            string.Format("Error while parsing autowire mode : '{0}' is an invalid value.",
                                          value), ex);
                    }

                    #endregion
                }
            }
            return mode;
        }

        /// <summary>
        /// Given a string containing delimited object names, returns
        /// a string array split on the object name delimeter.
        /// </summary>
        /// <param name="value">
        /// The string containing delimited object names.
        /// </param>
        /// <returns>
        /// A string array split on the object name delimeter.
        /// </returns>
        /// <seealso cref="ObjectDefinitionConstants.ObjectNameDelimiters"/>
        private string[] GetObjectNames(string value)
        {
            return StringUtils.Split(
                value, ObjectDefinitionConstants.ObjectNameDelimiters, true, true);
        }

        private static bool IsTrueStringValue(string value)
        {
            return ObjectDefinitionConstants.TrueValue.Equals(value);
        }

        private string GetNamespacePrefix(XmlElement element)
        {
            return StringUtils.HasText(element.Prefix) ? element.Prefix : "spring";
        }
    }
}