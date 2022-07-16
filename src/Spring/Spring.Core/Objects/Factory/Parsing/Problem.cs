#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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

using System.Text;
using Spring.Util;

namespace Spring.Objects.Factory.Parsing
{
    public class Problem
    {
        private string _message;

        private Location _location;

        private Exception _rootCause;


        /// <summary>
        /// Initializes a new instance of the <see cref="Problem"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="location">The location.</param>
        public Problem(string message, Location location)
            : this(message, location, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the Problem class.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="location"></param>
        /// <param name="rootCause"></param>
        public Problem(string message, Location location, Exception rootCause)
        {
            AssertUtils.ArgumentNotNull(message, "message");
            AssertUtils.ArgumentNotNull(location, "resource");

            _message = message;
            _location = location;
            _rootCause = rootCause;
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }

        public Location Location
        {
            get
            {
                return _location;
            }
        }

        public string ResourceDescription
        {
            get { return _location.Resource != null ? _location.Resource.Description : string.Empty; }
        }

        public override string ToString()
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("Configuration problem: ");
            sb.Append(Message);
            sb.Append("\nOffending resource: ").Append(ResourceDescription);

            return sb.ToString();

        }

    }
}
