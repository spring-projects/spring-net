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

#region Imports

using System;
using Common.Logging;
using Common.Logging.Simple;
using NUnit.Framework;

#endregion

namespace Spring.Core.IO
{
	/// <summary>
	/// Unit tests for the ResourceConverter class.
	/// </summary>
	/// <author>Rick Evans</author>
	[TestFixture]
	public sealed class ResourceConverterTests
	{
		/// <summary>
		/// The setup logic executed before the execution of this test fixture.
		/// </summary>
		[OneTimeSetUp]
		public void FixtureSetUp()
		{
			// enable (null appender) logging, just to ensure that the logging code is correct
            LogManager.Adapter = new NoOpLoggerFactoryAdapter();
		}

		[Test]
		public void CanConvertFrom()
		{
			ResourceConverter vrt = new ResourceConverter();
			Assert.IsTrue(vrt.CanConvertFrom(typeof (string)), "Conversion from a string instance must be supported.");
			Assert.IsFalse(vrt.CanConvertFrom(typeof (int)));
		}

		[Test]
		public void ConvertFrom()
		{
			ResourceConverter vrt = new ResourceConverter();
			object actual = vrt.ConvertFrom("file://localhost/");
			Assert.IsNotNull(actual);
			IResource res = actual as IResource;
			Assert.IsNotNull(res);
			Assert.IsFalse(res.Exists);
		}

		[Test]
		public void ConvertFromNullReference()
		{
			ResourceConverter vrt = new ResourceConverter();
            Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(null));
		}

		[Test]
		public void ConvertFromNonSupportedOptionBails()
		{
			ResourceConverter vrt = new ResourceConverter();
            Assert.Throws<NotSupportedException>(() => vrt.ConvertFrom(new TestFixtureAttribute()));
		}

		[Test]
		[Platform("Win")]
		public void ConvertFromWithEnvironmentVariableExpansion()
		{
            string filename = Guid.NewGuid().ToString();

			// TODO : won't pass on anything other than Win9x boxes...
			ResourceConverter vrt = new ResourceConverter();
			string path = string.Format(@"${{userprofile}}\{0}.txt", filename);
			IResource resource = (IResource) vrt.ConvertFrom(path);

			string userprofile = Environment.GetEnvironmentVariable("userprofile");

			Assert.IsFalse(resource.Exists,
			               string.Format("Darn. Should be supplying a rubbish non-existant resource. " +
			               	"You don't actually have a file called '{0}.txt' in your user directory do you?", filename));
			Assert.IsTrue(resource.Description.IndexOf(userprofile) > -1,
			              "Environment variable not expanded.");
		}

		[Test]
		public void DoesNotChokeOnUnresolvableEnvironmentVariableExpansion()
		{
            string foldername = Guid.NewGuid().ToString();
            string filename = Guid.NewGuid().ToString();

			ResourceConverter vrt = new ResourceConverter();
			string path = string.Format(@"${{{0}}}\{1}.txt", foldername, filename);
			IResource resource = (IResource) vrt.ConvertFrom(path);

			Assert.IsFalse(resource.Exists,
			               string.Format("Darn. Should be supplying a rubbish non-existant resource. " +
			               	"You don't actually have a file called '{0}.txt' in your user directory do you?", filename));
			Assert.IsTrue(resource.Description.IndexOf(foldername) > -1,
			              "Rubbish environment variable not preserved as-is.");


		}
	}
}