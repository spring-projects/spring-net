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

#if (!NET_1_0)

#region Imports

using System;
using System.Collections;
using System.EnterpriseServices;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting;
using AopAlliance.Intercept;
using NUnit.Framework;
using DotNetMock.Dynamic;
using Rhino.Mocks;
using Spring.Aop.Framework;
using Spring.Context.Support;
using Spring.Objects.Factory;
using Spring.Objects;
using Spring.Objects.Factory.Support;

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
        [ExpectedException(typeof(ArgumentException))]
        public void BailsWhenNotConfigured ()
        {
            ServicedComponentExporter exp = new ServicedComponentExporter();
            exp.AfterPropertiesSet ();
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

            TransactionAttribute[] attrs =  (TransactionAttribute[])type.GetCustomAttributes(typeof(TransactionAttribute), false);
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

            TransactionAttribute[] attrs =  (TransactionAttribute[])type.GetCustomAttributes(typeof(TransactionAttribute), false);
            Assert.AreEqual(1, attrs.Length);
            Assert.AreEqual(TransactionOption.RequiresNew, attrs[0].Value);
        }

        [Test]
        public void CanExportAopProxy()
        {
            // create an advised proxy and add to objectFactory
            // note, that we need to implement a signed interface here
            ProxyFactory aopProxyFactory = new ProxyFactory(new Type[] { typeof(IComparable) });
            MockMethodInterceptor methodInterceptor = new MockMethodInterceptor();
            aopProxyFactory.AddAdvice(methodInterceptor);
            IComparable aopProxy = (IComparable) aopProxyFactory.GetProxy();
//            ((AssemblyBuilder)aopProxy.GetType().Assembly).Save("Spring.Proxy.dll");

            // sanity check
            methodInterceptor.NextResult = 2;
            Assert.AreEqual(methodInterceptor.NextResult, aopProxy.CompareTo(this));
            Assert.AreEqual(1, methodInterceptor.Calls);

            StaticApplicationContext appCtx = new StaticApplicationContext(AbstractApplicationContext.DefaultRootContextName, null);
            appCtx.ObjectFactory.RegisterSingleton("objectTest", aopProxy);

            FileInfo assemblyFile = new FileInfo("ServiceComponentExporterTests.TestServicedComponents.dll");
            EnterpriseServicesExporter exporter = new EnterpriseServicesExporter();
            Type serviceType = ExportObject(exporter, assemblyFile, appCtx, "objectTest");
            try
            {
                // ServiceComponent will obtain its target from root context
                ContextRegistry.RegisterContext(appCtx);
                methodInterceptor.Calls = 0;

                IComparable testObject;
                testObject = (IComparable)Activator.CreateInstance(serviceType);
                methodInterceptor.NextResult = 3;
                Assert.AreEqual(methodInterceptor.NextResult, testObject.CompareTo(null));
                testObject = (IComparable)Activator.CreateInstance(serviceType);
                methodInterceptor.NextResult = 4;
                Assert.AreEqual(methodInterceptor.NextResult, testObject.CompareTo(null));
                
                Assert.AreEqual(2, methodInterceptor.Calls);
            }
            finally
            {
                exporter.UnregisterServicedComponents(assemblyFile);
                ContextRegistry.Clear();
            }
        }

	    private Type ExportObject(EnterpriseServicesExporter exporter, FileInfo assemblyFile, StaticApplicationContext appCtx, string objectName)
	    {
	        exporter.ObjectFactory = appCtx.ObjectFactory;
	        exporter.Assembly = Path.GetFileNameWithoutExtension(assemblyFile.Name);
	        exporter.ApplicationName = exporter.Assembly;
	        exporter.ActivationMode = ActivationOption.Library;
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

        private class MockMethodInterceptor : IMethodInterceptor
        {
            public object NextResult = null;
            public int Calls = 0;
            public IMethodInvocation LastInvocation;

            public object Invoke(IMethodInvocation invocation)
            {
                Calls++;
                LastInvocation = invocation;
                return NextResult;
            }
        }

        private Type CreateWrapperType(ServicedComponentExporter exporter, Type targetType, bool useSpring)
        {
            AssemblyName an = new AssemblyName();
            an.Name = "Spring.EnterpriseServices.Tests";
            AssemblyBuilder proxyAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder module = proxyAssembly.DefineDynamicModule(an.Name, an.Name + ".dll", true);

            Type baseType = typeof (ServicedComponent);
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

#endif // (!NET_1_0)
