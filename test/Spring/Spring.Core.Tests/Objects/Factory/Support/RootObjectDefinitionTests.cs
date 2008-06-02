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

using Spring.Objects.Support;
using Spring.Objects.Factory.Config;

using NUnit.Framework;

#endregion

namespace Spring.Objects.Factory.Support
{
	/// <summary>
	/// Unit tests for the RootObjectDefinition class.
    /// </summary>
    /// <author>Rick Evans</author>
	[TestFixture]
    public sealed class RootObjectDefinitionTests
    {
        [Test]
        [ExpectedException (typeof (ApplicationException))]
        public void InstantiationWithClassName () 
        {
            RootObjectDefinition def
                = new RootObjectDefinition (typeof (TestObject).AssemblyQualifiedName, new ConstructorArgumentValues (), new MutablePropertyValues ());
            Assert.IsFalse (def.HasObjectType);
            Assert.IsNull (def.ObjectType); // must bail...
        }

        [Test]
        public void InstantiationFromOther () {
            RootObjectDefinition other
                = new RootObjectDefinition (typeof (TestObject), AutoWiringMode.Constructor);
            other.IsSingleton = false;
            other.InitMethodName = "Umberto Eco";
            other.DestroyMethodName = "Pedulismus";
            other.ConstructorArgumentValues = new ConstructorArgumentValues ();
            other.DependencyCheck = DependencyCheckingMode.Objects;
            other.DependsOn = new string [] {"Jimmy", "Joyce"};
            other.EventHandlerValues = new EventValues ();
            other.IsAbstract = true;
            other.IsLazyInit = true;
            other.FactoryMethodName = "IfINeedYouIllJustUseYourSimpleName";
            other.FactoryObjectName = "PerspirationShop";
            other.ResourceDescription = "Ohhh";
            other.PropertyValues = new MutablePropertyValues ();
            other.PropertyValues.Add ("Age", 100);
            InstanceEventHandlerValue handler =
                new InstanceEventHandlerValue (new TestObject (), "Ping");
            handler.EventName = "Bing";
            other.EventHandlerValues.AddHandler (handler);
            other.ConstructorArgumentValues.AddGenericArgumentValue ("Wulf", "Sternhammer");

            RootObjectDefinition def = new RootObjectDefinition (other);

            // ...
            Assert.AreEqual (typeof (TestObject), def.ObjectType);
            Assert.AreEqual (AutoWiringMode.Constructor, def.AutowireMode);
            Assert.IsFalse (def.IsSingleton);
            Assert.AreEqual ("Umberto Eco", def.InitMethodName);
            Assert.AreEqual ("Pedulismus", def.DestroyMethodName);
            Assert.AreEqual (DependencyCheckingMode.Objects, def.DependencyCheck);
            Assert.IsTrue (def.IsAbstract);
            Assert.IsTrue (def.IsLazyInit);
            Assert.AreEqual ("IfINeedYouIllJustUseYourSimpleName", def.FactoryMethodName);
            Assert.AreEqual ("PerspirationShop", def.FactoryObjectName);
            Assert.AreEqual ("Ohhh", def.ResourceDescription);
            Assert.IsTrue (def.HasConstructorArgumentValues);
            Assert.AreEqual (other.ConstructorArgumentValues.ArgumentCount, def.ConstructorArgumentValues.ArgumentCount);
            Assert.AreEqual (other.PropertyValues.PropertyValues.Length, def.PropertyValues.PropertyValues.Length);
            Assert.AreEqual (other.EventHandlerValues.Events.Count, def.EventHandlerValues.Events.Count);
        }

        [Test]
        [ExpectedException (typeof (ObjectDefinitionValidationException))]
        public void ValidateLazyAndPrototypeCausesBail () 
        {
            RootObjectDefinition def
                = new RootObjectDefinition (typeof (TestObject), AutoWiringMode.No);
            def.IsLazyInit = true;
            def.IsSingleton = false;
            def.Validate ();
        }

		[Test]
		public void InstantiationWithNullCtorArgsAndNullPropsDoesNotResultInNullPropertyValues()
		{
			RootObjectDefinition def
				= new RootObjectDefinition (typeof (TestObject), null, null);
			Assert.IsNotNull(def.ConstructorArgumentValues,
				"Must never be null, but rather just empty.");
			Assert.AreEqual(0, def.ConstructorArgumentValues.ArgumentCount,
				"Must be empty if null was passed to the ctor.");
			Assert.IsNotNull(def.PropertyValues,
				"Must never be null, but rather just empty.");
			Assert.AreEqual(0, def.PropertyValues.PropertyValues.Length,
				"Must be empty if null was passed to the ctor.");
		}
    }

    internal class NoPublicCtors 
    {
        protected NoPublicCtors () {}
    }
}
