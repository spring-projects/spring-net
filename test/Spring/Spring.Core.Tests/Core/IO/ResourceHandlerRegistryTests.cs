#region License

/*
 * Copyright 2002-2010 the original author or authors.
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
using System.IO;
using NUnit.Framework;

#endregion

namespace Spring.Core.IO
{
	/// <summary>
	/// Unit tests for the ConfigurableResourceLoader class.
	/// </summary>
	/// <author>Aleksandar Seovic</author>
	[TestFixture]
	public sealed class ResourceHandlerRegistryTests
	{
		[Test]
		public void WithNullProtocolName()
		{
            Assert.Throws<ArgumentNullException>(() => ResourceHandlerRegistry.RegisterResourceHandler(null, GetType()));
		}

		[Test]
		public void WithNullIResourceHandlerType()
		{
            Assert.Throws<ArgumentNullException>(() => ResourceHandlerRegistry.RegisterResourceHandler("beep", (Type) null));
		}

		[Test]
		public void WithWhitespacedProtocolName()
		{
            Assert.Throws<ArgumentNullException>(() => ResourceHandlerRegistry.RegisterResourceHandler("\t   ", GetType()));
		}

		[Test]
		public void WithNonIResourceHandlerType()
		{
            Assert.Throws<ArgumentException>(() => ResourceHandlerRegistry.RegisterResourceHandler("beep", GetType()));
		}

		[Test]
		public void WithIResourceHandlerTypeWithNoValidCtor()
		{
            Assert.Throws<ArgumentException>(() => ResourceHandlerRegistry.RegisterResourceHandler("beep", typeof(IncompatibleResource)));
		}

        [Test]
        public void AddProtocolMappingSilentlyOverwritesExistingProtocol()
        {
            ResourceHandlerRegistry.RegisterResourceHandler("beep", typeof(FileSystemResource));
            // overwrite, must not complain...
            ResourceHandlerRegistry.RegisterResourceHandler("beep", typeof(AssemblyResource));
            IResource res = new ConfigurableResourceLoader().GetResource("beep://Spring.Core.Tests/Spring/TestResource.txt");
            Assert.IsNotNull(res, "Resource must not be null");
            Assert.AreEqual(typeof(AssemblyResource), res.GetType(),
                "The original IResource Type associated with the 'beep' protocol " +
                "must have been overwritten; expecting an AssemblyResource 'cos " +
                "we registered it last under the 'beep' protocol.");
        }

		/// <summary>
		/// Deso <b>not</b> expose a constructor that takes a single string argument.
		/// </summary>
		private sealed class IncompatibleResource : IResource 
		{
			public bool IsOpen
			{
				get { throw new NotImplementedException(); }
			}

			public Uri Uri
			{
				get { throw new NotImplementedException(); }
			}

			public FileInfo File
			{
				get { throw new NotImplementedException(); }
			}

			public string Description
			{
				get { throw new NotImplementedException(); }
			}

			public bool Exists
			{
				get { throw new NotImplementedException(); }
			}

			public IResource CreateRelative(string relativePath)
			{
				throw new NotImplementedException();
			}

			public Stream InputStream
			{
				get { throw new NotImplementedException(); }
			}
		}
	}
}