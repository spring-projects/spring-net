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

using Spring.Core.IO;
using Spring.Objects.Factory.Support;

namespace Spring.Objects.Factory.Xml
{
	/// <summary>
	/// Convenience extension of
	/// <see cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/>
	/// that reads object definitions from an XML document or element.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Delegates to
	/// <see cref="Spring.Objects.Factory.Xml.XmlObjectDefinitionReader"/>
	/// underneath; effectively equivalent to using a
	/// <see cref="Spring.Objects.Factory.Xml.XmlObjectDefinitionReader"/> for a
	/// <see cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/>.
	/// </p>
	/// <note>
	/// <i>objects</i> doesn't need to be the root element of
	/// the XML document: this class will parse all object definition elements in the
	/// XML stream.
	/// </note>
	/// <p>
	/// This class registers each object definition with the
	/// <see cref="Spring.Objects.Factory.Support.DefaultListableObjectFactory"/>
	/// superclass, and relies on the latter's implementation of the
	/// <see cref="Spring.Objects.Factory.IObjectFactory"/> interface. It supports
	/// singletons, prototypes and references to either of these kinds of object.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Rick Evans (.NET)</author>
	/// <see cref="Spring.Objects.Factory.Xml.XmlObjectDefinitionReader"/>
    [Serializable]
    public class XmlObjectFactory : DefaultListableObjectFactory
	{
	    /// <summary>
        /// Creates a new instance of the <see cref="XmlObjectFactory"/> class,
        /// with the given resource, which must be parsable using DOM.
        /// </summary>
        /// <param name="resource">
        /// The XML resource to load object definitions from.
        /// </param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of loading or parsing errors.
        /// </exception>
        public XmlObjectFactory(IResource resource) : this(resource, true, null)
        {
        }

        /// <summary>
		/// Creates a new instance of the <see cref="XmlObjectFactory"/> class,
		/// with the given resource, which must be parsable using DOM.
		/// </summary>
		/// <param name="resource">
		/// The XML resource to load object definitions from.
		/// </param>
        /// <param name="caseSensitive">Flag specifying whether to make this object factory case sensitive or not.</param>
        /// <exception cref="Spring.Objects.ObjectsException">
		/// In the case of loading or parsing errors.
		/// </exception>
		public XmlObjectFactory(IResource resource, bool caseSensitive) : this(resource, caseSensitive, null)
		{
		}

        /// <summary>
        /// Creates a new instance of the <see cref="XmlObjectFactory"/> class,
        /// with the given resource, which must be parsable using DOM, and the
        /// given parent factory.
        /// </summary>
        /// <param name="resource">
        /// The XML resource to load object definitions from.
        /// </param>
        /// <param name="parentFactory">The parent object factory (may be <see langword="null"/>).</param>
        /// <exception cref="Spring.Objects.ObjectsException">
        /// In the case of loading or parsing errors.
        /// </exception>
        public XmlObjectFactory(
            IResource resource, IObjectFactory parentFactory)
            : this(resource, true, parentFactory)
        {}

        /// <summary>
		/// Creates a new instance of the <see cref="XmlObjectFactory"/> class,
		/// with the given resource, which must be parsable using DOM, and the
		/// given parent factory.
		/// </summary>
		/// <param name="resource">
		/// The XML resource to load object definitions from.
		/// </param>
        /// <param name="caseSensitive">Flag specifying whether to make this object factory case sensitive or not.</param>
        /// <param name="parentFactory">The parent object factory (may be <see langword="null"/>).</param>
		/// <exception cref="Spring.Objects.ObjectsException">
		/// In the case of loading or parsing errors.
		/// </exception>
		public XmlObjectFactory(
			IResource resource, bool caseSensitive, IObjectFactory parentFactory)
			: base(caseSensitive, parentFactory)
		{
			ObjectDefinitionReader.LoadObjectDefinitions(resource);
		}

	    /// <summary>
        /// Gets object definition reader to use.
        /// </summary>
	    protected virtual IObjectDefinitionReader ObjectDefinitionReader
	    {
	        get
	        {
                return new XmlObjectDefinitionReader(this);
	        }
	    }
	}
}
