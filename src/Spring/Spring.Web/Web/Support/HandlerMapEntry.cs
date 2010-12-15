#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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

using System.Text.RegularExpressions;
using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Holds pairs of (url pattern, handler object name).
    /// </summary>
    /// <seealso cref="HandlerMap"/>
    /// <seealso cref="MappingHandlerFactory"/>
    /// <seealso cref="MappingHandlerFactoryConfigurer"/>
    /// <author>Erich Eichinger</author>
	public class HandlerMapEntry
	{
		private Regex _urlPattern;
		private string _handlerObjectName;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="urlPattern"></param>
        /// <param name="handlerObjectName"></param>
		public HandlerMapEntry(string urlPattern, string handlerObjectName)
		{
            AssertUtils.ArgumentNotNull(urlPattern, "urlPattern");
			AssertUtils.ArgumentNotNull(handlerObjectName, "handlerObjectName");
			this._urlPattern = new Regex(urlPattern, RegexOptions.Compiled|RegexOptions.ECMAScript|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);
			this._handlerObjectName = handlerObjectName;
		}

		///<summary>
		/// Create a new instance
		///</summary>
		///<param name="urlPattern"></param>
		///<param name="handlerObjectName"></param>
		public HandlerMapEntry(Regex urlPattern, string handlerObjectName)
		{
            AssertUtils.ArgumentNotNull(urlPattern, "urlPattern");
			AssertUtils.ArgumentNotNull(handlerObjectName, "handlerObjectName");
			this._urlPattern = urlPattern;
			this._handlerObjectName = handlerObjectName;
		}

		///<summary>
		/// Get the url pattern
		///</summary>
		public Regex UrlPattern
		{
			get { return this._urlPattern; }
		}

        /// <summary>
        /// Get the handler object name
        /// </summary>
		public string HandlerObjectName
		{
			get { return this._handlerObjectName; }
		}

        /// <summary>
        /// Return a string representation of this entry.
        /// </summary>
		public override string ToString()
		{
			return string.Format("HandlerMapEntry['{0}','{1}']", _urlPattern, _handlerObjectName);
		}
	}
}