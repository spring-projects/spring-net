#region License

/*
 * Copyright 2002-2010 the original author or authors.
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

using System.ServiceModel;

using Spring.Context;
using Spring.Context.Support;
using Spring.Objects.Factory.Xml;
using Spring.Objects.Factory.Support;

using NUnit.Framework;

#endregion

namespace Spring.ServiceModel.Config
{
    /// <summary>
    /// Unit tests for the  <see cref="ChannelFactoryObjectDefinitionParser"/> class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class ChannelFactoryObjectDefinitionParserTests
    {
        //[Test]
        public void BasicConfig()
        {
            NamespaceParserRegistry.RegisterParser(typeof(WcfNamespaceParser));
            IApplicationContext ctx = new XmlApplicationContext(
                ReadOnlyXmlTestResource.GetFilePath("ChannelFactoryObjectDefinitionParserTests.BasicConfig.xml", this.GetType()));

            Assert.IsTrue(ctx.ContainsObjectDefinition("channel"));

            RootObjectDefinition rod = ((IObjectDefinitionRegistry)ctx).GetObjectDefinition("channel") as RootObjectDefinition;
            Assert.IsNotNull(rod);

            Assert.IsTrue(rod.HasObjectType);
            Assert.AreEqual(typeof(ChannelFactoryObject<IContract>), rod.ObjectType);
            Assert.AreEqual(1, rod.ConstructorArgumentValues.NamedArgumentValues.Count);
            Assert.AreEqual("ecn", rod.ConstructorArgumentValues.GetNamedArgumentValue("endpointConfigurationName").Value);

            ChannelFactoryObject<IContract> cfo = ctx.GetObject("&channel") as ChannelFactoryObject<IContract>;
            Assert.IsNotNull(cfo);
            Assert.AreEqual(typeof(IContract), cfo.ObjectType);

            IContract contract = ctx.GetObject("channel") as IContract;
            Assert.IsNotNull(contract);
        }

        //[Test]
        public void CustomProperties()
        {
            NamespaceParserRegistry.RegisterParser(typeof(WcfNamespaceParser));
            IApplicationContext ctx = new XmlApplicationContext(
                ReadOnlyXmlTestResource.GetFilePath("ChannelFactoryObjectDefinitionParserTests.CustomProperties.xml", this.GetType()));

            Assert.IsTrue(ctx.ContainsObjectDefinition("channel"));

            RootObjectDefinition rod = ((IObjectDefinitionRegistry)ctx).GetObjectDefinition("channel") as RootObjectDefinition;
            Assert.IsNotNull(rod);

            Assert.IsTrue(rod.HasObjectType);
            Assert.AreEqual(typeof(ChannelFactoryObject<IContract>), rod.ObjectType);
            Assert.AreEqual(1, rod.ConstructorArgumentValues.NamedArgumentValues.Count);
            Assert.AreEqual("ecn", rod.ConstructorArgumentValues.GetNamedArgumentValue("endpointConfigurationName").Value);
            Assert.IsTrue(rod.PropertyValues.Contains("Credentials.Windows.ClientCredential"));
            Assert.AreEqual("Spring\\Bruno:gnirpS", rod.PropertyValues.GetPropertyValue("Credentials.Windows.ClientCredential").Value);

            ChannelFactoryObject<IContract> cfo = ctx.GetObject("&channel") as ChannelFactoryObject<IContract>;
            Assert.IsNotNull(cfo);
            Assert.AreEqual(typeof(IContract), cfo.ObjectType);
            Assert.AreEqual("Spring", cfo.Credentials.Windows.ClientCredential.Domain);
            Assert.AreEqual("Bruno", cfo.Credentials.Windows.ClientCredential.UserName);
            Assert.AreEqual("gnirpS", cfo.Credentials.Windows.ClientCredential.Password);

            IContract contract = ctx.GetObject("channel") as IContract;
            Assert.IsNotNull(contract);
        }

        //[Test]
        public void WithoutId()
        {
            NamespaceParserRegistry.RegisterParser(typeof(WcfNamespaceParser));
            IApplicationContext ctx = new XmlApplicationContext(
                ReadOnlyXmlTestResource.GetFilePath("ChannelFactoryObjectDefinitionParserTests.WithoutId.xml", this.GetType()));

            var channels = ctx.GetObjects<IContract>();
            Assert.AreEqual(1, channels.Count);
        }

        //[Test]
        public void WithTypeAlias()
        {
            NamespaceParserRegistry.RegisterParser(typeof(WcfNamespaceParser));
            IApplicationContext ctx = new XmlApplicationContext(
                ReadOnlyXmlTestResource.GetFilePath("ChannelFactoryObjectDefinitionParserTests.WithTypeAlias.xml", this.GetType()));

            Assert.IsTrue(ctx.ContainsObjectDefinition("channel"));

            RootObjectDefinition rod = ((IObjectDefinitionRegistry)ctx).GetObjectDefinition("channel") as RootObjectDefinition;
            Assert.IsNotNull(rod);

            Assert.IsFalse(rod.HasObjectType);
            Assert.AreEqual("Spring.ServiceModel.ChannelFactoryObject<IContract>, Spring.Services", rod.ObjectTypeName);
            Assert.AreEqual(1, rod.ConstructorArgumentValues.NamedArgumentValues.Count);
            Assert.AreEqual("ecn", rod.ConstructorArgumentValues.GetNamedArgumentValue("endpointConfigurationName").Value);

            ChannelFactoryObject<IContract> cfo = ctx.GetObject("&channel") as ChannelFactoryObject<IContract>;
            Assert.IsNotNull(cfo);
            Assert.AreEqual(typeof(IContract), cfo.ObjectType);

            IContract contract = ctx.GetObject("channel") as IContract;
            Assert.IsNotNull(contract);
        }


        #region Test classes

        [ServiceContract(Namespace = "http://Spring.Services.Tests")]
        public interface IContract
        {
            [OperationContract(Name = "MySomeMethod")]
            string SomeMethod(int param);
        }

        #endregion
    }
}
