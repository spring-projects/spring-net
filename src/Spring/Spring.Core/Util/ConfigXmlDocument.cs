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

using System.Xml;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// An <see cref="XmlDocument"/> implementation, who's elements retain information
    /// about their location in the original XML text document the were read from.
    /// </summary>
    /// <remarks>
    /// When loading a document, the used <see cref="XmlReader"/> must implement <see cref="IXmlLineInfo"/>.
    /// Typical XmlReader implementations like <see cref="XmlTextReader"/> support this interface.
    /// </remarks>
    /// <author>Erich Eichinger</author>
    public class ConfigXmlDocument : XmlDocument
    {
        /// <summary>
        /// Holds the current text position during loading a document
        /// </summary>
        private class CurrentTextPositionHolder : ITextPosition
        {
            private string _currentResourceName;
            private IXmlLineInfo _currentXmlLineInfo;

            public IXmlLineInfo CurrentXmlLineInfo
            {
                //get { return _currentXmlLineInfo; }
                set { _currentXmlLineInfo = value; }
            }

            public string CurrentResourceName
            {
                //get { return _currentResourceName; }
                set { _currentResourceName = value; }
            }

            public string Filename
            {
                get { return _currentResourceName; }
            }

            public int LineNumber
            {
                get { return (_currentXmlLineInfo != null) ? _currentXmlLineInfo.LineNumber : 0; }
            }

            public int LinePosition
            {
                get { return (_currentXmlLineInfo != null) ? _currentXmlLineInfo.LinePosition : 0; }
            }
        }

        private readonly CurrentTextPositionHolder _currentTextPositionHolder = new CurrentTextPositionHolder();

        /// <summary>
        /// Get info about the current text position during loading a document.
        /// Outside loading a document, the properties of <see cref="CurrentTextPosition"/>
        /// will always be <c>null</c>.
        /// </summary>
        protected ITextPosition CurrentTextPosition
        {
            get { return _currentTextPositionHolder; }
        }

        /// <summary>
        /// Overridden to create a <see cref="ConfigXmlElement"/> retaining the current
        /// text position information.
        /// </summary>
        public override XmlElement CreateElement(string prefix, string localName, string namespaceURI)
        {
            return new ConfigXmlElement(this.CurrentTextPosition, prefix, localName, namespaceURI, this);
        }

        /// <summary>
        /// Overridden to create a <see cref="ConfigXmlAttribute"/> retaining the current
        /// text position information.
        /// </summary>
        public override XmlAttribute CreateAttribute(string prefix, string localName, string namespaceURI)
        {
            return new ConfigXmlAttribute(this.CurrentTextPosition, prefix, localName, namespaceURI, this);
        }

        /// <summary>
        /// Load the document from the given <see cref="XmlReader"/>.
        /// Child nodes will store <paramref name="resourceName"/> as their <see cref="ITextPosition.Filename"/> property.
        /// </summary>
        ///<param name="resourceName">the name of the resource</param>
        ///<param name="xml">The XML source </param>
        public void LoadXml(string resourceName, string xml)
        {
            try
            {
                _currentTextPositionHolder.CurrentResourceName = resourceName;
                base.LoadXml(xml);
            }
            finally
            {
                _currentTextPositionHolder.CurrentResourceName = null;
            }
        }

        /// <summary>
        /// Load the document from the given <see cref="XmlReader"/>.
        /// </summary>
        ///<param name="filePath">The XML source </param>
        public override void Load(string filePath)
        {
            try
            {
                Uri baseUri = new Uri(Directory.GetCurrentDirectory());
                Stream istm = (Stream) new XmlUrlResolver().GetEntity(new Uri(baseUri, filePath), null, typeof (Stream));
                base.Load(istm);
            }
            finally
            {
                _currentTextPositionHolder.CurrentResourceName = null;
            }
        }

        /// <summary>
        /// Load the document from the given <see cref="XmlReader"/>.
        /// Child nodes will store <paramref name="resourceName"/> as their <see cref="ITextPosition.Filename"/> property.
        /// </summary>
        ///<param name="resourceName">the name of the resource</param>
        ///<param name="filePath">The XML source </param>
        public void Load(string resourceName, string filePath)
        {
            try
            {
                _currentTextPositionHolder.CurrentResourceName = resourceName;
                Uri baseUri = new Uri(Directory.GetCurrentDirectory());
                Stream istm = (Stream) new XmlUrlResolver().GetEntity(new Uri(baseUri, filePath), null, typeof (Stream));
                base.Load(istm);
            }
            finally
            {
                _currentTextPositionHolder.CurrentResourceName = null;
            }
        }

        /// <summary>
        /// Load the document from the given <see cref="XmlReader"/>.
        /// Child nodes will store <paramref name="resourceName"/> as their <see cref="ITextPosition.Filename"/> property.
        /// </summary>
        ///<param name="resourceName">the name of the resource</param>
        ///<param name="stream">The XML source </param>
        public void Load(string resourceName, Stream stream)
        {
            try
            {
                _currentTextPositionHolder.CurrentResourceName = resourceName;
                base.Load (stream );
            }
            finally
            {
                _currentTextPositionHolder.CurrentResourceName = null;
            }
        }

        /// <summary>
        /// Load the document from the given <see cref="XmlReader"/>.
        /// Child nodes will store <paramref name="resourceName"/> as their <see cref="ITextPosition.Filename"/> property.
        /// </summary>
        ///<param name="resourceName">the name of the resource</param>
        ///<param name="reader">The XML source </param>
        public void Load(string resourceName, TextReader reader)
        {
            try
            {
                _currentTextPositionHolder.CurrentResourceName = resourceName;
                base.Load (reader );
            }
            finally
            {
                _currentTextPositionHolder.CurrentResourceName = null;
            }
        }

        /// <summary>
        /// Load the document from the given <see cref="XmlReader"/>.
        /// Child nodes will store <paramref name="resourceName"/> as their <see cref="ITextPosition.Filename"/> property.
        /// </summary>
        ///<param name="resourceName">the name of the resource</param>
        ///<param name="reader">The XML source </param>
        public void Load(string resourceName, XmlReader reader)
        {
            try
            {
                _currentTextPositionHolder.CurrentResourceName = resourceName;
                this.Load (reader);
            }
            finally
            {
                _currentTextPositionHolder.CurrentResourceName = null;
            }
        }

        /// <summary>
        /// Load the document from the given <see cref="XmlReader"/>.
        /// Child nodes will store <c>null</c> as their <see cref="ITextPosition.Filename"/> property.
        /// </summary>
        /// <param name="reader">The XML source </param>
        public override void Load(XmlReader reader)
        {
            try
            {
                _currentTextPositionHolder.CurrentXmlLineInfo = reader as IXmlLineInfo;
                base.Load (reader);
            }
            finally
            {
                _currentTextPositionHolder.CurrentXmlLineInfo = null;
            }
        }

        ///<summary>
        ///Creates an <see cref="T:System.Xml.XmlNode"></see> object based on the information in the <see cref="T:System.Xml.XmlReader"></see>. The reader must be positioned on a node or attribute.
        ///Child nodes will store <paramref name="resourceName"/> as their <see cref="ITextPosition.Filename"/> property.
        ///</summary>
        ///<returns>
        ///The new XmlNode or null if no more nodes exist.
        ///</returns>
        ///<param name="resourceName">the name of the resource</param>
        ///<param name="reader">The XML source </param>
        ///<exception cref="T:System.InvalidOperationException">The reader is positioned on a node type that does not translate to a valid DOM node (for example, EndElement or EndEntity). </exception>
        public XmlNode ReadNode(string resourceName, XmlReader reader)
        {
            try
            {
                _currentTextPositionHolder.CurrentResourceName = resourceName;
                _currentTextPositionHolder.CurrentXmlLineInfo = reader as IXmlLineInfo;
                return this.ReadNode (reader);
            }
            finally
            {
                _currentTextPositionHolder.CurrentXmlLineInfo = null;
            }
        }

        ///<summary>
        ///Creates an <see cref="T:System.Xml.XmlNode"></see> object based on the information in the <see cref="T:System.Xml.XmlReader"></see>. The reader must be positioned on a node or attribute.
        ///Child nodes will store <c>null</c> as their <see cref="ITextPosition.Filename"/> property.
        ///</summary>
        ///<returns>
        ///The new XmlNode or null if no more nodes exist.
        ///</returns>
        ///<param name="reader">The XML source </param>
        ///<exception cref="T:System.InvalidOperationException">The reader is positioned on a node type that does not translate to a valid DOM node (for example, EndElement or EndEntity). </exception>
        public override XmlNode ReadNode(XmlReader reader)
        {
            try
            {
                _currentTextPositionHolder.CurrentXmlLineInfo = reader as IXmlLineInfo;
                return base.ReadNode (reader);
            }
            finally
            {
                _currentTextPositionHolder.CurrentXmlLineInfo = null;
            }
        }
    }
}
