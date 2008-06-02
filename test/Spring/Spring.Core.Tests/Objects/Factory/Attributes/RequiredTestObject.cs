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


namespace Spring.Objects.Factory.Attributes
{
    /// <summary>
    /// Test object for testing 'required' attribute functionality.
    /// </summary>
    /// <author>Mark Pollack</author>
    public class RequiredTestObject : IObjectNameAware, IObjectFactoryAware
    {
        private string name;

        private int age;

        private string favoriteColor; 

        private string jobTitle;

        [Required]
        public int Age
        {
            get { return age; }
            set { age = value; }
        }

        [MyRequired]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        [Required]
        public string FavoriteColor
        {
            set { favoriteColor = value; }
        }

        public string GetFavoriteColor()
        {
            return favoriteColor;
        }

        [Required]
        public string JobTitle
        {
            get { return jobTitle; }
            set { jobTitle = value; }
        }

        [Required]
        public string ObjectName
        {
            set { }
        }

        [Required]
        public IObjectFactory ObjectFactory
        {
            set {  }
        }
    }
}