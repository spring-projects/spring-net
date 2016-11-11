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
using System.Reflection.Emit;
using System.Web.Services;

using NUnit.Framework;

using Spring.Util;
using Spring.Core.IO;
using Spring.Objects.Factory.Xml;
using Spring.Objects.Factory;

#endregion

namespace Spring.Web.Services
{
    /// <summary>
    /// Unit tests for the WebServiceExporter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public sealed class WebServiceExporterTests
    {
        WebServiceExporter wse = null;

        [SetUp]
        public void SetUp()
        {
            const string xml =
    @"<?xml version='1.0' encoding='UTF-8' ?>
<objects xmlns='http://www.springframework.net'>
	<object id='noDecoratedService' type='Spring.Web.Services.WebServiceExporterTests+NoDecoratedService, Spring.Web.Tests'/>
    <object id='decoratedService' type='Spring.Web.Services.WebServiceExporterTests+DecoratedService, Spring.Web.Tests'/>
</objects>";
            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            IObjectFactory objectFactory = new XmlObjectFactory(new InputStreamResource(stream, string.Empty));

            wse = new WebServiceExporter();
            wse.ObjectFactory = objectFactory;
        }

        [Test]
        public void NullConfig()
        {
            wse.ObjectName = "NullConfig";
            Assert.Throws<ArgumentException>(() => wse.AfterPropertiesSet(), "The TargetName property is required.");
        }

        [Test]
        public void ProxiesTargetInterfaces()
        {
            wse.ObjectName = "ProxiesTargetInterfaces";
            wse.TargetName = "noDecoratedService";
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            Assert.IsTrue(typeof(IService).IsAssignableFrom(proxyType));
        }

        [Test]
        public void CreatesWebServiceAttributeWithNoDecoratedClassAndMinimalConfig()
        {
            wse.ObjectName = "CreatesWebServiceAttributeWithNoDecoratedClassAndMinimalConfig";
            wse.TargetName = "noDecoratedService";
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            object[] attrs = proxyType.GetCustomAttributes(typeof(WebServiceAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            WebServiceAttribute wsa = attrs[0] as WebServiceAttribute;
            Assert.AreEqual("CreatesWebServiceAttributeWithNoDecoratedClassAndMinimalConfig", wsa.Name);
            Assert.AreEqual(string.Empty, wsa.Description);
            Assert.AreEqual(WebServiceAttribute.DefaultNamespace, wsa.Namespace);
        }

        [Test]
        public void CreatesWebServiceAttributeWithNoDecoratedClassAndFullConfig()
        {
            wse.ObjectName = "CreatesWebServiceAttributeWithNoDecoratedClassAndFullConfig";
            wse.TargetName = "noDecoratedService";
            wse.Name = "My web service name";
            wse.Description = "My web service description";
            wse.Namespace = "http://www.springframework.net";
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            object[] attrs = proxyType.GetCustomAttributes(typeof(WebServiceAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            WebServiceAttribute wsa = attrs[0] as WebServiceAttribute;
            Assert.AreEqual(wse.Name, wsa.Name);
            Assert.AreEqual(wse.Description, wsa.Description);
            Assert.AreEqual(wse.Namespace, wsa.Namespace);
        }

        [Test]
        public void CreatesDefaultWebServiceBindingAttribute()
        {
            wse.ObjectName = "CreatesDefaultWebServiceBindingAttribute";
            wse.TargetName = "noDecoratedService";
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            object[] attrs = proxyType.GetCustomAttributes(typeof(WebServiceBindingAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            WebServiceBindingAttribute wsba = attrs[0] as WebServiceBindingAttribute;
            Assert.AreEqual(WsiProfiles.BasicProfile1_1, wsba.ConformsTo);
        }

        [Test]
        public void CreatesWebServiceBindingAttributeWithNoWsiProfile()
        {
            wse.ObjectName = "CreatesWebServiceBindingAttributeWithNoWsiProfile";
            wse.TargetName = "noDecoratedService";
            wse.WsiProfile = WsiProfiles.None;
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            object[] attrs = proxyType.GetCustomAttributes(typeof(WebServiceBindingAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            WebServiceBindingAttribute wsba = attrs[0] as WebServiceBindingAttribute;
            Assert.AreEqual(WsiProfiles.None, wsba.ConformsTo);
        }

        [Test]
        public void UsesExistingWebServiceBindingAttributeWithDecoratedClass()
        {
            wse.ObjectName = "UsesExistingWebServiceBindingAttributeWithDecoratedClass";
            wse.TargetName = "decoratedService";
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            object[] attrs = proxyType.GetCustomAttributes(typeof(WebServiceBindingAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            WebServiceBindingAttribute wsba = attrs[0] as WebServiceBindingAttribute;
            Assert.AreEqual(WsiProfiles.None, wsba.ConformsTo);
        }

        [Test]
        public void CreatesDefaultWebMethodAttributeWithNoDecoratedMethod()
        {
            wse.ObjectName = "CreatesDefaultWebMethodAttributeWithNoDecoratedMethod";
            wse.TargetName = "noDecoratedService";
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            MethodInfo method = proxyType.GetMethod("SomeMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.GetCustomAttributes(typeof(WebMethodAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);
        }

        [Test]
        public void CreatesCustomWebMethodAttributeWithNoDecoratedMethod()
        {
            wse.ObjectName = "CreatesCustomWebMethodAttributeWithNoDecoratedMethod";
            wse.TargetName = "noDecoratedService";
            wse.MemberAttributes.Add("SomeMethod", new WebMethodAttribute(true)); // default value is false
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            MethodInfo method = proxyType.GetMethod("SomeMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.GetCustomAttributes(typeof(WebMethodAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            WebMethodAttribute wma = attrs[0] as WebMethodAttribute;
            Assert.AreEqual(true, wma.EnableSession);
        }

        [Test]
        public void OverridesExistingWebServiceAttributeWithDecoratedClass()
        {
            wse.ObjectName = "OverridesExistingWebServiceAttributeWithDecoratedClass";
            wse.TargetName = "decoratedService";
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            object[] attrs = proxyType.GetCustomAttributes(typeof(WebServiceAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            WebServiceAttribute wsa = attrs[0] as WebServiceAttribute;
            Assert.AreEqual("OverridesExistingWebServiceAttributeWithDecoratedClass", wsa.Name);
            Assert.AreEqual(string.Empty, wsa.Description);
            Assert.AreEqual(WebServiceAttribute.DefaultNamespace, wsa.Namespace);
        }

        [Test]
        public void UsesExistingWebMethodAttributeWithDecoratedMethod()
        {
            wse.ObjectName = "UsesExistingWebMethodAttributeWithDecoratedMethod";
            wse.TargetName = "decoratedService";
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            MethodInfo method = proxyType.GetMethod("SomeMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.GetCustomAttributes(typeof(WebMethodAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            WebMethodAttribute wma = attrs[0] as WebMethodAttribute;
            Assert.AreEqual("SomeMethod description", wma.Description);
        }

        // TODO : attributes override
        [Test]
        [Ignore("Attributes override not implemented.")]
        public void OverridesExistingWebMethodAttributeWithDecoratedMethod()
        {
            wse.ObjectName = "OverridesExistingWebMethodAttributeWithDecoratedMethod";
            wse.TargetName = "decoratedService";
            wse.MemberAttributes.Add("SomeMethod", new WebMethodAttribute(true)); // default value is false
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            MethodInfo method = proxyType.GetMethod("SomeMethod");
            Assert.IsNotNull(method);

            object[] attrs = method.GetCustomAttributes(typeof(WebMethodAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            WebMethodAttribute wma = attrs[0] as WebMethodAttribute;
            Assert.AreEqual(true, wma.EnableSession);
        }


        [Test]
        public void AppliesCustomAttributeBuilderToType()
        {
            wse.ObjectName = "AppliesCustomAttributeBuilderToType";
            wse.TargetName = "noDecoratedService";
            wse.TypeAttributes = new CustomAttributeBuilder[] { 
                new CustomAttributeBuilder(typeof(WebServiceAttribute).GetConstructor(Type.EmptyTypes), ObjectUtils.EmptyObjects) };
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();

            object[] attrs = proxyType.GetCustomAttributes(typeof(WebServiceAttribute), true);
            Assert.IsNotEmpty(attrs);
            Assert.AreEqual(1, attrs.Length);

            WebServiceAttribute wsa = attrs[0] as WebServiceAttribute;
            Assert.AreEqual("AppliesCustomAttributeBuilderToType", wsa.Name);
        }

        [Test]
        public void UseCustomWebServiceType()
        {
            wse.ObjectName = "UseCustomWebServiceType";
            wse.TargetName = "noDecoratedService";
            wse.WebServiceBaseType = typeof(CustomWebService);
            wse.AfterPropertiesSet();

            Type proxyType = wse.GetExportedType();
            Assert.IsTrue(typeof(CustomWebService).IsAssignableFrom(proxyType));
        }

        #region Test classes

        public interface IService
        {
            string SomeMethod(int param);
        }

        public class NoDecoratedService : IService
        {
            public string SomeMethod(int param)
            {
                return param.ToString();
            }
        }

        [WebService(Name = "Decorated service")]
        [WebServiceBinding(ConformsTo=WsiProfiles.None)]
        public class DecoratedService : IService
        {
            [WebMethod(Description="SomeMethod description")]
            public string SomeMethod(int param)
            {
                return param.ToString();
            }
        }

        public class CustomWebService : WebService
        {
        }

        #endregion
    }
}