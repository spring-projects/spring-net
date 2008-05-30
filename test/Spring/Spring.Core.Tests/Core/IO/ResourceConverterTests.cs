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
	/// <version>$Id: ResourceConverterTests.cs,v 1.7 2007/08/08 17:49:59 bbaia Exp $</version>
	[TestFixture]
	public sealed class ResourceConverterTests
	{
		/// <summary>
		/// The setup logic executed before the execution of this test fixture.
		/// </summary>
		[TestFixtureSetUp]
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
		[ExpectedException(typeof (NotSupportedException))]
		public void ConvertFromNullReference()
		{
			ResourceConverter vrt = new ResourceConverter();
			vrt.ConvertFrom(null);
		}

		[Test]
		[ExpectedException(typeof (NotSupportedException))]
		public void ConvertFromNonSupportedOptionBails()
		{
			ResourceConverter vrt = new ResourceConverter();
			vrt.ConvertFrom(new TestFixtureAttribute());
		}

		[Test]
		public void ConvertFromWithEnvironmentVariableExpansion()
		{
			// TODO : won't pass on anything other than Win9x boxes...
			ResourceConverter vrt = new ResourceConverter();
			string path = @"${userprofile}\foo.txt";
			IResource resource = (IResource) vrt.ConvertFrom(path);

			string userprofile = Environment.GetEnvironmentVariable("userprofile");

			Assert.IsFalse(resource.Exists,
			               "Darn. Should be supplying a rubbish non-existant resource. " +
			               	"You don't actually have a file called 'foo.txt' in your user directory do you?");
			Assert.IsTrue(resource.Description.IndexOf(userprofile) > -1,
			              "Environment variable not expanded.");
		}

		[Test]
		public void DoesNotChokeOnUnresolvableEnvironmentVariableExpansion()
		{
			ResourceConverter vrt = new ResourceConverter();
			string path = @"${_go_ahead_I_wish_you_would_}\foo.txt";
			IResource resource = (IResource) vrt.ConvertFrom(path);

			Assert.IsFalse(resource.Exists,
			               "Darn. Should be supplying a rubbish non-existant resource. " +
			               	"You don't actually have a file called 'foo.txt' in your user directory do you?");
			Assert.IsTrue(resource.Description.IndexOf("_go_ahead_I_wish_you_would_") > -1,
			              "Rubbish environment variable not preserved as-is.");
		}
	}
}