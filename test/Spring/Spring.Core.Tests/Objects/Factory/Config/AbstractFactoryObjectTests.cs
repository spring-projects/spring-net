#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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
using NUnit.Framework;
using Rhino.Mocks;

#endregion

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the basic functionality of the AbstractFactoryObject class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class AbstractFactoryObjectTests
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [Test]
        public void DisposeCallbackIsNotInvokedOnDisposeIfInPrototypeMode()
        {
            IDisposable disposable = (IDisposable) mocks.CreateMock(typeof (IDisposable));            
            DummyFactoryObject factory = new DummyFactoryObject(disposable);
            mocks.ReplayAll();
            factory.IsSingleton = false;
            factory.GetObject();
            factory.Dispose();
			// in prototype mode, so the Dispose() method of the object must not be called...
            mocks.VerifyAll();
        }

        [Test]
        public void DisposeCallbackIsInvokedOnDispose()
        {
            IDisposable disposable = (IDisposable)mocks.CreateMock(typeof(IDisposable));
            disposable.Dispose();
            LastCall.On(disposable).Repeat.Once();            
			DummyFactoryObject factory = new DummyFactoryObject(disposable);
            mocks.ReplayAll();
            factory.AfterPropertiesSet();
            factory.Dispose();
            mocks.VerifyAll();
        }

        private sealed class DummyFactoryObject : AbstractFactoryObject 
        {
            public object theObject;

            public DummyFactoryObject() : this (new object())
            {
            }

            public DummyFactoryObject(object theObject)
            {
                this.theObject = theObject;
            }

            public override Type ObjectType
            {
                get { return typeof(object); }
            }

            protected override object CreateInstance()
            {
                return theObject;
            }
        }
	}
}
