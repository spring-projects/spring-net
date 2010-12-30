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

using System.Net;

using Spring.Util;

using NUnit.Framework;

namespace Spring.Http.Rest
{
    /// <summary>
    /// Unit tests for the HttpStatusCodeException class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class HttpStatusCodeExceptionTests
    {
        [Test]
        public void BinarySerialization()
        {
            HttpStatusCodeException exBefore = new HttpStatusCodeException(HttpStatusCode.Accepted, "Accepted description");

            HttpStatusCodeException exAfter = SerializationTestUtils.BinarySerializeAndDeserialize(exBefore) as HttpStatusCodeException;

            Assert.IsNotNull(exAfter);
            Assert.AreEqual(HttpStatusCode.Accepted, exAfter.StatusCode, "Invalid status code");
            Assert.AreEqual("Accepted description", exAfter.StatusDescription, "Invalid status description");
        }
    }
}
