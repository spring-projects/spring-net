#region License

/*
 * Copyright 2004 the original author or authors.
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using NUnit.Framework;
using Spring.Objects;

#endregion

namespace Spring.Util
{
    /// <summary>
    /// Unit tests for the AssertUtils class.
    /// </summary>
    /// <author>Rick Evans</author>
    [TestFixture]
    public sealed class AssertUtilsTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "foo")]
        public void IsTrueWithMesssage()
        {
            AssertUtils.IsTrue(false, "foo");
        }

        [Test]
        public void IsTrueWithMessageValidExpression()
        {
            AssertUtils.IsTrue(true, "foo");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "[Assertion failed] - this expression must be true")]
        public void IsTrue()
        {
            AssertUtils.IsTrue(false);
        }

        [Test]
        public void IsTrueValidExpression()
        {
            AssertUtils.IsTrue(true);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StateTrue()
        {
            AssertUtils.State(false, "foo");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNotNull()
        {
            AssertUtils.ArgumentNotNull(null, "foo");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentNotNullWithMessage()
        {
            AssertUtils.ArgumentNotNull(null, "foo", "Bang!");
        }

        [Test]
        public void ArgumentHasTextWithValidText()
        {
            AssertUtils.ArgumentHasText("... and no-one's getting fat 'cept Mama Cas!", "foo");
        }

        [Test]
        public void ArgumentHasTextWithValidTextAndMessage()
        {
            AssertUtils.ArgumentHasText("... and no-one's getting fat 'cept Mama Cas!", "foo", "Bang!");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentHasText()
        {
            AssertUtils.ArgumentHasText(null, "foo");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentHasTextWithMessage()
        {
            AssertUtils.ArgumentHasText(null, "foo", "Bang!");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentHasLengthArgumentIsNull()
        {
            AssertUtils.ArgumentHasLength(null, "foo");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentHasLengthArgumentIsNullWithMessage()
        {
            AssertUtils.ArgumentHasLength(null, "foo", "Bang!");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentHasLengthArgumentIsEmpty()
        {
            AssertUtils.ArgumentHasLength(new byte[0], "foo");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ArgumentHasLengthArgumentIsEmptyWithMessage()
        {
            AssertUtils.ArgumentHasLength(new byte[0], "foo", "Bang!");
        }

        [Test]
        public void ArgumentHasLengthArgumentHasElements()
        {
            AssertUtils.ArgumentHasLength(new byte[1], "foo");
        }

        [Test]
        public void ArgumentHasLengthArgumentHasElementsWithMessage()
        {
            AssertUtils.ArgumentHasLength(new byte[1], "foo", "Bang!");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentHasElementsArgumentIsNull()
        {
            AssertUtils.ArgumentHasElements(null, "foo");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentHasElementsArgumentIsEmpty()
        {
            AssertUtils.ArgumentHasElements(new object[0], "foo");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ArgumentHasElementsArgumentContainsNull()
        {
            AssertUtils.ArgumentHasElements(new object[] { new object(), null, new object() }, "foo");
        }

        [Test]
        public void ArgumentHasElementsArgumentContainsNonNullsOnly()
        {
            AssertUtils.ArgumentHasElements(new object[] { new object(), new object(), new object() }, "foo");
        }

        [Test]
        public void UnderstandsType()
        {
            MethodInfo getDescriptionMethod = typeof(ITestObject).GetMethod("GetDescription", new Type[0]);
            MethodInfo understandsMethod = typeof(AssertUtils).GetMethod("Understands", BindingFlags.Public|BindingFlags.Static, null, new Type[] {typeof (object), typeof(string), typeof (MethodBase)}, null);

            // null target, any type
            AssertNotUnderstandsType(null, "target", typeof(object), typeof(NotSupportedException), "Target 'target' is null.");
            // any target, null type
            AssertNotUnderstandsType(new object(), "target", null, typeof(ArgumentNullException), "Argument 'requiredType' cannot be null.");
        }

        [Test]
        public void UnderstandsMethod()
        {
            MethodInfo getDescriptionMethod = typeof(ITestObject).GetMethod("GetDescription", new Type[0]);
            MethodInfo understandsMethod = typeof(AssertUtils).GetMethod("Understands", BindingFlags.Public|BindingFlags.Static, null, new Type[] {typeof (object), typeof(string), typeof (MethodBase)}, null);

            // null target, static method
            AssertUtils.Understands(null, "target", understandsMethod);
            // null target, instance method
            AssertNotUnderstandsMethod(null, "target", getDescriptionMethod, typeof(NotSupportedException), "Target 'target' is null and target method 'Spring.Objects.ITestObject.GetDescription' is not static.");
            // compatible target, instance method
            AssertUtils.Understands(new TestObject(), "target", getDescriptionMethod);
            // incompatible target, instance method
                AssertNotUnderstandsMethod(new object(), "target", getDescriptionMethod, typeof(NotSupportedException), "Target 'target' of type 'System.Object' does not support methods of 'Spring.Objects.ITestObject'.");
            // compatible transparent proxy, instance method
            object compatibleProxy = new TestProxy(new TestObject()).GetTransparentProxy();
            AssertUtils.Understands(compatibleProxy, "compatibleProxy", getDescriptionMethod);
            // incompatible transparent proxy, instance method
            object incompatibleProxy = new TestProxy(new object()).GetTransparentProxy();
            AssertNotUnderstandsMethod(incompatibleProxy, "incompatibleProxy", getDescriptionMethod, typeof(NotSupportedException), "Target 'incompatibleProxy' is a transparent proxy that does not support methods of 'Spring.Objects.ITestObject'.");
        }

        private void AssertNotUnderstandsType(object target, string targetName, Type requiredType, Type exceptionType, string partialMessage)
        {
            try
            {
                AssertUtils.Understands(target, targetName, requiredType);
                Assert.Fail();
            }
            catch(Exception ex)
            {
                if (ex.GetType() != exceptionType)
                {
                    Assert.Fail("Expected Exception of type {0}, but was {1}", exceptionType, ex.GetType());
                }

                if (-1 == ex.Message.IndexOf(partialMessage))
                {
                    Assert.Fail("Expected Message '{0}', but got '{1}'", partialMessage, ex.Message);
                }
            }
        }

        private void AssertNotUnderstandsMethod(object target, string targetName, MethodBase method, Type exceptionType, string partialMessage)
        {
            try
            {
                AssertUtils.Understands(target, targetName, method);
                Assert.Fail();
            }
            catch(Exception ex)
            {
                if (ex.GetType() != exceptionType)
                {
                    Assert.Fail("Expected Exception of type {0}, but was {1}", exceptionType, ex.GetType());
                }

                if (-1 == ex.Message.IndexOf(partialMessage))
                {
                    Assert.Fail("Expected Message '{0}', but got '{1}'", partialMessage, ex.Message);
                }
            }
        }

        private class TestProxy : RealProxy, IRemotingTypeInfo
        {
            private readonly object targetInstance;

            public TestProxy(object targetInstance) 
                : base(typeof(MarshalByRefObject))
            {
                this.targetInstance = targetInstance;
            }

            public override IMessage Invoke(IMessage msg)
            {
                //                    return new ReturnMessage(result, null, 0, null, callMsg);
                throw new NotSupportedException();
            }

            public bool CanCastTo(Type fromType, object o)
            {
                bool res = fromType.IsAssignableFrom(targetInstance.GetType());
                return res;
            }

            public string TypeName
            {
                get { return targetInstance.GetType().AssemblyQualifiedName; }
                set { throw new System.NotSupportedException(); }
            }
        }
    }
}
