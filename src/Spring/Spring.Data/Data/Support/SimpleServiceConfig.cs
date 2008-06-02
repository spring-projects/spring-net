#region License

/*
 * Copyright © 2002-2007 the original author or authors.
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

#if (!NET_1_0)

using System.EnterpriseServices;

namespace Spring.Data.Support
{
    /// <summary>
    /// A class that contains the properties of ServiceConfig used by ServiceDomainPlatformTransactionManager.
    /// </summary>
    /// <remarks>This is done to enhance testability of the transaction manager since ServiceConfig does not override Equals or Hashcode</remarks>
    /// <author>Mark Pollack (.NET)</author>
    public class SimpleServiceConfig
    {
        private string transactionDescription;
        private bool trackingEnabled = true;
        private string trackingAppName;
        private string trackingComponentName;
        private TransactionOption transactionOption;
        private TransactionIsolationLevel isolationLevel;
        private int transactionTimeout;


        public string TransactionDescription
        {
            get { return transactionDescription; }
            set { transactionDescription = value; }
        }

        public bool TrackingEnabled
        {
            get { return trackingEnabled; }
            set { trackingEnabled = value; }
        }

        public string TrackingAppName
        {
            get { return trackingAppName; }
            set { trackingAppName = value; }
        }

        public string TrackingComponentName
        {
            get { return trackingComponentName; }
            set { trackingComponentName = value; }
        }

        public TransactionOption TransactionOption
        {
            get { return transactionOption; }
            set { transactionOption = value; }
        }

        public TransactionIsolationLevel IsolationLevel
        {
            get { return isolationLevel; }
            set { isolationLevel = value; }
        }

        public int TransactionTimeout
        {
            get { return transactionTimeout; }
            set { transactionTimeout = value; }
        }

        public ServiceConfig CreateServiceConfig()
        {
            ServiceConfig serviceConfig = new ServiceConfig();
            serviceConfig.TransactionDescription = this.transactionDescription;
            serviceConfig.TrackingEnabled = this.trackingEnabled;
            serviceConfig.TrackingAppName = this.trackingAppName;
            serviceConfig.TrackingComponentName = this.trackingComponentName;
            serviceConfig.Transaction = this.transactionOption;
            serviceConfig.IsolationLevel = this.isolationLevel;
            serviceConfig.TransactionTimeout = this.transactionTimeout;
            return serviceConfig;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            SimpleServiceConfig simpleServiceConfig = obj as SimpleServiceConfig;
            if (simpleServiceConfig == null) return false;
            if (!Equals(transactionDescription, simpleServiceConfig.transactionDescription)) return false;
            if (!Equals(trackingEnabled, simpleServiceConfig.trackingEnabled)) return false;
            if (!Equals(trackingAppName, simpleServiceConfig.trackingAppName)) return false;
            if (!Equals(trackingComponentName, simpleServiceConfig.trackingComponentName)) return false;
            if (!Equals(transactionOption, simpleServiceConfig.transactionOption)) return false;
            if (!Equals(isolationLevel, simpleServiceConfig.isolationLevel)) return false;
            if (transactionTimeout != simpleServiceConfig.transactionTimeout) return false;
            return true;
        }

        public override int GetHashCode()
        {
            int result = transactionDescription != null ? transactionDescription.GetHashCode() : 0;
            result = 29*result + trackingEnabled.GetHashCode();
            result = 29*result + (trackingAppName != null ? trackingAppName.GetHashCode() : 0);
            result = 29*result + (trackingComponentName != null ? trackingComponentName.GetHashCode() : 0);
            result = 29*result + transactionOption.GetHashCode();
            result = 29*result + isolationLevel.GetHashCode();
            result = 29*result + transactionTimeout;
            return result;
        }
    }
}
#endif