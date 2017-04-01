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
    /// Unit tests for the ObjectReferenceFactoryObject class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class ObjectReferenceFactoryObjectTests
    {
        private IObjectFactory factory;

        [SetUp]
        public void SetUp()
        {
            factory = A.Fake<IObjectFactory>();
        }

        [Test]
        public void NullTargetObjectName()
        {
            ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
            // simulate IFactoryObjectAware interface...
            Assert.Throws<ArgumentException>(() => fac.ObjectFactory = null);
        }

        [Test]
        public void WhitespaceTargetObjectName()
        {
            ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
            fac.TargetObjectName = string.Empty;
            // simulate IFactoryObjectAware interface...
            Assert.Throws<ArgumentException>(() => fac.ObjectFactory = null);
        }

        [Test]
        public void FactoryDoesNotContainTargetObject()
        {
            A.CallTo(() => factory.ContainsObject("bojangles")).Returns(false);

            ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
            fac.TargetObjectName = "bojangles";

            // simulate IFactoryObjectAware interface...
            Assert.Throws<NoSuchObjectDefinitionException>(() => fac.ObjectFactory = factory,
                "Must have bailed with a " +
                "NoSuchObjectDefinitionException 'cos the object doesn't " +
                "exist in the associated factory.");
        }

        [Test]
        public void DelegatesThroughToFactoryFor_IsSingleton()
        {
            A.CallTo(() => factory.ContainsObject("bojangles")).Returns(true);
            A.CallTo(() => factory.IsSingleton("bojangles")).Returns(true);

            ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
            fac.TargetObjectName = "bojangles";
            fac.ObjectFactory = factory;

            Assert.IsTrue(fac.IsSingleton);
        }

        [Test]
        public void DelegatesThroughToFactoryFor_GetObject()
        {
            A.CallTo(() => factory.ContainsObject("bojangles")).Returns(true);
            A.CallTo(() => factory.GetObject("bojangles")).Returns("Rick");

            ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
            fac.TargetObjectName = "bojangles";
            fac.ObjectFactory = factory;

            Assert.AreEqual("Rick", fac.GetObject());
        }

        [Test]
        public void DelegatesThroughToFactoryFor_ObjectType()
        {
            A.CallTo(() => factory.ContainsObject("bojangles")).Returns(true);
            A.CallTo(() => factory.GetType("bojangles")).Returns(GetType());

            ObjectReferenceFactoryObject fac = new ObjectReferenceFactoryObject();
            fac.TargetObjectName = "bojangles";
            fac.ObjectFactory = factory;

            Assert.AreEqual(GetType(), fac.ObjectType);
        }
    }
}