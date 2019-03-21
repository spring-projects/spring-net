#region License

/*
 * Copyright 2002-2006 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

namespace Spring.IocQuickStart.AppContext
{
	/// <summary>
	/// Summary description for Person.
	/// </summary>
	public class Person
	{
        private int age;
        private string name;
        
        /// <summary>
        /// Property Name (string)
        /// </summary>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        
        /// <summary>
        /// Property Age (int)
        /// </summary>
        public int Age
        {
            get { return this.age; }
            set { this.age = value; }
        }

        public Person()
        {
        }

        public override string ToString()
        {
            return "Person = [Name=" + Name + ", Age="+ Age + "]";
        }
	}
}
