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

using System;
using System.IO;
using System.Runtime.Serialization;
using TIBCO.EMS;

namespace Spring.Messaging.Ems.Common
{
    public interface IConnectionFactory : ISerializable, ICloneable
    {
        ConnectionFactory NativeConnectionFactory { get; }

        IConnection CreateConnection();
        IConnection CreateConnection(string userName, string password);
        object GetCertificateStore();
        string GetSSLProxyHost();
        string GetSSLProxyPassword();
        int GetSSLProxyPort();
        string GetSSLProxyUser();

        void SetCertificateStoreType(EMSSSLStoreType type, object storeInfo);
        void SetClientID(string clientID);
        void SetClientTracer(StreamWriter tracer);
        void SetConnAttemptCount(int attempts);
        void SetConnAttemptDelay(int delay);
        void SetConnAttemptTimeout(int timeout);
        void SetHostNameVerifier(EMSSSLHostNameVerifier verifier);
        void SetMetric(int metric);
        void SetMulticastDaemon(string port);
        void SetMulticastEnabled(bool enabled);
        void SetReconnAttemptCount(int attempts);
        void SetReconnAttemptDelay(int delay);
        void SetReconnAttemptTimeout(int timeout);
        void SetServerUrl(string serverUrl);
        void SetSSLAuthOnly(bool authOnly);
        void SetSSLProxy(string host, int port);
        void SetSSLProxyAuth(string username, string password);
        void SetSSLTrace(bool trace);
        void SetTargetHostName(string targetHostName);
        void SetUserName(string username);
        void SetUserPassword(string password);

        string ToString();
        FactoryLoadBalanceMetric Metric { get; set; }
    }
}