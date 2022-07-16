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

using System.Runtime.Serialization;

namespace Spring.Messaging.Ems.Common
{
    /// <summary>
    /// An interface containing all methods and properties on the TIBCO.EMS.ConnectionFactory class.
    /// Refer to the TIBCO EMS API documentation for more information.
    /// </summary>
    /// <remarks>
    /// All the 'GetXXX()' methods in the TIBCO.EMS.ConnectionFactory were translated to .NET properties
    /// </remarks>
    /// <author>Mark Pollack</author>
    public interface IConnectionFactory : ISerializable, ICloneable
    {
        /// <summary>
        /// Gets the native TIBCO EMS connection factory.
        /// </summary>
        /// <value>The native connection factory.</value>
        ConnectionFactory NativeConnectionFactory { get; }

        /// <summary>
        /// Creates the connection.
        /// </summary>
        /// <returns>The newly created connection</returns>
        IConnection CreateConnection();

        /// <summary>
        /// Creates the connection with the given username and password
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        IConnection CreateConnection(string userName, string password);

        /// <summary>
        /// Gets the certificate store info object associated with this connection factory.
        /// </summary>
        /// <value>The certificate store.</value>
        object CertificateStore { get; }

        /// <summary>
        /// Gets or sets the SSL proxy host.
        /// </summary>
        /// <value>The SSL proxy host.</value>
        string SSLProxyHost { set;  get; }

        /// <summary>
        /// Gets or sets the SSL proxy port.
        /// </summary>
        /// <value>The SSL proxy port.</value>
        int SSLProxyPort { set; get; }

        /// <summary>
        /// Gets the SSL proxy password.
        /// </summary>
        /// <value>The SSL proxy password.</value>
        string SSLProxyPassword { get; }

        /// <summary>
        /// Gets the SSL proxy user.
        /// </summary>
        /// <value>The SSL proxy user.</value>
        string SSLProxyUser { get; }

        /// <summary>
        /// Sets the type of the certificate store.
        /// </summary>
        /// <value>The type of the certificate store.</value>
        IEmsSSLStoreType CertificateStoreType { set;  }

        /// <summary>
        /// Sets the client ID.
        /// </summary>
        /// <value>The client ID.</value>
        string ClientID { set; }

        /// <summary>
        /// Sets the client tracer to a given output stream
        /// </summary>
        /// <value>The client tracer.</value>
        StreamWriter ClientTracer { set; }

        /// <summary>
        /// Sets the number of connection attempts
        /// </summary>
        /// <value>The number of connection attempts.</value>
        int ConnAttemptCount { set; }

        /// <summary>
        /// Sets delay between connection attempts
        /// </summary>
        /// <value>The delay between connection attempts.</value>
        int ConnAttemptDelay { set; }

        /// <summary>
        /// Sets the connection attempt timeout.
        /// </summary>
        /// <value>The connection attempt timeout.</value>
        int ConnAttemptTimeout { set; }

        /// <summary>
        /// Sets the custom host name verifier. Set to null to remove custom host name verifier.
        /// </summary>
        /// <value>The host name verifier.</value>
        EMSSSLHostNameVerifier HostNameVerifier { set; }

        /// <summary>
        /// Sets the load balance metric as an integer.
        /// </summary>
        /// <value>The load balance metric as int.</value>
        int MetricAsInt { set; }

        /// <summary>
        /// Sets the port on which the client will connect to the multicast daemon.
        /// </summary>
        /// <value>The multicast daemon port.</value>
        string MulticastDaemon { set; }

        /// <summary>
        /// Sets whether MessageConsumers subscribed to a multicast-enabled topic will receive messages over multicast.
        /// </summary>
        /// <value><c>true</c> to enable multicast; <c>false</c> to disable.</value>
        bool MulticastEnabled { set; }

        int ReconnAttemptCount { set; }

        int ReconnAttemptDelay { set; }

        int ReconnAttemptTimeout { set; }

        string ServerUrl { set; }

        bool SSLAuthOnly { set; }

        string SSLProxyAuthUsername { set; get; }

        string SSLProxyAuthPassword { set; get; }

        bool SSLTrace { set; }

        string TargetHostName { set; }

        string UserName { set; }

        string UserPassword { set; }

        string ToString();

        FactoryLoadBalanceMetric Metric { get; set; }
    }
}
