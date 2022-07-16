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

#region Imports

using System.Globalization;
using Spring.Objects;
using Spring.Objects.Factory.Support;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
	/// <see cref="Spring.Context.IApplicationContext"/> that allows concrete registration of
	/// objects and messages in code, rather than from external configuration sources.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Mainly useful for testing.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	public class StaticApplicationContext : GenericApplicationContext
	{
		/// <summary>
		/// Creates a new instance of the StaticApplicationContext class.
		/// </summary>
		public StaticApplicationContext() : this( null ) {}

		/// <summary>
		/// Creates a new instance of the StaticApplicationContext class.
		/// </summary>
		/// <param name="parentContext">The parent application context.</param>
		public StaticApplicationContext( IApplicationContext parentContext )
			: base( null, true, parentContext ) 
		{			
			RegisterSingleton( MessageSourceObjectName, typeof( StaticMessageSource ), null );
		}

		/// <summary>
		/// Creates a new, named instance of the StaticApplicationContext class.
		/// </summary>
		/// <param name="name">the context name</param>
		/// <param name="parentContext">The parent application context.</param>
		public StaticApplicationContext( string name, IApplicationContext parentContext )
			: base( name, true, parentContext ) 
		{			
			RegisterSingleton( MessageSourceObjectName, typeof( StaticMessageSource ), null );
		}

		/// <summary>
		/// Do nothing: we rely on callers to update our public methods.
		/// </summary>
		protected override void RefreshObjectFactory() {}

		/// <summary>
		/// Register a singleton object with the default object factory.
		/// </summary>
		/// <param name="name">The name of the object.</param>
		/// <param name="classType">The <see cref="System.Type"/> of the object.</param>
		/// <param name="propertyValues">The property values for the singleton instance.</param>
		public void RegisterSingleton( string name, Type classType, MutablePropertyValues propertyValues ) 
		{
			DefaultListableObjectFactory.RegisterObjectDefinition( name, new RootObjectDefinition( classType, propertyValues ) ); 
		}

		/// <summary>
		/// Registers a prototype object with the default object factory.
		/// </summary>
		/// <param name="name">The name of the prototype object.</param>
		/// <param name="classType">The <see cref="System.Type"/> of the prototype object.</param>
		/// <param name="propertyValues">The property values for the prototype instance.</param>
		public void RegisterPrototype( string name, Type classType, MutablePropertyValues propertyValues ) 
		{
            DefaultListableObjectFactory.RegisterObjectDefinition(name, new RootObjectDefinition(classType, propertyValues, false));
		}

		/// <summary>
		/// Associate the given message with the given code.
		/// </summary>
		/// <param name="code">The lookup code.</param>
		/// <param name="cultureInfo">
		/// The <see cref="System.Globalization.CultureInfo"/> that the message should be found within.
		/// </param>
		/// <param name="defaultMessage">The message associated with the lookup code.</param>
		public void AddMessage( string code, CultureInfo cultureInfo, string defaultMessage ) 
		{
			StaticMessageSource messageSource = (StaticMessageSource) GetObject( MessageSourceObjectName );
			messageSource.AddMessage( code, cultureInfo, defaultMessage );
		}
	}
}
