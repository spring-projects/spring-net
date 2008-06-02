#region License

/*
 * Copyright © 2002-2005 the original author or authors.
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
using DotNetMock.Dynamic;
using NUnit.Framework;

#endregion

namespace Spring.Core.TypeResolution
{
	/// <summary>
	/// Unit tests for the CachedTypeResolver class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class CachedTypeResolverTests
    {
        [Test]
        [ExpectedException(typeof(TypeLoadException))]
        public void ResolveWithNullTypeName() {

        	IDynamicMock mock = new DynamicMock(typeof(ITypeResolver));
			ITypeResolver mockResolver = (ITypeResolver) mock.Object;

            CachedTypeResolver resolver = new CachedTypeResolver(mockResolver);
            resolver.Resolve(null);
        }

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void InstantiateWithNullTypeResolver()
		{
			new CachedTypeResolver(null);
		}
    }
}
