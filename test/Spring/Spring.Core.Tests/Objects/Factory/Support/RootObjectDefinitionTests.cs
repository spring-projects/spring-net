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
        public void InstantiationWithClassName()
        {
            RootObjectDefinition def = new RootObjectDefinition(typeof(TestObject).AssemblyQualifiedName, new ConstructorArgumentValues(), new MutablePropertyValues());
            Assert.IsFalse(def.HasObjectType);
            Assert.Throws<ApplicationException>(() => Assert.IsNull(def.ObjectType)); // must bail...
        }

        [Test]
        public void InstantiationFromOther()
        {
            RootObjectDefinition other = new RootObjectDefinition( typeof( TestObject ), AutoWiringMode.Constructor );
            other.IsSingleton = false;
            other.InitMethodName = "Umberto Eco";
            other.DestroyMethodName = "Pedulismus";
            other.ConstructorArgumentValues = new ConstructorArgumentValues();
            other.DependencyCheck = DependencyCheckingMode.Objects;
            other.DependsOn = new string[] { "Jimmy", "Joyce" };
            other.EventHandlerValues = new EventValues();
            other.IsAbstract = true;
            other.IsLazyInit = true;
            other.FactoryMethodName = "IfINeedYouIllJustUseYourSimpleName";
            other.FactoryObjectName = "PerspirationShop";
            other.ResourceDescription = "Ohhh";
            other.PropertyValues = new MutablePropertyValues();
            other.PropertyValues.Add( "Age", 100 );
            InstanceEventHandlerValue handler =
                new InstanceEventHandlerValue( new TestObject(), "Ping" );
            handler.EventName = "Bing";
            other.EventHandlerValues.AddHandler( handler );
            other.ConstructorArgumentValues.AddGenericArgumentValue( "Wulf", "Sternhammer" );

            RootObjectDefinition def = new RootObjectDefinition( other );

            // ...
            Assert.AreEqual( typeof( TestObject ), def.ObjectType );
            Assert.AreEqual( AutoWiringMode.Constructor, def.AutowireMode );
            Assert.IsFalse( def.IsSingleton );
            Assert.AreEqual( "Umberto Eco", def.InitMethodName );
            Assert.AreEqual( "Pedulismus", def.DestroyMethodName );
            Assert.AreEqual( DependencyCheckingMode.Objects, def.DependencyCheck );
            Assert.IsTrue( def.IsAbstract );
            Assert.IsTrue( def.IsLazyInit );
            Assert.AreEqual( "IfINeedYouIllJustUseYourSimpleName", def.FactoryMethodName );
            Assert.AreEqual( "PerspirationShop", def.FactoryObjectName );
            Assert.AreEqual( "Ohhh", def.ResourceDescription );
            Assert.IsTrue( def.HasConstructorArgumentValues );
            Assert.AreEqual( other.ConstructorArgumentValues.ArgumentCount, def.ConstructorArgumentValues.ArgumentCount );
            Assert.AreEqual( other.PropertyValues.PropertyValues.Count, def.PropertyValues.PropertyValues.Count );
            Assert.AreEqual( other.EventHandlerValues.Events.Count, def.EventHandlerValues.Events.Count );
        }

        [Test]
        public void OverrideFromOther()
        {
            RootObjectDefinition rod = new RootObjectDefinition( typeof( TestObject ), AutoWiringMode.Constructor );
            rod.IsSingleton = false;
            rod.InitMethodName = "Umberto Eco";
            rod.DestroyMethodName = "Pedulismus";
            rod.ConstructorArgumentValues = new ConstructorArgumentValues();
            rod.ConstructorArgumentValues.AddGenericArgumentValue( "Wulf", "Sternhammer" );
            rod.DependencyCheck = DependencyCheckingMode.Objects;
            rod.DependsOn = new string[] { "Jimmy", "Joyce" };
            rod.IsAbstract = true;
            rod.IsLazyInit = true;
            rod.FactoryMethodName = "IfINeedYouIllJustUseYourSimpleName";
            rod.FactoryObjectName = "PerspirationShop";
            rod.ResourceDescription = "Ohhh";
            rod.PropertyValues = new MutablePropertyValues();
            rod.PropertyValues.Add( "Age", 100 );
            rod.EventHandlerValues = new EventValues();
            InstanceEventHandlerValue handler = new InstanceEventHandlerValue( new TestObject(), "Ping" );
            handler.EventName = "Bing";
            rod.EventHandlerValues.AddHandler( handler );

            ChildObjectDefinition cod = new ChildObjectDefinition( "parent" );
            cod.ObjectTypeName = "ChildObjectTypeName";
            cod.IsSingleton = true;
            cod.AutowireMode = AutoWiringMode.ByType;
            cod.InitMethodName = "InitChild";
            cod.DestroyMethodName = "DestroyChild";
            cod.ConstructorArgumentValues = new ConstructorArgumentValues();
            cod.ConstructorArgumentValues.AddNamedArgumentValue( "ctorarg", "ctorarg-value" );
            cod.DependencyCheck = DependencyCheckingMode.None;
            cod.DependsOn = new string[] { "Aleks", "Mark" };
            cod.IsAbstract = false;
            cod.IsLazyInit = false;
            cod.FactoryMethodName = "ChildFactoryMethodName";
            cod.FactoryObjectName = "ChildFactoryObjectName";
            cod.ResourceDescription = "ChildResourceDescription";
            cod.PropertyValues = new MutablePropertyValues();
            cod.PropertyValues.Add( "Prop1", "Val1" );
            cod.PropertyValues.Add( "Age", 50 );
            cod.EventHandlerValues = new EventValues();
            handler = new InstanceEventHandlerValue( new TestObject(), "Pong" );
            handler.EventName = "Bong";
            cod.EventHandlerValues.AddHandler( handler );

            rod.OverrideFrom( cod );

            // ...
            Assert.AreEqual( "ChildObjectTypeName", rod.ObjectTypeName );
            Assert.AreEqual( AutoWiringMode.ByType, rod.AutowireMode );
            Assert.AreEqual( true, rod.IsSingleton );
            Assert.AreEqual( "InitChild", rod.InitMethodName );
            Assert.AreEqual( "DestroyChild", rod.DestroyMethodName );
            Assert.AreEqual( DependencyCheckingMode.None, rod.DependencyCheck );
            Assert.AreEqual( 4, rod.DependsOn.Count);
            Assert.AreEqual( false, rod.IsAbstract );
            Assert.AreEqual( false, rod.IsLazyInit );
            Assert.AreEqual( "ChildFactoryMethodName", rod.FactoryMethodName );
            Assert.AreEqual( "ChildFactoryObjectName", rod.FactoryObjectName );
            Assert.AreEqual( "ChildResourceDescription", rod.ResourceDescription );
            Assert.AreEqual( 2, rod.ConstructorArgumentValues.ArgumentCount );
            Assert.AreEqual( 2, rod.PropertyValues.PropertyValues.Count );
            Assert.AreEqual( "Val1", rod.PropertyValues.GetPropertyValue("Prop1").Value);
            Assert.AreEqual( 50, rod.PropertyValues.GetPropertyValue("Age").Value);
            Assert.AreEqual( 2, rod.EventHandlerValues.Events.Count );
        }

        [Test]
        public void ValidateLazyAndPrototypeCausesBail()
        {
            RootObjectDefinition def
                = new RootObjectDefinition( typeof( TestObject ), AutoWiringMode.No );
            def.IsLazyInit = true;
            def.IsSingleton = false;
            Assert.Throws<ObjectDefinitionValidationException>(() => def.Validate());
        }

        [Test]
        public void InstantiationWithNullCtorArgsAndNullPropsDoesNotResultInNullPropertyValues()
        {
            RootObjectDefinition def
                = new RootObjectDefinition( typeof( TestObject ), null, null );
            Assert.IsNotNull( def.ConstructorArgumentValues,
                "Must never be null, but rather just empty." );
            Assert.AreEqual( 0, def.ConstructorArgumentValues.ArgumentCount,
                "Must be empty if null was passed to the ctor." );
            Assert.IsNotNull( def.PropertyValues,
                "Must never be null, but rather just empty." );
            Assert.AreEqual( 0, def.PropertyValues.PropertyValues.Count,
                "Must be empty if null was passed to the ctor." );
        }
    }

    internal class NoPublicCtors
    {
        protected NoPublicCtors() { }
    }
}
