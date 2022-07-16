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

using Spring.Util;

#endregion

namespace Spring.Web.Support
{
    /// <summary>
    /// Represents a MIME media type as defined by http://www.iana.org/assignments/media-types/
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class MimeMediaType
    {
        private static readonly List<string> ContentTypes = new List<string>( new string[] {
                                                            "application", "audio", "example", "image", "message",
                                                            "model", "multipart", "text", "video"
                                                        });

        #region Predefined Instances

        /// <summary>
        /// Predefines common application media types
        /// </summary>
        public sealed class Application
        {
            /// <summary>
            /// Represents "application/octet-stream"
            /// </summary>
            public static readonly MimeMediaType Octet = Parse("application/octet-stream");
            /// <summary>
            /// Represents "application/pdf"
            /// </summary>
            public static readonly MimeMediaType Pdf = Parse("application/pdf");
            /// <summary>
            /// Represents "application/rtf"
            /// </summary>
            public static readonly MimeMediaType Rtf = Parse("application/rtf");
            /// <summary>
            /// Represents "application/soap+xml"
            /// </summary>
            public static readonly MimeMediaType Soap = Parse("application/soap+xml");
            /// <summary>
            /// Represents "application/zip"
            /// </summary>
            public static readonly MimeMediaType Zip = Parse("application/zip");
        }

        /// <summary>
        /// Predefines common image media types
        /// </summary>
        public sealed class Image
        {
            /// <summary>
            /// Represents "image/gif"
            /// </summary>
            public static readonly MimeMediaType Gif = Parse("image/gif");
            /// <summary>
            /// Represents "image/jpeg"
            /// </summary>
            public static readonly MimeMediaType Jpeg = Parse("image/jpeg");
            /// <summary>
            /// Represents "image/tiff"
            /// </summary>
            public static readonly MimeMediaType Tiff = Parse("image/tiff");
        }

        /// <summary>
        /// Predefines common text media types
        /// </summary>
        public sealed class Text
        {
            /// <summary>
            /// Represents "text/html"
            /// </summary>
            public static readonly MimeMediaType Html = Parse("text/html");
            /// <summary>
            /// Represents "text/plain"
            /// </summary>
            public static readonly MimeMediaType Plain = Parse("text/plain");
            /// <summary>
            /// Represents "text/richtext"
            /// </summary>
            public static readonly MimeMediaType RichText = Parse("text/richtext");
            /// <summary>
            /// Represents "text/xml"
            /// </summary>
            public static readonly MimeMediaType Xml = Parse("text/xml");
            /// <summary>
            /// Represents "text/javascript"
            /// </summary>
            public static readonly MimeMediaType Javascript = Parse("text/javascript");
            /// <summary>
            /// Represents "text/css"
            /// </summary>
            public static readonly MimeMediaType Css = Parse("text/css");
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Parses a string into a <see cref="MimeMediaType"/> instance.
        /// </summary>
        /// <param name="mediaType">a valid string representation of a mediaType</param>
        /// <returns>a new <see cref="MimeMediaType"/> instance</returns>
        public static MimeMediaType Parse(string mediaType)
        {
            AssertUtils.ArgumentNotNull(mediaType, "mediaType");

            string[] parts = mediaType.Split('/');
            if (parts.Length > 2)
            {
                throw new ArgumentException("invalid mediaType");
            }

            if (parts.Length == 1)
            {
                return new MimeMediaType(parts[0]);
            }
            else
            {
                return new MimeMediaType(parts[0], parts[1]);
            }
        }

        #endregion Static Methods

        private readonly string _contentType;
        private readonly string _subType;

        /// <summary>
        /// Creates a new media type instance representing a generic "application/octet-stream"
        /// </summary>
        public MimeMediaType()
            :this("application", "octet-stream")
        {
        }

        /// <summary>
        /// Creates a new media type instance representing a generic content type
        /// with an unspecified subtype (e.g. "text/*")
        /// </summary>
        public MimeMediaType(string contentType)
            : this(contentType, "*")
        {
        }

        /// <summary>
        /// Creates a new media type instance representing a particular media type
        /// </summary>
        public MimeMediaType(string contentType, string subType)
        {
            _contentType = contentType.ToLower();
            if (!ContentTypes.Contains(_contentType))
            {
                throw new ArgumentException("unknown content type", "contentType");
            }
            _subType = (StringUtils.HasText(subType)) ? subType.ToLower() : "*";
        }

        /// <summary>
        /// Gets the content type of this media type instance
        /// </summary>
        public string ContentType
        {
            get { return _contentType; }
        }

        /// <summary>
        /// Gets the subtype of this media type instance
        /// </summary>
        public string SubType
        {
            get { return _subType; }
        }

        /// <summary>
        /// Returns a string representation of this <see cref="MimeMediaType"/> instance.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}/{1}", _contentType, _subType);
        }

        /// <summary>
        /// Compares this instance to another
        /// </summary>
        /// <param name="other">another <see cref="MimeMediaType"/> instance</param>
        protected bool Equals(MimeMediaType other)
        {
            if (other == null) return false;
            return Equals(_contentType, other._contentType) && Equals(_subType, other._subType);
        }

        ///<summary>
        ///Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="MimeMediaType"></see>.
        ///</summary>
        ///<returns>
        ///true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="MimeMediaType"></see>; otherwise, false.
        ///</returns>
        ///<param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="MimeMediaType"></see>.</param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as MimeMediaType);
        }

        ///<summary>
        ///Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        ///</summary>
        ///<returns>
        ///A hash code for the current <see cref="MimeMediaType"></see>.
        ///</returns>
        public override int GetHashCode()
        {
            return _contentType.GetHashCode() + 29*_subType.GetHashCode();
        }
    }
}
