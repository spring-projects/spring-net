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

using System.Collections;
using Spring.Util;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// Editor that can convert <see cref="System.String"/> values into
	/// <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/> instances.
	/// </summary>
	/// <remarks>
	/// The transaction attribute string must be parseable by the
	/// <see cref="Spring.Transaction.Interceptor.TransactionAttributeEditor"/> in this package.
	/// <p>
	/// Strings must be specified in the following syntax:<br/>
	/// <code>FQCN.methodName=&lt;transaction attribute string&gt;</code> (sans &lt;&gt;).
	/// </p>
	/// <example>
	/// <code>ExampleNamespace.ExampleClass.MyMethod=PROPAGATION_MANDATORY,ISOLATION_DEFAULT</code>
	///	</example>
	/// <note>
	/// The specified class must be the one where the methods are defined; in the case of
	/// implementing an interface, the interface class name must be specified.
	/// </note>
	/// <p>
	/// This will register all overloaded methods for a given name. Does not support explicit
	/// registration of certain overloaded methods. Supports wildcard style mappings (in
	/// the form "xxx*"), e.g. "Notify*" for "Notify" and "NotifyAll".
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	public class TransactionAttributeSourceEditor
	{
		#region PropertiesEditor Class
		/// <summary>
		/// Internal class to parse property values.
		/// </summary>
		protected internal class PropertiesEditor
		{
			private IDictionary _properties;

			/// <summary>
			/// Creates a new instance of the
			/// <see cref="Spring.Transaction.Interceptor.TransactionAttributeSourceEditor.PropertiesEditor"/> class.
			/// </summary>
			/// <param name="properties">The property values to be parsed.</param>
			public PropertiesEditor( string properties )
			{
				_properties = new Hashtable();
				parseProperties( properties );
			}

			/// <summary>
			/// Indexer to return values based on index value.
			/// </summary>
			public string this[ string index ]
			{
				get { return (string) _properties[index]; }
			}

			/// <summary>
			/// Returns the collection of keys for properties.
			/// </summary>
			public ICollection Keys
			{
				get { return _properties.Keys; }
			}

			private void parseProperties( string properties )
			{
				string[] tokens = StringUtils.DelimitedListToStringArray( properties, "\n");
				foreach ( string token in tokens )
				{
					string[] property = token.Split('=');
					_properties.Add( property[0].Trim(), property[1].Trim());
				}
			}
		}
		#endregion
	
		private ITransactionAttributeSource _attributeSource;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.TransactionAttributeSourceEditor.PropertiesEditor"/> class.
		/// </summary>
		public TransactionAttributeSourceEditor() {}

		/// <summary>
		/// Parses the input properties <see cref="System.String"/> into a valid
		/// <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/>
		/// instance
		/// </summary>
		/// <param name="attributeSource">The properties string to be parsed.</param>
		public void SetAsText( string attributeSource )
		{
			MethodMapTransactionAttributeSource source = new MethodMapTransactionAttributeSource();
			if ( attributeSource == null || attributeSource.Length == 0 )
			{
				_attributeSource = null;
			} else
			{
				PropertiesEditor editor = new PropertiesEditor(attributeSource);
				TransactionAttributeEditor tae = new TransactionAttributeEditor();

				foreach ( string name in editor.Keys )
				{
					string value = editor[name];
					tae.SetAsText( value );
					ITransactionAttribute transactionAttribute = tae.Value;
					source.AddTransactionalMethod( name, transactionAttribute );
				}
			}
			_attributeSource = source;
		}

		/// <summary>
		/// Gets the <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/>
		/// from this instance.
		/// </summary>
		public ITransactionAttributeSource Value
		{
			get { return _attributeSource; }
		}
	}
}
