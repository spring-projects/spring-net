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

#region Imports

#endregion

namespace Spring.Objects.Factory {

	/// <summary>
	/// Simple test of IObjectFactory initialization.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Rick Evans (.NET)</author>
    public class MustBeInitialized : IInitializingObject 
    {

        #region Constructor (s) / Destructor
        /// <summary>
        /// Creates a new instance of the <see cref="Spring.Objects.Factory.MustBeInitialized"/> class.
        /// </summary>
        public MustBeInitialized() {}
        #endregion

        #region Methods
        public void AfterPropertiesSet () 
        {
            inited = true;
        }

        /// <summary>
        /// Dummy business method that will fail unless the factory
        /// managed the object's lifecycle correctly.
        /// </summary>
        public virtual void BusinessMethod ()
        {
            if (!inited) 
            {
                throw new SystemException (
                    "Factory didn't call AfterPropertiesSet () on MustBeInitialized object");
            }
        }
        #endregion

        #region Fields
        private bool inited;
        #endregion
	}
}
