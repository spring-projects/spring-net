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

using System;
using NUnit.Framework;
using Spring.Objects;

namespace Spring.Expressions
{
    /// <summary>
    /// Tests the behavior of PropertyOrFieldNode expression node
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class PropertyOrFieldNodeTests
    {
        private class BaseClass
        {
            private ITestObject objectProp;

            public string StringProp { get { return "BaseStringProp"; }}
            public ITestObject ObjectProp { get { return objectProp; } set { objectProp = value; } }
        }

        private class DerivedClass : BaseClass
        {
            public new DateTime StringProp { get { return new DateTime(2008,1,1); }}
        }


        [Test]
        public void UseMostSpecificOverride()
        {
            PropertyOrFieldNode pofNode = new PropertyOrFieldNode();
            pofNode.Text = "StringProp";

            Assert.AreEqual(new DateTime(2008,1,1), ((IExpression) pofNode).GetValue(new DerivedClass()));
        }

#if !NETCOREAPP
        [Test]
        public void CanSetTransparentProxy()
        {
            PropertyOrFieldNode pofNode = new PropertyOrFieldNode();
            pofNode.Text = "ObjectProp";

            BaseClass ouc = new BaseClass();
            TestTransparentProxyFactory tpf = new TestTransparentProxyFactory(null, typeof(ITestObject), null);
            object tpo = tpf.GetTransparentProxy();
            Assert.IsTrue( tpo is ITestObject );
            ITestObject itpo = tpo as ITestObject;
            Assert.IsNotNull(itpo);
            pofNode.SetValue( ouc, null, itpo);
            Assert.AreSame( tpo, ouc.ObjectProp );
        }
#endif
    }
}