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

using Spring.Objects.Support;
using Spring.Objects.Factory.Config;
using NUnit.Framework;

namespace Spring.Objects.Factory.Support;

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
        Assert.That(def.HasObjectType, Is.False);
        Assert.Throws<ApplicationException>(() => Assert.That(def.ObjectType, Is.Null)); // must bail...
    }

    [Test]
    public void InstantiationFromOther()
    {
        RootObjectDefinition other = new RootObjectDefinition(typeof(TestObject), AutoWiringMode.Constructor);
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
        other.PropertyValues.Add("Age", 100);
        InstanceEventHandlerValue handler =
            new InstanceEventHandlerValue(new TestObject(), "Ping");
        handler.EventName = "Bing";
        other.EventHandlerValues.AddHandler(handler);
        other.ConstructorArgumentValues.AddGenericArgumentValue("Wulf", "Sternhammer");

        RootObjectDefinition def = new RootObjectDefinition(other);

        // ...
        Assert.That(def.ObjectType, Is.EqualTo(typeof(TestObject)));
        Assert.That(def.AutowireMode, Is.EqualTo(AutoWiringMode.Constructor));
        Assert.That(def.IsSingleton, Is.False);
        Assert.That(def.InitMethodName, Is.EqualTo("Umberto Eco"));
        Assert.That(def.DestroyMethodName, Is.EqualTo("Pedulismus"));
        Assert.That(def.DependencyCheck, Is.EqualTo(DependencyCheckingMode.Objects));
        Assert.That(def.IsAbstract, Is.True);
        Assert.That(def.IsLazyInit, Is.True);
        Assert.That(def.FactoryMethodName, Is.EqualTo("IfINeedYouIllJustUseYourSimpleName"));
        Assert.That(def.FactoryObjectName, Is.EqualTo("PerspirationShop"));
        Assert.That(def.ResourceDescription, Is.EqualTo("Ohhh"));
        Assert.That(def.HasConstructorArgumentValues, Is.True);
        Assert.That(def.ConstructorArgumentValues.ArgumentCount, Is.EqualTo(other.ConstructorArgumentValues.ArgumentCount));
        Assert.That(def.PropertyValues.PropertyValues.Count, Is.EqualTo(other.PropertyValues.PropertyValues.Count));
        Assert.That(def.EventHandlerValues.Events.Count, Is.EqualTo(other.EventHandlerValues.Events.Count));
    }

    [Test]
    public void OverrideFromOther()
    {
        RootObjectDefinition rod = new RootObjectDefinition(typeof(TestObject), AutoWiringMode.Constructor);
        rod.IsSingleton = false;
        rod.InitMethodName = "Umberto Eco";
        rod.DestroyMethodName = "Pedulismus";
        rod.ConstructorArgumentValues = new ConstructorArgumentValues();
        rod.ConstructorArgumentValues.AddGenericArgumentValue("Wulf", "Sternhammer");
        rod.DependencyCheck = DependencyCheckingMode.Objects;
        rod.DependsOn = new string[] { "Jimmy", "Joyce" };
        rod.IsAbstract = true;
        rod.IsLazyInit = true;
        rod.FactoryMethodName = "IfINeedYouIllJustUseYourSimpleName";
        rod.FactoryObjectName = "PerspirationShop";
        rod.ResourceDescription = "Ohhh";
        rod.PropertyValues = new MutablePropertyValues();
        rod.PropertyValues.Add("Age", 100);
        rod.EventHandlerValues = new EventValues();
        InstanceEventHandlerValue handler = new InstanceEventHandlerValue(new TestObject(), "Ping");
        handler.EventName = "Bing";
        rod.EventHandlerValues.AddHandler(handler);

        ChildObjectDefinition cod = new ChildObjectDefinition("parent");
        cod.ObjectTypeName = "ChildObjectTypeName";
        cod.IsSingleton = true;
        cod.AutowireMode = AutoWiringMode.ByType;
        cod.InitMethodName = "InitChild";
        cod.DestroyMethodName = "DestroyChild";
        cod.ConstructorArgumentValues = new ConstructorArgumentValues();
        cod.ConstructorArgumentValues.AddNamedArgumentValue("ctorarg", "ctorarg-value");
        cod.DependencyCheck = DependencyCheckingMode.None;
        cod.DependsOn = new string[] { "Aleks", "Mark" };
        cod.IsAbstract = false;
        cod.IsLazyInit = false;
        cod.FactoryMethodName = "ChildFactoryMethodName";
        cod.FactoryObjectName = "ChildFactoryObjectName";
        cod.ResourceDescription = "ChildResourceDescription";
        cod.PropertyValues = new MutablePropertyValues();
        cod.PropertyValues.Add("Prop1", "Val1");
        cod.PropertyValues.Add("Age", 50);
        cod.EventHandlerValues = new EventValues();
        handler = new InstanceEventHandlerValue(new TestObject(), "Pong");
        handler.EventName = "Bong";
        cod.EventHandlerValues.AddHandler(handler);

        rod.OverrideFrom(cod);

        // ...
        Assert.That(rod.ObjectTypeName, Is.EqualTo("ChildObjectTypeName"));
        Assert.That(rod.AutowireMode, Is.EqualTo(AutoWiringMode.ByType));
        Assert.That(rod.IsSingleton, Is.EqualTo(true));
        Assert.That(rod.InitMethodName, Is.EqualTo("InitChild"));
        Assert.That(rod.DestroyMethodName, Is.EqualTo("DestroyChild"));
        Assert.That(rod.DependencyCheck, Is.EqualTo(DependencyCheckingMode.None));
        Assert.That(rod.DependsOn.Count, Is.EqualTo(4));
        Assert.That(rod.IsAbstract, Is.EqualTo(false));
        Assert.That(rod.IsLazyInit, Is.EqualTo(false));
        Assert.That(rod.FactoryMethodName, Is.EqualTo("ChildFactoryMethodName"));
        Assert.That(rod.FactoryObjectName, Is.EqualTo("ChildFactoryObjectName"));
        Assert.That(rod.ResourceDescription, Is.EqualTo("ChildResourceDescription"));
        Assert.That(rod.ConstructorArgumentValues.ArgumentCount, Is.EqualTo(2));
        Assert.That(rod.PropertyValues.PropertyValues.Count, Is.EqualTo(2));
        Assert.That(rod.PropertyValues.GetPropertyValue("Prop1").Value, Is.EqualTo("Val1"));
        Assert.That(rod.PropertyValues.GetPropertyValue("Age").Value, Is.EqualTo(50));
        Assert.That(rod.EventHandlerValues.Events.Count, Is.EqualTo(2));
    }

    [Test]
    public void ValidateLazyAndPrototypeCausesBail()
    {
        RootObjectDefinition def
            = new RootObjectDefinition(typeof(TestObject), AutoWiringMode.No);
        def.IsLazyInit = true;
        def.IsSingleton = false;
        Assert.Throws<ObjectDefinitionValidationException>(() => def.Validate());
    }

    [Test]
    public void InstantiationWithNullCtorArgsAndNullPropsDoesNotResultInNullPropertyValues()
    {
        RootObjectDefinition def
            = new RootObjectDefinition(typeof(TestObject), null, null);
        Assert.That(def.ConstructorArgumentValues,
            Is.Not.Null,
            "Must never be null, but rather just empty.");
        Assert.That(def.ConstructorArgumentValues.ArgumentCount, Is.EqualTo(0),
            "Must be empty if null was passed to the ctor.");
        Assert.That(def.PropertyValues,
            Is.Not.Null,
            "Must never be null, but rather just empty.");
        Assert.That(def.PropertyValues.PropertyValues.Count, Is.EqualTo(0),
            "Must be empty if null was passed to the ctor.");
    }
}

internal class NoPublicCtors
{
    protected NoPublicCtors() { }
}
