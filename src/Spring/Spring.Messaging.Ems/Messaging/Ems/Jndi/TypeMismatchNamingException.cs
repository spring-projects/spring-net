#region License

/*
 * Copyright � 2002-2010 the original author or authors.
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

namespace Spring.Objects.Factory
{
	/// <summary>
    /// Exception thrown if a type mismatch is encountered for an object
    /// located in a JNDI environment.
	/// </summary>
	/// <author>Juergen Hoeller</author>
	/// <author>Mark Pollack (.NET)</author>
	[Serializable]
    public class TypeMismatchNamingException : NamingException
	{
        private Type requiredType;

        private Type actualType;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Objects.Factory.ObjectCreationException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public TypeMismatchNamingException(string message)
			: base(message)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMismatchNamingException"/> class
        /// building an explanation text from the given arguments.
        /// </summary>
        /// <param name="jndiName">The Jndi name.</param>
        /// <param name="requiredType">Type required type of the lookup.</param>
        /// <param name="actualType">The actual type that the lookup returned.</param>
        public TypeMismatchNamingException(String jndiName, Type requiredType, Type actualType) :
            base("Object of type [" + actualType + "] available at JNDI location [" +
                    jndiName + "] is not assignable to [" + requiredType.Name + "]")
        {
            this.requiredType = requiredType;
            this.actualType = actualType;
        }

        /// <summary>
        /// Gets the actual type that the lookup returned, if available.
        /// </summary>
        /// <value>The actual type that the lookup.</value>
        public Type ActualType
        {
            get { return actualType; }
        }

        /// <summary>
        /// Gets the required type for the lookup, if available.
        /// </summary>
        /// <value>The equired type for the lookup</value>
	    public Type RequiredType
	    {
            get { return requiredType; }
	    }
	}
}
