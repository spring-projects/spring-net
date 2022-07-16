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

using System.Security.Cryptography.X509Certificates;

namespace Spring.Messaging.Ems.Common
{
    public class EmsSSLSystemStoreInfo : IEmsSSLStoreType
    {

        private EMSSSLSystemStoreInfo emssslSystemStoreInfo;

        public EMSSSLSystemStoreInfo NativeEmssslSystemStoreInfo
        {
            get { return emssslSystemStoreInfo; }
            set { emssslSystemStoreInfo = value; }
        }

        public virtual EMSSSLSystemStoreInfo EmssslSystemStoreInfo
        {
            get { return emssslSystemStoreInfo; }
            set { emssslSystemStoreInfo = value; }
        }

        public string CertificateNameAsFullSubjectDn
        {
            set { EmssslSystemStoreInfo.SetCertificateNameAsFullSubjectDN(value); }
        }

        public StoreLocation CertificateStoreLocation {

            set { EmssslSystemStoreInfo.SetCertificateStoreLocation(value); }
        }

        public string CertificateStoreName
        {
            set { EmssslSystemStoreInfo.SetCertificateStoreName(value); }
        }
    }
}
