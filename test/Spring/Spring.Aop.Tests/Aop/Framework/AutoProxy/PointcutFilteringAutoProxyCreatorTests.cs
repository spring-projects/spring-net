#region License

/*
 * Copyright © 2002-2008 the original author or authors.
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
using NUnit.Framework;
using Spring.Aop.Support;
using Spring.Objects;

#endregion

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class PointcutFilteringAutoProxyCreatorTests
    {
        [Test]
        public void CreatesProxyOnlyIfPointcutAndObjectNameMatch()
        {
            // is match
            PointcutFilteringAutoProxyCreator apc = new PointcutFilteringAutoProxyCreator();
            apc.ObjectNames = new string[] { "test*" } ;;
            apc.Pointcut = new SdkRegularExpressionMethodPointcut(".*\\.GetHashCode");
            object result = apc.PostProcessAfterInitialization( new TestObject(), "testObject" );
            Assert.IsTrue(AopUtils.IsAopProxy(result));

            apc = new PointcutFilteringAutoProxyCreator();
            apc.ObjectNames = new string[] { "test*" } ;;
            apc.Pointcut = new SdkRegularExpressionMethodPointcut(".*\\.GetHashCODE");
            result = apc.PostProcessAfterInitialization( new TestObject(), "testObject" );
            Assert.IsFalse(AopUtils.IsAopProxy(result));

            apc = new PointcutFilteringAutoProxyCreator();
            apc.ObjectNames = new string[] { "tesT*" } ;;
            apc.Pointcut = new SdkRegularExpressionMethodPointcut(".*\\.GetHashCode");
            result = apc.PostProcessAfterInitialization( new TestObject(), "testObject" );
            Assert.IsFalse(AopUtils.IsAopProxy(result));
        }

        [Test]
        public void CreatesProxyOnPointcutMatch()
        {
            PointcutFilteringAutoProxyCreator apc = new PointcutFilteringAutoProxyCreator();
            apc.ObjectNames = null;
            apc.Pointcut = new SdkRegularExpressionMethodPointcut(".*\\.GetHashCode");
            object result = apc.PostProcessAfterInitialization( new TestObject(), "testObject" );
            Assert.IsTrue(AopUtils.IsAopProxy(result));
        }

        [Test]
        public void CreatesProxyOnNameMatch()
        {
            PointcutFilteringAutoProxyCreator apc = new PointcutFilteringAutoProxyCreator();
            apc.ObjectNames = new string[] { "test*" } ;
            apc.Pointcut = null;
            object result = apc.PostProcessAfterInitialization( new TestObject(), "testObject" );
            Assert.IsTrue(AopUtils.IsAopProxy(result));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentExceptionIfNoCriteriaSpecified()
        {
            PointcutFilteringAutoProxyCreator apc = new PointcutFilteringAutoProxyCreator();
            apc.ObjectNames = new string[] {} ;
            apc.Pointcut = null;
            object result = apc.PostProcessAfterInitialization( new TestObject(), "testObject" );
            Assert.IsTrue(AopUtils.IsAopProxy(result));
        }
    }
}