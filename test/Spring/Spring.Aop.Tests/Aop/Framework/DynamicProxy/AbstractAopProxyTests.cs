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

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Reflection;

using AopAlliance.Aop;
using AopAlliance.Intercept;

using FakeItEasy;

using Spring.Aop.Interceptor;
using Spring.Aop.Support;
using Spring.Expressions;
using Spring.Objects;
using Spring.Proxy;
using Spring.Threading;
using Spring.Util;

using NUnit.Framework;

#pragma warning disable SYSLIB0050

namespace Spring.Aop.Framework.DynamicProxy
{
    /// <summary>
    /// Useful base class for Aop proxy test cases.
    /// </summary>
    /// <author>Rod Johnson</author>
    /// <author>Juergen Hoeller</author>
    /// <author>Bruno Baia (.NET)</author>
    public abstract class AbstractAopProxyTests
    {
        protected MockTargetSource mockTargetSource = new MockTargetSource();

        /*
         * Make a clean target source available if code wants to use it.
         * The target must be set. Verification will be automatic in tearDown
         * to ensure that it was used appropriately by code.
         */

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            //            SystemUtils.RegisterLoadedAssemblyResolver();
        }

        [SetUp]
        protected void SetUp()
        {
            mockTargetSource.Reset();
        }

        [TearDown]
        protected void TearDown()
        {
            mockTargetSource.Verify();
        }

        protected abstract object CreateProxy(AdvisedSupport advisedSupport);

        protected abstract Type CreateAopProxyType(AdvisedSupport advisedSupport);

        protected virtual IAopProxy CreateAopProxy(AdvisedSupport advisedSupport)
        {
            Type proxyType = CreateAopProxyType(advisedSupport);
            ConstructorInfo proxyCtorInfo = proxyType.GetConstructor(new Type[] { typeof(IAdvised) });

            ExpressionEvaluator.GetValue(advisedSupport, "Activate()");
            return (IAopProxy)proxyCtorInfo.Invoke(new object[] { advisedSupport });
        }

        private object SerializeAndDeserialize(object s)
        {
            // Serialize the session
            using (Stream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.AssemblyFormat = FormatterAssemblyStyle.Full;
                formatter.TypeFormat = FormatterTypeStyle.TypesAlways;
                formatter.Serialize(stream, s);

                // Deserialize the session
                stream.Position = 0;
                object res = formatter.Deserialize(stream);
                return res;
            }
        }

        public interface ISerializableTestObject
        {
            string TestData { get; set; }
        }

        [Serializable]
        public class SerializableTestObject : ISerializableTestObject, IDeserializationCallback
        {
            public static int InstanceCount;

            public SerializableTestObject()
            {
                InstanceCount++;
            }

            public string TestData { get { return testData; } set { testData = value; } }

            private string testData;

            public void OnDeserialization(object sender)
            {
                // only count non-proxy type deserializations
                if (!AopUtils.IsAopProxy(this))
                {
                    InstanceCount++;
                }
            }
        }

        [Serializable]
        public class CustomSerializableTestObject : ISerializableTestObject, ISerializable, IDeserializationCallback
        {
            public static int InstanceCount;

            public CustomSerializableTestObject()
            {
                InstanceCount++;
            }

            protected CustomSerializableTestObject(SerializationInfo info, StreamingContext context)
            {
                TestData = info.GetString("testData");
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("testData", TestData);
            }

            public string TestData { get { return testData; } set { testData = value; } }

            public void OnDeserialization(object sender)
            {
                if (!AopUtils.IsAopProxy(this))
                {
                    InstanceCount++;
                }
            }

            [NonSerialized]
            private string testData;
        }

        [Test]
        public void CanSerializeDeserializeSerializable()
        {
            int instanceCount;
            ISerializableTestObject target = new SerializableTestObject();
            target.TestData = "testData";
            AdvisedSupport advised = new AdvisedSupport();
            advised.Target = target;
            advised.Interfaces = new Type[] { typeof(ISerializableTestObject) };
            //            advised.AddAdvisor(new DefaultPointcutAdvisor(new NopInterceptor()));

            ISerializableTestObject to = (ISerializableTestObject)CreateAopProxy(advised);

            instanceCount = SerializableTestObject.InstanceCount;
            to = (ISerializableTestObject)SerializeAndDeserialize(to);

            // new instance was created
            Assert.AreEqual(instanceCount + 1, SerializableTestObject.InstanceCount);
            // values were (de-)serialized
            Assert.AreEqual(target.TestData, to.TestData);
        }

        [Test]
        public void CanSerializeDeserializeISerializable()
        {
            int instanceCount;
            ISerializableTestObject target = new CustomSerializableTestObject();
            target.TestData = "testData";
            AdvisedSupport advised = new AdvisedSupport();
            advised.Target = target;
            advised.Interfaces = new Type[] { typeof(ISerializableTestObject) };
            //            advised.AddAdvisor(new DefaultPointcutAdvisor(new NopInterceptor()));

            ISerializableTestObject to = (ISerializableTestObject)CreateAopProxy(advised);

            instanceCount = CustomSerializableTestObject.InstanceCount;
            to = (ISerializableTestObject)SerializeAndDeserialize(to);

            // new instance was created
            Assert.AreEqual(instanceCount + 1, CustomSerializableTestObject.InstanceCount);
            // values were (de-)serialized
            Assert.AreEqual(target.TestData, to.TestData);
        }

        [Test(Description = "Simple test that if we set values we can get them out again.")]
        public void ValuesStick()
        {
            int age1 = 33;
            int age2 = 37;
            string name = "tony";

            TestObject target = new TestObject();
            target.Age = age1;
            ProxyFactory pf1 = new ProxyFactory(target);
            pf1.AddAdvisor(new DefaultPointcutAdvisor(new NopInterceptor()));
            pf1.AddAdvisor(new DefaultPointcutAdvisor(new TimestampIntroductionInterceptor()));
            ITestObject to = target;

            Assert.AreEqual(age1, to.Age);
            to.Age = age2;
            Assert.AreEqual(age2, to.Age);
            Assert.IsNull(to.Name);
            to.Name = name;
            Assert.AreEqual(name, to.Name);
        }

        public interface ITestPerson
        {
            long Id { get; set; }
            string Name { get; set; }
        }

        public interface ITestCustomer : ITestPerson
        {
            string Company { get; set; }
        }

        public class TestCustomer : ITestCustomer
        {
            private long _id;
            private string _name;
            private string _company;

            public long Id
            {
                get { return _id; }
                set { _id = value; }
            }

            public string Name
            {
                get { return _name; }
                set { _name = value; }
            }

            public string Company
            {
                get { return _company; }
                set { _company = value; }
            }
        }

        [Test(Description = "http://jira.springframework.org/browse/SPRNET-1174")]
        public void ImplementsInterfaceHierarchy()
        {
            IMethodInterceptor mi = A.Fake<IMethodInterceptor>();

            A.CallTo(() => mi.Invoke(A<IMethodInvocation>.That.Matches(x => x.Method.Name == "get_Id"))).Returns((long) 5).Once();
            A.CallTo(() => mi.Invoke(A<IMethodInvocation>.That.Matches(x => x.Method.Name == "get_Name"))).Returns("Customer Name").Once();
            A.CallTo(() => mi.Invoke(A<IMethodInvocation>.That.Matches(x => x.Method.Name == "get_Company"))).Returns("Customer Company").Once();

            AdvisedSupport advised = new AdvisedSupport();
            advised.AddAdvice(mi);
            advised.Interfaces = new[] { typeof(ITestCustomer) };

            ITestCustomer to = CreateProxy(advised) as ITestCustomer;
            Assert.IsNotNull(to);
            Assert.AreEqual((long) 5, to.Id, "Incorrect Id");
            Assert.AreEqual("Customer Name", to.Name, "Incorrect Name");
            Assert.AreEqual("Customer Company", to.Company, "Incorrect Company");
        }

        //[Test] - to be called from derived fixtures
        public virtual void Equality()
        {
            TestCustomer customer = new TestCustomer();

            AdvisedSupport advised = new AdvisedSupport();
            advised.Target = customer;
            advised.Interfaces = new Type[] { typeof(ITestCustomer) };

            ITestCustomer to = CreateProxy(advised) as ITestCustomer;
            Assert.IsNotNull(to);
            Assert.IsTrue( to.Equals(to), "identity must be equal" );
            Assert.AreEqual(to, to);
            Assert.AreEqual(to, ((IAdvised)to).TargetSource.GetTarget());
        }

        [Test]
        public void Does_proxy_interfacemethods_without_implementation_and_by_default_throws_NotSupportedException()
        {
            AdvisedSupport advised = new AdvisedSupport();
            advised.TargetSource = new DynamicTargetSource(typeof(object), null);
            advised.Interfaces = new Type[] { typeof(ITestObject) };

            ITestObject proxy = CreateProxy(advised) as ITestObject;
            Assert.IsNotNull(proxy);

            Assert.Throws<NotSupportedException>(() => proxy.GetDescription(), "Target 'target' is null.");
        }

        [Test]
        public void Does_proxy_interfacemethods_without_implementation_and_delegates_to_interceptors()
        {
            DynamicInvocationTestInterceptor invocationInterceptor = new DynamicInvocationTestInterceptor();
            DynamicTargetSource targetSource = new DynamicTargetSource(typeof(object), null);

            AdvisedSupport advised = new AdvisedSupport();
            advised.TargetSource = targetSource;
            advised.Interfaces = new Type[] { typeof(ITestObject) };
            advised.AddAdvice(invocationInterceptor);
            ITestObject proxy = CreateProxy(advised) as ITestObject;
            Assert.IsNotNull(proxy);

            // target null, call handled by interceptor
            targetSource.Target = null;
            invocationInterceptor.CallProceed = false;
            proxy.GetDescription();
            Assert.AreEqual("GetDescription", invocationInterceptor.LastMethodInvocation.Method.Name);
        }

        [Test]
        public void Does_proxy_interfacemethods_without_implementation_and_throws_ArgumentNullException_On_NullTarget()
        {
            DynamicInvocationTestInterceptor invocationInterceptor = new DynamicInvocationTestInterceptor();
            DynamicTargetSource targetSource = new DynamicTargetSource(typeof(object), null);

            AdvisedSupport advised = new AdvisedSupport();
            advised.TargetSource = targetSource;
            advised.Interfaces = new Type[] { typeof(ITestObject) };
            advised.AddAdvice(invocationInterceptor);
            ITestObject proxy = CreateProxy(advised) as ITestObject;
            Assert.IsNotNull(proxy);

            // target null, call not handled by interceptor
            targetSource.Target = null;
            invocationInterceptor.CallProceed = true;
            var ex = Assert.Throws<ArgumentNullException>(() => proxy.GetDescription());
            Assert.That(ex.Message, Does.Contain("'target'"));
        }

        [Test]
        public void Does_proxy_interfacemethods_without_implementation_and_throws_NotSupportedException_On_Incompatible_Target()
        {
            DynamicInvocationTestInterceptor invocationInterceptor = new DynamicInvocationTestInterceptor();
            DynamicTargetSource targetSource = new DynamicTargetSource(typeof(object), null);

            AdvisedSupport advised = new AdvisedSupport();
            advised.TargetSource = targetSource;
            advised.Interfaces = new Type[] { typeof(ITestObject) };
            advised.AddAdvice(invocationInterceptor);
            ITestObject proxy = CreateProxy(advised) as ITestObject;
            Assert.IsNotNull(proxy);

            // target incompatible, call not handled by interceptor
            targetSource.Target = new object();
            invocationInterceptor.CallProceed = true;
            Assert.Throws<NotSupportedException>(() => proxy.GetDescription(), "Target 'target' of type 'System.Object' does not support methods of 'Spring.Objects.ITestObject'.");
        }

        [Test]
        public void NoInterceptorWithNoTarget()
        {
            AdvisedSupport advised = new AdvisedSupport();
            advised.Interfaces = new Type[] { typeof(ITestObject) };

            ITestObject to = CreateProxy(advised) as ITestObject;
            Assert.Throws<NotSupportedException>(() => to.GetDescription(), "Target 'target' of type 'System.Object' does not support methods of 'Spring.Objects.ITestObject'.");
        }

        [Test]
        public void InterceptorHandledCallWithNoTarget()
        {
            int age = 26;
            IMethodInterceptor mock = A.Fake<IMethodInterceptor>();
            A.CallTo(() => mock.Invoke(A<IMethodInvocation>.That.Not.IsNull())).Returns(age);

            AdvisedSupport advised = new AdvisedSupport();
            advised.AddAdvice(mock);
            advised.Interfaces = new Type[] { typeof(ITestObject) };

            ITestObject to = CreateProxy(advised) as ITestObject;
            Assert.IsNotNull(to);
            Assert.IsTrue(to.Age == age, "Incorrect age");
        }

        [Test]
        public void InterceptorUnhandledCallWithNoTarget()
        {
            AdvisedSupport advised = new AdvisedSupport();
            advised.AddAdvice(new NopInterceptor());
            advised.Interfaces = new Type[] { typeof(ITestObject) };

            ITestObject to = CreateProxy(advised) as ITestObject;
            Assert.IsNotNull(to);
            Assert.Throws<NotSupportedException>(() => to.GetDescription(), "Target 'target' of type 'System.Object' does not support methods of 'Spring.Objects.ITestObject'.");
        }

        [Test]
        public void ProxyAProxy()
        {
            ITestObject target = new TestObject();
            target.Age = 26;

            AdvisedSupport advised = new AdvisedSupport(target);
            advised.AddAdvice(new NopInterceptor());
            IAopProxy aop = CreateAopProxy(advised);
            ITestObject proxy1 = (ITestObject)aop.GetProxy();
            Assert.AreEqual(target.Age, proxy1.Age, "Incorrect age");

            advised = new AdvisedSupport(proxy1);
            advised.AddAdvice(new NopInterceptor());
            aop = CreateAopProxy(advised);
            ITestObject proxy2 = (ITestObject)aop.GetProxy();
            Assert.AreEqual(target.Age, proxy2.Age, "Incorrect age");
        }

        // SPRNET-655
        [Test]
        public void ProxyAProxyWhereClassExplicitlyImplementsInterfacesWithSameMethodNamesAndSignatures()
        {
            TheCommand target = new TheCommand();

            // proxy
            AdvisedSupport advised = new AdvisedSupport(target);
            advised.AddAdvice(new NopInterceptor());
            object proxy = CreateProxy(advised);

            // proxy again
            advised = new AdvisedSupport(proxy);
            advised.AddAdvice(new NopInterceptor());
            proxy = CreateAopProxy(advised);

            IServiceCommand serviceCommand = proxy as IServiceCommand;
            Assert.IsNotNull(serviceCommand);
            serviceCommand.Execute();

            IBusinessCommand businessCommand = proxy as IBusinessCommand;
            Assert.IsNotNull(businessCommand);
            businessCommand.Execute();

            Assert.AreEqual(1, serviceCommand.ServiceCount);
            Assert.AreEqual(1, businessCommand.BusinessCount);
        }

        public interface IServiceCommand
        {
            int ServiceCount { get; }

            void Execute();
        }

        public interface IBusinessCommand
        {
            int BusinessCount { get; }

            void Execute();
        }

        public class TheCommand : IServiceCommand, IBusinessCommand
        {
            private int _serviceCount = 0;
            int IServiceCommand.ServiceCount
            {
                get { return _serviceCount; }
            }

            void IServiceCommand.Execute()
            {
                _serviceCount++;
            }

            private int _businessCount = 0;
            int IBusinessCommand.BusinessCount
            {
                get { return _businessCount; }
            }

            void IBusinessCommand.Execute()
            {
                _businessCount++;
            }
        }

        [Test]
        public void EqualsMethod()
        {
            TestObject target = new TestObject();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
            advised.Target = target;
            IAopProxy aopProxy = CreateAopProxy(advised);
            object proxy = aopProxy.GetProxy();

            Assert.IsNotNull(proxy);
            Assert.AreEqual(target, proxy, "Equals() returned false");
        }

        [Test]
        public void GetHashCodeMethod()
        {
            TestObject target = new TestObject();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
            advised.Target = target;
            IAopProxy aopProxy = CreateAopProxy(advised);
            object proxy = aopProxy.GetProxy();

            Assert.IsNotNull(proxy);
            Assert.AreEqual(target.GetHashCode(), proxy.GetHashCode(), "GetHashCode() not equal");
        }

        [Test]
        [Platform("Win")]
        public void ProxyMethodWithRefOutParametersWithDirectCall()
        {
            PublicRefOutTestObject target = new PublicRefOutTestObject();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IRefOutTestObject) });
            advised.Target = target;

            IAopProxy aopProxy = CreateAopProxy(advised);
            IRefOutTestObject proxy = aopProxy.GetProxy() as IRefOutTestObject;
            Assert.IsNotNull(proxy);

            TestsProxyMethodWithRefOutParameters(proxy);
        }

        [Test]
        [Platform("Win")]
        public void ProxyMethodWithRefOutParametersWithDynamicReflection()
        {
            PublicRefOutTestObject target = new PublicRefOutTestObject();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IRefOutTestObject) });
            advised.Target = target;
            NopInterceptor ni = new NopInterceptor();
            advised.AddAdvice(ni);

            IAopProxy aopProxy = CreateAopProxy(advised);
            IRefOutTestObject proxy = aopProxy.GetProxy() as IRefOutTestObject;
            Assert.IsNotNull(proxy);

            TestsProxyMethodWithRefOutParameters(proxy);

            Assert.AreEqual(1, ni.Count);
        }

        [Test]
        [Platform("Win")]
        public virtual void ProxyMethodWithRefOutParametersWithStandardReflection()
        {
            InternalRefOutTestObject target = new InternalRefOutTestObject();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IRefOutTestObject) });
            advised.Target = target;
            NopInterceptor ni = new NopInterceptor();
            advised.AddAdvice(ni);

            IAopProxy aopProxy = CreateAopProxy(advised);
            IRefOutTestObject proxy = aopProxy.GetProxy() as IRefOutTestObject;
            Assert.IsNotNull(proxy);

            TestsProxyMethodWithRefOutParameters(proxy);

            Assert.AreEqual(1, ni.Count);
        }

        private void TestsProxyMethodWithRefOutParameters(IRefOutTestObject instance)
        {
            int valueType = 0;
            TestObject obj = null;
            EnumValue enumValue = EnumValue.Initial;
            bool refValueType = false;
            int outValueType;
            String refObj = null;
            TestObject outObj;
            EnumValue refEnum = EnumValue.Initial;
            EnumValue outEnum = EnumValue.Initial;
            Guid refGuid = Guid.NewGuid();
            Guid outGuid = Guid.NewGuid();

            instance.DoIt(valueType, obj, enumValue, ref refValueType, out outValueType, ref refObj, out outObj, ref refEnum, out outEnum, ref refGuid, out outGuid);

            Assert.AreEqual(0, valueType);
            Assert.AreEqual(null, obj);
            Assert.AreEqual(EnumValue.Initial, enumValue);
            Assert.AreEqual(true, refValueType);
            Assert.AreEqual(1, outValueType);
            Assert.AreEqual("RefObj", refObj);
            Assert.IsNotNull(outObj);
            Assert.AreEqual("OutObj", outObj.Name);
            Assert.AreEqual(EnumValue.Ref, refEnum);
            Assert.AreEqual(EnumValue.Out, outEnum);
            Assert.AreEqual(Guid.Empty, refGuid);
            Assert.AreEqual(Guid.Empty, outGuid);
        }

        public enum EnumValue
        {
            Initial,
            Ref,
            Out,
            Another
        }

        public interface IRefOutTestObject
        {
            int DoIt(int valueType, TestObject obj, EnumValue enumValue,
                ref bool refValueType, out int outValueType,
                ref String refObj, out TestObject outObj,
                ref EnumValue refEnum, out EnumValue outEnum,
                ref Guid refGuid, out Guid outGuid);
        }

        public class PublicRefOutTestObject : IRefOutTestObject
        {
            public int DoIt(int valueType, TestObject obj, EnumValue enumValue,
                ref bool refValueType, out int outValueType,
                ref String refObj, out TestObject outObj,
                ref EnumValue refEnum, out EnumValue outEnum,
                ref Guid refGuid, out Guid outGuid)
            {
                valueType++;
                obj = new TestObject("Bruno", 27);
                enumValue = EnumValue.Another;
                refValueType = true;
                outValueType = 1;
                refObj += "RefObj";
                outObj = new TestObject("OutObj", 27);
                refEnum = EnumValue.Ref;
                outEnum = EnumValue.Out;
                refGuid = Guid.Empty;
                outGuid = Guid.Empty;

                return 0;
            }
        }

        internal class InternalRefOutTestObject : PublicRefOutTestObject
        {
        }

        [Test]
        [Platform("Win")]
        public void ProxyGenericMethodWithRefOutParametersWithDirectCall()
        {
            PublicRefOutGenericTestObject target = new PublicRefOutGenericTestObject();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IRefOutGenericTestObject) });
            advised.Target = target;

            IAopProxy aopProxy = CreateAopProxy(advised);
            IRefOutGenericTestObject proxy = aopProxy.GetProxy() as IRefOutGenericTestObject;
            Assert.IsNotNull(proxy);

            TestsProxyGenericMethodWithRefOutParameters(proxy);
        }

        [Test]
        [Platform("Win")]
        public void ProxyGenericMethodWithRefOutParametersWithDynamicReflection()
        {
            PublicRefOutGenericTestObject target = new PublicRefOutGenericTestObject();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IRefOutGenericTestObject) });
            advised.Target = target;
            NopInterceptor ni = new NopInterceptor();
            advised.AddAdvice(ni);

            IAopProxy aopProxy = CreateAopProxy(advised);
            IRefOutGenericTestObject proxy = aopProxy.GetProxy() as IRefOutGenericTestObject;
            Assert.IsNotNull(proxy);

            TestsProxyGenericMethodWithRefOutParameters(proxy);

            Assert.AreEqual(3, ni.Count);
        }

        [Test]
        [Platform("Win")]
        public virtual void ProxyGenericMethodWithRefOutParametersWithStandardReflection()
        {
            InternalRefOutGenericTestObject target = new InternalRefOutGenericTestObject();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IRefOutGenericTestObject) });
            advised.Target = target;
            NopInterceptor ni = new NopInterceptor();
            advised.AddAdvice(ni);

            IAopProxy aopProxy = CreateAopProxy(advised);
            IRefOutGenericTestObject proxy = aopProxy.GetProxy() as IRefOutGenericTestObject;
            Assert.IsNotNull(proxy);

            TestsProxyGenericMethodWithRefOutParameters(proxy);

            Assert.AreEqual(3, ni.Count);
        }

        private void TestsProxyGenericMethodWithRefOutParameters(IRefOutGenericTestObject instance)
        {
            EnumValue enumValue = EnumValue.Another;
            EnumValue refEnum = EnumValue.Ref;
            EnumValue outEnum = EnumValue.Out;
            EnumValue enumResult = instance.DoIt<EnumValue>(enumValue, ref refEnum, out outEnum);
            Assert.AreEqual(EnumValue.Another, refEnum);
            Assert.AreEqual(EnumValue.Another, outEnum);
            Assert.AreEqual(EnumValue.Another, enumResult);

            TestObject objectValue = new TestObject("Value", 28);
            TestObject refObject = new TestObject("Ref", 28);
            TestObject outObject = new TestObject("Out", 28);
            TestObject objectResult = instance.DoIt<TestObject>(objectValue, ref refObject, out outObject);
            Assert.AreEqual("Value", refObject.Name);
            Assert.AreEqual("Value", outObject.Name);
            Assert.AreEqual("Value", objectResult.Name);

            int intValue = 1;
            int refInt = 2;
            int outInt = 3;
            int intResult = instance.DoIt<int>(intValue, ref refInt, out outInt);
            Assert.AreEqual(1, refInt);
            Assert.AreEqual(1, outInt);
            Assert.AreEqual(1, intResult);
        }

        public interface IRefOutGenericTestObject
        {
            T DoIt<T>(T param, ref T refParam, out T outParam);
        }

        public class PublicRefOutGenericTestObject : IRefOutGenericTestObject
        {
            public T DoIt<T>(T paramValue, ref T refParam, out T outParam)
            {
                refParam = paramValue;
                outParam = paramValue;

                return paramValue;
            }
        }

        internal class InternalRefOutGenericTestObject : PublicRefOutGenericTestObject
        {
        }

        [Test]
        public void ToStringMethod()
        {
            TestObject target = new TestObject();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
            advised.Target = target;
            IAopProxy aopProxy = CreateAopProxy(advised);
            object proxy = aopProxy.GetProxy();

            Assert.IsNotNull(proxy);
            Assert.AreEqual(target.ToString(), proxy.ToString(), "ToString() not equal");
        }

        [Test]
        public void InterceptGenericMethod()
        {
            AbstractProxyTypeBuilderTests.ClassWithGenericMethod target =
                new AbstractProxyTypeBuilderTests.ClassWithGenericMethod();
            mockTargetSource.SetTarget(target);

            NopInterceptor ni = new NopInterceptor();

            AdvisedSupport advised = new AdvisedSupport(mockTargetSource);
            advised.AddAdvice(ni);

            AbstractProxyTypeBuilderTests.InterfaceWithGenericMethod proxy =
                CreateProxy(advised) as AbstractProxyTypeBuilderTests.InterfaceWithGenericMethod;
            Assert.IsNotNull(proxy);

            proxy.PolymorphicMethod<int>();
            proxy.PolymorphicMethod<string>();

            string result1 = proxy.PolymorphicMethod("coucou");
            Assert.AreEqual("coucou", result1);

            proxy.WithGenericParameter<string>();

            proxy.WithGenericParameterAndGenericArgument<string>("ola");

            string result2 = proxy.WithGenericParameterAndGenericArgumentAndGenericReturnType<string>("ola");
            Assert.AreEqual("ola", result2);

            proxy.WithInterfaceConstraint<bool>();

            proxy.WithBaseTypeConstraint<DerivedTestObject>();

            proxy.WithBaseTypeAndInterfaceConstraints<DerivedTestObject>();

            proxy.WithMixedConstraint<bool, DerivedTestObject>();
            Assert.AreEqual(10, ni.Count);

            //if (this is DecoratorAopProxyTests)
            //{
            //    DynamicProxyManager.SaveAssembly();
            //}
        }

        [Test]
        public void InterceptGenericInterface()
        {
            AbstractProxyTypeBuilderTests.ClassThatImplementsGenericInterface<TestObject> target =
                new AbstractProxyTypeBuilderTests.ClassThatImplementsGenericInterface<TestObject>();
            mockTargetSource.SetTarget(target);

            NopInterceptor ni = new NopInterceptor();

            AdvisedSupport advised = new AdvisedSupport(mockTargetSource);
            advised.AddAdvice(ni);

            AbstractProxyTypeBuilderTests.GenericInterface<TestObject> proxy =
                CreateProxy(advised) as AbstractProxyTypeBuilderTests.GenericInterface<TestObject>;
            Assert.IsNotNull(proxy);

            TestObject to1 = proxy.Create();
            Assert.IsNotNull(to1);

            TestObject to2 = proxy.Populate(new TestObject());
            Assert.IsNotNull(to2);
            Assert.AreEqual("Populated", to2.Name);

            Assert.AreEqual(2, ni.Count);
        }

        [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPRNET-340")]
        [Platform("Win")]
        public void MultiThreadedProxyCreation()
        {
            MultiThreadedProxyCreation(5);
        }

        private void MultiThreadedProxyCreation(int howMany)
        {
            AsyncTestMethod[] threads = new AsyncTestMethod[howMany];
            for (int i = 0; i < howMany; i++)
                threads[i] = new AsyncTestMethod(10, new ThreadStart(ProxyTestObject));
            for (int i = 0; i < howMany; i++)
                threads[i].Start();
            for (int i = 0; i < howMany; i++)
                threads[i].AssertNoException();
        }

        private void ProxyTestObject()
        {
            TestObject target = new TestObject();
            target.Age = 26;

            AdvisedSupport advised = new AdvisedSupport(target);
            advised.AddAdvice(new NopInterceptor());

            ITestObject proxy = CreateProxy(advised) as ITestObject;
            Assert.IsNotNull(proxy);
            Assert.AreEqual(target.Age, proxy.Age, "Incorrect age");
        }

        [Test]
        public void ProxyTargetTypeAttributes()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            object[] attrs = proxy.GetType().GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have had 1 attribute applied to the target type.");
            Assert.AreEqual(1, attrs.Length, "Should have had 1 attribute applied to the target type.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the target type.");
        }

        [Test]
        public void DoesNotProxyTargetTypeAttributesWithProxyTargetAttributesEqualsFalse()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            advised.ProxyTargetAttributes = false;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            object[] attrs = proxy.GetType().GetCustomAttributes(false);
            Assert.IsNotNull(attrs);
            Assert.AreEqual(0, attrs.Length, "Should not have attribute applied to the target type.");
        }

        [Test]
        public void ProxyTargetMethodAttributes()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("Spring.Aop.Framework.DynamicProxy.AbstractAopProxyTests+IMarkerInterface.MarkerMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                method = proxy.GetType().GetMethod("MarkerMethod");
            }
            Assert.IsNotNull(method);

            object[] attrs = method.GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have 1 attribute applied to the target method.");
            Assert.AreEqual(1, attrs.Length, "Should have 1 attribute applied to the target method.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the target method.");
        }

        [Test]
        public void DoesNotProxyTargetMethodAttributesWithProxyTargetAttributesEqualsFalse()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            advised.ProxyTargetAttributes = false;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("Spring.Aop.Framework.DynamicProxy.AbstractAopProxyTests+IMarkerInterface.MarkerMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                method = proxy.GetType().GetMethod("MarkerMethod");
            }
            Assert.IsNotNull(method);

            object[] attrs = method.GetCustomAttributes(false);
            Assert.IsNotNull(attrs);
            Assert.AreEqual(0, attrs.Length, "Should not have attribute applied to the target method.");
        }

        [Test]
        public void ProxyTargetMethodParameterAttributes()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("Spring.Aop.Framework.DynamicProxy.AbstractAopProxyTests+IMarkerInterface.MarkerMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                method = proxy.GetType().GetMethod("MarkerMethod");
            }
            Assert.IsNotNull(method);

            object[] attrs = method.GetParameters()[1].GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have had 1 attribute applied to the method's parameter.");
            Assert.AreEqual(1, attrs.Length, "Should have had 1 attribute applied to the method's parameter.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the method's parameter.");
        }

        [Test]
        public void DoesNotProxyTargetMethodParameterAttributesWithProxyTargetAttributesEqualsFalse()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            advised.ProxyTargetAttributes = false;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("Spring.Aop.Framework.DynamicProxy.AbstractAopProxyTests+IMarkerInterface.MarkerMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                method = proxy.GetType().GetMethod("MarkerMethod");
            }
            Assert.IsNotNull(method);

            object[] attrs = method.GetParameters()[1].GetCustomAttributes(false);
            Assert.IsNotNull(attrs);
            Assert.AreEqual(0, attrs.Length, "Should not have attribute applied to the method's parameter.");
        }

        [Test]
        public void ProxyTargetMethodReturnValueAttributes()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("Spring.Aop.Framework.DynamicProxy.AbstractAopProxyTests+IMarkerInterface.MarkerMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                method = proxy.GetType().GetMethod("MarkerMethod");
            }
            Assert.IsNotNull(method);

            object[] attrs = method.ReturnTypeCustomAttributes.GetCustomAttributes(false);
            Assert.IsNotNull(attrs, "Should have had 1 attribute applied to the method's return value.");
            Assert.AreEqual(1, attrs.Length, "Should have had 1 attribute applied to the method's return value.");
            Assert.AreEqual(typeof(MarkerAttribute), attrs[0].GetType(), "Wrong System.Type of Attribute applied to the method's return value.");
        }

        [Test]
        public void DoesNotProxyTargetMethodReturnValueAttributesWithProxyTargetAttributesEqualsFalse()
        {
            MarkerClass target = new MarkerClass();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(IMarkerInterface) });
            advised.Target = target;
            advised.ProxyTargetAttributes = false;
            IAopProxy aopProxy = CreateAopProxy(advised);

            object proxy = aopProxy.GetProxy();
            Assert.IsNotNull(proxy, "The proxy generated by a (valid) call to GetProxy() was null.");

            MethodInfo method = proxy.GetType().GetMethod("Spring.Aop.Framework.DynamicProxy.AbstractAopProxyTests+IMarkerInterface.MarkerMethod", BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                method = proxy.GetType().GetMethod("MarkerMethod");
            }
            Assert.IsNotNull(method);

            object[] attrs = method.ReturnTypeCustomAttributes.GetCustomAttributes(false);
            Assert.IsNotNull(attrs);
            Assert.AreEqual(0, attrs.Length, "Should not have attribute applied to the method's return value.");
        }

        public sealed class MarkerAttribute : Attribute
        {
        }

        public interface IMarkerInterface
        {
            void MarkerMethod(int param1, string param2);
        }

        [Marker]
        public class MarkerClass : IMarkerInterface
        {
            [Marker]
            [return: Marker]
            public void MarkerMethod(int param1, [Marker]string param2)
            {
            }

            [Marker]
            [return: Marker]
            public virtual void MarkerVirtualMethod(int param1, [Marker]string param2)
            {
            }
        }

        [Test]
        public void AddAdviceAtRuntime()
        {
            TestObject target = new TestObject();
            CountingBeforeAdvice cba = new CountingBeforeAdvice();

            ProxyFactory pf = new ProxyFactory(target);
            ITestObject proxy = (ITestObject)CreateProxy(pf);

            proxy.Age.ToString();
            Assert.AreEqual(0, cba.GetCalls());
            ((IAdvised)proxy).AddAdvice(cba);
            proxy.Age.ToString();
            Assert.AreEqual(1, cba.GetCalls());
        }

        [Test]
        public void SerializationAdviceAndTargetNotSerializable()
        {
            TestObject to = new TestObject();
            Assert.IsFalse(SerializationTestUtils.IsSerializable(to));

            ProxyFactory pf = new ProxyFactory(to);

            pf.AddAdvice(new NopInterceptor());
            ITestObject proxy = (ITestObject)CreateAopProxy(pf).GetProxy();

            Assert.IsFalse(SerializationTestUtils.IsSerializable(proxy));
        }

        [Test]
        public void SerializationAdviceNotSerializable()
        {
            SerializablePerson sp = new SerializablePerson();
            Assert.IsTrue(SerializationTestUtils.IsSerializable(sp));

            ProxyFactory pf = new ProxyFactory(sp);

            // This isn't serializable
            IAdvice i = new NopInterceptor();
            pf.AddAdvice(i);
            Assert.IsFalse(SerializationTestUtils.IsSerializable(i));
            Object proxy = CreateAopProxy(pf).GetProxy();

            Assert.IsFalse(SerializationTestUtils.IsSerializable(proxy));
        }

        [Test]
        public void SerializationSerializableTargetAndAdvice()
        {
            SerializablePerson personTarget = new SerializablePerson();
            personTarget.Name = "jim";
            personTarget.Age = 26;

            Assert.IsTrue(SerializationTestUtils.IsSerializable(personTarget));

            ProxyFactory pf = new ProxyFactory(personTarget);

            CountingThrowsAdvice cta = new CountingThrowsAdvice();

            pf.AddAdvice(new SerializableNopInterceptor());
            // Try various advice types
            pf.AddAdvice(new CountingBeforeAdvice());
            pf.AddAdvice(new CountingAfterReturningAdvice());
            pf.AddAdvice(cta);
            IPerson p = (IPerson)CreateAopProxy(pf).GetProxy();

            p.Echo(null);
            Assert.AreEqual(0, cta.GetCalls());
            try
            {
                p.Echo(new Exception());
            }
            catch (Exception)
            {
            }
            Assert.AreEqual(1, cta.GetCalls());

            // Will throw exception if it fails
            IPerson p2 = (IPerson)SerializationTestUtils.SerializeAndDeserialize(p);
            Assert.AreNotSame(p, p2);
            Assert.AreEqual(p.GetName(), p2.GetName());
            Assert.AreEqual(p.GetAge(), p2.GetAge());
            Assert.IsTrue(AopUtils.IsAopProxy(p2), "Deserialized object is an AOP proxy");

            IAdvised a1 = (IAdvised)p;
            IAdvised a2 = (IAdvised)p2;
            // Check we can manipulate state of p2
            Assert.AreEqual(a1.Advisors.Count, a2.Advisors.Count);

            // This should work as SerializablePerson is equal
            Assert.AreEqual(p, p2, "Proxies should be equal, even after one was serialized");

            // Check we can add a new advisor to the target
            NopInterceptor ni = new NopInterceptor();
            p2.GetAge();
            Assert.AreEqual(0, ni.Count);
            a2.AddAdvice(ni);
            p2.GetAge();
            Assert.AreEqual(1, ni.Count);

            cta = (CountingThrowsAdvice)a2.Advisors[3].Advice;
            p2.Echo(null);
            Assert.AreEqual(1, cta.GetCalls());
            try
            {
                p2.Echo(new Exception());
            }
            catch (Exception)
            {

            }
            Assert.AreEqual(2, cta.GetCalls());
        }

        /// <summary>
        /// Check that the two MethodInvocations necessary are independent
        /// and don't conflict.
        /// </summary>
        [Test]
        public void OneAdvisedObjectCallsAnother()
        {
            int age1 = 33;
            int age2 = 37;

            TestObject target1 = new TestObject();
            ProxyFactory pf1 = new ProxyFactory(target1);
            // Permit proxy and invocation checkers to get context from AopContext
            pf1.ExposeProxy = true;
            NopInterceptor di1 = new NopInterceptor();
            pf1.AddAdvice(0, di1);
            pf1.AddAdvice(1, new ProxyMatcherInterceptor());
            pf1.AddAdvice(2, new MethodInvocationMatcherInterceptor());
            ITestObject advised1 = (ITestObject)pf1.GetProxy();
            advised1.Age = age1; // = 1 invocation

            TestObject target2 = new TestObject();
            ProxyFactory pf2 = new ProxyFactory(target2);
            pf2.ExposeProxy = true;
            NopInterceptor di2 = new NopInterceptor();
            pf2.AddAdvice(0, di2);
            pf2.AddAdvice(1, new ProxyMatcherInterceptor());
            pf2.AddAdvice(2, new MethodInvocationMatcherInterceptor());
            ITestObject advised2 = (ITestObject)CreateProxy(pf2);
            advised2.Age = age2;
            advised1.Spouse = advised2; // = 2 invocations

            Assert.AreEqual(age1, advised1.Age, "Advised one has correct age"); // = 3 invocations
            Assert.AreEqual(age2, advised2.Age, "Advised two has correct age");
            // Means extra call on advised 2
            Assert.AreEqual(age2, advised1.Spouse.Age, "Advised one spouse has correct age"); // = 4 invocations on 1 and another one on 2
            Assert.AreEqual(4, di1.Count, "one was invoked correct number of times");
            // Got hit by call to advised1.getSpouse().Age
            Assert.AreEqual(3, di2.Count, "one was invoked correct number of times");
        }

        private class MethodInvocationMatcherInterceptor : IMethodInterceptor
        {
            public object Invoke(IMethodInvocation invocation)
            {
                MethodInfo m = invocation.Method;
                Object retval = invocation.Proceed();
                Assert.AreEqual(m, invocation.Method, "Method invocation should have same method on way back");
                return retval;
            }
        }

        private class ProxyMatcherInterceptor : IMethodInterceptor
        {
            public object Invoke(IMethodInvocation invocation)
            {
                Object proxy = AopContext.CurrentProxy;
                Object ret = invocation.Proceed();
                Assert.IsTrue(proxy == AopContext.CurrentProxy, "Proxy should be the same on way back");
                return ret;
            }
        }

        [Test]
        public void Reentrance()
        {
            int age1 = 33;

            TestObject target1 = new TestObject();
            ProxyFactory pf1 = new ProxyFactory(target1);
            NopInterceptor di1 = new NopInterceptor();
            pf1.AddAdvice(0, di1);
            ITestObject advised1 = (ITestObject)CreateProxy(pf1);

            advised1.Age = age1; // = 1 invocation
            advised1.Spouse = advised1; // = 2 invocations

            Assert.AreEqual(2, di1.Count, "one was invoked correct number of times");

            Assert.AreEqual(age1, advised1.Age, "Advised one has correct age"); // = 3 invocations
            Assert.AreEqual(3, di1.Count, "one was invoked correct number of times");

            // = 5 invocations, as reentrant call to spouse is advised also
            Assert.AreEqual(age1, advised1.Spouse.Age, "Advised spouse has correct age");

            Assert.AreEqual(5, di1.Count, "one was invoked correct number of times");
        }

        [Test]
        public void TargetCanGetProxy()
        {
            NopInterceptor di = new NopInterceptor();
            INeedsToSeeProxy target = new TargetChecker();
            ProxyFactory proxyFactory = new ProxyFactory(target);
            proxyFactory.ExposeProxy = true;
            Assert.IsTrue(proxyFactory.ExposeProxy);

            proxyFactory.AddAdvice(0, di);
            INeedsToSeeProxy proxied = (INeedsToSeeProxy)CreateProxy(proxyFactory);

            Assert.AreEqual(0, di.Count);
            Assert.AreEqual(0, target.Count);
            proxied.IncrementViaThis();
            Assert.AreEqual(1, target.Count, "Increment happened");

            Assert.AreEqual(1, di.Count, "Only one invocation via AOP as use of this wasn't proxied");
            // 1 invocation
            Assert.AreEqual(1, proxied.Count, "Increment happened");
            proxied.IncrementViaProxy(); // 2 invoocations
            Assert.AreEqual(2, target.Count, "Increment happened");
            Assert.AreEqual(4, di.Count, "3 more invocations via AOP as the first call was reentrant through the proxy");
        }

        /// <summary>
        /// Check that although a method is eligible for advice chain optimization and
        /// direct reflective invocation, it doesn't happen if we've asked to see the proxy,
        /// so as to guarantee a consistent programming model.
        /// </summary>
        [Test]
        public void TargetCanGetProxyEvenIfNoAdviceChain()
        {
            NeedsToSeeProxy target = new NeedsToSeeProxy();
            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(INeedsToSeeProxy) });
            advised.Target = target;
            advised.ExposeProxy = true;

            // Now let's try it with the special target
            IAopProxy aop = CreateAopProxy(advised);
            INeedsToSeeProxy proxied = (INeedsToSeeProxy)aop.GetProxy();

            // It will complain if it can't get the proxy
            proxied.IncrementViaProxy();
        }

        [Test]
        public void TargetCantGetProxyByDefault()
        {
            NeedsToSeeProxy et = new NeedsToSeeProxy();
            ProxyFactory pf1 = new ProxyFactory(et);
            Assert.IsFalse(pf1.ExposeProxy);
            INeedsToSeeProxy proxied = (INeedsToSeeProxy)CreateProxy(pf1);
            Assert.Throws<AopConfigException>(() => proxied.IncrementViaProxy());
        }

        [Test(Description = "Test that the proxy returns itself when the target returns 'this'.")]
        public void TargetReturnsThis()
        {
            // Test return value
            TestObject raw = new OwnSpouse();

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
            advised.Target = raw;

            ITestObject to = (ITestObject)CreateProxy(advised);
            Assert.IsTrue(to.Spouse == to, "this return is wrapped in proxy");
        }

        public class OwnSpouse : TestObject
        {
            public override ITestObject Spouse
            {
                get { return this; }
            }
        }

        [Test]
        public void TargetThrowsException()
        {
            Exception expectedException = new ApplicationException();

            // Test return value
            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
            advised.Target = new TestObject();
            IAopProxy aop = CreateAopProxy(advised);

            try
            {
                ITestObject to = (ITestObject)aop.GetProxy();
                to.Exceptional(expectedException);
                Assert.Fail("Should have thrown exception raised by target");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(expectedException, ex, "exception matches");
            }
        }

        [Test]
        public void InterceptorThrowsException()
        {
            Exception expectedException = new ApplicationException();

            IMethodInterceptor mi = new ThrowExceptionInterceptor(expectedException);

            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
            advised.Target = new TestObject();
            advised.AddAdvice(mi);
            IAopProxy aop = CreateAopProxy(advised);

            try
            {
                ITestObject to = (ITestObject)aop.GetProxy();
                to.Name = "Bruno";
                Assert.Fail("Should have thrown exception raised by interceptor");
            }
            catch (Exception ex)
            {
                Assert.AreEqual(expectedException, ex, "exception matches");
            }
        }

        private class ThrowExceptionInterceptor : IMethodInterceptor
        {
            private Exception _exception;

            public ThrowExceptionInterceptor(Exception ex)
            {
                _exception = ex;
            }

            public object Invoke(IMethodInvocation invocation)
            {
                throw _exception;
            }
        }

        // TODO : Introduction tests
        /*
                [Test(Description = "Test stateful interceptor")]
                public void MixinWithIntroductionAdvisor()
                {
                    TestObject to = new TestObject();
                    AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
                    advised.AddAdvisor(new LockMixinAdvisor());
                    advised.Target = to;

                    CheckTestObjectIntroduction(advised);
                }

                [Test]
                public void MixinWithIntroductionInfo()
                {
                    TestObject to = new TestObject();
                    AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
                    advised.AddAdvice(new LockMixin());
                    advised.Target = to;

                    CheckTestObjectIntroduction(advised);
                }

                private void CheckTestObjectIntroduction(AdvisedSupport advised)
                {
                    int newAge = 65;

                    ITestObject ito = (ITestObject) CreateProxy(advised);
                    ito.Age = newAge;
                    Assert.IsTrue(ito.Age == newAge);

                    ILockable lockable = (ILockable) ito;
                    Assert.IsFalse(lockable.Locked());
                    lockable.DoLock();

                    Assert.IsTrue(ito.Age == newAge);
                    try
                    {
                        ito.Age = 1;
                        Assert.Fail("Setters should fail when locked");
                    }
                    catch (LockedException)
                    {
                        // ok
                    }
                    Assert.IsTrue(ito.Age == newAge);

                    // Unlock
                    Assert.IsTrue(lockable.Locked());
                    lockable.Unlock();
                    ito.Age = 1;
                    Assert.IsTrue(ito.Age == 1);
                }
        */

        [Test]
        public void MultipleProceedCalls()
        {
            TestObject to = new TestObject();
            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
            advised.Target = to;
            advised.AddAdvice(new RetryAdvice());
            NopInterceptor ni = new NopInterceptor();
            advised.AddAdvice(ni);

            ITestObject ito = (ITestObject)CreateProxy(advised);
            ito.Age = 27;

            Assert.AreEqual(2, ni.Count);
        }

        public class RetryAdvice : IMethodInterceptor
        {
            public object Invoke(IMethodInvocation invocation)
            {
                invocation.Proceed();
                return invocation.Proceed();
            }
        }

        [Test]
        public void ReplaceArgument()
        {
            TestObject to = new TestObject();
            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
            advised.Target = to;
            advised.AddAdvisor(new StringSetterNullReplacementAdvisor(new StringSetterNullReplacementAdvice()));

            ITestObject ito = (ITestObject)CreateProxy(advised);
            int newAge = 5;
            ito.Age = newAge;
            Assert.IsTrue(ito.Age == newAge);
            String newName = "greg";
            ito.Name = newName;
            Assert.AreEqual(newName, ito.Name);

            ito.Name = null;

            // Null replacement magic should work
            Assert.IsTrue(ito.Name.Equals(""));
        }

        [Test]
        public void ReplaceArgumentDescendingPropagation()
        {
            TestObject to = new TestObject();
            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
            advised.Target = to;
            advised.AddAdvisor(new StringSetterNullReplacementAdvisor(new StringSetterNullReplacementAdvice()));
            NopInterceptor ni = new NopInterceptor();
            advised.AddAdvisor(new StringSetterNullReplacementAdvisor(ni));

            ITestObject ito = (ITestObject)CreateProxy(advised);
            int newAge = 5;
            ito.Age = newAge;
            Assert.IsTrue(ito.Age == newAge);
            String newName = "greg";
            ito.Name = newName;
            Assert.AreEqual(newName, ito.Name);

            ito.Name = null;

            // Null replacement magic should work
            Assert.IsTrue(ito.Name.Equals(""));

            // StringSetterNullReplacementAdvisor should not be fire twice
            Assert.AreEqual(0, ni.Count);
        }

        [Test]
        public void ReplaceArgumentAscendingPropagation()
        {
            TestObject to = new TestObject();
            AdvisedSupport advised = new AdvisedSupport(new Type[] { typeof(ITestObject) });
            advised.Target = to;
            advised.AddAdvice(new CheckArgumentsAfterProceedAdvice());
            advised.AddAdvisor(new StringSetterNullReplacementAdvisor(new StringSetterNullReplacementAdvice()));

            ITestObject ito = (ITestObject)CreateProxy(advised);
            ito.Name = null;
        }

        /// <summary>
        /// Fires on setter methods that take a string.
        /// </summary>
        public class StringSetterNullReplacementAdvisor : DynamicMethodMatcherPointcutAdvisor
        {
            public StringSetterNullReplacementAdvisor(IAdvice advice)
                : base(advice)
            {
            }

            public override bool Matches(MethodInfo method, Type targetType, object[] args)
            {
                return (args[0] == null);
            }

            public override bool Matches(MethodInfo method, Type targetType)
            {
                return method.Name.StartsWith("set_") &&
                    method.GetParameters().Length == 1 &&
                    method.GetParameters()[0].ParameterType == typeof(string);
            }
        }

        /// <summary>
        /// Replaces null arg with "".
        /// </summary>
        public class StringSetterNullReplacementAdvice : IMethodInterceptor
        {
            public object Invoke(IMethodInvocation invocation)
            {
                // We know it can only be invoked if there's a single parameter of type string
                invocation.Arguments[0] = "";
                return invocation.Proceed();
            }
        }

        public class CheckArgumentsAfterProceedAdvice : IMethodInterceptor
        {
            public object Invoke(IMethodInvocation invocation)
            {
                object returnValue = invocation.Proceed();

                // Null replacement magic should work
                Assert.IsNotNull(invocation.Arguments[0]);

                return returnValue;
            }
        }

        /// <summary>
        /// http://forum.springframework.net/showthread.php?t=504
        /// </summary>
        [Test]
        public void CanCastProxyToIAdvised()
        {
            TestObject to = new TestObject();
            AdvisedSupport advisedSupport = new AdvisedSupport(to);
            NopInterceptor ni = new NopInterceptor();
            advisedSupport.AddAdvice(0, ni);

            ITestObject ito = (ITestObject)CreateProxy(advisedSupport);
            Assert.AreEqual(0, ni.Count);
            ito.Age = 23;
            Assert.AreEqual(23, ito.Age);
            Assert.AreEqual(2, ni.Count);

            IAdvised advised = (IAdvised)ito;
            Assert.AreEqual(1, advised.Advisors.Count, "Have 1 advisor");
            Assert.AreEqual(ni, advised.Advisors[0].Advice);
            NopInterceptor ni2 = new NopInterceptor();
            advised.AddAdvice(1, ni2);
            ito.Name = "Bruno";
            Assert.AreEqual(3, ni.Count);
            Assert.AreEqual(1, ni2.Count);
            // will remove ni
            advised.RemoveAdvisor(0);
            Assert.IsNotNull(ito.Age);
            // Unchanged
            Assert.AreEqual(3, ni.Count);
            Assert.AreEqual(2, ni2.Count);

            CountingBeforeAdvice cba = new CountingBeforeAdvice();
            Assert.AreEqual(0, cba.GetCalls());
            advised.AddAdvice(cba);
            ito.Age = 16;
            Assert.AreEqual(16, ito.Age);
            Assert.AreEqual(2, cba.GetCalls());
        }

        [Test]
        public void CannotAddInterceptorWhenFrozen()
        {
            TestObject target = new TestObject();
            target.Age = 21;
            ProxyFactory pf = new ProxyFactory(target);
            Assert.IsFalse(pf.IsFrozen);
            pf.AddAdvice(new NopInterceptor());
            ITestObject proxied = (ITestObject)CreateProxy(pf);
            pf.IsFrozen = true;
            try
            {
                pf.AddAdvice(0, new NopInterceptor());
                Assert.Fail("Shouldn't be able to add interceptor when frozen");
            }
            catch (AopConfigException ex)
            {
                Assert.IsTrue(ex.Message.IndexOf("frozen") > -1);
            }

            // Check it still works: proxy factory state shouldn't have been corrupted
            Assert.AreEqual(target.Age, proxied.Age);
            Assert.AreEqual(1, ((IAdvised)proxied).Advisors.Count);
        }

        [Test(Description = "Check that casting to Advised can't get around advice freeze.")]
        public void CannotAddAdvisorWhenFrozenUsingCast()
        {
            TestObject target = new TestObject();
            target.Age = 21;
            ProxyFactory pf = new ProxyFactory(target);
            Assert.IsFalse(pf.IsFrozen);
            pf.AddAdvice(new NopInterceptor());
            ITestObject proxied = (ITestObject)CreateProxy(pf);
            pf.IsFrozen = true;
            IAdvised advised = (IAdvised)proxied;

            Assert.IsTrue(pf.IsFrozen);
            try
            {
                advised.AddAdvisor(new DefaultPointcutAdvisor(new NopInterceptor()));
                Assert.Fail("Shouldn't be able to add Advisor when frozen");
            }
            catch (AopConfigException ex)
            {
                Assert.IsTrue(ex.Message.IndexOf("frozen") > -1);
            }

            // Check it still works: proxy factory state shouldn't have been corrupted
            Assert.AreEqual(target.Age, proxied.Age);
            Assert.AreEqual(1, advised.Advisors.Count);
        }

        [Test]
        public void CannotRemoveAdvisorWhenFrozen()
        {
            TestObject target = new TestObject();
            target.Age = 21;
            ProxyFactory pf = new ProxyFactory(target);
            Assert.IsFalse(pf.IsFrozen);
            pf.AddAdvice(new NopInterceptor());
            ITestObject proxied = (ITestObject)CreateProxy(pf);
            pf.IsFrozen = true;
            IAdvised advised = (IAdvised)proxied;

            Assert.IsTrue(pf.IsFrozen);
            try
            {
                advised.RemoveAdvisor(0);
                Assert.Fail("Shouldn't be able to remove Advisor when frozen");
            }
            catch (AopConfigException ex)
            {
                Assert.IsTrue(ex.Message.IndexOf("frozen") > -1);
            }

            // Didn't get removed
            Assert.AreEqual(1, advised.Advisors.Count);
            pf.IsFrozen = false;
            // Can now remove it
            advised.RemoveAdvisor(0);
            // Check it still works: proxy factory state shouldn't have been corrupted
            Assert.AreEqual(target.Age, proxied.Age);
            Assert.AreEqual(0, advised.Advisors.Count);
        }

        [Test(Description = "Check that the string is informative.")]
        public void ProxyConfigString()
        {
            TestObject target = new TestObject();
            ProxyFactory pf = new ProxyFactory(target);
            pf.Interfaces = new Type[] { typeof(ITestObject) };
            pf.AddAdvice(new NopInterceptor());
            IMethodBeforeAdvice mba = new CountingBeforeAdvice();
            IAdvisor advisor = new DefaultPointcutAdvisor(new NameMatchMethodPointcut(), mba);
            pf.AddAdvisor(advisor);
            ITestObject proxied = (ITestObject)CreateProxy(pf);

            String proxyConfigString = ((IAdvised)proxied).ToProxyConfigString();
            Assert.IsTrue(proxyConfigString.IndexOf(advisor.ToString()) != -1);
            Assert.IsTrue(proxyConfigString.IndexOf("1 interface") != -1);
        }

        // TODO : Opaque can be implemented if really usefull (To increase performance)
        /*
                public void testCanPreventCastToAdvisedUsingOpaque()
                {
                    TestObject target = new TestObject();
                    ProxyFactory pf = new ProxyFactory(target);
                    pf.Interfaces = new Type[] { typeof(ITestObject) };
                    pf.AddAdvice(new NopInterceptor());
                    CountingBeforeAdvice mba = new CountingBeforeAdvice();
                    NameMatchMethodPointcut nmmp = new NameMatchMethodPointcut();
                    nmmp.MappedName = "set_Age";
                    IAdvisor advisor = new DefaultPointcutAdvisor(nmmp, mba);
                    pf.AddAdvisor(advisor);
                    Assert.IsFalse(pf.Opaque, "Opaque defaults to false");
                    pf.Opaque = true;
                    Assert.IsTrue(pf.Opaque, "Opaque now true for this config");
                    ITestObject proxied = (ITestObject) CreateProxy(pf);
                    proxied.Age = 10;
                    Assert.AreEqual(10, proxied.Age);
                    Assert.AreEqual(1, mba.GetCalls());

                    Assert.IsFalse(proxied is IAdvised, "Cannot be cast to Advised", );
                }
         */

        // TODO AdviceSupportListeners test
        /*
        [Test]
        public void AdviceSupportListeners()
        {
            TestObject target = new TestObject();
            target.Age = 21;

            ProxyFactory pf = new ProxyFactory(target);
            CountingAdvisorListener l = new CountingAdvisorListener(pc);
            pf.AddListener(l);
            RefreshCountingAdvisorChainFactory acf = new RefreshCountingAdvisorChainFactory();
            // Should be automatically added as a listener
            pf.AdvisorChainFactory(acf);
            Assert.IsFalse(pf.isActive());
            Assert.AreEqual(0, l.activates);
            Assert.AreEqual(0, acf.refreshes);
            ITestObject proxied = (ITestObject)CreateProxy(pf);
            Assert.AreEqual(1, acf.refreshes);
            Assert.AreEqual(1, l.activates);
            Assert.IsTrue(pc.isActive());
            Assert.AreEqual(target.Age, proxied.Age);
            Assert.AreEqual(0, l.adviceChanges);
            NopInterceptor ni = new NopInterceptor();
            pf.addAdvice(0, ni);
            Assert.AreEqual(1, l.adviceChanges);
            Assert.AreEqual(2, acf.refreshes);
            Assert.AreEqual(target.Age, proxied.Age);
            pf.removeAdvice(ni);
            Assert.AreEqual(2, l.adviceChanges);
            Assert.AreEqual(3, acf.refreshes);
            Assert.AreEqual(target.Age, proxied.Age);
            pf.getProxy();
            Assert.AreEqual(1, l.activates);

            pf.RemoveListener(l);
            Assert.AreEqual(2, l.adviceChanges);
            pf.AddAdvisor(new DefaultPointcutAdvisor(new NopInterceptor()));
            // No longer counting
            Assert.AreEqual(2, l.adviceChanges);
        }

        public class CountingAdvisorListener : IAdvisedSupportListener
        {
            public int adviceChanges;
            public int activates;
            private AdvisedSupport expectedSource;

            public CountingAdvisorListener(AdvisedSupport expectedSource)
            {
                this.expectedSource = expectedSource;
            }

            public void AdviceChanged(AdvisedSupport source)
            {
                Assert.AreEqual(expectedSource, source);
                ++adviceChanges;
            }

            public void Activated(AdvisedSupport source)
            {
                Assert.AreEqual(expectedSource,source);
                ++activates;
            }
        }

        public class RefreshCountingAdvisorChainFactory : IAdvisorChainFactory
        {
            public int refreshes;

            public void  AdviceChanged(AdvisedSupport source)
            {
                ++refreshes;
            }

            public IList GetInterceptors(IAdvised advised, object proxy, string methodId, MethodInfo method, Type targetType)
            {
                return AdvisorChainFactoryUtils.CalculateInterceptors(advised, proxy, method, targetType);
            }

            public void  Activated(AdvisedSupport source)
            {
                ++refreshes;
            }
        }
*/

        [Test]
        public void DynamicMethodPointcutThatAlwaysAppliesStatically()
        {
            TestObject to = new TestObject();
            ProxyFactory pf = new ProxyFactory(new Type[] { typeof(ITestObject) });
            TestDynamicPointcutAdvisor dp = new TestDynamicPointcutAdvisor(new NopInterceptor(), "get_Age");
            pf.AddAdvisor(dp);
            pf.Target = to;
            ITestObject it = (ITestObject)CreateProxy(pf);
            Assert.AreEqual(dp.count, 0);
            int age = it.Age;
            Assert.IsNotNull(age);  // avoid mono mcs CS0218
            Assert.AreEqual(dp.count, 1);
            it.Age = 11;
            Assert.AreEqual(it.Age, 11);
            Assert.AreEqual(dp.count, 2);
        }

        [Test]
        public void DynamicMethodPointcutThatAppliesStaticallyOnlyToSetters()
        {
            TestObject to = new TestObject();
            ProxyFactory pf = new ProxyFactory(new Type[] { typeof(ITestObject) });
            // Could apply dynamically to property Age but not to property Name
            ForSettersOnlyPointcutAdvisor dp = new ForSettersOnlyPointcutAdvisor(new NopInterceptor(), "Age");
            pf.AddAdvisor(dp);
            this.mockTargetSource.SetTarget(to);
            pf.TargetSource = mockTargetSource;
            ITestObject it = (ITestObject)CreateProxy(pf);
            Assert.AreEqual(dp.count, 0);
            int age = it.Age;
            Assert.IsNotNull(age); // avoid mono mcs error CS0219
            // Statically vetoed
            Assert.AreEqual(0, dp.count);
            it.Age = 11;
            Assert.AreEqual(it.Age, 11);
            Assert.AreEqual(dp.count, 1);
            // Applies statically but not dynamically
            it.Name = "joe";
            Assert.AreEqual(dp.count, 1);
        }

        private class TestDynamicPointcutAdvisor : DynamicMethodMatcherPointcutAdvisor
        {
            private string pattern;
            public int count = 0;

            public TestDynamicPointcutAdvisor(IAdvice advice, string pattern)
                :
                base(advice)
            {
                this.pattern = pattern;
            }

            public override bool Matches(MethodInfo method, Type targetType, object[] args)
            {
                bool run = method.Name.IndexOf(pattern) != -1;
                if (run) ++count;
                return run;
            }
        }

        private class ForSettersOnlyPointcutAdvisor : TestDynamicPointcutAdvisor
        {
            public ForSettersOnlyPointcutAdvisor(IAdvice advice, string pattern)
                :
                base(advice, pattern)
            {
            }

            public override bool Matches(MethodInfo method, Type targetType)
            {
                return method.Name.StartsWith("set_");
            }
        }

        [Test]
        public void StaticMethodPointcut()
        {
            TestObject to = new TestObject();
            ProxyFactory pf = new ProxyFactory(new Type[] { typeof(ITestObject) });
            NopInterceptor ni = new NopInterceptor();
            TestStaticPointcutAdvisor sp = new TestStaticPointcutAdvisor(ni, "get_Age");
            pf.AddAdvisor(sp);
            pf.Target = to;
            ITestObject it = (ITestObject)CreateProxy(pf);
            Assert.AreEqual(ni.Count, 0);
            int age = it.Age;
            Assert.IsNotNull(age); // avoid mono mcs error CS0219
            Assert.AreEqual(ni.Count, 1);
            it.Age = 11;
            Assert.AreEqual(it.Age, 11);
            Assert.AreEqual(ni.Count, 2);
        }

        private class TestStaticPointcutAdvisor : StaticMethodMatcherPointcutAdvisor
        {
            private string pattern;
            private int count;

            public TestStaticPointcutAdvisor(IAdvice advice, String pattern)
                :
                base(advice)
            {
                this.pattern = pattern;
            }

            public override bool Matches(MethodInfo method, Type targetType)
            {
                bool run = method.Name.IndexOf(pattern) != -1;
                if (run) ++count;
                return run;
            }
        }

        // TODO ? ReflectiveMethodInvocation is not Cloneable
        /*
                [Test(Description="There are times when we want to call proceed() twice.")]
                public void CloneInvocationToProceedThreeTimes()
                {
                    //We can do this if we clone the invocation.

                    TestObject to = new TestObject();
                    ProxyFactory pf = new ProxyFactory(to);
                    pf.AddInterface(typeof(ITestObject));

                    TwoBirthdayAdvice twoBirthdayAdvice = new TwoBirthdayAdvice();

                    TwoBirthdayPointcutAdvisor sp = new TwoBirthdayPointcutAdvisor(twoBirthdayAdvice);
                    pf.AddAdvisor(sp);
                    ITestObject ito = (ITestObject)CreateProxy(pf);

                    int age = 20;
                    ito.Age = age;
                    Assert.AreEqual(age, ito.Age);
                    // Should return the age before the third, AOP-induced birthday
                    Assert.AreEqual(age + 2, ito.haveBirthday());
                    // Return the final age produced by 3 birthdays
                    Assert.AreEqual(age + 3, ito.Age);
                }

                private class TwoBirthdayPointcutAdvisor : StaticMethodMatcherPointcutAdvisor
                {
                    public TwoBirthdayPointcutAdvisor(IAdvice advice)
                        : base(advice)
                    {
                    }

                    public override bool Matches(MethodInfo method, Type targetType)
                    {
                        return "haveBirthday".Equals(method.Name);
                    }
                }

                private class TwoBirthdayAdvice : IMethodInterceptor
                {
                    public object Invoke(IMethodInvocation invocation)
                    {
                        // Clone the invocation to proceed three times
                        // "The Moor's Last Sigh": this technology can cause premature aging
                        IMethodInvocation clone1 = ((ReflectiveMethodInvocation)invocation).InvocableClone();
                        IMethodInvocation clone2 = ((ReflectiveMethodInvocation)invocation).InvocableClone();
                        clone1.Proceed();
                        clone2.Proceed();
                        return invocation.Proceed();
                    }
                }

        //    // We want to change the arguments on a clone: it shouldn't affect the original.
        //    public void testCanChangeArgumentsIndependentlyOnClonedInvocation() throws Throwable
        //    {
        //        TestObject to = new TestObject();
        //        ProxyFactory pc = new ProxyFactory(to);
        //        pc.addInterface(typeof(ITestObject));

        //        // Changes the name, then changes it back.
        //        MethodInterceptor nameReverter = new MethodInterceptor() {
        //            public Object invoke(MethodInvocation mi) throws Throwable {
        //                MethodInvocation clone = ((ReflectiveMethodInvocation) mi).invocableClone();
        //                String oldName = ((ITestObject) mi.getThis()).Name;
        //                clone.getArguments()[0] = oldName;
        //                // Original method invocation should be unaffected by changes to argument list of clone
        //                mi.proceed();
        //                return clone.proceed();
        //            }
        //        };

        //        class NameSaver implements MethodInterceptor {
        //            private List names = new LinkedList();

        //            public Object invoke(MethodInvocation mi) throws Throwable {
        //                names.add(mi.getArguments()[0]);
        //                return mi.proceed();
        //            }
        //        }

        //        NameSaver saver = new NameSaver();

        //        pc.addAdvisor(new DefaultPointcutAdvisor(Pointcuts.SETTERS, nameReverter));
        //        pc.addAdvisor(new DefaultPointcutAdvisor(Pointcuts.SETTERS, saver));
        //        ITestObject it = (ITestObject) createProxy(pc);

        //        String name1 = "tony";
        //        String name2 = "gordon";

        //        to.setName(name1);
        //        Assert.AreEqual(name1, to.Name);

        //        it.setName(name2);
        //        // NameReverter saved it back
        //        Assert.AreEqual(name1, it.Name);
        //        Assert.AreEqual(2, saver.names.size());
        //        Assert.AreEqual(name2, saver.names.get(0));
        //        Assert.AreEqual(name1, saver.names.get(1));
        //    }
        */

        [Test]
        public void OverloadedMethodsWithDifferentAdvice()
        {
            Overloads target = new Overloads();
            ProxyFactory pf = new ProxyFactory(target);
            NopInterceptor overloadVoid = new NopInterceptor();
            pf.AddAdvisor(new OverloadVoidPointcutAdvisor(overloadVoid));
            NopInterceptor overloadInt = new NopInterceptor();
            pf.AddAdvisor(new OverloadIntPointcutAdvisor(overloadInt));

            IOverloads proxy = (IOverloads)CreateProxy(pf);

            Assert.AreEqual(0, overloadInt.Count);
            Assert.AreEqual(0, overloadVoid.Count);
            proxy.Overload();
            Assert.AreEqual(0, overloadInt.Count);
            Assert.AreEqual(1, overloadVoid.Count);
            Assert.AreEqual(25, proxy.Overload(25));
            Assert.AreEqual(1, overloadInt.Count);
            Assert.AreEqual(1, overloadVoid.Count);
            proxy.NoAdvice();
            Assert.AreEqual(1, overloadInt.Count);
            Assert.AreEqual(1, overloadVoid.Count);
        }

        public interface IOverloads
        {
            void Overload();
            int Overload(int i);
            string Overload(string foo);
            void NoAdvice();
        }

        public class Overloads : IOverloads
        {
            public void Overload()
            {
            }

            public int Overload(int i)
            {
                return i;
            }

            public string Overload(string foo)
            {
                return foo;
            }

            public void NoAdvice()
            {
            }
        }

        private class OverloadVoidPointcutAdvisor : StaticMethodMatcherPointcutAdvisor
        {
            public OverloadVoidPointcutAdvisor(IAdvice advice)
                : base(advice)
            {
            }

            public override bool Matches(MethodInfo method, Type targetType)
            {
                return method.Name.Equals("Overload") && method.GetParameters().Length == 0;
            }
        }

        private class OverloadIntPointcutAdvisor : StaticMethodMatcherPointcutAdvisor
        {
            public OverloadIntPointcutAdvisor(IAdvice advice)
                : base(advice)
            {
            }

            public override bool Matches(MethodInfo method, Type targetType)
            {
                return method.Name.Equals("Overload") && method.GetParameters().Length == 1 &&
                        method.GetParameters()[0].ParameterType == typeof(int);
            }
        }

        // TODO : IAdvised.TargetSource is read only (no setter)
        /*
                [Test]
                public void ExistingProxyChangesTarget()
                {
                    TestObject to1 = new TestObject();
                    to1.Age = 33;

                    TestObject to2 = new TestObject();
                    to2.Age = 26;
                    to2.Name = "Juergen";
                    TestObject to3 = new TestObject();
                    to3.Age = 37;
                    ProxyFactory pf = new ProxyFactory(to1);
                    NopInterceptor nop = new NopInterceptor();
                    pf.AddAdvice(nop);
                    ITestObject proxy = (ITestObject)CreateProxy(pf);
                    Assert.AreEqual(nop.Count, 0);
                    Assert.AreEqual(to1.Age, proxy.Age);
                    Assert.AreEqual(nop.Count, 1);
                    // Change to a new static target
                    pf.Target = to2;
                    Assert.AreEqual(to2.Age, proxy.Age);
                    Assert.AreEqual(nop.Count, 2);

                    // Change to a new dynamic target
                    HotSwappableTargetSource hts = new HotSwappableTargetSource(to3);
                    pf.TargetSource = hts;
                    Assert.AreEqual(to3.Age, proxy.Age);
                    Assert.AreEqual(nop.Count, 3);
                    hts.Swap(to1);
                    Assert.AreEqual(to1.Age, proxy.Age);
                    to1.Name = "Colin";
                    Assert.AreEqual(to1.Name, proxy.Name);
                    Assert.AreEqual(nop.Count, 5);

                    // Change back, relying on casting to Advised
                    IAdvised advised = (IAdvised)proxy;
                    Assert.AreSame(hts, advised.TargetSource);
                    SingletonTargetSource sts = new SingletonTargetSource(to2);
                    advised.TargetSource = sts;
                    Assert.AreEqual(to2.Name, proxy.Name);
                    Assert.AreSame(sts, advised.TargetSource);
                    Assert.AreEqual(to2.Age, proxy.Age);
                }

                [Test]
                public void ProxyIsBoundBeforeTargetSourceInvoked()
                {
                    TestObject target = new TestObject();
                    ProxyFactory pf = new ProxyFactory(target);
                    pf.AddAdvice(new DebugInterceptor());
                    pf.ExposeProxy = true;
                    ITestObject proxy = (ITestObject) CreateProxy(pf);
                    IAdvised config = (IAdvised) proxy;
                    // This class just checks proxy is bound before getTarget() call
                    config.setTargetSource(new TargetSource() {
                        public Class getTargetClass() {
                            return TestObject.class;
                        }

                        public boolean isStatic() {
                            return false;
                        }

                        public Object getTarget() throws Exception {
                            Assert.AreEqual(proxy, AopContext.currentProxy());
                            return target;
                        }

                        public void releaseTarget(Object target) throws Exception {
                        }
                    });

                    // Just test anything: it will fail if context wasn't found
                    Assert.AreEqual(0, proxy.Age);
                }
        */

        [Test]
        public void BeforeAdvisorIsInvoked()
        {
            CountingBeforeAdvice cba = new CountingBeforeAdvice();
            IAdvisor matchesNoArgsAdvisor = new NoArgsMethodPointcutAdvisor(cba);
            TestObject target = new TestObject();
            target.Age = 80;
            ProxyFactory pf = new ProxyFactory(target);
            pf.AddAdvice(new NopInterceptor());
            pf.AddAdvisor(matchesNoArgsAdvisor);
            Assert.AreEqual(matchesNoArgsAdvisor, pf.Advisors[1], "Advisor was added");
            ITestObject proxied = (ITestObject)CreateProxy(pf);
            Assert.AreEqual(0, cba.GetCalls());
            Assert.AreEqual(0, cba.GetCalls("get_Age"));
            Assert.AreEqual(target.Age, proxied.Age);
            Assert.AreEqual(1, cba.GetCalls());
            Assert.AreEqual(1, cba.GetCalls("get_Age"));
            Assert.AreEqual(0, cba.GetCalls("set_Age"));
            // Won't be advised
            proxied.Age = 26;
            Assert.AreEqual(1, cba.GetCalls());
            Assert.AreEqual(26, proxied.Age);
        }

        private class NoArgsMethodPointcutAdvisor : StaticMethodMatcherPointcutAdvisor
        {
            public NoArgsMethodPointcutAdvisor(IAdvice advice)
                : base(advice)
            {
            }

            public override bool Matches(MethodInfo method, Type targetType)
            {
                return method.GetParameters().Length == 0;
            }
        }

        // TODO : Multi advice not supported ?

        [Test]
        [Ignore("Multi advice not supported for now.")]
        public void MultiAdvice()
        {
            CountingMultiAdvice cma = new CountingMultiAdvice();
            IAdvisor matchesNoArgsAdvisor = new NoArgsOrNotExceptionalMethodPointcutAdvisor(cma);
            TestObject target = new TestObject();
            target.Age = 80;
            ProxyFactory pf = new ProxyFactory(target);
            pf.AddAdvice(new NopInterceptor());
            pf.AddAdvisor(matchesNoArgsAdvisor);
            Assert.AreEqual(matchesNoArgsAdvisor, pf.Advisors[1], "Advisor was added");
            ITestObject proxied = (ITestObject)CreateProxy(pf);

            Assert.AreEqual(0, cma.GetCalls());
            Assert.AreEqual(0, cma.GetCalls("get_Age"));
            Assert.AreEqual(target.Age, proxied.Age);
            Assert.AreEqual(2, cma.GetCalls());
            Assert.AreEqual(2, cma.GetCalls("get_Age"));
            Assert.AreEqual(0, cma.GetCalls("set_Age"));
            // Won't be advised
            proxied.Age = 26;
            Assert.AreEqual(2, cma.GetCalls());
            Assert.AreEqual(26, proxied.Age);
            Assert.AreEqual(4, cma.GetCalls());
            try
            {
                proxied.Exceptional(new ApplicationException("foo"));
                Assert.Fail("Should have thrown ApplicationException");
            }
            catch (ApplicationException)
            {
                // expected
            }
            Assert.AreEqual(6, cma.GetCalls());
        }

        private class NoArgsOrNotExceptionalMethodPointcutAdvisor : StaticMethodMatcherPointcutAdvisor
        {
            public NoArgsOrNotExceptionalMethodPointcutAdvisor(IAdvice advice)
                : base(advice)
            {
            }

            public override bool Matches(MethodInfo method, Type targetType)
            {
                return method.GetParameters().Length == 0 || method.Name == "Exceptional";
            }
        }

        [Test]
        public void BeforeAdviceThrowsException()
        {
            ApplicationException aex = new ApplicationException();
            CountingBeforeNonSetterAdvice ba = new CountingBeforeNonSetterAdvice(aex);

            TestObject target = new TestObject();
            target.Age = 80;
            NopInterceptor nop1 = new NopInterceptor();
            NopInterceptor nop2 = new NopInterceptor(2);
            ProxyFactory pf = new ProxyFactory(target);
            pf.AddAdvice(nop1);
            pf.AddAdvice(ba);
            pf.AddAdvice(nop2);
            ITestObject proxied = (ITestObject)CreateProxy(pf);
            // Won't throw an exception
            Assert.AreEqual(target.Age, proxied.Age);
            Assert.AreEqual(1, ba.GetCalls());
            Assert.AreEqual(1, ba.GetCalls("get_Age"));
            Assert.AreEqual(1, nop1.Count);
            Assert.AreEqual(1, nop2.Count);
            // Will fail, after invoking Nop1
            try
            {
                proxied.Age = 26;
                Assert.Fail("before advice should have ended chain");
            }
            catch (ApplicationException ex)
            {
                Assert.AreEqual(aex, ex);
            }
            Assert.AreEqual(2, ba.GetCalls());
            Assert.AreEqual(2, nop1.Count);
            // Nop2 didn't get invoked when the exception was thrown
            Assert.AreEqual(1, nop2.Count);
            // Shouldn't have changed value in joinpoint
            Assert.AreEqual(target.Age, proxied.Age);
        }

        private class CountingBeforeNonSetterAdvice : CountingBeforeAdvice
        {
            private Exception _exception;

            public CountingBeforeNonSetterAdvice(Exception ex)
            {
                _exception = ex;
            }

            public override void Before(MethodInfo method, object[] args, object target)
            {
                base.Before(method, args, target);

                if (method.Name.StartsWith("set_"))
                    throw _exception;
            }
        }

        [Test]
        public void AfterReturningAdvisorIsInvoked()
        {
            SummingAfterAdvice aa = new SummingAfterAdvice();
            IAdvisor matchesIntAdvisor = new ReturnsIntPointcutAdvisor(aa);
            TestObject target = new TestObject();
            ProxyFactory pf = new ProxyFactory(target);
            pf.AddAdvice(new NopInterceptor());
            pf.AddAdvisor(matchesIntAdvisor);
            Assert.AreEqual(matchesIntAdvisor, pf.Advisors[1], "Advisor was added");
            ITestObject proxied = (ITestObject)CreateProxy(pf);
            Assert.AreEqual(0, aa.sum);
            int i1 = 12;
            int i2 = 13;

            // Won't be advised
            proxied.Age = i1;
            Assert.AreEqual(i1, proxied.Age);
            Assert.AreEqual(i1, aa.sum);
            proxied.Age = i2;
            Assert.AreEqual(i2, proxied.Age);
            Assert.AreEqual(i1 + i2, aa.sum);
            Assert.AreEqual(i2, proxied.Age);
        }

        private class SummingAfterAdvice : IAfterReturningAdvice
        {
            public int sum;

            public void AfterReturning(Object returnValue, MethodInfo method, Object[] args, Object target)
            {
                sum += ((int)returnValue);
            }
        }

        private class ReturnsIntPointcutAdvisor : StaticMethodMatcherPointcutAdvisor
        {
            public ReturnsIntPointcutAdvisor(IAdvice advice)
                : base(advice)
            {
            }

            public override bool Matches(MethodInfo method, Type targetType)
            {
                return method.ReturnType == typeof(int);
            }
        }

        [Test]
        public void AfterReturningAdvisorIsNotInvokedOnException()
        {
            CountingAfterReturningAdvice car = new CountingAfterReturningAdvice();
            TestObject target = new TestObject();
            ProxyFactory pf = new ProxyFactory(target);
            pf.AddAdvice(new NopInterceptor());
            pf.AddAdvice(car);
            Assert.AreEqual(car, pf.Advisors[1].Advice, "Advice was wrapped in Advisor and added");
            ITestObject proxied = (ITestObject)CreateProxy(pf);
            Assert.AreEqual(0, car.GetCalls());
            int age = 10;
            proxied.Age = age;
            Assert.AreEqual(age, proxied.Age);
            Assert.AreEqual(2, car.GetCalls());
            Exception ex = new Exception();
            // On exception it won't be invoked
            try
            {
                proxied.Exceptional(ex);
                Assert.Fail("Should have thrown Exception");
            }
            catch (Exception caught)
            {
                Assert.AreSame(ex, caught);
            }
            Assert.AreEqual(2, car.GetCalls());
        }

#if !NETCOREAPP
        [Test]
        public void ThrowsAdvisorIsInvoked()
        {
            // Reacts to HttpException and RemotingException
            var th = new Spring.Aop.Framework.Adapter.ThrowsAdviceInterceptorTests.MyThrowsHandler();
            IAdvisor matchesEchoAdvisor = new EchoPointcutAdvisor(th);
            var target = new Spring.Aop.Framework.Adapter.ThrowsAdviceInterceptorTests.Echo();
            target.A = 16;
            ProxyFactory pf = new ProxyFactory(target);
            pf.AddAdvice(new NopInterceptor());
            pf.AddAdvisor(matchesEchoAdvisor);
            Assert.AreEqual(matchesEchoAdvisor, pf.Advisors[1], "Advisor was added");
            var proxied = (Spring.Aop.Framework.Adapter.ThrowsAdviceInterceptorTests.IEcho)CreateProxy(pf);
            Assert.AreEqual(0, th.GetCalls());
            Assert.AreEqual(target.A, proxied.A);
            Assert.AreEqual(0, th.GetCalls());
            Exception ex = new Exception();
            // Will be advised but doesn't match
            try
            {
                proxied.EchoException(1, ex);
                Assert.Fail("Should have thrown Exception");
            }
            catch (Exception caught)
            {
                Assert.AreEqual(ex, caught);
            }

            ex = new System.Web.HttpException();
            try
            {
                proxied.EchoException(1, ex);
                Assert.Fail("Should have thrown HttpException");
            }
            catch (System.Web.HttpException caught)
            {
                Assert.AreEqual(ex, caught);
            }
            Assert.AreEqual(1, th.GetCalls("HttpException"));
        }
#endif

        private class EchoPointcutAdvisor : StaticMethodMatcherPointcutAdvisor
        {
            public EchoPointcutAdvisor(IAdvice advice)
                : base(advice)
            {
            }

            public override bool Matches(MethodInfo method, Type targetType)
            {
                return method.Name.StartsWith("Echo");
            }
        }

#if !NETCOREAPP
        [Test]
        public void AddThrowsAdviceWithoutAdvisor()
        {
            // Reacts to ServletException and RemoteException
            var th = new Spring.Aop.Framework.Adapter.ThrowsAdviceInterceptorTests.MyThrowsHandler();

            var target = new Spring.Aop.Framework.Adapter.ThrowsAdviceInterceptorTests.Echo();
            target.A = 16;
            ProxyFactory pf = new ProxyFactory(target);
            pf.AddAdvice(new NopInterceptor());
            pf.AddAdvice(th);
            var proxied = (Spring.Aop.Framework.Adapter.ThrowsAdviceInterceptorTests.IEcho) CreateProxy(pf);
            Assert.AreEqual(0, th.GetCalls());
            Assert.AreEqual(target.A, proxied.A);
            Assert.AreEqual(0, th.GetCalls());
            Exception ex = new Exception();
            // Will be advised but doesn't match
            try
            {
                proxied.EchoException(1, ex);
                Assert.Fail("Should have thrown Exception");
            }
            catch (Exception caught)
            {
                Assert.AreEqual(ex, caught);
            }

            // Subclass of RemoteException
            ex = new System.Runtime.Remoting.RemotingTimeoutException();
            try
            {
                proxied.EchoException(1, ex);
                Assert.Fail("Should have thrown RemotingTimeoutException");
            }
            catch (System.Runtime.Remoting.RemotingTimeoutException caught)
            {
                Assert.AreEqual(ex, caught);
            }
            Assert.AreEqual(1, th.GetCalls("RemotingException"));
        }
#endif

        [Test]
        public void ArgumentsModification()
        {
        }


        public interface ITargetClass
        {
            string TargetMethod(string arg);
        }

        public class TargetClass : ITargetClass
        {
            public string TargetMethod(string arg)
            {
                if (arg == null)
                {
                    throw new ArgumentException();
                }
                return arg;
            }
        }

        public class Interceptor1 : IMethodInterceptor
        {
            public object Invoke(IMethodInvocation invocation)
            {
                invocation.Proceed();

                return invocation.Arguments[0];
            }
        }

        public class Interceptor2 : IMethodInterceptor
        {
            public object Invoke(IMethodInvocation invocation)
            {
                invocation.Arguments[0] = null;

                return invocation.Proceed();
            }
        }


        public interface INeedsToSeeProxy
        {
            int Count { get; }

            void IncrementViaThis();

            void IncrementViaProxy();

            void Increment();
        }

        public class NeedsToSeeProxy : INeedsToSeeProxy
        {
            private int _count;
            public int Count
            {
                get { return _count; }
            }

            public void IncrementViaThis()
            {
                this.Increment();
            }

            public void IncrementViaProxy()
            {
                INeedsToSeeProxy thisViaProxy = (INeedsToSeeProxy)AopContext.CurrentProxy;
                thisViaProxy.Increment();
                IAdvised advised = (IAdvised)thisViaProxy;
                CheckAdvised(advised);
            }

            protected virtual void CheckAdvised(IAdvised advised)
            {
            }

            public void Increment()
            {
                ++_count;
            }
        }

        public class TargetChecker : NeedsToSeeProxy
        {
            protected override void CheckAdvised(IAdvised advised)
            {
                // TODO replace this check: no longer possible
                //Assert.AreEqual(advised.getTarget(), this);
            }
        }

        public class DynamicTargetSource : ITargetSource
        {
            private object target;
            private Type targetType;

            public DynamicTargetSource(Type targetType, object target)
            {
                this.targetType = targetType;
                this.target = target;
            }

            public object Target
            {
                get { return target; }
                set { target = value; }
            }

            public Type TargetType
            {
                get { return targetType; }
                set { targetType = value; }
            }

            public bool IsStatic
            {
                get { return false; }
            }

            public virtual object GetTarget()
            {
                return target;
            }

            public void ReleaseTarget(object target)
            { }
        }

        public class DynamicInvocationTestInterceptor : IMethodInterceptor
        {
            public bool CallProceed = false;
            public IMethodInvocation LastMethodInvocation;

            public object Invoke(IMethodInvocation invocation)
            {
                LastMethodInvocation = invocation;
                if (CallProceed)
                {
                    return invocation.Proceed();
                }
                return null;
            }
        }
    }
}
