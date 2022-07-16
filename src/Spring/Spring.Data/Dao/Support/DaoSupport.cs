#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

using Common.Logging;
using Spring.Objects.Factory;

namespace Spring.Dao.Support
{
	/// <summary>
	/// Generic base class for DAOs, defining template methods for DAO initialization.
	/// </summary>
	/// <remarks>
    /// Extended by Spring's specific DAO support classes, such as:
    ///  AdoDaoSupport, HibernateDaoSupport, etc.
	/// </remarks>
	/// <author>Mark Pollack (.NET)</author>
	public abstract class DaoSupport : IInitializingObject
	{
		#region Fields

		#endregion

		#region Constants

		/// <summary>
		/// The shared <see cref="Common.Logging.ILog"/> instance for this class (and derived classes).
		/// </summary>
		protected static readonly ILog log =
			LogManager.GetLogger(typeof (DaoSupport));

		#endregion

		#region Constructor (s)
		/// <summary>
		/// Initializes a new instance of the <see cref="DaoSupport"/> class.
                /// </summary>
		public 	DaoSupport()
		{

		}

		#endregion

		#region Properties

		#endregion

		#region Methods

        /// <summary>
        /// Abstract subclasses must override this to check their configuration.
        /// </summary>
        /// <remarks>
        /// <p>Implementors should be marked as <code>sealed</code>, to make it clear that
        /// concrete subclasses are not supposed to override this template method themselves.
        /// </p>
        /// </remarks>
        protected abstract void CheckDaoConfig();

        /// <summary>
        /// Concrete subclasses can override this for custom initialization behavior.
        /// </summary>
        /// <remarks>
        /// Gets called after population of this instance's object properties.
        /// Exception thrown if InitDao fails will be rethrown as
        /// a ObjectInitializationException.
        /// </remarks>
        protected virtual void InitDao()
        {

        }


		#endregion

	    public void AfterPropertiesSet()
	    {
            // Let abstract subclasses check their configuration.
            CheckDaoConfig();

            // Let concrete implementations initialize themselves.
            try
            {
                InitDao();
            }
            catch (Exception ex)
            {
                throw new ObjectInitializationException("Initialization of DAO failed: " + ex.Message, ex);
            }
	    }
	}
}
