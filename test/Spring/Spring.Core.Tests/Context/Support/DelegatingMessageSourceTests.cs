#region License

/*
 * Copyright © 2002-2011 the original author or authors.
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
using NUnit.Framework;
using Rhino.Mocks;

#endregion

namespace Spring.Context.Support
{
	/// <summary>
	/// Unit tests for the DelegatingMessageSource class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class DelegatingMessageSourceTests
	{
	    private MockRepository mocks;

		private const string LookupKey = "rick";
		private IMessageSource _messageSource;

		private IMessageSource MockMessageSource
		{
			get
			{
			    return _messageSource;                
			}
		}

        /// <summary>
        /// The setup logic executed before the execution of each individual test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            _messageSource = mocks.StrictMock<IMessageSource>();
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
		    Expect.Call(MockMessageSource.GetMessage(LookupKey)).Return(expectedName);
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
            mocks.ReplayAll();
			string name = source.GetMessage(LookupKey);
			Assert.AreEqual(expectedName, name);
            mocks.VerifyAll();
		}

		[Test]
		public void GetMessageNoDelegateTarget()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
            Assert.Throws<NoSuchMessageException>(() => source.GetMessage(LookupKey));
		}

		[Test]
		public void GetMessageWithCulture()
		{
			const string expectedName = "Rick Evans";
		    Expect.Call(MockMessageSource.GetMessage(LookupKey, CultureInfo.InvariantCulture)).Return(expectedName);
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
            mocks.ReplayAll();		   
			string name = source.GetMessage(LookupKey, CultureInfo.InvariantCulture);
			Assert.AreEqual(expectedName, name);
            mocks.VerifyAll();
		}

		[Test]
		public void GetMessageWithCultureNoDelegateTarget()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
            Assert.Throws<NoSuchMessageException>(() => source.GetMessage(LookupKey, CultureInfo.InvariantCulture));
		}

		[Test]
		public void GetMessageWithParams()
		{
			const string expectedName = "Rick Evans";
		    Expect.Call(MockMessageSource.GetMessage(LookupKey, new string[] {"Rick", "Evans"})).Return(expectedName);
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
            mocks.ReplayAll();
			string name = source.GetMessage(LookupKey, "Rick", "Evans");
			Assert.AreEqual(expectedName, name);
            mocks.VerifyAll();
		}

		[Test]
		public void GetMessageWithParamsNoDelegateTarget()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
            Assert.Throws<NoSuchMessageException>(() => source.GetMessage(LookupKey, "Rick", "Evans"));
		}

		[Test]
		public void GetMessageWithCultureAndParams()
		{
			const string expectedName = "Rick Evans";
		    Expect.Call(MockMessageSource.GetMessage(LookupKey, CultureInfo.InvariantCulture, new string[] {"Rick", "Evans"}))
		        .Return(expectedName);
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
            mocks.ReplayAll();
			string name = source.GetMessage(LookupKey, CultureInfo.InvariantCulture, "Rick", "Evans");
			Assert.AreEqual(expectedName, name);
			mocks.VerifyAll();
		}

		[Test]
		public void GetMessageWithCultureAndParamsNoDelegateTarget()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
            Assert.Throws<NoSuchMessageException>(() => source.GetMessage(LookupKey, CultureInfo.InvariantCulture, "Rick", "Evans"));
		}

		[Test]
		public void GetMessageWithMessageSourceResolvableAndCulture()
		{
			const string expectedName = "Rick Evans";
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
		    Expect.Call(MockMessageSource.GetMessage((IMessageSourceResolvable)null, CultureInfo.InvariantCulture)).Return(expectedName);
            mocks.ReplayAll();
			string name = source.GetMessage(
				(IMessageSourceResolvable) null, CultureInfo.InvariantCulture);
			Assert.AreEqual(expectedName, name);
			mocks.VerifyAll();
		}

		[Test]
		public void GetMessageWithNoParentMessageSourceAndMessageSourceResolvableAndCulture()
		{
			const string expectedName = "Rick Evans";

		    IMessageSourceResolvable resolvable = mocks.StrictMock<IMessageSourceResolvable>();
		    Expect.Call(resolvable.DefaultMessage).Return(expectedName);
		    Expect.Call(resolvable.DefaultMessage).Return(expectedName);          

			DelegatingMessageSource source = new DelegatingMessageSource();
            mocks.ReplayAll();
			string name = source.GetMessage(resolvable, CultureInfo.InvariantCulture);
			Assert.AreEqual(expectedName, name);
			//mock.Verify();
            mocks.VerifyAll();
		}

		[Test]
		public void GetMessageWithNoParentMessageSourceAndNullDefaultMessageSourceResolvableWithNoCodesAndCulture()
		{
			IMessageSourceResolvable resolver = new DefaultMessageSourceResolvable(
				new string[] {}, new object[] {}, string.Empty);
			DelegatingMessageSource source = new DelegatingMessageSource();
            Assert.Throws<NoSuchMessageException>(() => source.GetMessage(resolver, CultureInfo.InvariantCulture));
		}

		[Test]
		public void GetMessageWithNoParentMessageSourceAndNullDefaultMessageSourceResolvableAndCulture()
		{
			IMessageSourceResolvable resolver = new DefaultMessageSourceResolvable(
				new string[] {"foo"}, new object[] {}, string.Empty);
			DelegatingMessageSource source = new DelegatingMessageSource();
            Assert.Throws<NoSuchMessageException>(() => source.GetMessage(resolver, CultureInfo.InvariantCulture));
		}

		[Test]
		public void GetResourceObject()
		{
			const string expectedName = "Rick Evans";
		    Expect.Call(MockMessageSource.GetResourceObject(LookupKey)).Return(expectedName);
		    DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
            mocks.ReplayAll();
			string name = (string) source.GetResourceObject(LookupKey);
			Assert.AreEqual(expectedName, name);
			mocks.VerifyAll();
		}

		[Test]
		public void GetResourceObjectWithNoParentMessageSource()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
            Assert.Throws<ApplicationContextException>(() => source.GetResourceObject(LookupKey));
		}

		[Test]
		public void GetResourceObjectWithCulture()
		{
			const string expectedName = "Rick Evans";
		    Expect.Call(MockMessageSource.GetResourceObject(LookupKey, CultureInfo.InvariantCulture)).Return(expectedName);
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
            mocks.ReplayAll();
			string name = (string) source.GetResourceObject(LookupKey, CultureInfo.InvariantCulture);
			Assert.AreEqual(expectedName, name);
			mocks.VerifyAll();
		}

		[Test]
		public void GetResourceObjectWithNoParentMessageSourceWithCulture()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
            Assert.Throws<ApplicationContextException>(() => source.GetResourceObject(LookupKey, CultureInfo.InvariantCulture));
		}

		[Test]
		public void ApplyResources()
		{
		    MockMessageSource.ApplyResources((object) 12, "rick", CultureInfo.InvariantCulture);
			DelegatingMessageSource source
				= new DelegatingMessageSource(MockMessageSource);
            mocks.ReplayAll();
			source.ApplyResources(12, "rick", CultureInfo.InvariantCulture);
			mocks.VerifyAll();
		}

		[Test]
		public void ApplyResourcesWithNoParentMessageSource()
		{
			DelegatingMessageSource source = new DelegatingMessageSource();
            Assert.Throws<ApplicationContextException>(() => source.ApplyResources(12, "rick", CultureInfo.InvariantCulture));
		}
    }
}
