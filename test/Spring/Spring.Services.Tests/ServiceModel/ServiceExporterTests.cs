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
using System.IO;
using System.Text;
using System.Reflection;
using System.Net.Security;
using System.ServiceModel;

using NUnit.Framework;
using Spring.Core.IO;
using Spring.Objects.Factory;
using Spring.Objects.Factory.Xml;
using System.Collections.Generic;

#endregion

namespace Spring.ServiceModel
{
    /// <summary>
    /// Unit tests for the ServiceExporter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public sealed class ServiceExporterTests
    {
        ServiceExporter se = null;

        [SetUp]
        public void SetUp()
        {
            const string xml =
    @"<?xml version='1.0' encoding='UTF-8' ?>
        <objects xmlns='http://www.springframework.net'>
	        <object id='service' type='Spring.ServiceModel.ServiceExporterTests+Service, Spring.Services.Tests'/>
	        <object id='serviceWithMultipleInterfaces' type='Spring.ServiceModel.ServiceExporterTests+ServiceWithMultipleInterfaces, Spring.Services.Tests'/>
            <object id='decoratedService' type='Spring.ServiceModel.ServiceExporterTests+DecoratedService, Spring.Services.Tests'/>
            <object id='anotherService' type='Spring.ServiceModel.ServiceExporterTests+AnotherService, Spring.Services.Tests'/>
        </objects>";

            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                IObjectFactory objectFactory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
                se = new ServiceExporter();
                se.ObjectFactory = objectFactory;
            }
        }

        [Test]
        //SPRNET-1262
        public void ProxiesDeclarativeAttributeWithConstructorArguments()
        {
            XmlObjectFactory objectFactory = null;

            const string xml =
                @"<?xml version='1.0' encoding='UTF-8' ?>
                <objects xmlns='http://www.springframework.net'>
                <object id='theService' type='Spring.ServiceModel.ServiceExporterTests+Service, Spring.Services.Tests'/>	
                <object id='service' type='Spring.ServiceModel.ServiceExporter, Spring.Services'>
	                    <property name='TargetName' value='theService'/>
	                    <property name='TypeAttributes'>
                            <list>
                                <object type='System.ServiceModel.ServiceKnownTypeAttribute, System.ServiceModel' abstract='true'>
                                    <constructor-arg name='type' value='System.Collections.Generic.List&lt;int>' />
                                </object>
                            </list>
                        </property>
                    </object>
                </objects>";

            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                objectFactory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));
            }

            Type proxyType = (Type)objectFactory.GetObject("service");
            object[] attrs = proxyType.GetCustomAttributes(false);

            ServiceKnownTypeAttribute attrib = null;

            //loop thru all retreived attributes, attempting to find the one that is of the expected type
            for (int i = 0; i < attrs.Length; i++)
            {
                attrib = attrs[i] as ServiceKnownTypeAttribute;

                //break out of the loop once we find the one we want
                if (attrib != null)
                    break;
            }

            Assert.NotNull(attrib, String.Format("No attributes of the expecte type {0} were found.", typeof(ServiceKnownTypeAttribute)));
            Assert.AreEqual(typeof(List<int>), attrib.Type, "Property 'Type' on Target Attribute not set!");
        }

        [Test]
        public void NullConfig()
        {
            se.ObjectName = "NullConfig";
            Assert.Throws<ArgumentException>(() => se.AfterPropertiesSet(), "The TargetName property is required.");
        }

        [Test] // SPRNET-1416
        public void CallProxy()
        {
            se.ObjectName = "ProxiesContractInterface";
            se.TargetName = "service";
            se.AfterPropertiesSet();

            Type proxyType = se.GetObject() as Type;
            Assert.IsNotNull(proxyType);
            IContract proxy = Activator.CreateInstance(proxyType) as IContract;
            Assert.IsNotNull(proxyType);
            Assert.AreEqual("1979", proxy.SomeMethod(1979));
        }

        [Test]
        public void ProxiesContractInterface()
        {
            se.ObjectName = "ProxiesContractInterface";
            se.TargetName = "service";
            se.AfterPropertiesSet();

            Type proxyType = se.GetObject() as Type;
            Assert.IsNotNull(proxyType);
            Assert.IsTrue(typeof(IContract).IsAssignableFrom(proxyType));
        }

        [Test(Description = "http://jira.springframework.org/browse/SPRNET-1179")]
        public void ProxiesOnlyContractInterface()
        {
            se.ObjectName = "ProxiesOnlyContractInterface";
            se.TargetName = "serviceWithMultipleInterfaces";
            se.ContractInterface = typeof(IContract);
            se.AfterPropertiesSet();

            Type proxyType = se.GetObject() as Type;
            Assert.IsNotNull(proxyType);
            Assert.IsTrue(typeof(IContract).IsAssignableFrom(proxyType));
        }

        [Test(Description = "https://jira.springsource.org/browse/SPRNET-1464")]
        public void ProxiesInheritedContractInterface()
        {
            se.ObjectName = "ProxiesInheritedContractInterface";
            se.TargetName = "anotherService";
            se.ContractInterface = typeof(IInheritedContract);
            se.AfterPropertiesSet();

            Type proxyType = se.GetObject() as Type;
            Assert.IsNotNull(proxyType);
            Assert.IsTrue(typeof(IInheritedContract).IsAssignableFrom(proxyType));
        }

        [Test(Description = "http://jira.springframework.org/browse/SPRNET-1179")]
        public void ProxiesOnlyContractInterfaceFailsIfNoContractInterface()
        {
            se.ObjectName = "ProxiesOnlyContractInterface";
            se.TargetName = "serviceWithMultipleInterfaces";
            // se.ContractInterface = typeof (IContract);
            Assert.Throws<ArgumentException>(() => se.AfterPropertiesSet(), "ServiceExporter cannot export service type 'Spring.ServiceModel.ServiceExporterTests+ServiceWithMultipleInterfaces' as a WCF service because it implements multiple interfaces. Specify the contract interface to expose via the ContractInterface property.");
        }

        [Test]
        public void ProxyTypeEqualsObjectName()
        {
            se.ObjectName = "ProxyTypeEqualsObjectName";
            se.TargetName = "service";
            se.AfterPropertiesSet();

            Type proxyType = se.GetObject() as Type;
            Assert.IsNotNull(proxyType);
            Assert.AreEqual("ProxyTypeEqualsObjectName", proxyType.FullName);
        }

        [Test]
        public void CreatesServiceContractAttributeWithNoDecoratedClassAndMinimalConfig()
        {
            se.ObjectName = "CreatesServiceContractAttributeWithNoDecoratedClassAndMinimalConfig";
            se.TargetName = "service";
            se.AfterPropertiesSet();

            Type proxyType = se.GetObject() as Type;
            Assert.IsNotNull(proxyType);
            object[] attrs = proxyType.GetCustomAttributes(typeof(ServiceContractAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            ServiceContractAttribute sca = attrs[0] as ServiceContractAttribute;
            Assert.IsNull(sca.CallbackContract);
            Assert.AreEqual(typeof(IContract).FullName, sca.ConfigurationName);
            Assert.AreEqual(typeof(IContract).Name, sca.Name);
            Assert.IsNull(sca.Namespace);
            Assert.AreEqual(ProtectionLevel.None, sca.ProtectionLevel);
            Assert.AreEqual(SessionMode.Allowed, sca.SessionMode);
        }

        [Test]
        public void CreatesServiceContractAttributeWithNoDecoratedClassAndFullConfig()
        {
            se.ObjectName = "CreatesServiceContractAttributeWithNoDecoratedClassAndFullConfig";
            se.TargetName = "service";
            se.CallbackContract = typeof(IDisposable);
            se.ConfigurationName = "CustomConfigName";
            se.Name = "serviceName";
            se.Namespace = "http://Spring.Services.Tests";
            se.ProtectionLevel = ProtectionLevel.Sign;
            se.SessionMode = SessionMode.Required;

            se.AfterPropertiesSet();

            Type proxyType = se.GetObject() as Type;
            Assert.IsNotNull(proxyType);
            object[] attrs = proxyType.GetCustomAttributes(typeof(ServiceContractAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            ServiceContractAttribute sca = attrs[0] as ServiceContractAttribute;
            Assert.AreEqual(se.CallbackContract, sca.CallbackContract);
            Assert.AreEqual(se.ConfigurationName, sca.ConfigurationName);
            Assert.AreEqual(se.Name, sca.Name);
            Assert.AreEqual(se.Namespace, sca.Namespace);
            Assert.AreEqual(se.ProtectionLevel, sca.ProtectionLevel);
            Assert.AreEqual(se.SessionMode, sca.SessionMode);
        }

        [Test]
        public void CreatesDefaultOperationContractAttributeWithNoDecoratedMethod()
        {
            se.ObjectName = "CreatesDefaultOperationContractAttributeWithNoDecoratedMethod";
            se.TargetName = "service";
            se.AfterPropertiesSet();

            Type proxyType = se.GetObject() as Type;
            Assert.IsNotNull(proxyType);
            MethodInfo method = proxyType.GetMethod("SomeMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.GetCustomAttributes(typeof(OperationContractAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);
        }

        [Test]
        public void CreatesCustomOperationContractAttributeWithNoDecoratedMethod()
        {
            OperationContractAttribute oca1 = new OperationContractAttribute();
            oca1.Name = "MySomeMethod";

            se.ObjectName = "CreatesCustomOperationContractAttributeWithNoDecoratedMethod";
            se.TargetName = "service";
            se.MemberAttributes.Add("SomeMethod", oca1);
            se.AfterPropertiesSet();

            Type proxyType = se.GetObject() as Type;
            Assert.IsNotNull(proxyType);
            MethodInfo method = proxyType.GetMethod("SomeMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.GetCustomAttributes(typeof(OperationContractAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            OperationContractAttribute oca2 = attrs[0] as OperationContractAttribute;
            Assert.AreEqual(oca1.Name, oca2.Name);
        }

        [Test]
        public void OverridesExistingServiceContractAttributeWithDecoratedClass()
        {
            se.ObjectName = "OverridesExistingServiceContractAttributeWithDecoratedClass";
            se.TargetName = "decoratedService";
            se.AfterPropertiesSet();

            Type proxyType = se.GetObject() as Type;
            Assert.IsNotNull(proxyType);
            object[] attrs = proxyType.GetCustomAttributes(typeof(ServiceContractAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            ServiceContractAttribute sca = attrs[0] as ServiceContractAttribute;
            Assert.IsNull(sca.Namespace);
        }

        [Test]
        public void UsesExistingOperationContractAttributeWithDecoratedMethod()
        {
            se.ObjectName = "UsesExistingOperationContractAttributeWithDecoratedMethod";
            se.TargetName = "decoratedService";
            se.AfterPropertiesSet();

            Type proxyType = se.GetObject() as Type;
            Assert.IsNotNull(proxyType);
            MethodInfo method = proxyType.GetMethod("SomeMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.GetCustomAttributes(typeof(OperationContractAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            OperationContractAttribute oca = attrs[0] as OperationContractAttribute;
            Assert.AreEqual("MySomeMethod", oca.Name);
        }

        #region Test classes

        public interface IContract
        {
            string SomeMethod(int param);
        }

        public interface IOtherContract
        { }

        public class Service : IContract
        {
            public string SomeMethod(int param)
            {
                return param.ToString();
            }
        }

        public class ServiceWithMultipleInterfaces : Service, IOtherContract
        {
        }

        [ServiceContract(Namespace = "http://Spring.Services.Tests")]
        public class DecoratedService : IContract
        {
            [OperationContract(Name = "MySomeMethod")]
            public string SomeMethod(int param)
            {
                return param.ToString();
            }
        }

        public interface IInheritedContract : IContract
        {
            string AnotherMethod(int param);
        }

        public class AnotherService : IInheritedContract
        {
            public string SomeMethod(int param)
            {
                return param.ToString();
            }

            public string AnotherMethod(int param)
            {
                return param.ToString();
            }
        }

        //[ServiceContract(Namespace = "http://Spring.Services.Tests")]
        //public interface IDecoratedContract
        //{
        //    [OperationContract]
        //    string SomeMethod(int param);
        //}

        //public class AnotherService : IDecoratedContract
        //{
        //    public string SomeMethod(int param)
        //    {
        //        return param.ToString();
        //    }
        //}

        #endregion
    }
}
