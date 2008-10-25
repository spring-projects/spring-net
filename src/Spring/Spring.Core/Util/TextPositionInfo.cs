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

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Holds text position information for e.g. error reporting purposes.
    /// </summary>
    /// <seealso cref="ConfigXmlElement" />
    /// <seealso cref="ConfigXmlAttribute" />
    public class TextPositionInfo : ITextPosition
    {
        private readonly string _filename;
        private readonly int _lineNumber;
        private readonly int _linePosition;

        /// <summary>
        /// Creates a new TextPositionInfo instance.
        /// </summary>
        public TextPositionInfo(string filename, int lineNumber, int linePosition)
        {
            _filename = filename;
            _lineNumber = lineNumber;
            _linePosition = linePosition;
        }

        /// <summary>
        /// Creates a new TextPositionInfo instance, copying values from another instance.
        /// </summary>
        public TextPositionInfo(ITextPosition other)
        {
            if (other != null)
            {
                this._filename = other.Filename;
                this._lineNumber = other.LineNumber;
                this._linePosition = other.LinePosition;
            }
        }

        /// <summary>
        /// The filename related to this text position
        /// </summary>
        public string Filename
        {
            get { return _filename; }
        }

        /// <summary>
        /// The line number related to this text position
        /// </summary>
        public int LineNumber
        {
            get { return _lineNumber; }
        }

        /// <summary>
        /// The line position related to this text position
        /// </summary>
        public int LinePosition
        {
            get { return _linePosition; }
        }
    }
}