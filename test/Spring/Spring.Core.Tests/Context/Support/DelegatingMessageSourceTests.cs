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
using System.Globalization;
using DotNetMock.Dynamic;
using NUnit.Framework;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
	/// Unit tests for the DelegatingMessageSource class.
    /// </summary>
    /// <author>Rick Evans</author>
    /// <version>$Id: DelegatingMessageSourceTests.cs,v 1.3 2006/04/09 07:19:07 markpollack Exp $</version>
	[TestFixture]
    public sealed class DelegatingMessageSourceTests
    {
		private const string LookupKey = "rick";
		private DynamicMock _mock;
		private IMessageSource _messageSource;

		private DynamicMock TheMock
		{
			get { return _mock; }
		}

		private IMessageSource MockMessageSource
		{
			get { return _messageSource; }
		}

        /// <summary>
        /// The setup logic executed before the execution of each individual test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
			_mock = new DynamicMock(typeof(IMessageSource));
			_messageSource = (IMessageSource) _mock.Object;
        }

        [Test]
        public void Instantiation()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
			Assert.IsNotNull(source.ParentMessageSource,
				"ParentMessageSource property must *never* be null.");
		}

		[Test]
		public void InstantiationWithSuppliedParentMessageSource()
		{
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
			Assert.IsNotNull(source.ParentMessageSource,
				"ParentMessageSource property must *never* be null.");
			Assert.IsTrue(Object.ReferenceEquals(source.ParentMessageSource, MockMessageSource));
		}

		[Test]
		public void GetMessage()
		{
			const string expectedName = "Rick Evans";
			TheMock.ExpectAndReturn("GetMessage", expectedName, LookupKey);
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
			string name = source.GetMessage(LookupKey);
			Assert.AreEqual(expectedName, name);
			TheMock.Verify();
		}

		[Test]
		[ExpectedException(typeof(NoSuchMessageException))]
		public void GetMessageNoDelegateTarget()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
			source.GetMessage(LookupKey);
		}

		[Test]
		public void GetMessageWithCulture()
		{
			const string expectedName = "Rick Evans";
			TheMock.ExpectAndReturn("GetMessage", expectedName, LookupKey, CultureInfo.InvariantCulture);
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
			string name = source.GetMessage(LookupKey, CultureInfo.InvariantCulture);
			Assert.AreEqual(expectedName, name);
			TheMock.Verify();
		}

		[Test]
		[ExpectedException(typeof(NoSuchMessageException))]
		public void GetMessageWithCultureNoDelegateTarget()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
			source.GetMessage(LookupKey, CultureInfo.InvariantCulture);
		}

		[Test]
		public void GetMessageWithParams()
		{
			const string expectedName = "Rick Evans";
			TheMock.ExpectAndReturn("GetMessage", expectedName, LookupKey, new string[] { "Rick", "Evans" });
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
			string name = source.GetMessage(LookupKey, "Rick", "Evans");
			Assert.AreEqual(expectedName, name);
			TheMock.Verify();
		}

		[Test]
		[ExpectedException(typeof(NoSuchMessageException))]
		public void GetMessageWithParamsNoDelegateTarget()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
			source.GetMessage(LookupKey, "Rick", "Evans");
		}

		[Test]
		public void GetMessageWithCultureAndParams()
		{
			const string expectedName = "Rick Evans";
			TheMock.ExpectAndReturn("GetMessage", expectedName, LookupKey, CultureInfo.InvariantCulture, new string[] { "Rick", "Evans" });
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
			string name = source.GetMessage(LookupKey, CultureInfo.InvariantCulture, "Rick", "Evans");
			Assert.AreEqual(expectedName, name);
			TheMock.Verify();
		}

		[Test]
		[ExpectedException(typeof(NoSuchMessageException))]
		public void GetMessageWithCultureAndParamsNoDelegateTarget()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
			source.GetMessage(LookupKey, CultureInfo.InvariantCulture, "Rick", "Evans");
		}

		[Test]
		public void GetMessageWithMessageSourceResolvableAndCulture()
		{
			const string expectedName = "Rick Evans";
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
			TheMock.ExpectAndReturn("GetMessage", expectedName, null, CultureInfo.InvariantCulture);
			string name = source.GetMessage(
				(IMessageSourceResolvable) null, CultureInfo.InvariantCulture);
			Assert.AreEqual(expectedName, name);
			TheMock.Verify();
		}

		[Test]
		public void GetMessageWithNoParentMessageSourceAndMessageSourceResolvableAndCulture()
		{
			const string expectedName = "Rick Evans";
			DynamicMock mock = new DynamicMock(typeof(IMessageSourceResolvable));
			IMessageSourceResolvable resolvable = (IMessageSourceResolvable) mock.Object;
			mock.ExpectAndReturn("DefaultMessage", "Rick Evans");
			mock.ExpectAndReturn("DefaultMessage", "Rick Evans");
			DelegatingMessageSource source = new DelegatingMessageSource();
			string name = source.GetMessage(resolvable, CultureInfo.InvariantCulture);
			Assert.AreEqual(expectedName, name);
			mock.Verify();
		}

		[Test]
		[ExpectedException(typeof(NoSuchMessageException))]
		public void GetMessageWithNoParentMessageSourceAndNullDefaultMessageSourceResolvableWithNoCodesAndCulture()
		{
			IMessageSourceResolvable resolver = new DefaultMessageSourceResolvable(
				new string[] {}, new object[] {}, string.Empty);
			DelegatingMessageSource source = new DelegatingMessageSource();
			source.GetMessage(resolver, CultureInfo.InvariantCulture);
		}

		[Test]
		[ExpectedException(typeof(NoSuchMessageException))]
		public void GetMessageWithNoParentMessageSourceAndNullDefaultMessageSourceResolvableAndCulture()
		{
			IMessageSourceResolvable resolver = new DefaultMessageSourceResolvable(
				new string[] {"foo"}, new object[] {}, string.Empty);
			DelegatingMessageSource source = new DelegatingMessageSource();
			source.GetMessage(resolver, CultureInfo.InvariantCulture);
		}

		[Test]
		public void GetResourceObject()
		{
			const string expectedName = "Rick Evans";
			TheMock.ExpectAndReturn("GetResourceObject", expectedName, LookupKey);
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
			string name = (string) source.GetResourceObject(LookupKey);
			Assert.AreEqual(expectedName, name);
			TheMock.Verify();
		}

		[Test]
		[ExpectedException(typeof(ApplicationContextException))]
		public void GetResourceObjectWithNoParentMessageSource()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
			source.GetResourceObject(LookupKey);
		}

		[Test]
		public void GetResourceObjectWithCulture()
		{
			const string expectedName = "Rick Evans";
			TheMock.ExpectAndReturn("GetResourceObject", expectedName, LookupKey, CultureInfo.InvariantCulture);
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
			string name = (string) source.GetResourceObject(LookupKey, CultureInfo.InvariantCulture);
			Assert.AreEqual(expectedName, name);
			TheMock.Verify();
		}

		[Test]
		[ExpectedException(typeof(ApplicationContextException))]
		public void GetResourceObjectWithNoParentMessageSourceWithCulture()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
			source.GetResourceObject(LookupKey, CultureInfo.InvariantCulture);
		}

		[Test]
		public void ApplyResources()
		{
			const string expectedName = "Rick Evans";
			TheMock.ExpectAndReturn("ApplyResources", expectedName, 12, "rick", CultureInfo.InvariantCulture);
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
			source.ApplyResources(12, "rick", CultureInfo.InvariantCulture);
			TheMock.Verify();
		}

		[Test]
		[ExpectedException(typeof(ApplicationContextException))]
		public void ApplyResourcesWithNoParentMessageSource()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
			source.ApplyResources(12, "rick", CultureInfo.InvariantCulture);
		}
    }
}
