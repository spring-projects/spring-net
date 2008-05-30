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

using System;

namespace SpringAir.Domain
{
    /// <summary>
    /// A person having a (flight) reservation.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: Passenger.cs,v 1.1 2005/10/01 22:52:57 springboy Exp $</version>
    [Serializable]
    public class Passenger : Entity
    {
        private string firstName;
        private string surname;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="SpringAir.Domain.Passenger"/> class.
        /// </summary>
        public Passenger()
        {
        }


        public Passenger(long id, string firstName, string surname) : base(id)
        {
            this.firstName = firstName;
            this.surname = surname;
        }

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string Surname
        {
            get { return surname; }
            set { surname = value; }
        }
    }
}