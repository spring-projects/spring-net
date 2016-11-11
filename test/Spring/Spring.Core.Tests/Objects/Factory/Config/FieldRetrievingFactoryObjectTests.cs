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
using NUnit.Framework;

#endregion

namespace Spring.Objects.Factory.Config
{
    /// <summary>
    /// Unit tests for the FieldRetrievingFactoryObject class.
    /// </summary>
    /// <author>Juergen Hoeller</author>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class FieldRetrievingFactoryObjectTests
    {
        [Test]
        public void StaticField()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.StaticField = "Spring.Objects.Factory.Config.FinalFielder.Name, Spring.Core.Tests";
            fac.ObjectName = "foo";
            fac.AfterPropertiesSet();
            Assert.AreEqual(typeof (string), fac.ObjectType);
            object actual = fac.GetObject();
            Assert.AreEqual(FinalFielder.Name, actual);
        }

        [Test]
        public void StaticFieldWithWhitespace()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.StaticField = "\tSpring.Objects.Factory.Config.FinalFielder.Name, Spring.Core.Tests        ";
            fac.ObjectName = "foo";
            fac.AfterPropertiesSet();
            Assert.AreEqual(typeof (string), fac.ObjectType);
            object actual = fac.GetObject();
            Assert.AreEqual(FinalFielder.Name, actual);
        }

        [Test]
        public void StaticFieldThatAintAssemblyQualifiedShouldStillBeResolved()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.StaticField = "Spring.Objects.Factory.Config.FinalFielder.Name";
            fac.ObjectName = "foo";
            fac.AfterPropertiesSet();
            Assert.AreEqual(typeof (string), fac.ObjectType);
            object actual = fac.GetObject();
            Assert.AreEqual(FinalFielder.Name, actual);
        }

        [Test]
        public void StaticFieldSystemClass()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.StaticField = "System.DBNull.Value";
            fac.ObjectName = "foo";
            fac.AfterPropertiesSet();
            Assert.AreEqual(typeof (DBNull), fac.ObjectType);
            object actual = fac.GetObject();
            Assert.AreEqual(DBNull.Value, actual);
        }

        [Test]
        public void UsesObjectNameForStaticField()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.ObjectName = "System.Type.Delimiter";
            fac.AfterPropertiesSet();
            Assert.AreEqual(typeof (char), fac.ObjectType);
            object actual = fac.GetObject();
            Assert.AreEqual(Type.Delimiter, actual);
        }

        [Test]
        public void OnlyUsesObjectNameForStaticFieldIfTheStaticFieldHasNotBeenSet()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.ObjectName = "System.DBNull.Value";
            fac.StaticField = "Spring.Objects.Factory.Config.FinalFielder.Name";
            fac.AfterPropertiesSet();
            Assert.AreEqual(typeof (string), fac.ObjectType);
            object actual = fac.GetObject();
            Assert.AreEqual(FinalFielder.Name, actual);
        }

        [Test]
        public void StaticFieldViaClassAndFieldName()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.TargetField = "Name";
            fac.TargetType = typeof (FinalFielder);
            fac.ObjectName = "foo";
            fac.AfterPropertiesSet();
            object actual = fac.GetObject();
            Assert.AreEqual(FinalFielder.Name, actual);
        }

        [Test]
        public void IsSingleton()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            Assert.IsTrue(fac.IsSingleton);
        }

        [Test]
        public void InstanceField()
        {
            FinalFielder expected = new FinalFielder();
            expected.Age = 56;
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.TargetObject = expected;
            fac.TargetField = "Age";
            fac.ObjectName = "foo";
            fac.AfterPropertiesSet();
            object actual = fac.GetObject();
            Assert.AreEqual(expected.Age, actual);
        }

        [Test]
        public void NothingSet()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.ObjectName = "foo";
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet());
        }

        [Test]
        public void JustTargetField()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.TargetField = "Space";
            fac.ObjectName = "foo";
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet());
        }

        [Test]
        public void JustTargetType()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.TargetType = GetType();
            fac.ObjectName = "foo";
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet());
        }

        [Test]
        public void JustTargetObject()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.TargetObject = this;
            fac.ObjectName = "foo";
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet());
        }

        [Test]
        public void BailsWhenStaticFieldPassedGumpfh()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.StaticField = "Goob"; // no field specified
            fac.ObjectName = "foo";
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet());
        }

        [Test]
        public void StaticFieldIsCaseINsenSiTiVE()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.StaticField = "System.Type.emptytyPES, Mscorlib";
            fac.ObjectName = "foo";
            fac.AfterPropertiesSet();
            Assert.AreEqual(typeof (Type[]), fac.ObjectType);
            Type[] actual = fac.GetObject() as Type[];
            Assert.IsNotNull(actual);
            Assert.AreEqual(Type.EmptyTypes.Length, actual.Length);
        }

        [Test]
        public void BailsOnNonExistantField()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.StaticField = "Spring.Objects.Factory.Config.FinalFielder.Rubbish, Spring.Core.Tests";
            fac.ObjectName = "foo";
            Assert.Throws<ObjectDefinitionStoreException>(() => fac.AfterPropertiesSet());
        }

        [Test]
        public void BailsWhenBothTargetTypeAndTargetObjectPropsAreSet()
        {
            FieldRetrievingFactoryObject fac = new FieldRetrievingFactoryObject();
            fac.TargetObject = this;
            fac.TargetType = GetType();
            Assert.Throws<ArgumentException>(() => fac.AfterPropertiesSet());
        }
    }

    internal sealed class FinalFielder
    {
        public const string Name = "WalkingTall";

        public int Age = 43;
    }
}