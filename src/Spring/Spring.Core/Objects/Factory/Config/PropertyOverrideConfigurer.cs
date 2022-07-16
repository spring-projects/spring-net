#region License

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

#endregion

using System.Collections.Specialized;
using System.Globalization;
using Common.Logging;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Overrides default values in one or more object definitions.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Instances of this class <b>override</b> already existing values, and is
	/// thus best suited to replacing defaults. If you need to <i>replace</i>
	/// placeholder values, consider using the
	/// <seea cref="Spring.Objects.Factory.Config.PropertyPlaceholderConfigurer"/>
	/// class instead.
	/// </p>
	/// <p>
	/// In contrast to the
	/// <see cref="Spring.Objects.Factory.Config.PropertyPlaceholderConfigurer"/>
	/// class, the original object definition can have default
	/// values or no values at all for such object properties. If an overriding
	/// configuration file does not have an entry for a certain object property,
	/// the default object value is left as is. Also note that it is not
	/// immediately obvious to discern which object definitions will be mutated by
	/// one or more
	/// <see cref="Spring.Objects.Factory.Config.PropertyOverrideConfigurer"/>s
	/// simply by looking at the object configuration.
	/// </p>
	/// <p>
	/// Each line in a referenced configuration file is expected to take the
	/// following form...
	/// </p>
	/// <code escaped="true">
	///		<add key="name.property" value="the override"/>
	/// </code>
	/// <p>
	/// The <c>name.property</c> key refers to the object name and the
	/// property that is to be overridden; and the value is the overridding
	/// value that will be inserted into the appropriate object definition's
	/// named property.
	/// </p>
	/// <p>
	/// Please note that in the case of multiple
	/// <see cref="Spring.Objects.Factory.Config.PropertyOverrideConfigurer"/>s
	/// that define different values for the same object definition value, the
	/// <b>last</b> overridden value will win (due to the fact that the values
	/// supplied by previous
	/// <see cref="Spring.Objects.Factory.Config.PropertyOverrideConfigurer"/>s
	/// will be overridden).
	/// </p>
	/// </remarks>
	/// <example>
	/// <p>
	/// The following XML context definition defines an object that has a number
	/// of properties, all of which have <b>default</b> values...
	/// </p>
	/// <code escaped="true">
	/// <object id="connStringProvider"
	///		type="MyNamespace.OracleConnectionStringProvider, MyAssembly">
	///		<property name="dataSource" value="MyOracleDB"/>
	///		<property name="userId" value="sa"/>
	///		<property name="password" value="g0ly4dk1n"/>
	///		<property name="integratedSecurity"	value="true"/>
	/// </object>
	/// </code>
	/// <p>
	/// What follows is a .NET config file snippet for the above example (assuming
	/// the need to override one of the default values)...
	/// </p>
	/// <code escaped="true">
	///	<name-values>
	///		<add key="database.userid" value="test"/>
	///		<add key="database.password" value="0bl0m0v"/>
	/// </name-values>
	/// </code>
	/// </example>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
	/// <seealso cref="Spring.Objects.Factory.Config.PropertyResourceConfigurer"/>
	/// <seealso cref="Spring.Objects.Factory.Config.PropertyPlaceholderConfigurer"/>
	/// <seealso cref="Spring.Objects.Factory.Config.IObjectDefinition"/>
    [Serializable]
    public class PropertyOverrideConfigurer : PropertyResourceConfigurer
	{
		private static readonly ILog _logger = LogManager.GetLogger(typeof (PropertyOverrideConfigurer));

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
		protected override void ProcessProperties(
			IConfigurableListableObjectFactory factory, NameValueCollection props)
		{
			foreach (string key in props.AllKeys)
			{
				ProcessKey(factory, key, props[key]);
			}
		}

		/// <summary>
		/// Process the given key as 'name.property' entry.
		/// </summary>
		/// <param name="factory">
		/// The object factory containing the object definitions that are to be
		/// processed.
		/// </param>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If an error occurs.
		/// </exception>
		/// <exception cref="Spring.Objects.FatalObjectException">
		/// If the property was not well formed (i.e. not in the format "name.property").
		/// </exception>
		protected virtual void ProcessKey(
			IConfigurableListableObjectFactory factory, string key, string value)
		{
			int dotIndex = key.IndexOf('.');
			if (dotIndex == -1)
			{
				throw new FatalObjectException(
					string.Format(CultureInfo.InvariantCulture,
					              "Invalid key '{0}': expected 'objectName.property' form.", key));
			}
			string name = key.Substring(0, dotIndex);
			string objectProperty = key.Substring(dotIndex + 1);
			IObjectDefinition definition = factory.GetObjectDefinition(name);
			if(definition != null)
			{
                PropertyValue pv = definition.PropertyValues.GetPropertyValue(objectProperty);
                if (pv != null && pv.Value is RuntimeObjectReference)
                {
                    definition.PropertyValues.Add(objectProperty, new RuntimeObjectReference(value));
                }
                else if (pv != null && pv.Value is ExpressionHolder)
                {
                    definition.PropertyValues.Add(objectProperty, new ExpressionHolder(value));
                }
                else
                {
                    definition.PropertyValues.Add(objectProperty, value);
                }
			}
			else
			{
				#region Instrumentation

				if (_logger.IsWarnEnabled)
				{
					_logger.Warn(string.Format(CultureInfo.InvariantCulture,
						"Cannot find object '{0}' when overriding properties; check configuration.", name));
				}

				#endregion
			}

			#region Instrumentation

			if (_logger.IsDebugEnabled)
			{
				_logger.Debug(string.Format(CultureInfo.InvariantCulture,
				                            "Property '{0}' set to '{1}'.", key, value));
			}

			#endregion
		}
	}
}
