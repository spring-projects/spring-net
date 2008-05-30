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
using System.Collections;
using System.Collections.Specialized;
using System.Text;

#endregion

namespace SpringAir.Domain
{
    /// <summary>
    /// An aircraft.
    /// </summary>
    /// <author>Keith Donald</author>
    /// <author>Rick Evans (.NET)</author>
    /// <version>$Id: Aircraft.cs,v 1.6 2007/03/16 13:14:10 bbaia Exp $</version>
    [Serializable]
    public class Aircraft : Entity
    {
        private string model;
        private IDictionary cabins = new ListDictionary();

        #region Constructor (s) / Destructor

        public Aircraft()
        {
        }

        public Aircraft(string model, params Cabin[] cabins)
            : this(Transient, model, cabins)
        {
        }

        public Aircraft(long id, string model, params Cabin[] cabins) : base(id)
        {
            this.model = model;
            foreach (Cabin cabin in cabins)
            {
                this.cabins.Add(cabin.CabinClass, cabin);
            }
        }

        #endregion

        #region Properties

        public string Model
        {
            get { return this.model; }
			set { this.model = value; }
        }

        public Cabin[] Cabins
        {
            get { return (Cabin[]) new ArrayList(cabins.Values).ToArray(typeof(Cabin)); }
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="System.String"/> representation of this
        /// <see cref="SpringAir.Domain.Aircraft"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> representation of this
        /// <see cref="SpringAir.Domain.Aircraft"/>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append(Model).Append(" [");
            
            string separator = "";
            foreach (Cabin cabin in cabins.Values)
            {
                buffer.Append(separator).Append(cabin);
                separator = ", ";
            }
            buffer.Append("]");
            return buffer.ToString();
        }
    }
}