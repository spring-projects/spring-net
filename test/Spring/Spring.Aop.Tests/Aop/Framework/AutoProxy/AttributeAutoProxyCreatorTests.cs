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

using NUnit.Framework;

using Spring.Objects;

#endregion

namespace Spring.Aop.Framework.AutoProxy
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class AttributeAutoProxyCreatorTests
    {
        public interface IEmptyInterface
        {}

        [AttributeUsage( AttributeTargets.Class|AttributeTargets.Method, Inherited=false )]
        private class ApcTestAttribute : Attribute {}

        private class ApcTestObject: IEmptyInterface {}

        [ApcTest]
        private class AttributedApcTestObject : ApcTestObject
        {}

        private class DerivedAttributedApcTestObject : AttributedApcTestObject
        {
            [ApcTest]
            public void SomeMethod() {}
        }

        [Test]
        public void ThrowsOnMissingAttributeTypeList()
        {
            AttributeAutoProxyCreator apc = new AttributeAutoProxyCreator();
            Assert.Throws<ArgumentNullException>(() => apc.PostProcessAfterInitialization(new ApcTestObject(), "testObject"));
        }

        [Test]
        public void ThrowsOnAssigningNullAttributeList()
        {
            AttributeAutoProxyCreator apc = new AttributeAutoProxyCreator();
            Assert.Throws<ArgumentNullException>(() => apc.AttributeTypes = null);
        }

        [Test]
        public void AllowsEmptyAttributeList()
        {
            AttributeAutoProxyCreator apc = new AttributeAutoProxyCreator();
            apc.AttributeTypes = new Type[0];     
            apc.PostProcessAfterInitialization( new ApcTestObject(), "testObject" );
        }

        [Test]
        public void DefaultsToNotCheckInherited()
        {
            AttributeAutoProxyCreator apc = new AttributeAutoProxyCreator();
            Assert.IsFalse(apc.CheckInherited);            
        }

        [Test]
        public void CreatesProxyOnAttributeMatch()
        {
            AttributeAutoProxyCreator apc = new AttributeAutoProxyCreator();
            apc.AttributeTypes = new Type[] { typeof(ApcTestAttribute) };
            
            object result = apc.PostProcessAfterInitialization( new AttributedApcTestObject(), "testObject" );
            Assert.IsTrue( AopUtils.IsAopProxy( result ) );
        }

        [Test]
        public void CreatesProxyOnInheritedAttributeMatchWhenCheckInherited()
        {
            AttributeAutoProxyCreator apc = new AttributeAutoProxyCreator();
            apc.AttributeTypes = new Type[] { typeof(ApcTestAttribute) };
            apc.CheckInherited = true;            
            object result = apc.PostProcessAfterInitialization( new DerivedAttributedApcTestObject(), "testObject" );
            Assert.IsTrue( AopUtils.IsAopProxy( result ) );
        }

        [Test]
        public void DoesNotCreateProxyOnInheritedAttributeMatchWhenNotCheckInherited()
        {
            AttributeAutoProxyCreator apc = new AttributeAutoProxyCreator();
            apc.AttributeTypes = new Type[] { typeof(ApcTestAttribute) };
            apc.CheckInherited = false;            
            object result = apc.PostProcessAfterInitialization( new DerivedAttributedApcTestObject(), "testObject" );
            Assert.IsFalse( AopUtils.IsAopProxy( result ) );
        }

        [Test]
        public void DoesNotCreateProxyIfNoAttributeMatch()
        {
            AttributeAutoProxyCreator apc = new AttributeAutoProxyCreator();
            apc.AttributeTypes = new Type[] { typeof(ApcTestAttribute) };
            
            object result = apc.PostProcessAfterInitialization( new TestObject(), "testObject" );
            Assert.IsFalse( AopUtils.IsAopProxy( result ) );
        }

        [Test]
        public void DoesNotCheckMethodLevelAttributes()
        {
            AttributeAutoProxyCreator apc = new AttributeAutoProxyCreator();
            apc.AttributeTypes = new Type[] { typeof(ApcTestAttribute) };
            apc.CheckInherited = false; // (!)

            // does not check method level attributes!
            object result = apc.PostProcessAfterInitialization( new DerivedAttributedApcTestObject(), "testObject" );
            Assert.IsFalse( AopUtils.IsAopProxy( result ) );
        }

        [Test]
        public void DoesNotCreateProxyIfEmptyAtributeList()
        {
            AttributeAutoProxyCreator apc = new AttributeAutoProxyCreator();
            apc.AttributeTypes = new Type[0];
            
            object result = apc.PostProcessAfterInitialization( new ApcTestObject(), "testObject" );
            Assert.IsFalse( AopUtils.IsAopProxy( result ) );
        }
    }
}