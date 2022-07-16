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

using System.Text;

using Spring.Util;

#endregion

namespace Spring.Core.IO
{
    /// <summary>
    /// A <see cref="IResource"/> adapter implementation encapsulating a simple string.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class StringResource : AbstractResource
    {
        #region Fields

        private readonly string _description;
        private readonly string _content;
        private readonly Encoding _encoding;

        #endregion

        /// <summary>
        /// Creates a new instance of the <see cref="StringResource"/> class.
        /// </summary>
        public StringResource(string content)
            : this(content, Encoding.Default, null)
        {            
        }

        /// <summary>
        /// Creates a new instance of the <see cref="StringResource"/> class.
        /// </summary>
        public StringResource(string content, Encoding encoding)
            : this(content, encoding, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="StringResource"/> class.
        /// </summary>
        public StringResource(string content, Encoding encoding, string description)
        {
            AssertUtils.ArgumentNotNull(encoding, "encoding");

            _content = content==null ? string.Empty : content;
            _encoding = encoding;
            _description = description == null ? string.Empty : description;
        }

        /// <summary>
        /// Get the <see cref="System.IO.Stream"/> to 
        /// for accessing this resource.
        /// </summary>
        public override Stream InputStream
        {
            get
            {
                return new MemoryStream(_encoding.GetBytes(_content), false);
            }
        }

        /// <summary>
        /// Returns a description for this resource.
        /// </summary>
        /// <value>
        /// A description for this resource.
        /// </value>
        /// <seealso cref="Spring.Core.IO.IResource.Description"/>
        public override string Description
        {
            get { return _description; }
        }

        /// <summary>
        /// This implementation always returns true
        /// </summary>
        public override bool IsOpen
        {
            get { return true; }
        }

        /// <summary>
        /// This implemementation always returns true
        /// </summary>
        public override bool Exists
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the encoding used to create a byte stream of the <see cref="Content"/> string.
        /// </summary>
        public Encoding Encoding
        {
            get { return _encoding; }
        }

        /// <summary>
        /// Gets the content encapsulated by this <see cref="StringResource"/>.
        /// </summary>
        public string Content
        {
            get { return _content; }
        }
    }
}
