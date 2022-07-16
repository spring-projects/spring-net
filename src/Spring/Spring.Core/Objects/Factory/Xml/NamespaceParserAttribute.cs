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

namespace Spring.Objects.Factory.Xml
{
    /// <summary>
    /// Attribute that should be used to specify the default namespace
    /// and schema location for a custom namespace parser.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class NamespaceParserAttribute : Attribute
    {
        private string ns;
        private string schemaLocation;
		private Type schemaLocationAssemblyHint;

        /// <summary>
        /// Creates a new instance of <see cref="NamespaceParserAttribute"/>.
        /// </summary>
        public NamespaceParserAttribute()
        {}

        /// <summary>
        /// Gets or sets the default namespace for the configuration parser.
        /// </summary>
        /// <value>
        /// The default namespace for the configuration parser.
        /// </value>
        public string Namespace
        {
            get { return ns; }
            set { ns = value; }
        }

        /// <summary>
        /// Gets or sets the default schema location for the configuration parser.
        /// </summary>
        /// <value>
        /// The default schema location for the configuration parser.
        /// </value>
        /// <remarks>
		/// If the <see cref="SchemaLocationAssemblyHint"/>  property is set, the <see cref="SchemaLocation"/> will always resolve to an assembly-resource
		/// and the set <see cref="SchemaLocation"/> will be interpreted relative to this assembly.
		/// </remarks>
        public string SchemaLocation
        {
            get { return schemaLocation; }
            set { schemaLocation = value; }
        }

    	/// <summary>
    	/// Gets or sets a type from the assembly containing the schema
    	/// </summary>
    	/// <remarks>
    	/// If this property is set, the <see cref="SchemaLocation"/> will always resolve to an assembly-resource
    	/// and the <see cref="SchemaLocation"/> will be interpreted relative to this assembly.
    	/// </remarks>
    	public Type SchemaLocationAssemblyHint
    	{
    		get { return schemaLocationAssemblyHint; }
    		set { schemaLocationAssemblyHint = value; }
    	}
    }
}
