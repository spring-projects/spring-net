#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
    /// An <see cref="XmlElement"/> holding information about its original text source location.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class ConfigXmlElement : XmlElement, ITextPosition
    {
        private ITextPosition _textPositionInfo;

        ///<summary>
        /// Creates a new instance of <see cref="ConfigXmlElement"/>, storing a copy of the passed 
        /// <paramref name="currentTextPositionPositionInfo"/>.
        ///</summary>
        public ConfigXmlElement(ITextPosition currentTextPositionPositionInfo, string prefix, string localName, string namespaceURI, XmlDocument doc) 
            : base(prefix, localName, namespaceURI, doc)
        {
            _textPositionInfo = new TextPositionInfo(currentTextPositionPositionInfo);
        }

        /// <summary>
        /// The name of the resource this element was read from
        /// </summary>
        public string Filename
        {
            get { return _textPositionInfo.Filename; }
        }

        /// <summary>
        /// The line number within the resource this element was read from
        /// </summary>
        public int LineNumber
        {
            get { return _textPositionInfo.LineNumber; }
        }

        /// <summary>
        /// The line position within the resource this element was read from.
        /// </summary>
        public int LinePosition
        {
            get { return _textPositionInfo.LinePosition; }
        }

        ///<summary>
        ///Creates a duplicate of this node.
        ///</summary>
        ///<param name="deep">true to recursively clone the subtree under the specified node; false to clone only the node itself </param>
        public override XmlNode CloneNode(bool deep)
        {
            XmlNode node = base.CloneNode(deep);
            ConfigXmlElement element = node as ConfigXmlElement;
            if (element != null)
            {
                element._textPositionInfo = new TextPositionInfo(this._textPositionInfo);
            }
            return node;
        }
    }
}