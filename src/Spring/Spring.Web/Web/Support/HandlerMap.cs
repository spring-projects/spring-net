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

using System.Collections;
using System.Text.RegularExpressions;
using System.Web;
using Common.Logging;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Holds a list of url <see cref="Regex"/> expressions and their corresponding names of responsible <see cref="IHttpHandlerFactory"/>
    /// or <see cref="IHttpHandler"/> objects managed by the container.
    /// </summary>
    /// <author>Erich Eichinger</author>
	public class HandlerMap : IDictionary
	{
		private readonly ILog Log = LogManager.GetLogger(typeof(HandlerMap));

		private ArrayList _internalTable = new ArrayList();

        /// <summary>
        /// Maps the specified url pattern expression to an object name denoting a <see cref="IHttpHandlerFactory"/> or <see cref="IHttpHandler"/> object.
        /// </summary>
        /// <param name="urlPattern">the string pattern (see conform to <see cref="Regex"/> syntax)</param>
        /// <param name="handlerObjectName">an object name denoting the spring-managed handler definition</param>
		public void Add(string urlPattern, string handlerObjectName)
		{
			this._internalTable.Add( new HandlerMapEntry(urlPattern, handlerObjectName) );
		}

        /// <summary>
        /// Maps the specified url pattern expression to an object name denoting a <see cref="IHttpHandlerFactory"/> or <see cref="IHttpHandler"/> object.
        /// </summary>
        /// <param name="urlPattern">the url pattern</param>
        /// <param name="handlerObjectName">an object name denoting the spring-managed handler definition</param>
		public void Add(Regex urlPattern, string handlerObjectName)
		{
			this._internalTable.Add( new HandlerMapEntry(urlPattern, handlerObjectName) );
		}

        /// <summary>
        /// Maps the <paramref name="virtualPath"/> to a handler object name by matching against all registered patterns.
        /// </summary>
        /// <param name="virtualPath">the virtual path</param>
        /// <returns>the object name</returns>
		public HandlerMapEntry MapPath( string virtualPath )
		{
			if(Log.IsDebugEnabled) Log.Debug( string.Format( "looking up mapping for url '{0}'", virtualPath ) );
			for(int i=0;i<this._internalTable.Count;i++)
			{
				HandlerMapEntry handlerMapEntry = (HandlerMapEntry)this._internalTable[i];
				if ( handlerMapEntry.UrlPattern.IsMatch( virtualPath ) )
				{
					if (Log.IsDebugEnabled) Log.Debug(string.Format("found mapping '{0}' for url '{1}'", handlerMapEntry, virtualPath));
					return handlerMapEntry;
				}
			}

			if (Log.IsDebugEnabled) Log.Debug(string.Format("no mapping found for url '{0}'", virtualPath));
			return null;
		}

        /// <summary>
        /// Add a new mapping
        /// </summary>
        /// <param name="key">an url pattern string</param>
        /// <param name="value">a handler object name string</param>
		void IDictionary.Add(object key, object value)
		{
			this.Add((string) key, (string) value);
		}

        /// <summary>
        /// Add or replace a mapping
        /// </summary>
        /// <remarks>
        /// Getter will throw a <see cref="NotSupportedException"/>!
        /// </remarks>
        /// <param name="key">an url pattern string</param>
		object IDictionary.this[object key]
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				this.Add( (string)key,(string)value );
			}
		}

        /// <summary>
        /// Clear the mapping table.
        /// </summary>
		public void Clear()
		{
			this._internalTable.Clear();
		}

        /// <summary>
        /// Always returns false.
        /// </summary>
		public bool IsReadOnly
		{
			get
			{
                return false;
                // return this._internalTable.IsReadOnly;
			}
		}

        ///<summary>
        ///Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size.
        ///</summary>
        ///
        ///<returns>
        ///true if the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size; otherwise, false.
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public bool IsFixedSize
		{
			get { return false; /*return this._internalTable.IsFixedSize;*/ }
		}

        ///<summary>
        ///Copies all <see cref="HandlerMapEntry"/> entries to the specified array.
        ///</summary>
        public void CopyTo(Array array, int index)
		{
			this._internalTable.CopyTo(array, index);
		}

        /// <summary>
        /// Get the number of registered mappings.
        /// </summary>
		public int Count
		{
			get { return this._internalTable.Count; }
		}

        ///<summary>
        ///Gets an object that can be used to synchronize access.
        ///</summary>
        public object SyncRoot
		{
			get { return this._internalTable.SyncRoot; }
		}

        /// <summary>
        /// Always returns false.
        /// </summary>
		public bool IsSynchronized
		{
			get { return false; /*return this._internalTable.IsSynchronized;*/ }
		}

        /// <summary>
        /// Get an enumerator for iterating over the list of registered mappings.
        /// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) this._internalTable).GetEnumerator();
		}

		#region Unsupported methods

        /// <summary>
        /// Not supported by this implementation.
        /// </summary>
		bool IDictionary.Contains(object key)
		{
			throw new NotSupportedException();
		}

        /// <summary>
        /// Not supported by this implementation.
        /// </summary>
		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			throw new NotSupportedException();
		}

        /// <summary>
        /// Not supported by this implementation.
        /// </summary>
		void IDictionary.Remove(object key)
		{
			throw new NotSupportedException();
		}

        /// <summary>
        /// Not supported by this implementation.
        /// </summary>
		ICollection IDictionary.Keys
		{
			get
			{
				throw new NotSupportedException();
			}
		}

        /// <summary>
        /// Not supported by this implementation.
        /// </summary>
		ICollection IDictionary.Values
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		#endregion
	}
}
