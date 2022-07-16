#region License

/*
 * Copyright  2002-2005 the original author or authors.
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
using System.Collections.Specialized;
using System.Globalization;
using System.Xml;

using Common.Logging;

using Spring.Collections;
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
            SchemaLocation = "/Spring.Objects.Factory.Xml/spring-objects-2.0.xsd"
        )
    ]
//    [Obsolete("ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead", false)]
    public class ObjectsNamespaceParser : AbstractObjectDefinitionParser, INamespaceParser
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
        /// Parse the specified XmlElement and register the resulting
        /// ObjectDefinitions with the <see cref="ParserContext.Registry"/> IObjectDefinitionRegistry
        /// embedded in the supplied <see cref="ParserContext"/>
        /// </summary>
        /// <param name="element">The element to be parsed.</param>
        /// <param name="parserContext">The object encapsulating the current state of the parsing process.
        /// Provides access to a IObjectDefinitionRegistry</param>
        /// <returns>The primary object definition.</returns>
        /// <remarks>
        /// 	<p>
        /// This method is never invoked if the parser is namespace aware
        /// and was called to process the root node.
        /// </p>
        /// </remarks>
//        [Obsolete("not used anymore - ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead", false)]
        public override IObjectDefinition ParseElement(XmlElement element, ParserContext parserContext)
        {
            // TODO (EE): overridden just to stay binary compatible between 1.2.0 and 1.2.1
            return base.ParseElement(element, parserContext);
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
        protected override AbstractObjectDefinition ParseInternal(XmlElement element, ParserContext parserContext)
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
                // atm this will call back into this ns parsers
                ObjectDefinitionHolder odh = parserContext.ParserHelper.ParseObjectDefinitionElement(element);
                if (odh != null)
                {
                    return odh.ObjectDefinition as AbstractObjectDefinition;
                }
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
        [Obsolete("not used anymore - ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead", false)]
        public ObjectDefinitionHolder Decorate(XmlNode node, ObjectDefinitionHolder definition,
                                               ParserContext parserContext)
        {
            return null;
        }

        private void ParseAlias(XmlElement aliasElement, IObjectDefinitionRegistry registry)
        {
            string name = GetAttributeValue(aliasElement, ObjectDefinitionConstants.NameAttribute);
            string alias = GetAttributeValue(aliasElement, ObjectDefinitionConstants.AliasAttribute);
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
        [Obsolete("not used anymore - ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead", false)]
        protected virtual void ImportObjectDefinitionResource(XmlElement resource, ParserContext parserContext)
        {
            string location = GetAttributeValue(resource, ObjectDefinitionConstants.ImportResourceAttribute);
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
                GetAttributeValue(element, ObjectDefinitionConstants.ListenerMethodAttribute),
                GetAttributeValue(element, ObjectDefinitionConstants.ListenerEventAttribute));

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
                        sourceAtt.Value :
                        TypeResolutionUtils.ResolveType(sourceAtt.Value) as object;
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
        [Obsolete("not used anymore - ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead", false)]
        protected ObjectDefinitionHolder ParseObjectDefinition(XmlElement element, ParserContext parserContext)
        {
            ObjectDefinitionHolder holder = null;
            try
            {
                holder = ParseObjectDefinitionElement(element, parserContext, false);
                if (holder == null)
                {
                    return null;
                }
            }
            catch (ObjectDefinitionStoreException)
            {
                throw;
            }
            catch (Exception ex)
            {
                //throw new ObjectDefinitionStoreException(string.Format("Failed parsing object definition '{0}'", element.OuterXml), ex);
                parserContext.ReaderContext.ReportException(element, null, null, ex);
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

            return holder;
        }

        /// <summary>
        /// Parse an object definition and register it with the object factory..
        /// </summary>
        /// <param name="element">The element containing the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <seealso cref="Spring.Objects.Factory.Support.ObjectDefinitionReaderUtils.RegisterObjectDefinition"/>
        [Obsolete("not used anymore - ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead", false)]
        protected ObjectDefinitionHolder ParseAndRegisterObjectDefinition(XmlElement element, ParserContext parserContext)
        {
            ObjectDefinitionHolder holder = ParseObjectDefinition(element, parserContext);
            ObjectDefinitionReaderUtils.RegisterObjectDefinition(holder, parserContext.ReaderContext.Registry);
            return holder;
        }

        /// <summary>
        /// Parse an object definition and register it with the object factory..
        /// </summary>
        /// <param name="element">The element containing the object definition.</param>
        /// <param name="parserContext">The parser context.</param>
        /// <seealso cref="Spring.Objects.Factory.Support.ObjectDefinitionReaderUtils.RegisterObjectDefinition"/>
        [Obsolete("not used anymore - ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead", false)]
        protected void RegisterObjectDefinition(XmlElement element, ParserContext parserContext)
        {
            ParseAndRegisterObjectDefinition(element, parserContext);
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
        [Obsolete("not used anymore - ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead", false)]
        protected ObjectDefinitionHolder ParseObjectDefinitionElement(XmlElement element, ParserContext parserContext, bool nestedDefinition)
        {
            return parserContext.ParserHelper.ParseObjectDefinitionElement(element, parserContext.ContainingObjectDefinition);
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
        [Obsolete("This method will be dropped, override ObjectDefinitionParserHelper.PostProcessObjectNameAndAliases instead", false)]
        protected internal virtual string CalculateId(XmlElement element, List<string> aliases)
        {
            return null;
        }

        /// <summary>
        /// Parse a standard object definition.
        /// </summary>
        /// <param name="element">The element containing the object definition.</param>
        /// <param name="id">The id of the object definition.</param>
        /// <param name="parserContext">parsing state holder</param>
        /// <returns>The object (definition).</returns>
//        [Obsolete("not used anymore - ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead", false)]
        protected internal virtual IConfigurableObjectDefinition ParseObjectDefinitionElement(
            XmlElement element, string id, ParserContext parserContext)
        {
            string typeName = null;
            try
            {
                if (element.HasAttribute(ObjectDefinitionConstants.TypeAttribute))
                {
                    typeName = GetAttributeValue(element, ObjectDefinitionConstants.TypeAttribute);
                    if (StringUtils.IsNullOrEmpty(typeName))
                    {
                        throw new ObjectDefinitionStoreException(
                            parserContext.ReaderContext.Resource, id,
                            "The 'type' attribute does not need to be present, but if it is it must not be empty: got '" +
                            typeName + "'.");
                    }
                }

                string parent = GetAttributeValue(element, ObjectDefinitionConstants.ParentAttribute);


                AbstractObjectDefinition od
                    = parserContext.ReaderContext.ObjectDefinitionFactory.CreateObjectDefinition(
                        typeName, parent, parserContext.ReaderContext.Reader.Domain);

                ParserContext childParserContext = new ParserContext(parserContext.ParserHelper, od);

                ParseMetaElements(element, od);
                ParseQualifierElements(id, element, parserContext, od);
                MutablePropertyValues pvs = ParsePropertyElements(id, element, childParserContext);
                ConstructorArgumentValues arguments = ParseConstructorArgSubElements(id, element, childParserContext);
                EventValues events = ParseEventHandlerSubElements(id, element, childParserContext);
                MethodOverrides methodOverrides = ParseMethodOverrideSubElements(id, element, childParserContext);

                bool isPage = StringUtils.HasText(typeName) && typeName != null && typeName.ToLower().EndsWith(".aspx");
                if (!isPage)
                {
                    od.ConstructorArgumentValues = arguments;
                }

                od.PropertyValues = pvs;
                od.MethodOverrides = methodOverrides;
                od.EventHandlerValues = events;
                if (element.HasAttribute(ObjectDefinitionConstants.DependsOnAttribute))
                {
                    string dependsOn = GetAttributeValue(element, ObjectDefinitionConstants.DependsOnAttribute);
                    od.DependsOn = GetObjectNames(dependsOn);
                }
                od.FactoryMethodName = GetAttributeValue(element, ObjectDefinitionConstants.FactoryMethodAttribute);
                od.FactoryObjectName = GetAttributeValue(element, ObjectDefinitionConstants.FactoryObjectAttribute);
                string dependencyCheck = GetAttributeValue(element, ObjectDefinitionConstants.DependencyCheckAttribute);
                if (ObjectDefinitionConstants.DefaultValue.Equals(dependencyCheck))
                {
                    dependencyCheck = childParserContext.ParserHelper.Defaults.DependencyCheck;
                }
                od.DependencyCheck = GetDependencyCheck(dependencyCheck);
                string autowire = GetAttributeValue(element, ObjectDefinitionConstants.AutowireAttribute);
                if (ObjectDefinitionConstants.DefaultValue.Equals(autowire))
                {
                    autowire = childParserContext.ParserHelper.Defaults.Autowire;
                }
                od.AutowireMode = GetAutowireMode(autowire);

                string autowireCandidates = GetAttributeValue(element, ObjectDefinitionConstants.AutowireCandidateAttribute);
                if (string.IsNullOrEmpty(autowireCandidates) || ObjectDefinitionConstants.DefaultValue.Equals(autowireCandidates))
                {
                    if (!string.IsNullOrEmpty(childParserContext.ParserHelper.Defaults.AutowireCandidates))
                    {
                        string[] patterns = childParserContext.ParserHelper.Defaults.AutowireCandidates.Split(',');
                        od.IsAutowireCandidate = PatternMatchUtils.SimpleMatch(patterns, id);
                    }
                }
                else
                {
                    od.IsAutowireCandidate = ObjectDefinitionConstants.TrueValue.Equals(autowireCandidates);
                }
                string primary = GetAttributeValue(element, ObjectDefinitionConstants.PrimaryAttribute);
                if (string.IsNullOrEmpty(primary))
                {
                    primary = ObjectDefinitionConstants.FalseValue;
                }
                od.IsPrimary = IsTrueStringValue(primary);
                string initMethodName = GetAttributeValue(element, ObjectDefinitionConstants.InitMethodAttribute);
                if (initMethodName != null)
                {
                    if (StringUtils.HasText(initMethodName))
                        od.InitMethodName = initMethodName;
                }
                else
                {
                    if (StringUtils.HasText(childParserContext.ParserHelper.Defaults.InitMethod))
                        od.InitMethodName = childParserContext.ParserHelper.Defaults.InitMethod;
                }
                string destroyMethodName = GetAttributeValue(element, ObjectDefinitionConstants.DestroyMethodAttribute);
                if (destroyMethodName != null)
                {
                    if (StringUtils.HasText(destroyMethodName))
                    {
                        od.DestroyMethodName = destroyMethodName;
                    }
                }
                else
                {
                    if (StringUtils.HasText(childParserContext.ParserHelper.Defaults.DestroyMethod))
                        od.DestroyMethodName = childParserContext.ParserHelper.Defaults.DestroyMethod;
                }
                if (element.HasAttribute(ObjectDefinitionConstants.SingletonAttribute))
                {
                    od.IsSingleton = IsTrueStringValue(GetAttributeValue(element, ObjectDefinitionConstants.SingletonAttribute, string.Empty).ToLower(CultureInfo.CurrentCulture));
                }
                string lazyInit = GetAttributeValue(element, ObjectDefinitionConstants.LazyInitAttribute);
                if (ObjectDefinitionConstants.DefaultValue.Equals(lazyInit) && od.IsSingleton)
                {
                    // just apply default to singletons, as lazy-init has no meaning for prototypes...
                    lazyInit = childParserContext.ParserHelper.Defaults.LazyInit;
                }
                od.IsLazyInit = IsTrueStringValue(lazyInit);

                // try to get the line info
                string resourceDescription = childParserContext.ParserHelper.ReaderContext.Resource.Description;
                if (StringUtils.HasText(resourceDescription))
                {
                    int line = ConfigurationUtils.GetLineNumber(element);
                    if (line > 0)
                    {
                        resourceDescription += " line " + line;
                    }
                }
                od.ResourceDescription = resourceDescription;

                string isAbstract = GetAttributeValue(element, ObjectDefinitionConstants.AbstractAttribute);
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
            catch (Exception ex)
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
            string methodName = GetAttributeValue(element, ObjectDefinitionConstants.LookupMethodNameAttribute);
            string targetObjectName = GetAttributeValue(element, ObjectDefinitionConstants.LookupMethodObjectNameAttribute);
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
            string methodName = GetAttributeValue(element, ObjectDefinitionConstants.ReplacedMethodNameAttribute);
            string targetReplacerObjectName = GetAttributeValue(element, ObjectDefinitionConstants.ReplacedMethodReplacerNameAttribute);
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
                XmlElement argElement = (XmlElement)node;
                string match = GetAttributeValue(argElement, ObjectDefinitionConstants.ReplacedMethodArgumentTypeMatchAttribute);
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
        /// Parse the meta upplied meta attributes if the given object element
        /// </summary>
        protected void ParseMetaElements(XmlElement element, ObjectMetadataAttributeAccessor attributeAccessor)
        {
            foreach (XmlNode node in this.SelectNodes(element, ObjectDefinitionConstants.MetaElement))
            {
                string key = GetAttributeValue((XmlElement)node, ObjectDefinitionConstants.KeyAttribute);
                string value = GetAttributeValue((XmlElement)node, ObjectDefinitionConstants.ValueAttribute);

                ObjectMetadataAttribute attribute = new ObjectMetadataAttribute(key, value);
                attribute.Source = (XmlElement)node;
                attributeAccessor.AddMetadataAttribute(attribute);
            }
        }

        /// <summary>
        /// Parse qualifier sub-elements of the given bean element.
        /// </summary>
        public void ParseQualifierElements(string name, XmlElement element, ParserContext parserContext, AbstractObjectDefinition od)
        {
            foreach (XmlNode node in this.SelectNodes(element, ObjectDefinitionConstants.QualifierElement))
            {
                ParseQualifierElement(name, (XmlElement) node, parserContext, od);
            }
        }

        /// <summary>
        /// Parse a qualifier element.
        /// </summary>
        public void ParseQualifierElement(string name, XmlElement element, ParserContext parserContext, AbstractObjectDefinition od)
        {
            string typeName = GetAttributeValue(element, ObjectDefinitionConstants.TypeAttribute);
            string value = GetAttributeValue(element, ObjectDefinitionConstants.ValueAttribute);

            if (string.IsNullOrEmpty(typeName))
            {
                throw new ObjectDefinitionStoreException(
                                    parserContext.ReaderContext.Resource, name,
                                    "Tag 'qualifier' must have a 'type' attribute");
            }

            var qualifier = new AutowireCandidateQualifier(typeName);
            qualifier.Source = element;

            if (!string.IsNullOrEmpty(value))
                qualifier.SetAttribute(AutowireCandidateQualifier.VALUE_KEY, value);

            foreach (XmlNode node in this.SelectNodes(element, ObjectDefinitionConstants.AttributeElement))
            {
                var attributeEle = node as XmlElement;
                string attributeKey = GetAttributeValue(attributeEle, ObjectDefinitionConstants.KeyAttribute);
                string attributeValue = GetAttributeValue(attributeEle, ObjectDefinitionConstants.ValueAttribute);

                if (!string.IsNullOrEmpty(attributeKey) && !string.IsNullOrEmpty(attributeValue))
                {
                    var attribute = new ObjectMetadataAttribute(attributeKey, attributeValue);
                    attribute.Source = attributeEle;
                    qualifier.AddMetadataAttribute(attribute);
                }
                else
                {
                    throw new ObjectDefinitionStoreException(
                                        parserContext.ReaderContext.Resource, name,
                                        "Qualifier 'attribute' tag must have a 'key' and 'value'");
                }
            }
            od.AddQualifier(qualifier);
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
                ParsePropertyElement(name, properties, (XmlElement)node, parserContext);
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
            string indexAttr = GetAttributeValue(element, ObjectDefinitionConstants.IndexAttribute);
            string typeAttr = GetAttributeValue(element, ObjectDefinitionConstants.TypeAttribute);
            string nameAttr = GetAttributeValue(element, ObjectDefinitionConstants.ArgumentNameAttribute);

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
            string propertyName = GetAttributeValue(element, ObjectDefinitionConstants.NameAttribute);
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
            if (element.NamespaceURI == Namespace)
            {
                switch(element.LocalName)
                {
                    case ObjectDefinitionConstants.ObjectElement:
                        {
                            return parserContext.ParserHelper.ParseObjectDefinitionElement(element, parserContext.ContainingObjectDefinition);
                        }
                    case ObjectDefinitionConstants.RefElement:
                        {
                            return ParseReference(element, parserContext.ParserHelper, name);
                        }
                    case ObjectDefinitionConstants.IdRefElement:
                        {
                            return ParseIdReference(element, parserContext.ParserHelper, name);
                        }
                    case ObjectDefinitionConstants.ListElement:
                        {
                            return ParseListElement(element, name, parserContext);
                        }
                    case ObjectDefinitionConstants.SetElement:
                        {
                            return ParseSetElement(element, name, parserContext);
                        }
                    case ObjectDefinitionConstants.DictionaryElement:
                        {
                            return ParseDictionaryElement(element, name, parserContext);
                        }
                    case ObjectDefinitionConstants.NameValuesElement:
                        {
                            return ParseNameValueCollectionElement(element, name, parserContext);
                        }
                    case ObjectDefinitionConstants.ValueElement:
                        {
                            return ParseValueElement(element, name);
                        }
                    case ObjectDefinitionConstants.ExpressionElement:
                        {
                            return ParseExpressionElement(element, name, parserContext);
                        }
                    case ObjectDefinitionConstants.NullElement:
                        {
                            // it's a distinguished null value...
                            return null;
                        }
                default:
                        throw new ObjectDefinitionStoreException(
                            parserContext.ReaderContext.Resource,
                            name,
                            "Unknown subelement of <property>: <" + element.Name + ">");
                }
            }

            return parserContext.ParserHelper.ParseCustomElement(element, parserContext.ContainingObjectDefinition);

//            parserContext.ParserHelper.parse
//            // it may match another Parser
//            INamespaceParser otherParser = parserContext. (element.NamespaceURI);
//            if (otherParser != null)
//            {
//                // The other parser uses nestings tags and thus returns the definition
//                // of the parsed object.
//                return otherParser.ParseElement(element, new ParserContext(parserContext.ParserHelper));
//            }
//
//            throw new ObjectDefinitionStoreException(
//                parserContext.ReaderContext.Resource,
//                name,
//                "Unknown subelement of <property>: <" + element.Name + ">");
        }

//        private static INamespaceParser GetParser(string nspace)
//        {
//            // finds the configuration parser for the given namespace
//            try
//            {
//                return NamespaceParserRegistry.GetParser(nspace);
//            }
//            catch (Exception)
//            {
//                // The parser for the given namespace is not found
//                return null;
//            }
//        }

        private static object ParseIdReference(XmlElement element, ObjectDefinitionParserHelper parserHelper, string name)
        {
            // a generic reference to any name of any object
            string objectRef = GetAttributeValue(element, ObjectDefinitionConstants.ObjectRefAttribute);
            if (StringUtils.IsNullOrEmpty(objectRef))
            {
                // a reference to the id of another object in the same XML file
                objectRef = GetAttributeValue(element, ObjectDefinitionConstants.LocalRefAttribute);
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
            string objectRef = GetAttributeValue(element, ObjectDefinitionConstants.ObjectRefAttribute);
            if (StringUtils.IsNullOrEmpty(objectRef))
            {
                // is it a reference to the id of another object in the same XML file?
                objectRef = GetAttributeValue(element, ObjectDefinitionConstants.LocalRefAttribute);
                if (StringUtils.IsNullOrEmpty(objectRef))
                {
                    // is it a reference to the id of another object in a parent context?
                    objectRef = GetAttributeValue(element, ObjectDefinitionConstants.ParentAttribute);
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
            string valueType = GetAttributeValue(element, ObjectDefinitionConstants.TypeAttribute);
            if (StringUtils.IsNullOrEmpty(valueType))
            {
                return ParseTextValueElement(element, name);
            }
            else
            {
                return new TypedStringValue(ParseTextValueElement(element, name), valueType);
            }
        }

        private object ParseExpressionElement(XmlElement element, string name, ParserContext parserContext)
        {
            string expression = GetAttributeValue(element, ObjectDefinitionConstants.ValueAttribute);
            ExpressionHolder holder = new ExpressionHolder(expression);
            holder.Properties = ParsePropertyElements(name, element, parserContext);
            return holder;
        }

        /// <summary>
        /// Gets a list definition.
        /// </summary>
        /// <param name="collectionEle">
        /// The element describing the list definition.
        /// </param>
        /// <param name="name">
        /// The name of the object (definition) associated with the list definition.
        /// </param>
        /// <param name="parserContext">
        /// The namespace-aware parser.
        /// </param>
        /// <returns>The list definition.</returns>
        protected virtual IList ParseListElement(XmlElement collectionEle, string name, ParserContext parserContext)
        {
            string elementTypeName = GetAttributeValue(collectionEle, "element-type");
            XmlNodeList nl = collectionEle.ChildNodes;
            ManagedList target = new ManagedList(nl.Count);

            if (StringUtils.HasText(elementTypeName))
            {
                target.ElementTypeName = elementTypeName;
            }
            target.MergeEnabled = ParseMergeAttribute(collectionEle, parserContext.ParserHelper);

            foreach (XmlNode node in collectionEle.ChildNodes)
            {
                XmlElement ele = node as XmlElement;
                if (ele != null)
                {
                    target.Add(ParsePropertySubElement(ele, name, parserContext));
                }
            }
            return target;
        }

        private bool ParseMergeAttribute(XmlElement collectionElement, ObjectDefinitionParserHelper helper)
        {
            string val = collectionElement.GetAttribute(ObjectDefinitionConstants.MergeAttribute);
            if (ObjectDefinitionConstants.DefaultValue.Equals(val))
            {
                val = helper.Defaults.Merge;
            }
            return ObjectDefinitionConstants.TrueValue.Equals(val);
        }

        /// <summary>
        /// Gets a set definition.
        /// </summary>
        /// <param name="collectionEle">
        /// The element describing the set definition.
        /// </param>
        /// <param name="name">
        /// The name of the object (definition) associated with the set definition.
        /// </param>
        /// <param name="parserContext">
        /// The namespace-aware parser.
        /// </param>
        /// <returns>The set definition.</returns>
        protected Set ParseSetElement(XmlElement collectionEle, string name, ParserContext parserContext)
        {
            string elementTypeName = GetAttributeValue(collectionEle, "element-type");
            XmlNodeList nl = collectionEle.ChildNodes;
            ManagedSet target = new ManagedSet(nl.Count);

            if (StringUtils.HasText(elementTypeName))
            {
                target.ElementTypeName = elementTypeName;
            }
            target.MergeEnabled = ParseMergeAttribute(collectionEle, parserContext.ParserHelper);

            foreach (XmlNode node in collectionEle.ChildNodes)
            {
                XmlElement ele = node as XmlElement;
                if (ele != null)
                {
                    object sub = ParsePropertySubElement(ele, name, parserContext);
                    target.Add(sub);
                }
            }
            return target;
        }

        /// <summary>
        /// Gets a dictionary definition.
        /// </summary>
        /// <param name="mapEle">The element describing the dictionary definition.</param>
        /// <param name="name">The name of the object (definition) associated with the dictionary definition.</param>
        /// <param name="parserContext">The namespace-aware parser.</param>
        /// <returns>The dictionary definition.</returns>
        protected IDictionary ParseDictionaryElement(XmlElement mapEle, string name, ParserContext parserContext)
        {
            ManagedDictionary dictionary = new ManagedDictionary();
            string keyTypeName = GetAttributeValue(mapEle, "key-type");
            string valueTypeName = GetAttributeValue(mapEle, "value-type");
            if (StringUtils.HasText(keyTypeName))
            {
                dictionary.KeyTypeName = keyTypeName;
            }
            if (StringUtils.HasText(valueTypeName))
            {
                dictionary.ValueTypeName = valueTypeName;
            }
            dictionary.MergeEnabled = ParseMergeAttribute(mapEle, parserContext.ParserHelper);

            XmlNodeList entryElements = SelectNodes(mapEle, ObjectDefinitionConstants.EntryElement);
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
                        XmlElement keyElement = (XmlElement)keyNode;
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
        [Obsolete("not used anymore - ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead", false)]
        protected virtual XmlNodeList SelectNodes(XmlElement element, string childElementName)
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
        [Obsolete("not used anymore - ObjectsNamespaceParser will be dropped with 2.x, use ObjectDefinitionParserHelper instead", false)]
        protected XmlNode SelectSingleNode(XmlElement element, string childElementName)
        {
            XmlNamespaceManager nsManager = new XmlNamespaceManager(new NameTable());
            nsManager.AddNamespace(GetNamespacePrefix(element), element.NamespaceURI);
            return element.SelectSingleNode(GetNamespacePrefix(element) + ":" + childElementName, nsManager);
        }

        /// <summary>
        /// Gets a name value collection mapping definition.
        /// </summary>
        /// <param name="nameValueEle">
        /// The element describing the name value collection mapping definition.
        /// </param>
        /// <param name="name">
        /// The name of the object (definition) associated with the
        /// name value collection mapping definition.
        /// </param>
        /// <param name="parserContext">the context carrying parsing state information</param>
        /// <returns>The name value collection definition.</returns>
        protected NameValueCollection ParseNameValueCollectionElement(XmlElement nameValueEle, string name, ParserContext parserContext)
        {
            ManagedNameValueCollection nvc = new ManagedNameValueCollection();
            nvc.MergeEnabled = ParseMergeAttribute(nameValueEle, parserContext.ParserHelper);

            XmlNodeList addElements = nameValueEle.GetElementsByTagName(ObjectDefinitionConstants.AddElement);
            foreach (XmlElement addElement in addElements)
            {
                string key = GetAttributeValue(addElement, ObjectDefinitionConstants.KeyAttribute);
                string value = GetAttributeValue(addElement, ObjectDefinitionConstants.ValueAttribute);
                string delimiters = GetAttributeValue(addElement, ObjectDefinitionConstants.DelimitersAttribute);

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
                    nvc.Add(key, value);
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
            if (element == null) return string.Empty;

            string innerText = element.InnerText;
            // check the xml:space attribute
            bool preserveWhitespace = 0 == string.Compare("preserve", element.GetAttribute("space", "http://www.w3.org/XML/1998/namespace"), true);
            bool isEmpty = preserveWhitespace ? innerText.Length == 0 : StringUtils.IsNullOrEmpty(innerText);
            return isEmpty ? String.Empty : innerText;
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
                    code = (DependencyCheckingMode)Enum.Parse(
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
                    mode = (AutoWiringMode)Enum.Parse(
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

//        /// <summary>
//        /// Returns the value of the element's attribute or <c>null</c>, if the attribute is not specified.
//        /// </summary>
//        /// <remarks>
//        /// This is a helper for bypassing the behavior of <see cref="XmlElement.GetAttribute(string)"/>
//        /// to return <see cref="string.Empty"/> if the attribute does not exist.
//        /// </remarks>
//        protected static string GetAttributeValue(XmlElement element, string attributeName)
//        {
//            if (element.HasAttribute(attributeName))
//            {
//                return element.GetAttribute(attributeName);
//            }
//            return null;
//        }
//
//        /// <summary>
//        /// Returns the value of the element's attribute or <paramref name="defaultValue"/>,
//        /// if the attribute is not specified.
//        /// </summary>
//        /// <remarks>
//        /// This is a helper for bypassing the behavior of <see cref="XmlElement.GetAttribute(string)"/>
//        /// to return <see cref="string.Empty"/> if the attribute does not exist.
//        /// </remarks>
//        protected static string GetAttributeValue(XmlElement element, string attributeName, string defaultValue)
//        {
//            if (element.HasAttribute(attributeName))
//            {
//                return element.GetAttribute(attributeName);
//            }
//            return defaultValue;
//        }
    }
}
