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

using Apache.NMS;

namespace Spring.Messaging.Nms.Connections
{
    /// <summary>
    ///  An adapter for a target JMS {@link javax.jms.ConnectionFactory}, applying the
    ///  given user credentials to every standard <code>CreateConnection()</code> call,
    ///  that is, implicitly invoking <code>CreateConnection(username, password)</code>
    ///  on the target.All other methods simply delegate to the corresponding methods
    ///  of the target ConnectionFactory.
    /// </summary>
    /// <remarks>
    /// Can be used to proxy a target NMS ConnectionFactory that does not have user
    /// credentials configured. Client code can work with the ConnectionFactory without
    /// passing in username and password on every <code>CreateConnection()</code> call.
    /// If the "Username" is empty, this proxy will simply delegate to the standard
    /// <code>CreateConnection()</code> method of the target ConnectionFactory.
    /// This can be used to keep a UserCredentialsConnectionFactoryAdapter 
    /// definition just for the<i> option</i> of implicitly passing in user credentials
    /// if the particular target ConnectionFactory requires it.
    /// </remarks>
    public class UserCredentialsConnectionFactoryAdapter:IConnectionFactory
    {
        private readonly IConnectionFactory _wrappedConnectionFactory;

        private readonly ThreadLocal<NmsUserCredentials> threadLocalCredentials = new ThreadLocal<NmsUserCredentials>();

        public UserCredentialsConnectionFactoryAdapter(IConnectionFactory wrappedConnectionFactory)
        {
            this._wrappedConnectionFactory = wrappedConnectionFactory;
        }


        /// <summary>
        /// Set user credentials for this proxy and the current thread. 
        /// The given username and password will be applied to all subsequent
        /// <code>CreateConnection()</code>  calls on this ConnectionFactory proxy.
        /// This will override any statically specified user credentials,
        /// that is, values of the "username" and "password" properties.
        /// </summary>
        public void SetCredentialsForCurrentThread(string userName, string password)
        {
            this.threadLocalCredentials.Value = new NmsUserCredentials(userName, password);
        }

        /// <summary>
        /// Remove any user credentials for this proxy from the current thread.
        /// Statically specified user credentials apply again afterwards.
        /// </summary>
        public void RemoveCredentialsFromCurrentThread()
        {
            this.threadLocalCredentials.Value = null;
        }


        private string _userName;
        
        /// <summary>
        /// Set the username that this adapter should use for retrieving Connections.
        /// </summary>
        public string UserName
        {
            get =>  _userName;
            set => _userName = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private string UserNameInternal => threadLocalCredentials.Value != null ? threadLocalCredentials.Value.UserName : UserName;
        private string PasswordInternal => threadLocalCredentials.Value != null ? threadLocalCredentials.Value.Password : Password;
        
        
        private string _password;

        /// <summary>
        /// Set the password that this adapter should use for retrieving Connections.
        /// </summary>
        public string Password
        {
            get => _password;
            set => _password = string.IsNullOrEmpty(value) ? null : value;
        }

        public IConnection CreateConnection()
        {
            return CreateConnectionForSpecificCredentials(UserNameInternal, PasswordInternal);
        }

        private IConnection CreateConnectionForSpecificCredentials(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) == false)
            {
                return CreateConnection(userName, password);
            }

            return _wrappedConnectionFactory.CreateConnection();
        }

        public IConnection CreateConnection(string userName, string password)
        {
            return _wrappedConnectionFactory.CreateConnection(userName, password);
        }

        public Task<IConnection> CreateConnectionAsync()
        {
            return _wrappedConnectionFactory.CreateConnectionAsync(UserNameInternal, PasswordInternal);
        }

        public Task<IConnection> CreateConnectionAsync(string userName, string password)
        {
            return _wrappedConnectionFactory.CreateConnectionAsync(userName, password);
        }

        public INMSContext CreateContext()
        {
            return _wrappedConnectionFactory.CreateContext(UserNameInternal, PasswordInternal);
        }

        public INMSContext CreateContext(AcknowledgementMode acknowledgementMode)
        {
            return _wrappedConnectionFactory.CreateContext(UserNameInternal, PasswordInternal, acknowledgementMode);
        }

        public INMSContext CreateContext(string userName, string password)
        {
            return _wrappedConnectionFactory.CreateContext(userName, password);
        }

        public INMSContext CreateContext(string userName, string password, AcknowledgementMode acknowledgementMode)
        {
            return _wrappedConnectionFactory.CreateContext(userName, password, acknowledgementMode);
        }

        public Task<INMSContext> CreateContextAsync()
        {
            return _wrappedConnectionFactory.CreateContextAsync(UserNameInternal, PasswordInternal);
        }

        public Task<INMSContext> CreateContextAsync(AcknowledgementMode acknowledgementMode)
        {
            return _wrappedConnectionFactory.CreateContextAsync(UserNameInternal, PasswordInternal, acknowledgementMode);
        }

        public Task<INMSContext> CreateContextAsync(string userName, string password)
        {
            return _wrappedConnectionFactory.CreateContextAsync(userName, password);
        }

        public Task<INMSContext> CreateContextAsync(string userName, string password, AcknowledgementMode acknowledgementMode)
        {
            return _wrappedConnectionFactory.CreateContextAsync(userName, password, acknowledgementMode);
        }

        public Uri BrokerUri
        {
            get => _wrappedConnectionFactory.BrokerUri;
            set => _wrappedConnectionFactory.BrokerUri = value;
        }

        public IRedeliveryPolicy RedeliveryPolicy
        {
            get => _wrappedConnectionFactory.RedeliveryPolicy;
            set => _wrappedConnectionFactory.RedeliveryPolicy = value;
        }

        public ConsumerTransformerDelegate ConsumerTransformer
        {
            get => _wrappedConnectionFactory.ConsumerTransformer;
            set => _wrappedConnectionFactory.ConsumerTransformer = value;
        }

        public ProducerTransformerDelegate ProducerTransformer
        {
            get => _wrappedConnectionFactory.ProducerTransformer;
            set => _wrappedConnectionFactory.ProducerTransformer = value;
        }

        private class NmsUserCredentials
        {
            public string UserName { get; }
            public string Password { get; }

            public NmsUserCredentials(string userName, string password)
            {
                UserName = userName;
                Password = password;
            }
        }
    }
}
