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
    public class EmsSSLFileStoreInfo : IEmsSSLStoreType
    {
        private EMSSSLFileStoreInfo nativeEmsSSLFileStoreInfo;

        public virtual EMSSSLFileStoreInfo NativeEmsSslFileStoreInfo
        {
            get { return nativeEmsSSLFileStoreInfo; }
            set { nativeEmsSSLFileStoreInfo = value; }
        }

        public X509Certificate SslClientIdentity
        {
            set { NativeEmsSslFileStoreInfo.SetSSLClientIdentity(value); }
        }

        public string SslClientIdentityAsString
        {
            set { NativeEmsSslFileStoreInfo.SetSSLClientIdentity(value); }
        }

        public string SslPassword
        {
            set { NativeEmsSslFileStoreInfo.SetSSLPassword(value.ToCharArray()); }
        }

        public X509Certificate SslTrustedCertificate
        {
            set { NativeEmsSslFileStoreInfo.SetSSLTrustedCertificate(value); }
        }

        public string SslTrustedCertificateAsString
        {
            set { NativeEmsSslFileStoreInfo.SetSSLTrustedCertificate(value); }
        }

    }
}
