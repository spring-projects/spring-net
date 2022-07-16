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

using System.Runtime.Remoting;

using Common.Logging;
using Spring.Core;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;

namespace Spring.Remoting
{
    /// <summary>
    /// Convenience class to configure remoting infrastructure from the IoC container.
    /// </summary>
    /// <author>Bruno Baia</author>
    public class RemotingConfigurer : IObjectFactoryPostProcessor, IOrdered
    {
		#region Fields

        private static readonly ILog log = LogManager.GetLogger(typeof(RemotingConfigurer));

        private int _order = Int32.MinValue;

        private IResource _filename;
        private bool _useConfigFile = true;
        private bool _ensureSecurity = false;

		#endregion

		#region Constructor(s) / Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RemotingConfigurer"/> class.
        /// </summary>
        public RemotingConfigurer()
        {}

		#endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of the remoting configuration file.
        /// </summary>
        /// <remarks>
        /// If filename is <see langword="null"/> or not set,
        /// current AppDomain's configuration file will be used.
        /// </remarks>
        public IResource Filename
        {
            get { return _filename; }
            set { _filename = value; }
        }

        /// <summary>
        /// Indicates whether a configuration file is used.
        /// Default value is <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// If <see langword="false"/>, default remoting configuration will be used.
        /// </remarks>
        public bool UseConfigFile
        {
            get { return _useConfigFile; }
            set { _useConfigFile = value; }
        }

        /// <summary>
        /// Gets or sets if security is enabled.
        /// </summary>
        /// <remarks>
        /// This property is only available since .NET Framework 2.0.
        /// </remarks>
        public bool EnsureSecurity
        {
            get { return _ensureSecurity; }
            set { _ensureSecurity = value; }
        }

        #endregion

        #region IObjectFactoryPostProcessor Members

        /// <summary>
        /// Modify the application context's internal object factory after its
        /// standard initialization.
        /// </summary>
        /// <param name="factory">
        /// The object factory used by the application context.
        /// </param>
        /// <seealso cref="Spring.Objects.Factory.Config.IObjectFactoryPostProcessor.PostProcessObjectFactory(IConfigurableListableObjectFactory)"/>
        public void PostProcessObjectFactory(IConfigurableListableObjectFactory factory)
        {
            string filename = null;
            if (UseConfigFile)
            {
                filename = (Filename == null)
                    ? AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
                    : Filename.File.FullName;
            }

            RemotingConfiguration.Configure(filename, EnsureSecurity);

            #region Instrumentation

            if (log.IsDebugEnabled)
            {
                if (filename == null)
                {
                    log.Debug("Default remoting infrastructure loaded.");
                }
                else
                {
                    log.Debug(String.Format("Remoting infrastructure configured using file '{0}'.", filename));
                }
            }

            #endregion
        }

        #endregion

        #region IOrdered Members

        /// <summary>
        /// Return the order value of this object,
        /// where a higher value means greater in terms of sorting.
        /// </summary>
        public int Order
        {
            get { return _order; }
        }

        #endregion
    }
}
