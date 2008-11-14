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
using System.Reflection;
using System.Reflection.Emit;

using NUnit.Framework;
using DotNetMock.Dynamic;
using Spring.Objects.Factory;
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

            Type type = CreateWrapperType(exp, typeof(TestObject));

            TransactionAttribute[] attrs =  (TransactionAttribute[])type.GetCustomAttributes(typeof(TransactionAttribute), false);
            Assert.AreEqual(1, attrs.Length);
            Assert.AreEqual(TransactionOption.RequiresNew, attrs[0].Value);
        }

        #region Private helpers classes

//        private delegate Type CreateWrapperTypeHandler(ModuleBuilder module, Type baseType, IObjectFactory objectFactory, bool useSpring);

        private Type CreateWrapperType(ServicedComponentExporter exporter, Type type)
        {
            AssemblyName an = new AssemblyName();
			an.Name = "Spring.EnterpriseServices.Tests";
            AssemblyBuilder proxyAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder module = proxyAssembly.DefineDynamicModule(an.Name, an.Name + ".dll", true);

            IDynamicMock mockObjectFactory = new DynamicMock(typeof(IObjectFactory));
            mockObjectFactory.ExpectAndReturn("GetType", type, new object[]{ exporter.TargetName });

            object result = exporter.CreateWrapperType(module, typeof(ServicedComponent), (IObjectFactory) mockObjectFactory.Object, false); //createWrapperTypeMethodInfo.Invoke(exporter, new object[] { module, mockObjectFactory.Object });
            Assert.IsNotNull(result);
            Assert.IsTrue(result is Type);

            return (Type)result;
        }

        #endregion
	}
}

#endif // (!NET_1_0)
