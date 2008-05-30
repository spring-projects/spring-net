#region License

/*
 * Copyright © 2002-2006 the original author or authors.
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

#endregion

namespace Spring.AopQuickStart.Attributes
{
    /// <summary>
    /// This attribute can be used to mark properties and methods 
    /// on which each call should be logged.
    /// </summary>
    /// <author>Bruno Baia</author>
    /// <version>$Id: ConsoleLoggingAttribute.cs,v 1.1 2006/12/02 13:30:02 bbaia Exp $</version>
    public class ConsoleLoggingAttribute : Attribute
    {
#if NET_2_0
        private ConsoleColor _color = ConsoleColor.Gray;

        /// <summary>
        /// Gets or sets the foreground color of the console.
        /// </summary>
        public ConsoleColor Color
        {
            get { return _color; }
            set { _color = value; }
        }

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="Spring.AopQuickStart.Attributes.ConsoleLoggingAttribute"/> class.
        /// </summary>
        /// <param name="level">
        /// The foreground color of the console.
        /// </param>
        public ConsoleLoggingAttribute(ConsoleColor color)
        {
            _color = color;
        }
#endif

        /// <summary>
		/// Creates a new instance of the
        /// <see cref="Spring.AopQuickStart.Attributes.ConsoleLoggingAttribute"/> class.
		/// </summary>
		public ConsoleLoggingAttribute()
		{
		}
    }
}