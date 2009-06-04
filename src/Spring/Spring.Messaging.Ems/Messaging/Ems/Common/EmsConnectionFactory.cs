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

        public object CertificateStore
        {
            get { return nativeConnectionFactory.GetCertificateStore(); }
        }

        public string SSLProxyHost
        {
            get { return nativeConnectionFactory.GetSSLProxyHost(); }
        }

        public string SSLProxyPassword
        {
            get { return nativeConnectionFactory.GetSSLProxyPassword(); }
        }

        public int SSLProxyPort
        {
            get { return nativeConnectionFactory.GetSSLProxyPort(); }
        }

        public string SSLProxyUser
        {
            get { return nativeConnectionFactory.GetSSLProxyUser(); }
        }

        public void SetCertificateStoreType(EMSSSLStoreType type, object storeInfo)
        {
            nativeConnectionFactory.SetCertificateStoreType(type, storeInfo);
        }

        public string ClientID
        {
            set { nativeConnectionFactory.SetClientID(value); }
        }

        public StreamWriter ClientTracer
        {
            set { nativeConnectionFactory.SetClientTracer(value); }
        }

        public int ConnAttemptCount
        {
            set { nativeConnectionFactory.SetConnAttemptCount(value); }
        }

        public int ConnAttemptDelay
        {
            set { nativeConnectionFactory.SetConnAttemptDelay(value); }
        }

        public int ConnAttemptTimeout
        {
            set { nativeConnectionFactory.SetConnAttemptTimeout(value); }
        }

        public EMSSSLHostNameVerifier HostNameVerifier
        {
            set { nativeConnectionFactory.SetHostNameVerifier(value); }
        }

        public int MetricAsInt
        {
            set { nativeConnectionFactory.SetMetric(value); }
        }

        public string MulticastDaemon
        {
            set { nativeConnectionFactory.SetMulticastDaemon(value); }
        }

        public bool MulticastEnabled
        {
            set { nativeConnectionFactory.SetMulticastEnabled(value); }
        }

        public int ReconnAttemptCount
        {
            set { nativeConnectionFactory.SetReconnAttemptCount(value); }
        }

        public int ReconnAttemptDelay
        {
            set { nativeConnectionFactory.SetReconnAttemptDelay(value); }
        }

        public int ReconnAttemptTimeout
        {
            set { nativeConnectionFactory.SetReconnAttemptTimeout(value); }
        }

        public string ServerUrl
        {
            set { nativeConnectionFactory.SetServerUrl(value); }
        }

        public bool SSLAuthOnly
        {
            set { nativeConnectionFactory.SetSSLAuthOnly(value); }
        }

        public void SetSSLProxy(string host, int port)
        {
            nativeConnectionFactory.SetSSLProxy(host, port);
        }

        public string[] SSLProxyAuth
        {
            set { nativeConnectionFactory.SetSSLProxyAuth(value[0], value[1]); }
        }

        public bool SSLTrace
        {
            set { nativeConnectionFactory.SetSSLTrace(value); }
        }

        public string TargetHostName
        {
            set { nativeConnectionFactory.SetTargetHostName(value); }
        }

        public string UserName
        {
            set { nativeConnectionFactory.SetUserName(value); }
        }

        public string UserPassword
        {
            set { nativeConnectionFactory.SetUserPassword(value); } 
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