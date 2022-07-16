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

using Spring.Objects.Factory;

namespace Spring.Remoting
{
    /// <summary>
    /// Factory for creating a reference to a
    /// remote server activated object (SAO).
    /// </summary>
    /// <remarks>
    /// This is useful alternative to adminstrative type registration on
    /// the client when you would like the client to have only
    /// a reference to the interface that an SAO implements and not the
    /// actual SAO implentation.
    /// </remarks>
	/// <author>Aleksandar Seovic</author>
	/// <author>Mark Pollack</author>
	/// <author>Bruno Baia</author>
    public class SaoFactoryObject : IFactoryObject, IInitializingObject
    {
		#region Fields

        private Type serviceInterface;
        private string serviceUrl;

		#endregion

        #region Properties

        /// <summary>
        /// The remote service interface.
        /// </summary>
        public Type ServiceInterface
        {
            get { return serviceInterface; }
            set { serviceInterface = value; }
        }

        /// <summary>
        /// The URI of the well known object
        /// </summary>
        public string ServiceUrl
        {
            get { return serviceUrl; }
            set { serviceUrl = value; }
        }

        #endregion

		#region Constructor(s) / Destructor

		/// <summary>
		/// Creates a new instance of the SaoFactoryObject class.
		/// </summary>
		public SaoFactoryObject()
		{
		}

		#endregion

		#region IInitializingObject Members

		/// <summary>
		/// Callback method called once all factory properties have been set.
		/// </summary>
		/// <exception cref="System.Exception">if an error occured</exception>
		public void AfterPropertiesSet()
		{
			if (ServiceUrl == null)
			{
				throw new ArgumentException("The ServiceUrl property is required.");
			}

			if (ServiceInterface == null)
			{
				throw new ArgumentException("The ServiceInterface property is required.");
			}

			if (!ServiceInterface.IsInterface)
			{
				throw new ArgumentException("ServiceInterface must be an interface");
			}
		}

		#endregion

		#region IFactoryObject Members

		/// <summary>
		/// Is the object managed by this factory a singleton or a prototype?
		/// </summary>
		public bool IsSingleton
		{
			get { return false; }
		}

		/// <summary>
		/// The type of object to be created.
		/// </summary>
		public Type ObjectType
		{
			get { return serviceInterface; }
		}

		/// <summary>
		/// Return the SAO proxy.
		/// </summary>
		/// <returns>the SAO proxy</returns>
		public object GetObject()
		{
			return Activator.GetObject(serviceInterface, serviceUrl);
		}

		#endregion
    }
}
