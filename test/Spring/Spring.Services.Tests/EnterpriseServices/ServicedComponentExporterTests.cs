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
using System.Collections;
using System.EnterpriseServices;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using AopAlliance.Intercept;
using NUnit.Framework;

using Spring.Context;
using Spring.Context.Support;
using Spring.Objects;

#endregion

namespace Spring.EnterpriseServices
{
    /// <summary>
    /// Unit tests for the ServicedComponentExporter class.
    /// </summary>
    /// <author>Bruno Baia</author>
    [TestFixture]
    public class ServicedComponentExporterTests
    {
        [TearDown]
        public void TearDown()
        {
            ContextRegistry.Clear();
        }

        [Test]
        public void BailsWhenNotConfigured()
        {
            ServicedComponentExporter exp = new ServicedComponentExporter();
            try
            {
                exp.AfterPropertiesSet();
                Assert.Fail("Did not throw expected ArgumentException!");
            }
            catch (ArgumentException)
            {
                
            }
           
        }

        [Test]
        public void RegistersSimpleObjectWithNonDefaultTransactionOption()
        {
            ServicedComponentExporter exp = new ServicedComponentExporter();
            exp.TargetName = "objectTest";
            exp.ObjectName = "objectTestProxy";
            exp.TypeAttributes = new ArrayList();
            exp.TypeAttributes.Add(new TransactionAttribute(TransactionOption.RequiresNew));
            exp.AfterPropertiesSet();

            Type type = CreateWrapperType(exp, typeof(TestObject), false);

            TransactionAttribute[] attrs = (TransactionAttribute[])type.GetCustomAttributes(typeof(TransactionAttribute), false);
            Assert.AreEqual(1, attrs.Length);
            Assert.AreEqual(TransactionOption.RequiresNew, attrs[0].Value);
        }

        [Test]
        public void RegistersManagedObjectWithNonDefaultTransactionOption()
        {
            ServicedComponentExporter exp = new ServicedComponentExporter();
            exp.TargetName = "objectTest";
            exp.ObjectName = "objectTestProxy";
            exp.TypeAttributes = new ArrayList();
            exp.TypeAttributes.Add(new TransactionAttribute(TransactionOption.RequiresNew));
            exp.AfterPropertiesSet();

            Type type = CreateWrapperType(exp, typeof(TestObject), true);

            TransactionAttribute[] attrs = (TransactionAttribute[])type.GetCustomAttributes(typeof(TransactionAttribute), false);
            Assert.AreEqual(1, attrs.Length);
            Assert.AreEqual(TransactionOption.RequiresNew, attrs[0].Value);
        }

        [Test]
        [Explicit("causes troubles due to registry pollution by COM registration")]
        public void CanExportAopProxyToLibrary()
        {
            // NOTE: the method interceptor will return the number of method calls intercepted
            FileInfo assemblyFile = new FileInfo("ServiceComponentExporterTests.TestServicedComponents.dll");
            XmlApplicationContext appCtx = new XmlApplicationContext("ServiceComponentExporterTests.TestServicedComponents.Services.xml");
            EnterpriseServicesExporter exporter = new EnterpriseServicesExporter();
            exporter.ActivationMode = ActivationOption.Library;
            Type serviceType = ExportObject(exporter, assemblyFile, appCtx, "objectTest");
            try
            {
                // ServiceComponent will obtain its target from root context
//                ContextRegistry.RegisterContext(appCtx);

                IComparable testObject;
                testObject = (IComparable)Activator.CreateInstance(serviceType);
                Assert.AreEqual(1, testObject.CompareTo(null));
                testObject = (IComparable)Activator.CreateInstance(serviceType);
                Assert.AreEqual(2, testObject.CompareTo(null));
            }
            finally
            {
                exporter.UnregisterServicedComponents(assemblyFile);
                ContextRegistry.Clear();
            }
        }

        [Test, Explicit("causes troubles due to registry pollution by COM registration")]
        public void CanExportAopProxyToServer()
        {
            FileInfo assemblyFile = new FileInfo("ServiceComponentExporterTests.TestServicedComponents.exe");
            XmlApplicationContext appCtx = new XmlApplicationContext("ServiceComponentExporterTests.TestServicedComponents.Services.xml");
            EnterpriseServicesExporter exporter = new EnterpriseServicesExporter();
            exporter.ActivationMode = ActivationOption.Server;
            Type serviceType = ExportObject(exporter, assemblyFile, appCtx, "objectTest");
            try
            {
                // ServiceComponent will obtain its target from root context
                IComparable testObject;
                testObject = (IComparable)Activator.CreateInstance(serviceType);
                Assert.AreEqual(1, testObject.CompareTo(null));
                testObject = (IComparable)Activator.CreateInstance(serviceType);
                Assert.AreEqual(2, testObject.CompareTo(null));
            }
            finally
            {
                exporter.UnregisterServicedComponents(assemblyFile);
            }
        }

        private Type ExportObject(EnterpriseServicesExporter exporter, FileInfo assemblyFile, IConfigurableApplicationContext appCtx, string objectName)
        {
            exporter.ObjectFactory = appCtx.ObjectFactory;
            exporter.Assembly = Path.GetFileNameWithoutExtension(assemblyFile.Name);
            exporter.ApplicationName = exporter.Assembly;
            exporter.AccessControl = new ApplicationAccessControlAttribute(false);
            exporter.UseSpring = true;

            ServicedComponentExporter exp = new ServicedComponentExporter();
            exp.TargetName = objectName;
            exp.ObjectName = objectName + "Service";
            exp.TypeAttributes = new ArrayList();
            exp.TypeAttributes.Add(new TransactionAttribute(TransactionOption.RequiresNew));
            exp.AfterPropertiesSet();

            exporter.Components.Add(exp);

            Assembly assembly = exporter.GenerateComponentAssembly(assemblyFile);
            exporter.RegisterServicedComponents(assemblyFile);
            return assembly.GetType(objectName + "Service");
        }

        #region Private helpers & classes

        public class CountingMethodInterceptor : IMethodInterceptor
        {
            private int Calls = 0;

            public object Invoke(IMethodInvocation invocation)
            {
                Calls++;
                return Calls;
            }
        }

        private Type CreateWrapperType(ServicedComponentExporter exporter, Type targetType, bool useSpring)
        {
            AssemblyName an = new AssemblyName();
            an.Name = "Spring.EnterpriseServices.Tests";
            AssemblyBuilder proxyAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder module = proxyAssembly.DefineDynamicModule(an.Name, an.Name + ".dll", true);

            Type baseType = typeof(ServicedComponent);
            if (useSpring)
            {
                baseType = EnterpriseServicesExporter.CreateSpringServicedComponentType(module, baseType);
            }

            object result = exporter.CreateWrapperType(module, baseType, targetType, useSpring);
            Assert.IsNotNull(result);
            Assert.IsTrue(result is Type);

            return (Type)result;
        }

        #endregion
    }
}
