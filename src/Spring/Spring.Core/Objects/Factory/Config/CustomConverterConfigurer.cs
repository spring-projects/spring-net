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

using System.Collections;
using System.ComponentModel;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
	/// implementation that allows for convenient registration of custom
	/// <see cref="System.ComponentModel.TypeConverter"/>s.
	/// </summary>
	/// <remarks>
	/// <note>
	/// The use of this class is <i>typically</i> not required; the .NET
	/// mechanism of associating a
	/// <see cref="System.ComponentModel.TypeConverter"/> with a
	/// <see cref="System.Type"/>  via the use of the
	/// <see cref="System.ComponentModel.TypeConverterAttribute"/> is the
	/// recommended (and standard) way. This class primarily exists to cover
	/// those cases where third party classes to which one does not have the
	/// source need to be exposed to the type conversion mechanism.
	/// </note>
	/// <p>
	/// Because the
	/// <see cref="Spring.Objects.Factory.Config.CustomConverterConfigurer"/>
	/// class implements the
	/// <see cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
	/// interface, instances of this class that have been exposed in the
	/// scope of an
	/// <see cref="Spring.Context.IApplicationContext"/> will
	/// <i>automatically</i> be picked up by the application context and made
	/// available to the IoC container whenever type conversion is required. If
	/// one is using a
	/// <see cref="Spring.Objects.Factory.Config.CustomConverterConfigurer"/>
	/// object definition within the scope of an
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/>, no such automatic
	/// pickup of the
	/// <see cref="Spring.Objects.Factory.Config.CustomConverterConfigurer"/>
	/// is performed (custom converters will have to be added manually using the
	/// <see cref="Spring.Objects.Factory.Config.IConfigurableObjectFactory.RegisterCustomConverter(Type, TypeConverter)"/>
	/// method). For <i>most</i> application scenarios, one will get better
	/// mileage using the <see cref="Spring.Context.IApplicationContext"/>
	/// abstraction.
	/// </p>
	/// </remarks>
	/// <example>
	/// <p>
	/// The following examples all assume XML based configuration, and use
	/// inner object definitions to define the custom
	/// <see cref="System.ComponentModel.TypeConverter"/> objects (nominally to
	/// avoid polluting the object name space, but also because the
	/// configuration simply reads better that way).
	/// </p>
	/// <code escaped="true">
	/// <object id="customConverterConfigurer"
	///	  type="Spring.Objects.Factory.Config.CustomConverterConfigurer, Spring.Core">
	///   <property name="CustomConverters">
	///     <dictionary>
	///       <entry key="System.Date">
	///         <object type="MyNamespace.MyCustomDateEditor"/>
	///       </entry>
	///       <entry key="MyNamespace.MyObject, MyAssembly">
	///         <object id="myConverter"
	///           type="MyNamespace.MObjectConverter, MyOtherAssembly">
	///           <property name="aProperty" value="..."/>
	///         </object>
	///       </entry>
	///     </dictionary>
	///   </property>
	/// </object>
	/// </code>
	/// <p>
	/// The following example illustrates a complete (albeit naieve) use case
	/// for this class, including a custom
	/// <see cref="System.ComponentModel.TypeConverter"/> implementation, said
	/// converters domain class, and the XML configuration that hooks the
	/// converter in place and makes it available to a Spring.NET container for
	/// use during object resolution.
	/// </p>
	/// <p>
	/// The domain class is a simple data-only object that contains the data
	/// required to send an email message (such as the host and user account
	/// name). A developer would prefer to use a string of the form
	/// <c>UserName=administrator,Password=r1l0k1l3y,Host=localhost</c> to
	/// configure the mail settings and just let the container take care of the
	/// conversion.
	/// </p>
	/// <code language="C#">
	/// namespace ExampleNamespace
	/// {
	///		public sealed class MailSettings
	///		{
	///			private string _userName;
	///			private string _password;
	///			private string _host;
	///
	///			public string Host
	///			{
	///				get { return _host; }
	///				set { _host = value; }
	///			}
	///
	///		 	public string UserName
	///			{
	///				get { return _userName; }
	///				set { _userName = value; }
	///			}
	///
	///			public string Password
	///			{
	///				get { return _password; }
	///				set { _password = value; }
	///			}
	///		}
	///
	///		public sealed class MailSettingsConverter : TypeConverter
	///		{
	///			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	///		 	{
	/// 			if (typeof (string) == sourceType)
	/// 			{
	/// 				return true;
	/// 			}
	/// 			return base.CanConvertFrom(context, sourceType);
	///			}
	///
	/// 		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	/// 		{
	/// 			string text = value as string;
	/// 			if(text != null)
	/// 			{
	/// 				MailSettings mailSettings = new MailSettings();
	/// 				string[] tokens = text.Split(',');
	/// 				for (int i = 0; i &lt; tokens.Length; ++i)
	/// 				{
	/// 					string token = tokens[i];
	/// 					string[] settings = token.Split('=');
	/// 					typeof(MailSettings).GetProperty(settings[0])
	/// 						.SetValue(mailSettings, settings[1], null);
	/// 				}
	/// 				return mailSettings;
	///				}
	///				return base.ConvertFrom(context, culture, value);
	///			}
	///		}
	///
	///		// a very naieve class that uses the MailSettings class...
	///		public sealed class ExceptionLogger
	///		{
	///			private MailSettings _mailSettings;
	///
	///			public MailSettings MailSettings {
	///			{
	///				set { _mailSettings = value; }
	///			}
	///
	///			public void Log(object value)
	///			{
	///				Exception ex = value as Exception;
	///				if(ex != null)
	///				{
	///					// use _mailSettings instance...
	///				}
	///			}
	///		}
	///	}
	/// </code>
	/// <p>
	/// The attendant XML configuration for the above classes would be...
	/// </p>
	/// <code escaped="true">
	/// <object id="emailingExceptionLogger" type="ExampleNamespace.ExceptionLogger, MyAssembly">
	/// 	<property name="MailSettings" value="UserName=administrator,Password=r1l0k1l3y,Host=localhost" />
	/// </object>
	/// <object id="customConverterConfigurer" type="Spring.Objects.Factory.Config.CustomConverterConfigurer, Spring.Core">
	/// 	<property name="CustomConverters">
	/// 		<dictionary>
	/// 			<entry key="ExampleNamespace.MailSettings, MyAssembly">
	/// 				<object type="ExampleNamespace.MailSettingsConverter, MyAssembly" />
	/// 			</entry>
	/// 		</dictionary>
	/// 	</property>
	/// </object>
	/// </code>
	/// </example>
	/// <author>Juergen Hoeller</author>
	/// <author>Simon White (.NET)</author>
	/// <seealso cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor"/>
	/// <seealso cref="Spring.Context.IApplicationContext"/>
	/// <seealso cref="Spring.Objects.Factory.Config.IConfigurableObjectFactory.RegisterCustomConverter(Type, TypeConverter)"/>
    [Serializable]
    public class CustomConverterConfigurer : AbstractConfigurer
	{
		private IDictionary _customConverters;

		/// <summary>
		/// The custom converters to register.
		/// </summary>
		/// <remarks>
		/// <p>
		/// The <see cref="System.Collections.IDictionary"/> uses the type name
		/// of the class that requires conversion as the key, and an
		/// <b>instance</b> of the
		/// <see cref="System.ComponentModel.TypeConverter"/> that will effect
		/// the conversion. Alternatively, the actual
		/// <see cref="System.Type"/> of the class that requires conversion
		/// can be used as the key.
		/// </p>
		/// </remarks>
		/// <example>
		/// <p>
		/// <code language="C#">
		/// IDictionary converters = new Hashtable();
		/// converters.Add( "System.Date", new MyCustomDateConverter() );
		/// // a System.Type instance can also be used as the key...
		/// converters.Add( typeof(Color), new MyCustomRBGColorConverter() );
		/// </code>
		/// </p>
		/// </example>
		public IDictionary CustomConverters
		{
			set { this._customConverters = value; }
		}

		/// <summary>
		/// Registers any custom converters with the supplied
		/// <paramref name="factory"/>.
		/// </summary>
		/// <param name="factory">
		/// The object factory to register the converters with.
		/// </param>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In case of errors.
		/// </exception>
		public override void PostProcessObjectFactory(
			IConfigurableListableObjectFactory factory)
		{
			if (_customConverters != null)
			{
				foreach (DictionaryEntry entry in _customConverters)
				{
					Type requiredType = ResolveRequiredType(entry.Key, "key", "custom type converter");
					TypeConverter converter = ResolveConverter(entry.Value);
					factory.RegisterCustomConverter(requiredType, converter);
				}
			}
		}

		/// <summary>
		/// Resolves the supplied <paramref name="value"/> into a
		/// <see cref="System.ComponentModel.TypeConverter"/> instance.
		/// </summary>
		/// <param name="value">
		/// The object that is to be resolved into a
		/// <see cref="System.ComponentModel.TypeConverter"/> instance.
		/// </param>
		/// <returns>
		/// A resolved <see cref="System.ComponentModel.TypeConverter"/> instance.
		/// </returns>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// If the supplied <paramref name="value"/> is <see langword="null"/>,
		/// or the supplied <paramref name="value"/> cannot be resolved.
		/// </exception>
		protected virtual TypeConverter ResolveConverter(object value)
		{
			TypeConverter converter = value as TypeConverter;
			if (converter == null)
			{
				throw new ObjectInitializationException(
					"Mapped value for custom converter is not a " +
						"[System.ComponentModel.TypeConverter] instance.");
			}
			return converter;
		}

	}
}
