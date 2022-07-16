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

using Spring.Util;

namespace Spring.Globalization.Formatters
{
    /// <summary>
    /// Implementation of <see cref="IFormatter"/> that can be used to
    /// format and parse boolean values.
    /// </summary>
    /// <author>Erich Eichinger</author>
    public class BooleanFormatter:IFormatter
    {
        private string trueString;
        private string falseString;
        private bool ignoreCase;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFormatter"/> class
        /// using default values
        /// </summary>
        public BooleanFormatter()
            : this(true,bool.TrueString,bool.FalseString)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFormatter"/> class
        /// </summary>
        public BooleanFormatter(bool ignoreCase,string trueString,string falseString)
        {
            this.trueString = trueString;
            this.falseString = falseString;
            this.ignoreCase = ignoreCase;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Set/Get value to control casesensitivity of <see cref="Parse"/>
        /// </summary>
        /// <remarks>
        /// Defaults to <value>true</value>
        /// </remarks>
        public bool IgnoreCase
        {
            get { return this.ignoreCase; }
            set { this.ignoreCase = value; }
        }

        /// <summary>
        /// Set/Get value to recognize as boolean "true" value
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="bool.TrueString" />
        /// </remarks>
        public string TrueString
        {
            get { return this.trueString; }
            set { this.trueString = value; }
        }

        /// <summary>
        /// Set/Get value to recognize as boolean "false" value
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="bool.FalseString" />
        /// </remarks>
        public string FalseString
        {
            get { return this.falseString; }
            set { this.falseString = value; }
        }

        #endregion

        /// <summary>
        /// Formats the specified boolean value.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>Formatted boolean value.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="value"/> is not of type <see cref="System.Boolean"/>.</exception>
        public string Format(object value)
        {
            AssertUtils.ArgumentNotNull( value,"value" );
            if(!(value is bool))
            {
                throw new ArgumentException( "BooleanFormatter can only be used to format instances of [System.Boolean]" );
            }
            return ((bool) value) ? this.trueString : this.falseString;
        }

        /// <summary>
        /// Parses the specified boolean value according to settings of <see cref="TrueString"/> and <see cref="FalseString"/>
        /// </summary>
        /// <param name="value">The boolean value to parse.</param>
        /// <returns>Parsed boolean value as a <see cref="System.Boolean"/>.</returns>
        /// <exception cref="ArgumentException">If <paramref name="value"/> does not match <see cref="TrueString"/> or <see cref="FalseString"/>.</exception>
        public object Parse(string value)
        {
            if(!StringUtils.HasText( value ))
            {
                return false;
            }

            if ( 0 == string.Compare( value, this.trueString, this.ignoreCase ))
            {
                return true;
            }
            else if(0 == string.Compare( value,this.falseString,this.ignoreCase ))
            {
                return false;
            }
            else
            {
                throw new ArgumentException( string.Format("input value '{0}' is not recognized as a boolean", value) );
            }
        }
    }
}
