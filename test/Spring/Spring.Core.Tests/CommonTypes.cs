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
using System.Collections;

namespace Spring
{
    public class Inventor
    {
        public string Name;
        public string Nationality;
        public string[] Inventions;
        public DateTime? DateOfGraduation;
        private DateTime dob;
        private Place pob;

        public Inventor() : this(null, DateTime.MinValue, null)
        {}

        public Inventor(string name, DateTime dateOfBirth, string nationality)
        {
            this.Name = name;
            this.dob = dateOfBirth;
            this.Nationality = nationality;
            this.pob = new Place();
        }

        public DateTime DOB
        {
            get { return dob; }
            set { dob = value; }
        }

        /// <summary>
        /// R/W PlaceOfBirth property
        /// </summary>
        public Place POB
        {
            get { return pob; }
            set { pob = value; }
        }

        /// <summary>
        /// Readonly
        /// </summary>
        public Place PlaceOfBirth
        {
            get { return pob; }
        }

        public int GetAge(DateTime on)
        {
            // not very accurate, but it will do the job ;-)
            return on.Year - dob.Year;
        }
    }

    public class Place
    {
        public string City;
        public string Country;
    }

    public class Society
    {
        public string Name = "League of Extraordinary Gentlemen";
        public static string Advisors = "advisors";
        public static string President = "president";
        public const byte ByteConst = 1;

        private IList members = new ArrayList();
        private IDictionary officers = new Hashtable();

        public IList Members
        {
            get { return members; }
        }

        public IDictionary Officers
        {
            get { return officers; }
        }

        public bool IsMember(string name)
        {
            bool found = false;
            foreach (Inventor inventor in members)
            {
                if (inventor.Name == name)
                {
                    found = true;
                    break;
                }
            }
            return found;
        }
    }


}