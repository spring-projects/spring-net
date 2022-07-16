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
using Spring.Remoting.Support;

namespace Spring.Remoting
{
    /// <summary>
    /// Factory for creating a reference to a
    /// client activated object (CAO).
    /// </summary>
	/// <author>Aleksandar Seovic</author>
	/// <author>Mark Pollack</author>
	/// <author>Bruno Baia</author>
    public class CaoFactoryObject : IFactoryObject, IInitializingObject
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(CaoFactoryObject));

        #endregion

		#region Fields

        private string remoteTargetName;
        private string serviceUrl;
		private object[] constructorArguments;

		#endregion

        #region Properties

        /// <summary>
        /// The remote target name to activate.
        /// </summary>
        public string RemoteTargetName
        {
            get { return remoteTargetName; }
            set { remoteTargetName = value; }
        }

        /// <summary>
        /// The Uri of the remote type.
        /// </summary>
        public string ServiceUrl
        {
            get { return serviceUrl; }
            set { serviceUrl = value; }
        }

		/// <summary>
		/// Argument list used to call the CAO constructor.
		/// </summary>
		public object[] ConstructorArguments
		{
			get { return constructorArguments; }
			set { constructorArguments = value; }
		}

        #endregion

		#region Constructor(s) / Destructor

		/// <summary>
		/// Creates a new instance of the <see cref="CaoFactoryObject"/> class.
		/// </summary>
		public CaoFactoryObject()
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
			if (RemoteTargetName == null)
			{
				throw new ArgumentException("The RemoteTargetName property is required.");
			}

            if (ServiceUrl == null)
            {
                throw new ArgumentException("The ServiceUrl property is required.");
            }
        }

		#endregion

		#region IFactoryObject Members

		/// <summary>
		/// Always return false.
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
			get { return typeof(MarshalByRefObject); }
		}

		/// <summary>
		/// Return the CAO proxy.
		/// </summary>
		/// <returns>the CAO proxy</returns>
		public object GetObject()
		{
		    string url = serviceUrl.TrimEnd('/') + '/' + remoteTargetName;
            if (LOG.IsDebugEnabled)
            {
                LOG.Debug("Accessing CAO object of type ICaoRemoteFactory object at url = [" + url + "]");
            }
			ICaoRemoteFactory remoteFactory = (ICaoRemoteFactory) Activator.GetObject(typeof(ICaoRemoteFactory), url);

			if (constructorArguments != null)
			{
				return remoteFactory.GetObject(constructorArguments);
			}
			else
			{
				return remoteFactory.GetObject();
			}
		}

		#endregion
    }
}
