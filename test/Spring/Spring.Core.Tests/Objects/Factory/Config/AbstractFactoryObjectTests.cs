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

using System;

using FakeItEasy;

using NUnit.Framework;

namespace Spring.Objects.Factory.Config
{
	/// <summary>
	/// Unit tests for the basic functionality of the AbstractFactoryObject class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class AbstractFactoryObjectTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void DisposeCallbackIsNotInvokedOnDisposeIfInPrototypeMode()
        {
            IDisposable disposable = A.Fake<IDisposable>();
            DummyFactoryObject factory = new DummyFactoryObject(disposable);

            factory.IsSingleton = false;
            factory.GetObject();
            factory.Dispose();

			// in prototype mode, so the Dispose() method of the object must not be called...
            A.CallTo(() => disposable.Dispose()).MustNotHaveHappened();
        }

        [Test]
        public void DisposeCallbackIsInvokedOnDispose()
        {
            IDisposable disposable = A.Fake<IDisposable>();

            DummyFactoryObject factory = new DummyFactoryObject(disposable);
            factory.AfterPropertiesSet();
            factory.Dispose();

            A.CallTo(() => disposable.Dispose()).MustHaveHappenedOnceExactly();
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
