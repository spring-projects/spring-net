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

using System.Configuration;
using System.Reflection;
using System.Xml;

using Common.Logging;
using Spring.Core;
using Spring.Core.TypeResolution;
using Spring.Reflection.Dynamic;
using Spring.Util;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
	/// Creates an <see cref="Spring.Context.IApplicationContext"/> instance
	/// using context definitions supplied in a custom configuration and
	/// configures the <see cref="ContextRegistry"/> with that instance.
	/// </summary>
	/// <remarks>
	/// Implementations of the <see cref="Spring.Context.IApplicationContext"/>
	/// interface <b>must</b> provide the following two constructors:
	/// <list type="number">
	/// <item>
	/// <description>
	/// A constructor that takes a string array of resource locations.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// A constructor that takes a reference to a parent application context
	/// and a string array of resource locations (and in that order).
	/// </description>
	/// </item>
	/// </list>
	/// <p>
	/// Note that if the <c>type</c> attribute is not present in the declaration
	/// of a particular context, then a default
	/// <see cref="Spring.Context.IApplicationContext"/> <see cref="System.Type"/>
	/// is assumed. This default
	/// <see cref="Spring.Context.IApplicationContext"/> <see cref="System.Type"/>
	/// is currently the <see cref="Spring.Context.Support.XmlApplicationContext"/>
	/// <see cref="System.Type"/>; please note the exact <see cref="System.Type"/>
	/// of this default <see cref="Spring.Context.IApplicationContext"/> is an
	/// implementation detail, that, while unlikely, may do so in the future.
	/// to
	/// </p>
	/// </remarks>
	/// <example>
	/// <p>
	/// This is an example of specifying a context that reads its resources from
	/// an embedded Spring.NET XML object configuration file...
	/// </p>
	/// <code escaped="true">
	/// <configuration>
	///     <configSections>
	///		    <sectionGroup name="spring">
	///			    <section name="context" type="Spring.Context.Support.ContextHandler, Spring.Core"/>
	///		    </sectionGroup>
	///     </configSections>
	///     <spring>
	///		    <context>
	///			    <resource uri="assembly://MyAssemblyName/MyResourceNamespace/MyObjects.xml"/>
	///		    </context>
	///     </spring>
	/// </configuration>
	/// </code>
	/// <p>
	/// This is an example of specifying a context that reads its resources from
	/// a custom configuration section within the same application / web
	/// configuration file and uses case insensitive object lookups. 
	/// </p>
	/// <p>
	/// Please note that you <b>must</b> adhere to the naming
	/// of the various sections (i.e. '&lt;sectionGroup name="spring"&gt;' and
	/// '&lt;section name="context"&gt;'.
	/// </p>
	/// <code escaped="true">
	/// <configuration>
	///     <configSections>
	///		    <sectionGroup name="spring">
	///			    <section name="context"
	///				    type="Spring.Context.Support.ContextHandler, Spring.Core"/>
	///			    <section name="objects"
	///				    type="Spring.Context.Support.DefaultSectionHandler, Spring.Core" />
	///		    </sectionGroup>
	///     </configSections>
	///     <spring>
	///		    <context
	///		        caseSensitive="false"
	///			    type="Spring.Context.Support.XmlApplicationContext, Spring.Core">
	///			    <resource uri="config://spring/objects"/>
	///		    </context>
	///		    <objects xmlns="http://www.springframework.net">
	///			    <!-- object definitions... -->
	///		    </objects>
	///     </spring>
	/// </configuration>
	/// </code>
	/// <p>
	/// And this is an example of specifying a hierarchy of contexts. The
	/// hierarchy in this case is only a simple parent->child hierarchy, but
	/// hopefully it illustrates the nesting of context configurations. This
	/// nesting of contexts can be arbitrarily deep, and is one way... child
	/// contexts know about their parent contexts, but parent contexts do not
	/// know how many child contexts they have (if any), or have references
	/// to any such child contexts.
	/// </p>
	/// <code escaped="true">
	/// <configuration>
	/// 	<configSections>
	/// 		<sectionGroup name='spring'>
	/// 			<section name='context'	type='Spring.Context.Support.ContextHandler, Spring.Core'/>
	/// 			<section name='objects'	type='Spring.Context.Support.DefaultSectionHandler, Spring.Core' />
    ///             <sectionGroup name="child">
	/// 			    <section name='objects' type='Spring.Context.Support.DefaultSectionHandler, Spring.Core' />
    ///             </sectionGroup>
	/// 		</sectionGroup>
	/// 	</configSections>
	/// 	
	///     <spring>
	/// 		<context name='Parent'>
	/// 			<resource uri='config://spring/objects'/>
	/// 		    <context name='Child'>
	/// 			    <resource uri='config://spring/childObjects'/>
	/// 		    </context>
	/// 		</context>
	/// 		<!-- parent context's objects -->
	/// 		<objects xmlns='http://www.springframework.net'>
	/// 			<object id='Parent' type='Spring.Objects.TestObject,Spring.Core.Tests'>
	///                 <property name='name' value='Parent'/>
	///             </object>
	/// 		</objects>
	/// 		<!-- child context's objects -->
    ///         <child>
    /// 		    <objects xmlns='http://www.springframework.net'>
	/// 			    <object id='Child' type='Spring.Objects.TestObject,Spring.Core.Tests'>
	///                     <property name='name' value='Child'/>
	///                 </object>
    /// 		    </objects>
    ///         </child>
	///     </spring>
	/// </configuration>
	/// </code>
	/// </example>
	/// <author>Mark Pollack</author>
	/// <author>Aleksandar Seovic</author>
	/// <author>Rick Evans</author>
	/// <seealso cref="ContextRegistry"/>
	public class ContextHandler : IConfigurationSectionHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(ContextHandler));
		
		/// <summary>
		/// The <see cref="System.Type"/> of <see cref="Spring.Context.IApplicationContext"/>
		/// created if no <c>type</c> attribute is specified on a <c>context</c> element.
		/// </summary>
		/// <seealso cref="GetContextType"/>
		protected virtual Type DefaultApplicationContextType
		{
			get { return typeof (XmlApplicationContext); }
		}

		/// <summary>
		/// Get the context's case-sensitivity to use if none is specified
		/// </summary>
		/// <remarks>
		/// <p>
		/// Derived handlers may override this property to change their default case-sensitivity.
		/// </p>
		/// <p>
		/// Defaults to 'true'.
		/// </p>
		/// </remarks>
		protected virtual bool DefaultCaseSensitivity
		{
			get { return true; }
		}
		
        /// <summary>
        /// Specifies, whether the instantiated context will be automatically registered in the 
        /// global <see cref="ContextRegistry"/>.
        /// </summary>
	    protected virtual bool AutoRegisterWithContextRegistry
	    {
            get { return true;  }
	    }

	    /// <summary>
	    /// Creates an <see cref="Spring.Context.IApplicationContext"/> instance
	    /// using the context definitions supplied in a custom
	    /// configuration section.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// This <see cref="Spring.Context.IApplicationContext"/> instance is
	    /// also used to configure the <see cref="ContextRegistry"/>.
	    /// </p>
	    /// </remarks>
	    /// <param name="parent">
	    /// The configuration settings in a corresponding parent
	    /// configuration section.
	    /// </param>
	    /// <param name="configContext">
	    /// The configuration context when called from the ASP.NET
	    /// configuration system. Otherwise, this parameter is reserved and
	    /// is <see langword="null"/>.
	    /// </param>
	    /// <param name="section">
	    /// The <see cref="System.Xml.XmlNode"/> for the section.
	    /// </param>
	    /// <returns>
	    /// An <see cref="Spring.Context.IApplicationContext"/> instance
	    /// populated with the object definitions supplied in the configuration
	    /// section.
	    /// </returns>
	    public object Create(object parent, object configContext, XmlNode section)
	    {
	    	XmlElement contextElement = section as XmlElement;

			#region Sanity Checks

	    	if (contextElement == null)
	    	{
	    		throw ConfigurationUtils.CreateConfigurationException(
	    			"Context configuration section must be an XmlElement.");
	    	}

	    	// sanity check on parent
	    	if ( (parent != null) && !(parent is IApplicationContext) )
	    	{
	    		throw ConfigurationUtils.CreateConfigurationException( 
	    			String.Format("Parent context must be of type IApplicationContext, but was '{0}'", parent.GetType().FullName));
	    	}

	    	#endregion
	        		    	
			// determine name of context to be created
			string contextName = GetContextName(configContext, contextElement);
			if (!StringUtils.HasLength(contextName))
			{
				contextName = AbstractApplicationContext.DefaultRootContextName;
			}
	    	
			#region Instrumentation
			if (Log.IsDebugEnabled) Log.Debug(string.Format("creating context '{0}'", contextName ) );
			#endregion
	    	
	    	IApplicationContext context = null;
	        try
	        {
	        	IApplicationContext parentContext = parent as IApplicationContext;
	        	
	            // determine context type
	        	Type contextType = GetContextType(contextElement, parentContext);

	        	// determine case-sensitivity
	        	bool caseSensitive = GetCaseSensitivity(contextElement);

	        	// get resource-list
	        	IList<string> resources = GetResources(contextElement);
	        	
	        	// finally create the context instance
				context = InstantiateContext(parentContext, configContext, contextName, contextType, caseSensitive, resources);
                // and register with global context registry
                if (AutoRegisterWithContextRegistry && !ContextRegistry.IsContextRegistered(context.Name))
                {
                    ContextRegistry.RegisterContext(context);                    
                }

				// get and create child context definitions
                IList<XmlNode> childContexts = GetChildContexts(contextElement);
				CreateChildContexts(context, configContext, childContexts);

	        	if (Log.IsDebugEnabled) Log.Debug( string.Format("context '{0}' created for name '{1}'", context, contextName) );
	        }
	        catch (Exception ex)
	        {
	            if (!ConfigurationUtils.IsConfigurationException(ex))
	            {
                    throw ConfigurationUtils.CreateConfigurationException(
                        String.Format("Error creating context '{0}': {1}", 
                        contextName, ReflectionUtils.GetExplicitBaseException(ex).Message), ex);
	            }
	            throw;
	        }
	        return context;
	    }

	    /// <summary>
	    /// Create all child-contexts in the given <see cref="XmlNodeList"/> for the given context.
	    /// </summary>
	    /// <param name="parentContext">The parent context to use</param>
	    /// <param name="configContext">The current configContext <see cref="IConfigurationSectionHandler.Create"/></param>
	    /// <param name="childContexts">The list of child context elements</param>
	    protected virtual void CreateChildContexts(IApplicationContext parentContext, object configContext, IList<XmlNode> childContexts)
		{	
			// create child contexts for 'the most recently created context'...
			foreach (XmlNode childContext in childContexts)
			{
				this.Create(parentContext, configContext, childContext);
			}
		}

		/// <summary>
		/// Instantiates a new context.
		/// </summary>
		protected virtual IApplicationContext InstantiateContext(IApplicationContext parentContext, object configContext, string contextName, Type contextType, bool caseSensitive, IList<string> resources)
		{
			IApplicationContext context;
			ContextInstantiator instantiator;
			
			if (parentContext == null)
			{
				instantiator = new RootContextInstantiator(contextType, contextName, caseSensitive, new List<string>(resources).ToArray());
			} 
			else
			{
                instantiator = new DescendantContextInstantiator(parentContext, contextType, contextName, caseSensitive, new List<string>(resources).ToArray());
			}
			
			if (IsLazy)
			{
				// TODO
			}
			context = instantiator.InstantiateContext();
			return context;
		}

		/// <summary>
		/// Gets the context's name specified in the name attribute of the context element.
		/// </summary>
		/// <param name="configContext">The current configContext <see cref="IConfigurationSectionHandler.Create"/></param>
		/// <param name="contextElement">The context element</param>
		protected virtual string GetContextName(object configContext, XmlElement contextElement)
		{
			string contextName;
			contextName = contextElement.GetAttribute(ContextSchema.NameAttribute);
			return contextName;
		}
		
		/// <summary>
		/// Extracts the context-type from the context element. 
		/// If none is specified, returns the parent's type.
		/// </summary>
		private Type GetContextType(XmlElement contextElement, IApplicationContext parentContext)
		{
			Type contextType;
			if (parentContext != null)
			{
				// set default context type to parent's type (allows for type inheritance)
				contextType = GetConfiguredContextType(contextElement, parentContext.GetType());
			}
			else
			{
				contextType = GetConfiguredContextType(contextElement, this.DefaultApplicationContextType);
			}
			return contextType;
		}

		/// <summary>
		/// Extracts the case-sensitivity attribute from the context element
		/// </summary>
		private  bool GetCaseSensitivity(XmlElement contextElement)
		{
			bool caseSensitive = DefaultCaseSensitivity;
			
			string caseSensitiveAttr = contextElement.GetAttribute(ContextSchema.CaseSensitiveAttribute);
			if (StringUtils.HasText(caseSensitiveAttr))
			{
				caseSensitive = Boolean.Parse(caseSensitiveAttr);
			}
			return caseSensitive;
		}

		/// <summary>
	    /// Gets the context <see cref="System.Type"/> specified in the type
	    /// attribute of the context element.
	    /// </summary>
	    /// <remarks>
	    /// <p>
	    /// If this attribute is not defined it defaults to the
	    /// <see cref="Spring.Context.Support.XmlApplicationContext"/> type.
	    /// </p>
	    /// </remarks>
	    /// <exception cref="TypeMismatchException">
	    /// If the context type does not implement the
	    /// <see cref="Spring.Context.IApplicationContext"/> interface.
	    /// </exception>
	    private Type GetConfiguredContextType(XmlElement contextElement, Type defaultContextType)
	    {
            string typeName = contextElement.GetAttribute(ContextSchema.TypeAttribute);
			
            if (StringUtils.IsNullOrEmpty(typeName))
            {
                return defaultContextType;
            }
            else
            {
                Type type = TypeResolutionUtils.ResolveType(typeName);
                if (typeof(IApplicationContext).IsAssignableFrom(type))
                {
                    return type;
                }
                else
                {
                    throw new TypeMismatchException( type.Name + " does not implement IApplicationContext.");
                }
            }
	    }

	    /// <summary>
	    /// Returns <see langword="true"/> if the context should be lazily
	    /// initialized.
	    /// </summary>
	    private bool IsLazy
	    {
	        get { return false; }
	    }

	    /// <summary>
	    /// Returns the array of resources containing object definitions for
	    /// this context.
	    /// </summary>
	    private IList<string> GetResources( XmlElement contextElement )
	    {
            List<string> resourceNodes = new List<string>(contextElement.ChildNodes.Count);
	        foreach (XmlNode possibleResourceNode in contextElement.ChildNodes)
	        {
	            XmlElement possibleResourceElement = possibleResourceNode as XmlElement;
	            if(possibleResourceElement != null &&
	               possibleResourceElement.LocalName == ContextSchema.ResourceElement)
	            {
	                string resourceName = possibleResourceElement.GetAttribute(ContextSchema.URIAttribute);
	                if(StringUtils.HasText(resourceName))
	                {
	                    resourceNodes.Add(resourceName);
	                }
	            }
	        }
	        return resourceNodes;
	    }

        /// <summary>
        /// Returns the array of child contexts for this context.
        /// </summary>
        private IList<XmlNode> GetChildContexts(XmlElement contextElement)
        {
            List<XmlNode> contextNodes = new List<XmlNode>(contextElement.ChildNodes.Count);
            foreach (XmlNode possibleContextNode in contextElement.ChildNodes)
            {
                XmlElement possibleContextElement = possibleContextNode as XmlElement;
                if (possibleContextElement != null &&
                   possibleContextElement.LocalName == ContextSchema.ContextElement)
                {
                    contextNodes.Add(possibleContextElement);
                }
            }
            return contextNodes;
        }

	    #region Inner Class : ContextInstantiator

	    private abstract class ContextInstantiator
	    {
	        protected ContextInstantiator(
	            Type contextType, string contextName, bool caseSensitive, string[] resources)
	        {
	            _contextType = contextType;
	            _contextName = contextName;
                _caseSensitive = caseSensitive;
	            _resources = resources;
	        }

            public IApplicationContext InstantiateContext()
            {
                ConstructorInfo ctor = GetContextConstructor();
                if (ctor == null)
                {
                    string errorMessage = "No constructor with string[] argument found for context type [" + ContextType.Name + "]";
                    throw ConfigurationUtils.CreateConfigurationException(errorMessage);
                }
                IApplicationContext context = InvokeContextConstructor(ctor);
                return context;
            }

	        protected abstract ConstructorInfo GetContextConstructor();

	        protected abstract IApplicationContext InvokeContextConstructor(
	            ConstructorInfo ctor);

	        protected Type ContextType
	        {
	            get { return _contextType; }
	        }

	        protected string ContextName
	        {
	            get { return _contextName; }
	        }

	        protected bool CaseSensitive
	        {
	            get { return _caseSensitive; }
	        }

	        protected IList<string> Resources
	        {
	            get { return _resources; }
	        }

	        private Type _contextType;
	        private string _contextName;
            private bool _caseSensitive;
	        private IList<string> _resources;
	    }

	    #endregion

	    #region Inner Class : RootContextInstantiator

	    private sealed class RootContextInstantiator : ContextInstantiator
	    {
	        public RootContextInstantiator(
	            Type contextType, string contextName, bool caseSensitive, string[] resources)
	            : base(contextType, contextName, caseSensitive, resources)
	        {
	        }

	        protected override ConstructorInfo GetContextConstructor()
	        {
	            return ContextType.GetConstructor(new Type[] {typeof(string), typeof(bool), typeof(string[])});
	        }

	        protected override IApplicationContext InvokeContextConstructor(
	            ConstructorInfo ctor)
	        {
                return (IApplicationContext)(new SafeConstructor(ctor).Invoke(new object[] {ContextName, CaseSensitive, Resources}));
	        }
	    }

	    #endregion

	    #region Inner Class : DescendantContextInstantiator

	    private sealed class DescendantContextInstantiator : ContextInstantiator
	    {
	        public DescendantContextInstantiator(
	            IApplicationContext parentContext, Type contextType,
	            string contextName, bool caseSensitive, string[] resources)
	            : base(contextType, contextName, caseSensitive, resources)
	        {
	            this.parentContext = parentContext;
	        }

	        protected override ConstructorInfo GetContextConstructor()
	        {
	            return ContextType.GetConstructor(
	                new Type[] {typeof(string), typeof(bool), typeof(IApplicationContext), typeof(string[])});
	        }

	        protected override IApplicationContext InvokeContextConstructor(
	            ConstructorInfo ctor)
	        {
                return (IApplicationContext)(new SafeConstructor(ctor).Invoke(new object[] {ContextName, CaseSensitive, this.parentContext, Resources}));
	        }

	        private IApplicationContext parentContext;
	    }

	    #endregion

	    #region Context Schema Constants

	    /// <summary>
	    /// Constants defining the structure and values associated with the
	    /// schema for laying out Spring.NET contexts in XML.
	    /// </summary>
	    private sealed class ContextSchema
	    {
	        /// <summary>
	        /// Defines a single
	        /// <see cref="Spring.Context.IApplicationContext"/>.
	        /// </summary>
	        public const string ContextElement = "context";

	        /// <summary>
	        /// Specifies a context name.
	        /// </summary>
	        public const string NameAttribute = "name";

            /// <summary>
            /// Specifies if context should be case sensitive or not. Default is <c>true</c>.
            /// </summary>
            public const string CaseSensitiveAttribute = "caseSensitive";

	        /// <summary>
	        /// Specifies a <see cref="System.Type"/>.
	        /// </summary>
	        /// <remarks>
	        /// <p>
	        /// Does not have to be fully assembly qualified, but its generally regarded
	        /// as better form if the <see cref="System.Type"/> names of one's objects
	        /// are specified explicitly.
	        /// </p>
	        /// </remarks>
	        public const string TypeAttribute = "type";

	        /// <summary>
	        /// Specifies whether context should be lazy initialized.
	        /// </summary>
	        public const string LazyAttribute = "lazy";

	        /// <summary>
	        /// Defines an <see cref="Spring.Core.IO.IResource"/>
	        /// </summary>
	        public const string ResourceElement = "resource";

	        /// <summary>
	        /// Specifies the URI for an
	        /// <see cref="Spring.Core.IO.IResource"/>.
	        /// </summary>
	        public const string URIAttribute = "uri";
	    }

	    #endregion
	}
}
