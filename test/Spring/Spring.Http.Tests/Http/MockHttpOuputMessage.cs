#region License

/*
 * Copyright 2002-2011 the original author or authors.
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
using System.Text;

namespace Spring.Http
{
    /// <summary>
    /// Mocked IHttpOutputMessage implementation.
    /// </summary>
    /// <author>Arjen Poutsma</author>
    /// <author>Bruno Baia (.NET)</author>
    public class MockHttpOutputMessage : IHttpOutputMessage
    {
        private HttpHeaders headers;
        private Action<Stream> body;
        private byte[] bodyAsBytes;

        public MockHttpOutputMessage()
        {
            this.headers = new HttpHeaders();
        }

        public HttpHeaders Headers
        {
            get { return this.headers; }
        }

        public Action<Stream> Body
        {
            set { this.body = value; }
        }

        public byte[] GetBodyAsBytes()
        {
            if (bodyAsBytes == null)
            {
                using (MemoryStream requestStream = new MemoryStream())
                {
                    this.body(requestStream);
                    bodyAsBytes = requestStream.ToArray();
                }
            }
            return bodyAsBytes;
        }

        public String GetBodyAsString(Encoding charset)
        {
            byte[] bytes = GetBodyAsBytes();
            return charset.GetString(bytes);
        }
    }
}
