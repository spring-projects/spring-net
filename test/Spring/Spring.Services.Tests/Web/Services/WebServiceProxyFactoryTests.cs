#region License

/*
 * Copyright 2007 the original author or authors.
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
using System.Reflection;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Services.Description;
using System.ServiceModel;
using System.ServiceModel.Description;
using NUnit.Framework;
using Spring.Core.IO;
using Spring.Context;
using Spring.Context.Support;
using Spring.Util;

#endregion

namespace Spring.Web.Services
{
    /// <summary>
    /// Unit tests for the WebServiceProxyFactory class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class WebServiceProxyFactoryTests
    {
        [Test]
        public void BailsWhenNotConfigured()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            Assert.Throws<ArgumentException>(() => wspf.AfterPropertiesSet());
        }

        [Test]
        public void DocumentLiteralWithDefaultConfig()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ServiceUri = new AssemblyResource("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/document-literal.wsdl");
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            object[] typeAttrs = proxyType.GetCustomAttributes(typeof(WebServiceBindingAttribute), false);
            Assert.IsTrue(typeAttrs.Length > 0);

            WebServiceBindingAttribute wsba = (WebServiceBindingAttribute)typeAttrs[0];
            Assert.AreEqual("HelloWorldServiceSoap", wsba.Name);
            Assert.AreEqual("http://www.springframwework.net", wsba.Namespace);

            MethodInfo sayHelloWorldMethod = proxyType.GetMethod("SayHelloWorld");
            Assert.IsNotNull(sayHelloWorldMethod);

            object[] sdmaAttrs = sayHelloWorldMethod.GetCustomAttributes(typeof(SoapDocumentMethodAttribute), false);
            Assert.IsTrue(sdmaAttrs.Length > 0);

            SoapDocumentMethodAttribute sdma = (SoapDocumentMethodAttribute)sdmaAttrs[0];
            Assert.AreEqual("http://www.springframwework.net/SayHelloWorld", sdma.Action);
            Assert.AreEqual(SoapParameterStyle.Wrapped, sdma.ParameterStyle);
            Assert.AreEqual(string.Empty, sdma.Binding);
            Assert.AreEqual(false, sdma.OneWay);
            Assert.AreEqual(string.Empty, sdma.RequestElementName);
            Assert.AreEqual("http://www.springframwework.net", sdma.RequestNamespace);
            Assert.AreEqual(string.Empty, sdma.ResponseElementName);
            Assert.AreEqual("http://www.springframwework.net", sdma.ResponseNamespace);
            Assert.AreEqual(SoapBindingUse.Literal, sdma.Use);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void DocumentLiteralWithOneWay()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ServiceUri = new AssemblyResource("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/document-literal.wsdl");
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            MethodInfo logHelloWorldMethod = proxyType.GetMethod("LogHelloWorld");
            Assert.IsNotNull(logHelloWorldMethod);

            object[] methodAttrs = logHelloWorldMethod.GetCustomAttributes(typeof(SoapDocumentMethodAttribute), false);
            Assert.IsTrue(methodAttrs.Length > 0);

            SoapDocumentMethodAttribute sdma = (SoapDocumentMethodAttribute)methodAttrs[0];
            Assert.AreEqual("http://www.springframwework.net/LogHelloWorld", sdma.Action);
            Assert.AreEqual(SoapParameterStyle.Wrapped, sdma.ParameterStyle);
            Assert.AreEqual(string.Empty, sdma.Binding);
            Assert.AreEqual(true, sdma.OneWay);
            Assert.AreEqual(string.Empty, sdma.RequestElementName);
            Assert.AreEqual("http://www.springframwework.net", sdma.RequestNamespace);
            Assert.AreEqual(string.Empty, sdma.ResponseElementName);
            Assert.AreEqual(null, sdma.ResponseNamespace);
            Assert.AreEqual(SoapBindingUse.Literal, sdma.Use);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void DocumentLiteralWithMessageName()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ServiceUri = new AssemblyResource("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/document-literal.wsdl");
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            MethodInfo logHelloMethod = proxyType.GetMethod("LogHello");
            Assert.IsNotNull(logHelloMethod);

            object[] methodAttrs = logHelloMethod.GetCustomAttributes(typeof(SoapDocumentMethodAttribute), false);
            Assert.IsTrue(methodAttrs.Length > 0);

            SoapDocumentMethodAttribute sdma = (SoapDocumentMethodAttribute)methodAttrs[0];
            Assert.AreEqual("http://www.springframwework.net/MyLogHello", sdma.Action);
            Assert.AreEqual(SoapParameterStyle.Wrapped, sdma.ParameterStyle);
            Assert.AreEqual(string.Empty, sdma.Binding);
            Assert.AreEqual(false, sdma.OneWay);
            Assert.AreEqual("MyLogHello", sdma.RequestElementName);
            Assert.AreEqual("http://www.springframwework.net", sdma.RequestNamespace);
            Assert.AreEqual("MyLogHelloResponse", sdma.ResponseElementName);
            Assert.AreEqual("http://www.springframwework.net", sdma.ResponseNamespace);
            Assert.AreEqual(SoapBindingUse.Literal, sdma.Use);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void DocumentLiteralWithNamedOutParameter()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ServiceUri = new AssemblyResource("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/document-literal.wsdl");
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            MethodInfo sayHelloMethod = proxyType.GetMethod("SayHello");
            Assert.IsNotNull(sayHelloMethod);

            object[] sdmaAttrs = sayHelloMethod.GetCustomAttributes(typeof(SoapDocumentMethodAttribute), false);
            Assert.IsTrue(sdmaAttrs.Length > 0);

            SoapDocumentMethodAttribute sdma = (SoapDocumentMethodAttribute)sdmaAttrs[0];
            Assert.AreEqual("http://www.springframwework.net/SayHello", sdma.Action);
            Assert.AreEqual(SoapParameterStyle.Wrapped, sdma.ParameterStyle);
            Assert.AreEqual(string.Empty, sdma.Binding);
            Assert.AreEqual(false, sdma.OneWay);
            Assert.AreEqual(string.Empty, sdma.RequestElementName);
            Assert.AreEqual("http://www.springframwework.net", sdma.RequestNamespace);
            Assert.AreEqual(string.Empty, sdma.ResponseElementName);
            Assert.AreEqual("http://www.springframwework.net", sdma.ResponseNamespace);
            Assert.AreEqual(SoapBindingUse.Literal, sdma.Use);

            object[] xeAttrs = sayHelloMethod.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(XmlElementAttribute), false);
            Assert.IsTrue(xeAttrs.Length > 0);

            XmlElementAttribute xea = (XmlElementAttribute)xeAttrs[0];
            Assert.AreEqual("out", xea.ElementName);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void DocumentLiteralWithNamedOutArrayParameter()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ServiceUri = new AssemblyResource("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/document-literal.wsdl");
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            MethodInfo sayHelloArrayMethod = proxyType.GetMethod("SayHelloArray");
            Assert.IsNotNull(sayHelloArrayMethod);

            object[] sdmaAttrs = sayHelloArrayMethod.GetCustomAttributes(typeof(SoapDocumentMethodAttribute), false);
            Assert.IsTrue(sdmaAttrs.Length > 0);

            SoapDocumentMethodAttribute sdma = (SoapDocumentMethodAttribute)sdmaAttrs[0];
            Assert.AreEqual("http://www.springframwework.net/SayHelloArray", sdma.Action);
            Assert.AreEqual(SoapParameterStyle.Wrapped, sdma.ParameterStyle);
            Assert.AreEqual(string.Empty, sdma.Binding);
            Assert.AreEqual(false, sdma.OneWay);
            Assert.AreEqual(string.Empty, sdma.RequestElementName);
            Assert.AreEqual("http://www.springframwework.net", sdma.RequestNamespace);
            Assert.AreEqual(string.Empty, sdma.ResponseElementName);
            Assert.AreEqual("http://www.springframwework.net", sdma.ResponseNamespace);
            Assert.AreEqual(SoapBindingUse.Literal, sdma.Use);

            object[] xaAttrs = sayHelloArrayMethod.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(XmlArrayAttribute), false);
            Assert.IsTrue(xaAttrs.Length > 0);

            XmlArrayAttribute xaa = (XmlArrayAttribute)xaAttrs[0];
            Assert.AreEqual("out", xaa.ElementName);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void RpcLiteralWithNamedOutParameter()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ServiceUri = new AssemblyResource("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/rpc-literal.wsdl");
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            MethodInfo sayHelloMethod = proxyType.GetMethod("SayHello");
            Assert.IsNotNull(sayHelloMethod);

            object[] srmaAttrs = sayHelloMethod.GetCustomAttributes(typeof(SoapRpcMethodAttribute), false);
            Assert.IsTrue(srmaAttrs.Length > 0);

            SoapRpcMethodAttribute srma = (SoapRpcMethodAttribute)srmaAttrs[0];
            Assert.AreEqual("http://www.springframwework.net/SayHello", srma.Action);
            Assert.AreEqual(string.Empty, srma.Binding);
            Assert.AreEqual(false, srma.OneWay);
            Assert.AreEqual(string.Empty, srma.RequestElementName);
            Assert.AreEqual("http://www.springframwework.net", srma.RequestNamespace);
            Assert.AreEqual(string.Empty, srma.ResponseElementName);
            Assert.AreEqual("http://www.springframwework.net", srma.ResponseNamespace);
            Assert.AreEqual(SoapBindingUse.Literal, srma.Use);

            object[] xeAttrs = sayHelloMethod.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(XmlElementAttribute), false);
            Assert.IsTrue(xeAttrs.Length > 0);

            XmlElementAttribute xea = (XmlElementAttribute)xeAttrs[0];
            Assert.AreEqual("out", xea.ElementName);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void RpcLiteralWithNamedOutArrayParameter()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ServiceUri = new AssemblyResource("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/rpc-literal.wsdl");
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            MethodInfo sayHelloArrayMethod = proxyType.GetMethod("SayHelloArray");
            Assert.IsNotNull(sayHelloArrayMethod);

            object[] srmaAttrs = sayHelloArrayMethod.GetCustomAttributes(typeof(SoapRpcMethodAttribute), false);
            Assert.IsTrue(srmaAttrs.Length > 0);

            SoapRpcMethodAttribute srma = (SoapRpcMethodAttribute)srmaAttrs[0];
            Assert.AreEqual("http://www.springframwework.net/SayHelloArray", srma.Action);
            Assert.AreEqual(string.Empty, srma.Binding);
            Assert.AreEqual(false, srma.OneWay);
            Assert.AreEqual(string.Empty, srma.RequestElementName);
            Assert.AreEqual("http://www.springframwework.net", srma.RequestNamespace);
            Assert.AreEqual(string.Empty, srma.ResponseElementName);
            Assert.AreEqual("http://www.springframwework.net", srma.ResponseNamespace);
            Assert.AreEqual(SoapBindingUse.Literal, srma.Use);

            object[] xaAttrs = sayHelloArrayMethod.ReturnTypeCustomAttributes.GetCustomAttributes(typeof(XmlArrayAttribute), false);
            Assert.IsTrue(xaAttrs.Length > 0);

            XmlArrayAttribute xaa = (XmlArrayAttribute)xaAttrs[0];
            Assert.AreEqual("out", xaa.ElementName);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void RpcLiteralWithDefaultConfig()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ServiceUri = new AssemblyResource("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/rpc-literal.wsdl");
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            object[] typeAttrs = proxyType.GetCustomAttributes(typeof(WebServiceBindingAttribute), false);
            Assert.IsTrue(typeAttrs.Length > 0);

            WebServiceBindingAttribute wsba = (WebServiceBindingAttribute)typeAttrs[0];
            Assert.AreEqual("HelloWorldServiceSoap", wsba.Name);
            Assert.AreEqual("http://www.springframwework.net", wsba.Namespace);

            MethodInfo sayHelloWorldMethod = proxyType.GetMethod("SayHelloWorld");
            Assert.IsNotNull(sayHelloWorldMethod);

            object[] srmaAttrs = sayHelloWorldMethod.GetCustomAttributes(typeof(SoapRpcMethodAttribute), false);
            Assert.IsTrue(srmaAttrs.Length > 0);

            SoapRpcMethodAttribute srma = (SoapRpcMethodAttribute)srmaAttrs[0];
            Assert.AreEqual("http://www.springframwework.net/SayHelloWorld", srma.Action);
            Assert.AreEqual(string.Empty, srma.Binding);
            Assert.AreEqual(false, srma.OneWay);
            Assert.AreEqual(string.Empty, srma.RequestElementName);
            Assert.AreEqual("http://www.springframwework.net", srma.RequestNamespace);
            Assert.AreEqual(string.Empty, srma.ResponseElementName);
            Assert.AreEqual("http://www.springframwework.net", srma.ResponseNamespace);
            Assert.AreEqual(SoapBindingUse.Literal, srma.Use);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void RpcLiteralWithOneWay()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ServiceUri = new AssemblyResource("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/rpc-literal.wsdl");
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            MethodInfo logHelloWorldMethod = proxyType.GetMethod("LogHelloWorld");
            Assert.IsNotNull(logHelloWorldMethod);

            object[] methodAttrs = logHelloWorldMethod.GetCustomAttributes(typeof(SoapRpcMethodAttribute), false);
            Assert.IsTrue(methodAttrs.Length > 0);

            SoapRpcMethodAttribute srma = (SoapRpcMethodAttribute)methodAttrs[0];
            Assert.AreEqual("http://www.springframwework.net/LogHelloWorld", srma.Action);
            Assert.AreEqual(string.Empty, srma.Binding);
            Assert.AreEqual(true, srma.OneWay);
            Assert.AreEqual(string.Empty, srma.RequestElementName);
            Assert.AreEqual("http://www.springframwework.net", srma.RequestNamespace);
            Assert.AreEqual(string.Empty, srma.ResponseElementName);
            Assert.AreEqual(null, srma.ResponseNamespace);
            Assert.AreEqual(SoapBindingUse.Literal, srma.Use);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void RpcLiteralWithMessageName()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ServiceUri = new AssemblyResource("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/rpc-literal.wsdl");
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            MethodInfo logHelloMethod = proxyType.GetMethod("LogHello");
            Assert.IsNotNull(logHelloMethod);

            object[] methodAttrs = logHelloMethod.GetCustomAttributes(typeof(SoapRpcMethodAttribute), false);
            Assert.IsTrue(methodAttrs.Length > 0);

            SoapRpcMethodAttribute srma = (SoapRpcMethodAttribute)methodAttrs[0];
            Assert.AreEqual("http://www.springframwework.net/MyLogHello", srma.Action);
            Assert.AreEqual(string.Empty, srma.Binding);
            Assert.AreEqual(false, srma.OneWay);
            Assert.AreEqual(string.Empty, srma.RequestElementName);
            Assert.AreEqual("http://www.springframwework.net", srma.RequestNamespace);
            Assert.AreEqual(string.Empty, srma.ResponseElementName);
            Assert.AreEqual("http://www.springframwework.net", srma.ResponseNamespace);
            Assert.AreEqual(SoapBindingUse.Literal, srma.Use);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void WrapProxyType()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ProxyType = typeof(FakeProxyClass);
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void ConfigureSoapHttpClientProtocolWithServiceUri()
        {
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/configurableFactory.xml");
            ContextRegistry.Clear();
            ContextRegistry.RegisterContext(ctx);

            object proxy = ctx.GetObject("withServiceUri");
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            SoapHttpClientProtocol shcp = proxy as SoapHttpClientProtocol;
            Assert.AreEqual("http://www.springframework.org/", shcp.Url);
            Assert.AreEqual(10000, shcp.Timeout);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void ConfigureSoapHttpClientProtocolWithProxyType()
        {
            IApplicationContext ctx = new XmlApplicationContext("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/configurableFactory.xml");
            ContextRegistry.Clear();
            ContextRegistry.RegisterContext(ctx);

            object proxy = ctx.GetObject("withProxyType");
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            SoapHttpClientProtocol shcp = proxy as SoapHttpClientProtocol;
            Assert.AreEqual("http://www.springframework.org/", shcp.Url);
            Assert.AreEqual(10000, shcp.Timeout);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        [Test]
        public void UseCustomClientProtocolType()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
            wspf.ServiceUri = new AssemblyResource("assembly://Spring.Services.Tests/Spring.Data.Spring.Web.Services/document-literal.wsdl");
            wspf.ServiceInterface = typeof(IHelloWorld);
            wspf.WebServiceProxyBaseType = typeof(CustomClientProtocol);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is IHelloWorld);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);
            Assert.IsTrue(proxy is CustomClientProtocol);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        #region NestedSchema

        [Test]
        public void NestedSchema()
        {
            WebServiceProxyFactory wspf = new WebServiceProxyFactory();
			// cf Post Build Events
            wspf.ServiceUri = new FileSystemResource("file://~/Spring/Web/Services/nestedSchema.wsdl");
            wspf.ServiceInterface = typeof(INestedSchema);
            wspf.AfterPropertiesSet();

            object proxy = wspf.GetObject();
            Assert.IsNotNull(proxy);

            Type proxyType = proxy.GetType();
            Assert.IsTrue(proxy is INestedSchema);
            Assert.IsTrue(proxy is SoapHttpClientProtocol);

            // Try to instantiate the proxy type
            ObjectUtils.InstantiateType(proxyType);
        }

        public interface INestedSchema
        {
            string testMethod(UserCredentials userCredentials);
        }

        public class UserCredentials
        {
        }

        #endregion

        public void WcfBasicHttpBinding()
        {
            using (ServiceHost host = new ServiceHost(typeof(WcfServiceImpl), new Uri("http://localhost:8000/WcfBasicHttpBinding/Service")))
            {
                host.AddServiceEndpoint(typeof(IWcfService), new BasicHttpBinding(), string.Empty);

                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                host.Description.Behaviors.Add(smb);

                host.Open();

                WebServiceProxyFactory wspf = new WebServiceProxyFactory();
                wspf.ServiceUri = new UrlResource("http://localhost:8000/WcfBasicHttpBinding/Service");
                wspf.ServiceInterface = typeof(IWcfService);
                wspf.AfterPropertiesSet();

                object proxy = wspf.GetObject();
                Assert.IsNotNull(proxy);

                Type proxyType = proxy.GetType();
                Assert.IsTrue(proxy is IWcfService);
                Assert.IsTrue(proxy is SoapHttpClientProtocol);

                IWcfService srv = proxy as IWcfService;
                Assert.AreEqual("5", srv.WcfMethod(5));
            }
        }

        [ServiceContract(Namespace="http://www.springframework.net/")]
        public interface IWcfService
        {
            [OperationContract]
            string WcfMethod(int count);
        }

        public class WcfServiceImpl : IWcfService
        {
            public string WcfMethod(int count)
            {
                return count.ToString();
            }
        }

        #region Test Classes

        public interface IHelloWorld
        {
            string SayHelloWorld();
            string SayHello(string name);
            string[] SayHelloArray(string name);
            void LogHelloWorld();
            void LogHello(string name);
        }

        /// <summary>
        /// Fake Web Service proxy that does not implement any interface.
        /// </summary>
        /// <remarks>
        /// Note that methods matchs IHelloWorld interface methods.
        /// </remarks>
        [WebServiceBinding(Name="FakeWebService")]
        public class FakeProxyClass : SoapHttpClientProtocol
        {
            public FakeProxyClass()
            {
                this.Url = "http://www.fcporto.pt/";
            }

            public string SayHelloWorld()
            {
                return "Hello World !";
            }

            public string SayHello(string name)
            {
                return String.Format("Hello {0} !", name);
            }

            public string[] SayHelloArray(string name)
            {
                return new string[] { "Hello ", name, " !" };
            }

            public void LogHelloWorld()
            {
            }

            public void LogHello(string name)
            {
            }
        }

        public class CustomClientProtocol : SoapHttpClientProtocol
        {
        }

        #endregion
    }
}
