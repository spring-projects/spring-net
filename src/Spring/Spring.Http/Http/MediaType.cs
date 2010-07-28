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

using System;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;

using Spring.Util;

namespace Spring.Http
{
    /**
     * Represents an Internet Media Type, as defined in the HTTP specification.
     *
     * <p>Consists of a {@linkplain #getType() type} and a {@linkplain #getSubtype() subtype}.
     * Also has functionality to parse media types from a string using {@link #parseMediaType(String)},
     * or multiple comma-separated media types using {@link #parseMediaTypes(String)}.
     *
     * @author Arjen Poutsma
     * @author Juergen Hoeller
     * @since 3.0
     * @see <a href="http://tools.ietf.org/html/rfc2616#section-3.7">HTTP 1.1, section 3.7</a>
     */
    public class MediaType : IComparable<MediaType> 
    {
        /**
         * Public constant media type that includes all media ranges (i.e. <code>&#42;/&#42;</code>).
         */
        public static readonly MediaType ALL = new MediaType("*", "*");

        /**
         *  Public constant media type for {@code application/atom+xml}.
         */
        public static readonly MediaType APPLICATION_ATOM_XML = new MediaType("application", "atom+xml");

        /**
         * Public constant media type for {@code application/x-www-form-urlencoded}.
         *  */
        public static readonly MediaType APPLICATION_FORM_URLENCODED = new MediaType("application", "x-www-form-urlencoded");

        /**
         * Public constant media type for {@code application/json}.
         * */
        public static readonly MediaType APPLICATION_JSON = new MediaType("application", "json");

        /**
         * Public constant media type for {@code application/octet-stream}.
         *  */
        public static readonly MediaType APPLICATION_OCTET_STREAM = new MediaType("application", "octet-stream");

        /**
         * Public constant media type for {@code application/xhtml+xml}.
         *  */
        public static readonly MediaType APPLICATION_XHTML_XML = new MediaType("application", "xhtml+xml");

        /**
         * Public constant media type for {@code image/gif}.
         */
        public static readonly MediaType IMAGE_GIF = new MediaType("image", "gif");

        /**
         * Public constant media type for {@code image/jpeg}.
         */
        public static readonly MediaType IMAGE_JPEG = new MediaType("image", "jpeg");

        /**
         * Public constant media type for {@code image/png}.
         */
        public static readonly MediaType IMAGE_PNG = new MediaType("image", "png");

        /**
         * Public constant media type for {@code image/xml}.
         */
        public static readonly MediaType APPLICATION_XML = new MediaType("application", "xml");

        /**
         * Public constant media type for {@code multipart/form-data}.
         *  */
        public static readonly MediaType MULTIPART_FORM_DATA = new MediaType("multipart", "form-data");

        /**
         * Public constant media type for {@code text/html}.
         *  */
        public static readonly MediaType TEXT_HTML = new MediaType("text", "html");

        /**
         * Public constant media type for {@code text/plain}.
         *  */
        public static readonly MediaType TEXT_PLAIN = new MediaType("text", "plain");

        /**
         * Public constant media type for {@code text/xml}.
         *  */
        public static readonly MediaType TEXT_XML = new MediaType("text", "xml");


        private const string WILDCARD_TYPE = "*";

        private const string PARAM_QUALITY_FACTOR = "q";

        private const string PARAM_CHARSET = "charset";

        private string type;

        private string subtype;

        private IDictionary<string, string> parameters;

        /**
         * Return the primary type.
         */
        public string Type
        {
            get { return this.type; }
        }

        /**
         * Return the subtype.
         */
        public string Subtype
        {
            get { return this.subtype; }
        }

        /**
         * Indicate whether the {@linkplain #getType() type} is the wildcard character <code>&#42;</code> or not.
         */
        public bool IsWildcardType
        {
            get { return WILDCARD_TYPE == type; }
        }

        /**
         * Indicate whether the {@linkplain #getSubtype() subtype} is the wildcard character <code>&#42;</code> or not.
         * @return whether the subtype is <code>&#42;</code>
         */
        public bool IsWildcardSubtype
        {
            get { return WILDCARD_TYPE == subtype; }
        }

        /**
         * Return the character set, as indicated by a <code>charset</code> parameter, if any.
         * @return the character set; or <code>null</code> if not available
         */
        public string CharSet
        {
            get
            {
                string charSet = null;
                this.parameters.TryGetValue(PARAM_CHARSET, out charSet);
                return charSet;
                //string charSet = this.parameters[PARAM_CHARSET];
                //return (charSet != null ? Charset.forName(charSet) : null);
            }
        }

        /**
         * Return the quality value, as indicated by a <code>q</code> parameter, if any.
         * Defaults to <code>1.0</code>.
         * @return the quality factory
         */
        public double QualityValue
        {
            get
            {
                string qualityFactory = null;
                return this.parameters.TryGetValue(PARAM_QUALITY_FACTOR, out qualityFactory) 
                    ? Double.Parse(qualityFactory, CultureInfo.InvariantCulture) 
                    : 1D;
            }
        }

        /**
         * Create a new {@link MediaType} for the given primary type.
         * <p>The {@linkplain #getSubtype() subtype} is set to <code>&#42;</code>, parameters empty.
         * @param type the primary type
         * @throws IllegalArgumentException if any of the parameters contain illegal characters
         */
        public MediaType(string type) :
            this(type, WILDCARD_TYPE)
        {
        }

        /**
         * Create a new {@link MediaType} for the given primary type and subtype.
         * <p>The parameters are empty.
         * @param type the primary type
         * @param subtype the subtype
         * @throws IllegalArgumentException if any of the parameters contain illegal characters
         */
        public MediaType(string type, string subtype) :
            this(type, subtype, new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase))
        {
        }

        /**
         * Create a new {@link MediaType} for the given type, subtype, and character set.
         * @param type the primary type
         * @param subtype the subtype
         * @param charSet the character set
         * @throws IllegalArgumentException if any of the parameters contain illegal characters
         */
        public MediaType(string type, string subtype, string charSet) :
            this(type, subtype)
        {
            this.parameters.Add(PARAM_CHARSET, charSet);
        }

        /**
         * Create a new {@link MediaType} for the given type, subtype, and quality value.
         *
         * @param type the primary type
         * @param subtype the subtype
         * @param qualityValue the quality value
         * @throws IllegalArgumentException if any of the parameters contain illegal characters
         */
        public MediaType(String type, String subtype, double qualityValue) :
            this(type, subtype)
        {
            this.parameters.Add(PARAM_QUALITY_FACTOR, qualityValue.ToString(CultureInfo.InvariantCulture));
        }

        /**
         * Copy-constructor that copies the type and subtype of the given {@link MediaType},
         * and allows for different parameter.
         * @param other the other media type
         * @param parameters the parameters, may be <code>null</code>
         * @throws IllegalArgumentException if any of the parameters contain illegal characters
         */
        public MediaType(MediaType other, IDictionary<string, string> parameters) :
            this(other.Type, other.Subtype, parameters)
        {
        }

        /**
         * Create a new {@link MediaType} for the given type, subtype, and parameters.
         * @param type the primary type
         * @param subtype the subtype
         * @param parameters the parameters, may be <code>null</code>
         * @throws IllegalArgumentException if any of the parameters contain illegal characters
         */
        public MediaType(string type, string subtype, IDictionary<string, string> parameters)
        {
            AssertUtils.ArgumentHasText(type, "'type' must not be empty");
            AssertUtils.ArgumentHasText(subtype, "'subtype' must not be empty");
            //checkToken(type);
            //checkToken(subtype);
            this.type = type.ToLowerInvariant();
            this.subtype = subtype.ToLowerInvariant();
            this.parameters = new Dictionary<string, string>(parameters, StringComparer.InvariantCultureIgnoreCase);
            //if (parameters.Count > 0) 
            //{
            //    NameValueCollection m = new NameValueCollection(parameters.Count, null, new CaseInsensitiveComparer());
            //    for (Map.Entry<String, String> entry : parameters.entrySet()) {
            //        String attribute = entry.getKey();
            //        String value = entry.getValue();
            //        checkParameters(attribute, value);
            //        m.put(attribute, unquote(value));
            //    }
            //    this.parameters = Collections.unmodifiableMap(m);
            //}
            //else 
            //{
            //    this.parameters = Collections.emptyMap();
            //}
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj is MediaType)
            {
                MediaType otherMediaType = (MediaType)obj;
                if (this.type == otherMediaType.type && 
                    this.subtype == otherMediaType.subtype)
                {
                    if (otherMediaType.parameters.Count == this.parameters.Count)
                    {
                        foreach(string key in this.parameters.Keys)
                        {
                            if (!otherMediaType.parameters.ContainsKey(key) ||
                                !String.Equals(otherMediaType.parameters[key], this.parameters[key]))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            int result = this.type.GetHashCode();
            result = 31 * result + this.subtype.GetHashCode();
            result = 31 * result + this.parameters.GetHashCode();
            return result;
        }

        public override string ToString() 
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.type);
            builder.Append('/');
            builder.Append(this.subtype);
            foreach(string key in this.parameters.Keys) 
            {
                builder.Append(';');
                builder.Append(key);
                builder.Append('=');
                builder.Append(this.parameters[key]);
            }
            return builder.ToString();
        }

        /**
         * Checks the given token string for illegal characters, as defined in RFC 2616, section 2.2.
         * @throws IllegalArgumentException in case of illegal characters
         * @see <a href="http://tools.ietf.org/html/rfc2616#section-2.2">HTTP 1.1, section 2.2</a>
         */
        //private void checkToken(String s) {
        //    for (int i=0; i < s.length(); i++ ) {
        //        char ch = s.charAt(i);
        //        if (!TOKEN.get(ch)) {
        //            throw new IllegalArgumentException("Invalid token character '" + ch + "' in token \"" + s + "\"");
        //        }
        //    }
        //}

        //private void checkParameters(String attribute, String value) {
        //    Assert.hasLength(attribute, "parameter attribute must not be empty");
        //    Assert.hasLength(value, "parameter value must not be empty");
        //    checkToken(attribute);
        //    if (PARAM_QUALITY_FACTOR.equals(attribute)) {
        //        value = unquote(value);
        //        double d = Double.parseDouble(value);
        //        Assert.isTrue(d >= 0D && d <= 1D,
        //                "Invalid quality value \"" + value + "\": should be between 0.0 and 1.0");
        //    }
        //    else if (PARAM_CHARSET.equals(attribute)) {
        //        value = unquote(value);
        //        Charset.forName(value);
        //    }
        //    else if (!isQuotedString(value)) {
        //        checkToken(value);
        //    }
        //}

        //private boolean isQuotedString(String s) {
        //    return s.length() > 1 && s.startsWith("\"") && s.endsWith("\"") ;
        //}

        //private String unquote(String s) {
        //    if (s == null) {
        //        return null;
        //    }
        //    return isQuotedString(s) ? s.substring(1, s.length() - 1) : s;
        //}

        /**
         * Return a generic parameter value, given a parameter name.
         * @param name the parameter name
         * @return the parameter value; or <code>null</code> if not present
         */
        public string GetParameter(string name)
        {
            return this.parameters[name];
        }

        /**
         * Indicate whether this {@link MediaType} includes the given media type.
         * <p>For instance, {@code text/*} includes {@code text/plain}, {@code text/html}, and {@code application/*+xml}
         * includes {@code application/soap+xml}, etc. This method is non-symmetic.
         * @param other the reference media type with which to compare
         * @return <code>true</code> if this media type includes the given media type; <code>false</code> otherwise
         */
        public bool Includes(MediaType other)
        {
            if (other == null)
            {
                return false;
            }
            if (this.IsWildcardType)
            {
                // */* includes anything
                return true;
            }
            else if (this.type == other.type)
            {
                if (this.subtype == other.subtype || this.IsWildcardSubtype)
                {
                    return true;
                }
                // application/*+xml includes application/soap+xml
                int thisPlusIdx = this.subtype.IndexOf('+');
                int otherPlusIdx = other.subtype.IndexOf('+');
                if (thisPlusIdx != -1 && otherPlusIdx != -1)
                {
                    string thisSubtypeNoSuffix = this.subtype.Substring(0, thisPlusIdx);

                    string thisSubtypeSuffix = this.subtype.Substring(thisPlusIdx + 1);
                    string otherSubtypeSuffix = other.subtype.Substring(otherPlusIdx + 1);
                    if (thisSubtypeSuffix == otherSubtypeSuffix && WILDCARD_TYPE == thisSubtypeNoSuffix)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /**
         * Indicate whether this {@link MediaType} is compatible with the given media type.
         * <p>For instance, {@code text/*} is compatible with {@code text/plain}, {@code text/html}, and vice versa.
         * In effect, this method is similar to {@link #includes(MediaType)}, except that it's symmetric.
         * @param other the reference media type with which to compare
         * @return <code>true</code> if this media type is compatible with the given media type; <code>false</code> otherwise
         */
        public bool IsCompatibleWith(MediaType other)
        {
            if (other == null)
            {
                return false;
            }
            if (this.IsWildcardType || other.IsWildcardType)
            {
                return true;
            }
            else if (this.type == other.type)
            {
                if (this.subtype == other.subtype || this.IsWildcardSubtype || other.IsWildcardSubtype)
                {
                    return true;
                }
                // application/*+xml is compatible with application/soap+xml, and vice-versa
                int thisPlusIdx = this.subtype.IndexOf('+');
                int otherPlusIdx = other.subtype.IndexOf('+');
                if (thisPlusIdx != -1 && otherPlusIdx != -1)
                {
                    string thisSubtypeNoSuffix = this.subtype.Substring(0, thisPlusIdx);
                    string otherSubtypeNoSuffix = other.subtype.Substring(0, otherPlusIdx);

                    string thisSubtypeSuffix = this.subtype.Substring(thisPlusIdx + 1);
                    string otherSubtypeSuffix = other.subtype.Substring(otherPlusIdx + 1);

                    if (thisSubtypeSuffix == otherSubtypeSuffix &&
                        (WILDCARD_TYPE == thisSubtypeNoSuffix || WILDCARD_TYPE == otherSubtypeNoSuffix))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #region IComparable<MediaType> Membres

        /**
         * Compares this {@link MediaType} to another alphabetically.
         * @param other media type to compare to
         * @see #sortBySpecificity(List)
         */
        public int CompareTo(MediaType other)
        {
            int comp = this.type.CompareTo(other.type);
            if (comp != 0)
            {
                return comp;
            }
            comp = this.subtype.CompareTo(other.subtype);
            if (comp != 0)
            {
                return comp;
            }
            comp = this.parameters.Count - other.parameters.Count;
            if (comp != 0)
            {
                return comp;
            }
            foreach(string key in this.parameters.Keys)
            {
                if (!other.parameters.ContainsKey(key))
                {
                    return -1;
                }
                comp = String.Compare(this.parameters[key], other.parameters[key]);
                if (comp != 0)
                {
                    return comp;
                }
            }
            return 0;
        }

        #endregion

        /**
         * Parse the given String into a single {@link MediaType}.
         * @param mediaType the string to parse
         * @return the media type
         * @throws IllegalArgumentException if the string cannot be parsed
         */
        public static MediaType ParseMediaType(string mediaType)
        {
            AssertUtils.ArgumentHasText(mediaType, "'mediaType' must not be empty");
            
            string[] parts = mediaType.Split(';');
            string fullType = parts[0].Trim();
            if (fullType == WILDCARD_TYPE)
            {
                fullType = "*/*";
            }
            int subIndex = fullType.IndexOf('/');
            if (subIndex == -1)
            {
                throw new ArgumentException(
                    String.Format("'{0}' does not contain '/'", mediaType), 
                    "mediaType");
            }
            if (subIndex == fullType.Length - 1)
            {
                throw new ArgumentException(
                   String.Format("'{0}' does not contain subtype after '/'", mediaType),
                   "mediaType");
            }
            string type = fullType.Substring(0, subIndex);
            string subtype = fullType.Substring(subIndex + 1);

            IDictionary<string, string> parameters = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            if (parts.Length > 1)
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    string parameter = parts[i].Trim();
                    int eqIndex = parameter.IndexOf('=');
                    if (eqIndex != -1)
                    {
                        string attribute = parameter.Substring(0, eqIndex);
                        string value = parameter.Substring(eqIndex + 1);
                        parameters.Add(attribute, value);
                    }
                }
            }

            return new MediaType(type, subtype, parameters);
        }

        /**
         * Parse the given, comma-seperated string into a list of {@link MediaType} objects.
         * <p>This method can be used to parse an Accept or Content-Type header.
         * @param mediaTypes the string to parse
         * @return the list of media types
         * @throws IllegalArgumentException if the string cannot be parsed
         */
        public static List<MediaType> ParseMediaTypes(string mediaTypes)
        {
            List<MediaType> mediaTypeList = new List<MediaType>();
            if (!StringUtils.HasLength(mediaTypes))
            {
                return mediaTypeList;
            }
            string[] tokens = mediaTypes.Split(',');
            foreach (string token in tokens)
            {
                mediaTypeList.Add(ParseMediaType(token));
            }
            return mediaTypeList;
        }

        /**
         * Return a string representation of the given list of {@link MediaType} objects.
         * <p>This method can be used to for an {@code Accept} or {@code Content-Type} header.
         * @param mediaTypes the string to parse
         * @return the list of media types
         * @throws IllegalArgumentException if the String cannot be parsed
         */
        public static string ToString(IEnumerable<MediaType> mediaTypes)
        {
            StringBuilder builder = new StringBuilder();
            foreach(MediaType mediaType in mediaTypes)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(mediaType);
            }
            return builder.ToString();
        }

        /**
         * Sorts the given list of {@link MediaType} objects by specificity.
         * <p>Given two media types:
         * <ol>
         *   <li>if either media type has a {@linkplain #isWildcardType() wildcard type}, then the media type without the
         *   wildcard is ordered before the other.</li>
         *   <li>if the two media types have different {@linkplain #getType() types}, then they are considered equal and
         *   remain their current order.</li>
         *   <li>if either media type has a {@linkplain #isWildcardSubtype() wildcard subtype}, then the media type without
         *   the wildcard is sorted before the other.</li>
         *   <li>if the two media types have different {@linkplain #getSubtype() subtypes}, then they are considered equal
         *   and remain their current order.</li>
         *   <li>if the two media types have different {@linkplain #getQualityValue() quality value}, then the media type
         *   with the highest quality value is ordered before the other.</li>
         *   <li>if the two media types have a different amount of {@linkplain #getParameter(String) parameters}, then the
         *   media type with the most parameters is ordered before the other.</li>
         * </ol>
         * <p>For example:
         * <blockquote>audio/basic &lt; audio/* &lt; *&#047;*</blockquote>
         * <blockquote>audio/* &lt; audio/*;q=0.7; audio/*;q=0.3</blockquote>
         * <blockquote>audio/basic;level=1 &lt; audio/basic</blockquote>
         * <blockquote>audio/basic == text/html</blockquote>
         * <blockquote>audio/basic == audio/wave</blockquote>
         * @param mediaTypes the list of media types to be sorted
         * @see <a href="http://tools.ietf.org/html/rfc2616#section-14.1">HTTP 1.1, section 14.1</a>
         */
        public static void SortBySpecificity(List<MediaType> mediaTypes)
        {
            AssertUtils.ArgumentNotNull(mediaTypes, "mediaTypes");

            if (mediaTypes.Count > 1)
            {
                mediaTypes.Sort(SPECIFICITY_COMPARER);
            }
        }

        /**
         * Sorts the given list of {@link MediaType} objects by quality value.
         * <p>Given two media types:
         * <ol>
         *   <li>if the two media types have different {@linkplain #getQualityValue() quality value}, then the media type
         *   with the highest quality value is ordered before the other.</li>
         *   <li>if either media type has a {@linkplain #isWildcardType() wildcard type}, then the media type without the
         *   wildcard is ordered before the other.</li>
         *   <li>if the two media types have different {@linkplain #getType() types}, then they are considered equal and
         *   remain their current order.</li>
         *   <li>if either media type has a {@linkplain #isWildcardSubtype() wildcard subtype}, then the media type without
         *   the wildcard is sorted before the other.</li>
         *   <li>if the two media types have different {@linkplain #getSubtype() subtypes}, then they are considered equal
         *   and remain their current order.</li>
         *   <li>if the two media types have a different amount of {@linkplain #getParameter(String) parameters}, then the
         *   media type with the most parameters is ordered before the other.</li>
         * </ol>
         * @param mediaTypes the list of media types to be sorted
         * @see #getQualityValue()
         */
        public static void SortByQualityValue(List<MediaType> mediaTypes)
        {
            AssertUtils.ArgumentNotNull(mediaTypes, "mediaTypes");

            if (mediaTypes.Count > 1)
            {
                mediaTypes.Sort(QUALITY_VALUE_COMPARER);
            }
        }

        public static IComparer<MediaType> SPECIFICITY_COMPARER = new SpecificityComparer();
        public static IComparer<MediaType> QUALITY_VALUE_COMPARER = new QualityValueComparer();

        #region SpecificityComparer

        private class SpecificityComparer : IComparer<MediaType>
        {
            public int Compare(MediaType x, MediaType y)
            {
                if (x.IsWildcardType && !y.IsWildcardType)
                { // */* < audio/*
                    return 1;
                }
                else if (y.IsWildcardType && !x.IsWildcardType)
                { // audio/* > */*
                    return -1;
                }
                else if (x.type != y.type)
                { // audio/basic == text/html
                    return 0;
                }
                else
                { // mediaType1.type == mediaType2.type
                    if (x.IsWildcardSubtype && !y.IsWildcardSubtype)
                    { // audio/* < audio/basic
                        return 1;
                    }
                    else if (y.IsWildcardSubtype && !x.IsWildcardSubtype)
                    { // audio/basic > audio/*
                        return -1;
                    }
                    else if (x.subtype != y.subtype)
                    { // audio/basic == audio/wave
                        return 0;
                    }
                    else
                    { // mediaType2.subtype == mediaType2.subtype
                        double quality1 = x.QualityValue;
                        double quality2 = y.QualityValue;
                        int qualityComparison = quality2.CompareTo(quality1);
                        if (qualityComparison != 0)
                        {
                            return qualityComparison;  // audio/*;q=0.7 < audio/*;q=0.3
                        }
                        else
                        {
                            int paramsSize1 = x.parameters.Count;
                            int paramsSize2 = y.parameters.Count;
                            return (paramsSize2 < paramsSize1 ? -1 : (paramsSize2 == paramsSize1 ? 0 : 1)); // audio/basic;level=1 < audio/basic
                        }
                    }
                }
            }
        }

        #endregion

        #region QualityValueComparer

        private class QualityValueComparer : IComparer<MediaType>
        {
            public int Compare(MediaType x, MediaType y)
            {
                double quality1 = x.QualityValue;
                double quality2 = y.QualityValue;
                int qualityComparison = quality2.CompareTo(quality1);
                if (qualityComparison != 0)
                {
                    return qualityComparison;  // audio/*;q=0.7 < audio/*;q=0.3
                }
                else if (x.IsWildcardType && !y.IsWildcardType)
                { // */* < audio/*
                    return 1;
                }
                else if (y.IsWildcardType && !x.IsWildcardType)
                { // audio/* > */*
                    return -1;
                }
                else if (x.type != y.type)
                { // audio/basic == text/html
                    return 0;
                }
                else
                { // mediaType1.type == mediaType2.type
                    if (x.IsWildcardSubtype && !y.IsWildcardSubtype)
                    { // audio/* < audio/basic
                        return 1;
                    }
                    else if (y.IsWildcardSubtype && !x.IsWildcardSubtype)
                    { // audio/basic > audio/*
                        return -1;
                    }
                    else if (x.subtype != y.subtype)
                    { // audio/basic == audio/wave
                        return 0;
                    }
                    else
                    {
                        int paramsSize1 = x.parameters.Count;
                        int paramsSize2 = y.parameters.Count;
                        return (paramsSize2 < paramsSize1 ? -1 : (paramsSize2 == paramsSize1 ? 0 : 1)); // audio/basic;level=1 < audio/basic
                    }
                }
            }
        }

        #endregion
    }
}
