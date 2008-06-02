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

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the basic functionality of the AbstractFactoryObject class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class AbstractFactoryObjectTests
    {
        [Test]
        public void DisposeCallbackIsNotInvokedOnDisposeIfInPrototypeMode()
        {
        	IDynamicMock mock = new DynamicMock(typeof(IDisposable));
            DummyFactoryObject factory = new DummyFactoryObject(mock.Object);
            factory.IsSingleton = false;
            factory.GetObject();
            factory.Dispose();
			// in prototype mode, so the Dispose() method of the object must not be called...
			mock.Verify();
        }

        [Test]
        public void DisposeCallbackIsInvokedOnDispose()
        {
			IDynamicMock mock = new DynamicMock(typeof(IDisposable));
			mock.Expect("Dispose");
			DummyFactoryObject factory = new DummyFactoryObject(mock.Object);
            factory.AfterPropertiesSet();
            factory.Dispose();
			mock.Verify();
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
