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
using System.Collections.Specialized;
using System.Reflection;
using Common.Logging;
using Spring.Util;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// Simple implementation of the <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/>
	/// that allows attributes to be matched by registered name.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class NameMatchTransactionAttributeSource : ITransactionAttributeSource, IEnumerable
	{
        /// <summary>
        /// Logger available to subclasses, static for optimal serialization
        /// </summary>
        [NonSerialized()]
        protected static readonly ILog log = LogManager.GetLogger(typeof(NameMatchTransactionAttributeSource));

        /// <summary>
        /// Keys are method names; values are ITransactionAttributes
        /// </summary>
		private readonly IDictionary nameMap;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.NameMatchTransactionAttributeSource"/> class.
		/// </summary>
		public NameMatchTransactionAttributeSource()
		{
			nameMap = new Hashtable();
		}

        /// <summary>
        /// Enumerate the string/<see cref="ITransactionAttribute"/> mapping entries.
        /// </summary>
	    public IEnumerator GetEnumerator()
	    {
	        return nameMap.GetEnumerator();
	    }

        /// <summary>
        /// Add a mapping.
        /// </summary>
        public void Add(string methodPattern, ITransactionAttribute txAttribute)
        {
            AddTransactionMethod(methodPattern, txAttribute);
        }

        /// <summary>
        /// Add a mapping.
        /// </summary>
        public void Add(string methodPattern, string txAttributeText)
        {
            TransactionAttributeEditor editor = new TransactionAttributeEditor();
            editor.SetAsText(txAttributeText);
            ITransactionAttribute txAttribute = editor.Value;
            AddTransactionMethod(methodPattern, txAttribute);
        }

	    /// <summary>
		/// Set a name/attribute map, consisting of method names (e.g. "MyMethod") and
		/// <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/> instances
        /// (or Strings to be converted to ITransactionAttribute instances).
		/// </summary>
		public IDictionary NameMap
		{
			set
			{
			    foreach (DictionaryEntry entry in value)
			    {
                    string name = entry.Key as string;
                    ITransactionAttribute attribute = null;
                    if (entry.Value is ITransactionAttribute)
                    {
                        attribute = entry.Value as ITransactionAttribute;
                    }
                    else
                    {
                        TransactionAttributeEditor editor = new TransactionAttributeEditor();
                        editor.SetAsText(entry.Value.ToString());
                        attribute = editor.Value;
                    }
                    AddTransactionMethod(name, attribute);
			    }
			}
		}

	    /// <summary>
	    /// Parses the given properties into a name/attribute map.
	    /// </summary>
	    /// <remarks>
	    /// Expects method names as keys and string attributes definitions as values,
	    /// parsable into <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/>
	    /// instances via
	    /// <see cref="Spring.Transaction.Interceptor.TransactionAttributeEditor"/>.
	    /// </remarks>
	    /// <value>The properties of the transaction.</value>
	    public NameValueCollection NameProperties
	    {
	        set
	        {
	            SetProperties(value);
	        }
	    }

	    /// <summary>
		/// Does the supplied <paramref name="methodName"/> match the supplied <paramref name="mappedName"/>?
		/// </summary>
		/// <remarks>
        /// The default implementation checks for "xxx*", "*xxx" and "*xxx*" matches,
        /// as well as direct equality. Can be overridden in subclasses.
		/// </remarks>
		/// <param name="methodName">The method name of the class.</param>
		/// <param name="mappedName">The name in the descriptor.</param>
		/// <returns><b>True</b> if the names match.</returns>
		protected virtual bool IsMatch( string methodName, string mappedName )
		{
            return PatternMatchUtils.SimpleMatch(mappedName, methodName);
		}

		/// <summary>
		/// Add an attribute for a transactional method.
		/// </summary>
		/// <remarks>
		/// <p>
		/// Method names can end with "*" for matching multiple methods.
		/// </p>
		/// </remarks>
		/// <param name="methodName">The transactional method name.</param>
		/// <param name="attribute">
		/// The attribute to be associated with the method.
		/// </param>
		public void AddTransactionMethod( string methodName, ITransactionAttribute attribute )
        {
            #region Instrumentation
            if (log.IsDebugEnabled)
            {
                log.Debug("Adding transactional method [" + methodName + "] with attribute [" + attribute + "]");
            }
            #endregion
            nameMap.Add( methodName, attribute );
		}

		/// <summary>
		/// Parses the given properties into a name/attribute map.
		/// </summary>
		/// <remarks>
		/// Expects method names as keys and string attributes definitions as values,
		/// parsable into <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/>
		/// instances via
		/// <see cref="Spring.Transaction.Interceptor.TransactionAttributeEditor"/>.
		/// </remarks>
		/// <param name="transactionAttributes">The properties of the transaction.</param>
		public void SetProperties( NameValueCollection transactionAttributes )
		{
			TransactionAttributeEditor editor = new TransactionAttributeEditor();
			foreach ( string methodName in transactionAttributes.Keys )
			{
				string value = transactionAttributes[methodName];
				editor.SetAsText(value);
				AddTransactionMethod(methodName, editor.Value);
			}
		}

	    #region ITransactionAttributeSource Members
		/// <summary>
		/// Return the <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/> for this
		/// method.
		/// </summary>
		/// <param name="method">The method to check.</param>
		/// <param name="targetType">
		/// The target <see cref="System.Type"/>. May be null, in which case the declaring
		/// class of the supplied <paramref name="method"/> must be used.
		/// </param>
		/// <returns>
		/// A <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/> or
		/// null if the method is non-transactional.
		/// </returns>
		public ITransactionAttribute ReturnTransactionAttribute(MethodInfo method, Type targetType)
		{
			string methodName = method.Name;
			ITransactionAttribute attribute = (ITransactionAttribute) nameMap[methodName];
			if ( attribute != null )
			{
				return attribute;
			}
			else
			{
				string bestNameMatch = null;
				foreach ( string mappedName in nameMap.Keys )
				{
					if ( ( IsMatch( methodName, mappedName ) ) && ( bestNameMatch == null || bestNameMatch.Length <= mappedName.Length ) )
					{
						attribute = (ITransactionAttribute) nameMap[mappedName];
						bestNameMatch = mappedName;
					}
				}
			}
			return attribute;
		}
		#endregion
	}
}
