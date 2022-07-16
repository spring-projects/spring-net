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

using System.ServiceModel;

using Spring.Objects.Factory;

namespace Spring.ServiceModel
{
    /// <summary>
    /// <see cref="IFactoryObject"/> that creates a channel that is used by clients
    /// to send messages to a specified endpoint address.
    /// </summary>
    /// <typeparam name="T">The type of channel produced by the channel factory.</typeparam>
    /// <author>Bruno Baia</author>
    public class ChannelFactoryObject<T> : ChannelFactory<T>, IFactoryObject
    {
        #region Logging

        private static readonly Common.Logging.ILog Log = Common.Logging.LogManager.GetLogger(typeof(ChannelFactoryObject<>));

        #endregion

        private bool _isSingleton = true;
        private string _endpointConfigurationName;

        /// <summary>
        /// Creates a new instance of the <see cref="ChannelFactoryObject{T}"/> class.
        /// </summary>
        /// <param name="endpointConfigurationName">
        /// The configuration name used for the endpoint.
        /// </param>
        public ChannelFactoryObject(string endpointConfigurationName)
            : base(endpointConfigurationName)
        {
            this._endpointConfigurationName = endpointConfigurationName;
        }

        /// <summary>
        /// Gets the configuration name used for the endpoint.
        /// </summary>
        public string EndpointConfigurationName
        {
            get { return this._endpointConfigurationName; }
        }

        #region IFactoryObject Membres

        /// <summary>
        /// Return an instance (possibly shared or independent) of the channel
        /// managed by this factory.
        /// </summary>
        /// <returns>
        /// An instance (possibly shared or independent) of the channel managed by
        /// this factory.
        /// </returns>
        public object GetObject()
        {
            #region Instrumentation

            if (Log.IsDebugEnabled)
            {
                Log.Debug(String.Format(
                    "Creating channel of type '{0}' for the specified endpoint '{1}'...",
                    typeof(T).FullName, this._endpointConfigurationName));
            }

            #endregion

            return this.CreateChannel();
        }

        /// <summary>
        /// Return the <see cref="System.Type"/> of channel that this
        /// <see cref="Spring.Objects.Factory.IFactoryObject"/> creates.
        /// </summary>
        public Type ObjectType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Is the object managed by this factory a singleton or a prototype ?
        /// </summary>
        /// <remarks>
        /// Default value is <see langword="true"/>.
        /// </remarks>
        public bool IsSingleton
        {
            get { return this._isSingleton; }
            set { this._isSingleton = value; }
        }

        #endregion
    }
}
