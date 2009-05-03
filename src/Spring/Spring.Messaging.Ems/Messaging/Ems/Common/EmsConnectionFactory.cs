#region License

/*
 * Copyright © 2002-2009 the original author or authors.
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

using System.IO;
using System.Runtime.Serialization;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Common
{
    public class EmsConnectionFactory : IConnectionFactory
    {
        private ConnectionFactory nativeConnectionFactory;

        public EmsConnectionFactory(ConnectionFactory nativeConnectionFactory)
        {
            this.nativeConnectionFactory = nativeConnectionFactory;
        }


        #region Implementation of ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            nativeConnectionFactory.GetObjectData(info, context);
        }

        #endregion

        #region Implementation of ICloneable

        public object Clone()
        {
            return nativeConnectionFactory.Clone();
        }

        #endregion

        #region Implementation of IConnectionFactory

        public ConnectionFactory NativeConnectionFactory
        {
            get { return this.nativeConnectionFactory; }
        }

        public IConnection CreateConnection()
        {
            Connection nativeConnection = nativeConnectionFactory.CreateConnection();
            return new EmsConnection(nativeConnection);
        }

        public IConnection CreateConnection(string userName, string password)
        {
            Connection nativeConnection = nativeConnectionFactory.CreateConnection(userName, password);
            return new EmsConnection(nativeConnection);
        }

        public object GetCertificateStore()
        {
            return nativeConnectionFactory.GetCertificateStore();
        }

        public string GetSSLProxyHost()
        {
            return nativeConnectionFactory.GetSSLProxyHost();
        }

        public string GetSSLProxyPassword()
        {
            return nativeConnectionFactory.GetSSLProxyPassword();
        }

        public int GetSSLProxyPort()
        {
            return nativeConnectionFactory.GetSSLProxyPort();
        }

        public string GetSSLProxyUser()
        {
            return nativeConnectionFactory.GetSSLProxyUser();
        }

        public void SetCertificateStoreType(EMSSSLStoreType type, object storeInfo)
        {
            nativeConnectionFactory.SetCertificateStoreType(type, storeInfo);
        }

        public void SetClientID(string clientID)
        {
            nativeConnectionFactory.SetClientID(clientID);
        }

        public void SetClientTracer(StreamWriter tracer)
        {
            nativeConnectionFactory.SetClientTracer(tracer);
        }

        public void SetConnAttemptCount(int attempts)
        {
            nativeConnectionFactory.SetConnAttemptCount(attempts);
        }

        public void SetConnAttemptDelay(int delay)
        {
            nativeConnectionFactory.SetConnAttemptDelay(delay);
        }

        public void SetConnAttemptTimeout(int timeout)
        {
            nativeConnectionFactory.SetConnAttemptTimeout(timeout);
        }

        public void SetHostNameVerifier(EMSSSLHostNameVerifier verifier)
        {
            nativeConnectionFactory.SetHostNameVerifier(verifier);
        }

        public void SetMetric(int metric)
        {
            nativeConnectionFactory.SetMetric(metric);
        }

        public void SetMulticastDaemon(string port)
        {
            nativeConnectionFactory.SetMulticastDaemon(port);
        }

        public void SetMulticastEnabled(bool enabled)
        {
            nativeConnectionFactory.SetMulticastEnabled(enabled);
        }

        public void SetReconnAttemptCount(int attempts)
        {
            nativeConnectionFactory.SetReconnAttemptCount(attempts);
        }

        public void SetReconnAttemptDelay(int delay)
        {
            nativeConnectionFactory.SetReconnAttemptDelay(delay);
        }

        public void SetReconnAttemptTimeout(int timeout)
        {
            nativeConnectionFactory.SetReconnAttemptTimeout(timeout);
        }

        public void SetServerUrl(string serverUrl)
        {
            nativeConnectionFactory.SetServerUrl(serverUrl);
        }

        public void SetSSLAuthOnly(bool authOnly)
        {
            nativeConnectionFactory.SetSSLAuthOnly(authOnly);
        }

        public void SetSSLProxy(string host, int port)
        {
            nativeConnectionFactory.SetSSLProxy(host, port);
        }

        public void SetSSLProxyAuth(string username, string password)
        {
            nativeConnectionFactory.SetSSLProxyAuth(username, password);
        }

        public void SetSSLTrace(bool trace)
        {
            nativeConnectionFactory.SetSSLTrace(trace);
        }

        public void SetTargetHostName(string targetHostName)
        {
            nativeConnectionFactory.SetTargetHostName(targetHostName);
        }

        public void SetUserName(string username)
        {
            nativeConnectionFactory.SetUserName(username);
        }

        public void SetUserPassword(string password)
        {
            nativeConnectionFactory.SetUserPassword(password);
        }

        public FactoryLoadBalanceMetric Metric
        {
            get
            {
                return nativeConnectionFactory.Metric;
            }
            set {
                nativeConnectionFactory.Metric = value;
            }
        }

        #endregion
    }
}