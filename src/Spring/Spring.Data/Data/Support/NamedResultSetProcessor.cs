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

namespace Spring.Data.Support
{
	/// <summary>
	/// Provides a name to a ResultSetProcessor for use with AdoOperation subclasses
	/// such as StoredProcedure.
	/// </summary>
	/// <author>Mark Pollack (.NET)</author>
	public class NamedResultSetProcessor 
	{
		private readonly object resultSetProcessor;
	    private readonly string name;

		/// <summary>
		/// Initializes a new instance of the <see cref="NamedResultSetProcessor"/> class with a
		/// IRowCallback instance
        /// </summary>
        /// <param name="name">The name of the associated row callback for use with output 
        /// result set mappers.</param>
        /// <param name="rowcallback">A row callback instance.</param>
		public NamedResultSetProcessor(string name, IRowCallback rowcallback)
		{
            this.name = name;
		    resultSetProcessor = rowcallback;
		}
	    
	    /// <summary>
        /// Initializes a new instance of the <see cref="NamedResultSetProcessor"/> class with a
        /// IRowMapper instance
	    /// </summary>
	    /// <param name="name">The name of the associated row mapper.</param>
	    /// <param name="rowMapper">A IRowMapper instance.</param>
	    public NamedResultSetProcessor(string name, IRowMapper rowMapper)
	    {
	        this.name = name;
	        resultSetProcessor = rowMapper;
	    }
	    
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedResultSetProcessor"/> class with a
        /// IResultSetExtractor instance
        /// </summary>
        /// <param name="name">The name of the associated result set extractor.</param>
        /// <param name="resultSetExtractor">A IResultSetExtractor instance.</param>
        public NamedResultSetProcessor(string name, IResultSetExtractor resultSetExtractor)
        {
            this.name = name;
            resultSetProcessor = resultSetExtractor;
        }

		public string Name
	    {
	        get
	        {
	            return name;
	        }
	    }
	    
	    public object ResultSetProcessor
	    {
	        get
	        {
	            return resultSetProcessor;
	        }
	    }
	}
}
