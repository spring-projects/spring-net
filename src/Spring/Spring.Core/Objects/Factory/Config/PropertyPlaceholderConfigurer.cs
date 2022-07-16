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

using System.Collections.Specialized;
using System.Globalization;

using Common.Logging;

using Spring.Collections;
using Spring.Util;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Resolves placeholder values in one or more object definitions.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The default placeholder syntax follows the NAnt style: <c>${...}</c>.
	/// Instances of this class can be configured in the same way as any other
	/// object in a Spring.NET container, and so custom placeholder prefix
	/// and suffix values can be set via the <see cref="PlaceholderPrefix"/>
	/// and <see cref="PlaceholderSuffix"/> properties.
	/// </p>
	/// <example>
	/// <p>
	/// The following example XML context definition defines an object that has
	/// a number of placeholders. The placeholders can easily be distinguished
	/// by the presence of the <c>${}</c> characters.
	/// </p>
	/// <code escaped="true">
	/// <object id="connStringProvider"
	///		type="MyNamespace.OracleConnectionStringProvider, MyAssembly">
	///		<property name="dataSource" value="${database.datasource}"/>
	///		<property name="userId" value="${database.userid}"/>
	///		<property name="password" value="${database.password}"/>
	///		<property name="integratedSecurity"
	///			value="${database.integratedsecurity}"/>
	/// </object>
	/// </code>
	/// <p>
	/// The associated XML configuration file for the above example containing the
	/// values for the placeholders would contain a snippet such as ..
	/// </p>
	/// <code escaped="true">
	///	<name-values>
	///		<add key="database.datasource" value="MyOracleDB"/>
	///		<add key="database.userid" value="sa"/>
	///		<add key="database.password" value="g0ly4dk1n"/>
	///		<add key="database.integratedsecurity" value="true"/>
	/// </name-values>
	/// </code>
	/// <p>
	/// The preceding XML snippet listing the various property keys and their
	/// associated values needs to be inserted into the .NET config file of
	/// your application (or Web.config file for your ASP.NET web application,
	/// as the case may be), like so...
	/// </p>
	/// <code escaped="true">
	///	<name-values>
	///		<add key="database.datasource" value="MyOracleDB"/>
	///		<add key="database.userid" value="sa"/>
	///		<add key="database.password" value="g0ly4dk1n"/>
	///		<add key="database.integratedsecurity" value="true"/>
	/// </name-values>
	/// </code>
	/// </example>
	/// <p>
	/// <see cref="Spring.Objects.Factory.Config.PropertyPlaceholderConfigurer"/>
	/// checks simple property values, lists, dictionaries, sets, constructor
	/// values, object type name, and object names in
	/// runtime object references (
	/// <see cref="Spring.Objects.Factory.Config.RuntimeObjectReference"/>).
	/// Furthermore, placeholder values can also cross-reference other
	/// placeholders, in the manner of the following example where the
	/// <c>rootPath</c> property is cross-referenced by the <c>subPath</c>
	/// property.
	/// </p>
	/// <example>
	/// <code escaped="true">
	/// <name-values>
	///		<add key="rootPath" value="myrootdir"/>
	///		<add key="subPath" value="${rootPath}/subdir"/>
	/// </name-values>
	/// </code>
	/// </example>
	/// <p>
	/// In contrast to the
	/// <see cref="Spring.Objects.Factory.Config.PropertyOverrideConfigurer"/>
	/// class, this configurer only permits the replacement of explicit
	/// placeholders in object definitions. Therefore, the original definition
	/// cannot specify any default values for its object properties, and the
	/// placeholder configuration file is expected to contain an entry for each
	/// defined placeholder. That is, if an object definition contains a
	/// placeholder <c>${foo}</c>, there should be an associated
	/// <c>&lt;add key="foo" value="..."/&gt;</c> entry in the
	/// referenced placeholder configuration file. Default property values
	/// can be defined via the inherited
	/// <see cref="Spring.Objects.Factory.Config.PropertyResourceConfigurer.Properties"/>
	/// collection to overcome any perceived limitation of this feature.
	/// </p>
	/// <p>
	/// If a configurer cannot resolve a placeholder, and the value of the
	/// <see cref="Spring.Objects.Factory.Config.PropertyPlaceholderConfigurer.IgnoreUnresolvablePlaceholders"/>
	/// property is currently set to <see langword="false"/>, an
	/// <see cref="Spring.Objects.Factory.ObjectDefinitionStoreException"/>
	/// will be thrown. If you want to resolve properties from multiple configuration
	/// resources, simply specify multiple resources via the
	/// <see cref="Spring.Objects.Factory.Config.PropertyResourceConfigurer.Locations"/>
	/// property. Finally, please note that you can also define multiple
	/// <see cref="Spring.Objects.Factory.Config.PropertyPlaceholderConfigurer"/>
	/// instances, each with their own custom placeholder syntax.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
	/// <seealso cref="Spring.Objects.Factory.Config.PropertyResourceConfigurer"/>
	/// <seealso cref="Spring.Objects.Factory.Config.PropertyOverrideConfigurer"/>
	/// <seealso cref="Spring.Objects.Factory.Config.IObjectDefinition"/>
    [Serializable]
    public class PropertyPlaceholderConfigurer : PropertyResourceConfigurer
	{
		/// <summary>
		/// The default placeholder prefix.
		/// </summary>
		public static readonly string DefaultPlaceholderPrefix = "${";

		/// <summary>
		/// The default placeholder suffix.
		/// </summary>
		public static readonly string DefaultPlaceholderSuffix = "}";


		private readonly ILog logger;


		private bool ignoreUnresolvablePlaceholders = false;
		private string placeholderPrefix = DefaultPlaceholderPrefix;
		private string placeholderSuffix = DefaultPlaceholderSuffix;
		private EnvironmentVariableMode environmentVariableMode = EnvironmentVariableMode.Fallback;
	    private bool includeAncestors;

        /// <summary>
        /// Initializes the new instance
        /// </summary>
	    public PropertyPlaceholderConfigurer()
	    {
            logger = LogManager.GetLogger(this.GetType());
	    }

	    #region Properties

        /// <summary>
		/// The placeholder prefix (the default is <c>${</c>).
		/// </summary>
		/// <seealso cref="DefaultPlaceholderPrefix"/>
		public string PlaceholderPrefix
		{
			set { placeholderPrefix = value; }
		}

		/// <summary>
		/// The placeholder suffix (the default is <c>}</c>)
		/// </summary>
		/// <seealso cref="DefaultPlaceholderSuffix"/>
		public string PlaceholderSuffix
		{
			set { placeholderSuffix = value; }
		}

		/// <summary>
		/// Indicates whether unresolved placeholders should be ignored.
		/// </summary>
		public bool IgnoreUnresolvablePlaceholders
		{
            get { return ignoreUnresolvablePlaceholders; }
            set { ignoreUnresolvablePlaceholders = value; }
		}

		/// <summary>
		/// Controls how environment variables will be used to
		/// replace property placeholders.
		/// </summary>
		/// <remarks>
		/// <p>
		/// See the overview of the
		/// <see cref="Spring.Objects.Factory.Config.EnvironmentVariableMode"/>
		/// enumeration for the available options.
		/// </p>
		/// </remarks>
		public EnvironmentVariableMode EnvironmentVariableMode
		{
			set { environmentVariableMode = value; }
        }

	    public bool IncludeAncestors
	    {
            set { includeAncestors = value; }
	    }

        #endregion

	    /// <summary>
	    /// Apply the given properties to the supplied
	    /// <see cref="Spring.Objects.Factory.Config.IConfigurableListableObjectFactory"/>.
	    /// </summary>
	    /// <param name="factory">
	    /// The <see cref="Spring.Objects.Factory.Config.IConfigurableListableObjectFactory"/>
	    /// used by the application context.
	    /// </param>
	    /// <param name="props">The properties to apply.</param>
	    /// <exception cref="Spring.Objects.ObjectsException">
	    /// If an error occured.
	    /// </exception>
	    protected override void ProcessProperties(IConfigurableListableObjectFactory factory, NameValueCollection props)
	    {
	        PlaceholderResolveHandlerAdapter resolveAdapter = new PlaceholderResolveHandlerAdapter(this, props);
	        ObjectDefinitionVisitor visitor = new ObjectDefinitionVisitor(resolveAdapter.ParseAndResolveVariables);

	        var objectDefinitionNames = factory.GetObjectDefinitionNames(includeAncestors);
	        for (int i = 0; i < objectDefinitionNames.Count; ++i)
	        {
	            string name = objectDefinitionNames[i];
	            IObjectDefinition definition = factory.GetObjectDefinition(name, includeAncestors);

	            if (definition == null)
	            {
	                logger.ErrorFormat("'{0}' can't be found in factorys'  '{1}' object definition (includeAncestor {2})",
	                                   name, factory, includeAncestors);
	                continue;
	            }

	            try
	            {
	                visitor.VisitObjectDefinition(definition);
	            }
	            catch (ObjectDefinitionStoreException ex)
	            {
	                throw new ObjectDefinitionStoreException(
	                    definition.ResourceDescription, name, ex.Message);
	            }
	        }

	        factory.AddEmbeddedValueResolver(resolveAdapter);
	    }

	    /// <summary>
		/// Parse values recursively to be able to resolve cross-references between
		/// placeholder values.
		/// </summary>
		/// <param name="properties">
		/// The map of constructor arguments / property values.
		/// </param>
		/// <param name="strVal">The string to be resolved.</param>
        /// <param name="visitedPlaceholders">The placeholders that have already been visited
        /// during the current resolution attempt (used to detect circular references
        /// between placeholders). Only non-null if we're parsing a nested placeholder.</param>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If an error occurs.
		/// </exception>
		/// <returns>The resolved string.</returns>
		public virtual string ParseString(
			NameValueCollection properties, string strVal, ISet visitedPlaceholders)
		{
			int startIndex = strVal.IndexOf(placeholderPrefix);
			while (startIndex != -1)
			{
				int endIndex = strVal.IndexOf(
					placeholderSuffix, startIndex + placeholderPrefix.Length);
				if (endIndex != -1)
				{
					int pos = startIndex + placeholderPrefix.Length;
					string placeholder = strVal.Substring(pos, endIndex - pos);
                    if (visitedPlaceholders.Contains(placeholder))
                    {
                        throw new ObjectDefinitionStoreException(
                                    string.Format(
                                        CultureInfo.InvariantCulture,
                                        "Circular placeholder reference '{0}' detected " +
                                            "in property definitions [{1}].",
                                        placeholder, properties));
                    }
				    visitedPlaceholders.Add(placeholder);
					string resolvedValue = ResolvePlaceholder(placeholder, properties, environmentVariableMode);
					if (resolvedValue != null)
					{
                        resolvedValue = ParseString(properties, resolvedValue, visitedPlaceholders);

						#region Instrumentation

						if (logger.IsDebugEnabled)
						{
							logger.Debug(string.Format(
								CultureInfo.InvariantCulture,
								"Resolving placeholder '{0}' to '{1}'.", placeholder, resolvedValue));
						}

						#endregion

						strVal = strVal.Substring(0, startIndex) + resolvedValue + strVal.Substring(endIndex + 1);
						startIndex = strVal.IndexOf(placeholderPrefix, startIndex + resolvedValue.Length);
					}
					else if (ignoreUnresolvablePlaceholders)
					{
						// simply return the unprocessed value...
						return strVal;
					}
					else
					{
						throw new ObjectDefinitionStoreException(string.Format(
								CultureInfo.InvariantCulture,
								"Could not resolve placeholder '{0}'.", placeholder));
					}
				    visitedPlaceholders.Remove(placeholder);
				}
				else
				{
					startIndex = -1;
				}
			}
			return strVal;
		}

		/// <summary>
		/// Resolve the given placeholder using the given name value collection,
		/// performing an environment variables check according to the given mode.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The default implementation delegates to
		/// <see cref="ResolvePlaceholder(string, NameValueCollection)"/>
		/// before/afer the environment variable check. Subclasses can override
		/// this for custom resolution strategies, including customized points
		/// for the environment properties check.
		/// </p>
		/// </remarks>
		/// <param name="placeholder">The placeholder to resolve</param>
		/// <param name="props">
		/// The merged name value collection of this configurer.
		/// </param>
		/// <param name="mode">The environment variable mode.</param>
		/// <returns>
		/// The resolved value or <see langword="null"/> if none.
		/// </returns>
		/// <seelso cref="Spring.Objects.Factory.Config.EnvironmentVariableMode"/>
		protected virtual string ResolvePlaceholder(string placeholder,
		                                            NameValueCollection props,
		                                            EnvironmentVariableMode mode)
		{
			string propertyValue = null;
			if (mode == Spring.Objects.Factory.Config.EnvironmentVariableMode.Override)
			{
				propertyValue = Environment.GetEnvironmentVariable(placeholder);
			}
			if (propertyValue == null)
			{
				propertyValue = ResolvePlaceholder(placeholder, props);
			}
			if (propertyValue == null
				&& mode == Spring.Objects.Factory.Config.EnvironmentVariableMode.Fallback)
			{
				propertyValue = Environment.GetEnvironmentVariable(placeholder);
			}
			return propertyValue;
		}

		/// <summary>
		/// Resolve the given placeholder using the given name value collection.
		/// </summary>
		/// <remarks>
		/// <p>
		/// This (the default) implementation simply looks up the value of the
		/// supplied <paramref name="placeholder"/> key.
		/// </p>
		/// <p>
		/// Subclasses can override this for customized placeholder-to-key
		/// mappings or custom resolution strategies, possibly just using the
		/// given name value collection as fallback.
		/// </p>
		/// </remarks>
		/// <param name="placeholder">The placeholder to resolve.</param>
		/// <param name="props">
		/// The merged name value collection of this configurer.
		/// </param>
		/// <returns>The resolved value.</returns>
		protected virtual string ResolvePlaceholder(
			string placeholder, NameValueCollection props)
		{
			return props[placeholder];
		}

        #region Helper class

        private class PlaceholderResolveHandlerAdapter : IStringValueResolver
        {
            private readonly PropertyPlaceholderConfigurer ppc;
            private readonly NameValueCollection props;

            public PlaceholderResolveHandlerAdapter(PropertyPlaceholderConfigurer outerPPC, NameValueCollection props)
            {
                ppc = outerPPC;
                this.props = props;
            }

            public string ParseAndResolveVariables(string name)
            {
                return ppc.ParseString(props, name, new HashedSet());
            }
        }

        #endregion
    }
}
