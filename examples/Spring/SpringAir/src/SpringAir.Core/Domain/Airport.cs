#region Licence

/*
 * Copyright © 2002-2005 the original author or authors.
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

using System;
using System.Text;

#endregion

namespace SpringAir.Domain
{
    /// <summary>
    /// Models an airport.
    /// </summary>
    /// <author>Keith Donald</author>
    /// <author>Rick Evans (.NET)</author>
    /// <version>$Id: Airport.cs,v 1.7 2005/12/13 00:42:50 bbaia Exp $</version>
    [Serializable]
    public class Airport : Entity
    {
        private string code;
        private string city;
        private string name;

        #region Constructors

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="SpringAir.Domain.Airport"/> class.
		/// </summary>
		public Airport()
		{
		}

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Domain.Airport"/> class.
        /// </summary>
        /// <param name="code">
        /// The short code uniquely identifying this airport.
        /// </param>
        /// <param name="city">
        /// The city in which this airport is based.
        /// </param>
        /// <param name="name">
        /// A full name of this airport.
        /// </param>
        public Airport(string code, string city, string name)
            : this (Transient, code, city, name)
        {
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Domain.Airport"/> class.
        /// </summary>
        /// <param name="id">
        /// The number that uniquely identifies this airport.
        /// </param>
        /// <param name="code">
        /// The short code uniquely identifying this airport.
        /// </param>
        /// <param name="city">
        /// The city in which this airport is based.
        /// </param>
        /// <param name="name">
        /// A full name of this airport.
        /// </param>
        public Airport(long id, string code, string city, string name) : base(id)
        {
            this.code = code;
            this.city = city;
            this.name = name;
        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets or Sets the short code uniquely identifying this airport.
        /// </summary>
        public string Code
        {
            get { return this.code; }
			set { this.code = value; }
        }

        /// <summary>
        /// Gets or Sets the city in which this airport is based.
        /// </summary>
        public string City
        {
            get { return this.city; }
			set { this.city = value; }
        }

        /// <summary>
        /// Gets or Sets the full name of this airport.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return this.name; }
			set { this.name = value; }
        }

        /// <summary>
        /// A description of this airport; concatenation of the full
        /// name of this airport and airport code.
        /// </summary>
        public string Description
        {
            get { return this.name + (this.Code.Length > 0 ? " (" + this.Code + ")" : ""); }
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="System.String"/> representation of this
        /// <see cref="SpringAir.Domain.Airport"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> representation of this
        /// <see cref="SpringAir.Domain.Airport"/>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append(Name).Append(", ")
                .Append(City).Append(" (").Append(Code).Append(")");
            return buffer.ToString();
        }
    }
}