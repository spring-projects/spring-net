#region License

/*
 * Copyright 2004 the original author or authors.
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
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace Spring.Validation
{
	/// <summary>
	/// Unit tests for the ValidationException class.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	[TestFixture]
	public sealed class ValidationExceptionTests
	{
        [Test]
        public void InstantiationUsingDefaultConstructor()
        {
            ValidationException ex = new ValidationException();
            Assert.IsNull(ex.ValidationErrors);
        }
        
        [Test]
		public void InstantiationSupplyingValidationErrors()
		{
            ValidationException ex = new ValidationException(new ValidationErrors());
            Assert.IsTrue(ex.ValidationErrors.IsEmpty);
		}

        [Test]
        public void InstantiationSupplyingMessageAndValidationErrors()
        {
            ValidationException ex = new ValidationException("my message", new ValidationErrors());
            Assert.AreEqual("my message", ex.Message);
            Assert.IsTrue(ex.ValidationErrors.IsEmpty);
        }
    
        [Test]
        public void InstantiationSupplyingMessageValidationErrorsAndRootCause()
        {
            Exception rootCause = new Exception("root cause");
            ValidationException ex = new ValidationException("my message", rootCause, new ValidationErrors());
            Assert.AreEqual("my message", ex.Message);
            Assert.IsTrue(ex.ValidationErrors.IsEmpty);
            Assert.AreEqual(rootCause, ex.InnerException);
            Assert.AreEqual("root cause", ex.InnerException.Message);
        }

        [Test]
        public void TestExceptionSerialization()
        {
            MemoryStream buffer = new MemoryStream();
            BinaryFormatter serializer = new BinaryFormatter();

            Exception rootCause = new Exception("root cause");
            ValidationException e1 = new ValidationException("my message", rootCause, new ValidationErrors());
            serializer.Serialize(buffer, e1);
            buffer.Position = 0;
            ValidationException e2 = (ValidationException)serializer.Deserialize(buffer);

            Assert.AreEqual("my message", e2.Message);
            Assert.IsTrue(e2.ValidationErrors.IsEmpty);
            Assert.AreEqual("root cause", e2.InnerException.Message);
        }
	}
}