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

using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace Spring.Objects.Factory
{
	/// <summary>
	/// Unit tests for the NoSuchObjectDefinitionException class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class NoSuchObjectDefinitionExceptionTests
	{
		public const string NotFoundObjectDefinitionName = "myObject";

		public static readonly Type NotFoundObjectDefinitionType = typeof (TestObject);

		[Test]
		public void SerializesObjectNameFieldCorrectly()
		{
			NoSuchObjectDefinitionException ex
				= new NoSuchObjectDefinitionException(NotFoundObjectDefinitionName, "Cannot dynamically build object key...");
			NoSuchObjectDefinitionException deserializedException = Serialize(ex);
			Assert.IsNotNull(deserializedException);
			Assert.AreEqual(NotFoundObjectDefinitionName, deserializedException.ObjectName, "'ObjectName' property was not serialized correctly.");
		}

		[Test]
		public void SerializesObjectTypeFieldCorrectly()
		{
			NoSuchObjectDefinitionException ex
				= new NoSuchObjectDefinitionException(NotFoundObjectDefinitionType, null);
			NoSuchObjectDefinitionException deserializedException = Serialize(ex);
			Assert.IsNotNull(deserializedException);
			Assert.AreEqual(NotFoundObjectDefinitionType, deserializedException.ObjectType, "'ObjectType' property was not serialized correctly.");
		}

		private NoSuchObjectDefinitionException Serialize(NoSuchObjectDefinitionException inputException)
		{
			NoSuchObjectDefinitionException deserializedException = null;
			string tempDir = Environment.GetEnvironmentVariable("TEMP");
			string tempFilename = tempDir + @"\foo.dat";
			FileInfo file = new FileInfo(tempFilename);
			try
			{
				Stream outstream = file.OpenWrite();
				new BinaryFormatter().Serialize(outstream, inputException);
				outstream.Flush();
				outstream.Close();
				Stream instream = file.OpenRead();
				deserializedException = new BinaryFormatter().Deserialize(instream) as NoSuchObjectDefinitionException;
				instream.Close();
			}
			finally
			{
				try
				{
					file.Delete();
				}
				catch
				{
				}
			}
			return deserializedException;
		}
	}
}