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
        object CertificateStore { get; }
        string SSLProxyHost { get; }
        string SSLProxyPassword { get; }
        int SSLProxyPort { get; }
        string SSLProxyUser { get; }

        void SetCertificateStoreType(EMSSSLStoreType type, object storeInfo);

        string ClientID { set; }
        StreamWriter ClientTracer { set; }
        int ConnAttemptCount { set; }
        int ConnAttemptDelay { set; }
        int ConnAttemptTimeout { set; }
        EMSSSLHostNameVerifier HostNameVerifier { set; }

        int MetricAsInt { set; }


        string MulticastDaemon { set; }
        bool MulticastEnabled { set; }
        int ReconnAttemptCount { set; }
        int ReconnAttemptDelay { set; }
        int ReconnAttemptTimeout { set; }
        string ServerUrl { set; }
        bool SSLAuthOnly { set; }
        
        void SetSSLProxy(string host, int port);

        string[] SSLProxyAuth { set; }

        bool SSLTrace { set; }
        string TargetHostName { set; }
        string UserName { set; }
        string UserPassword { set; }

        string ToString();
        FactoryLoadBalanceMetric Metric { get; set; }
    }
}