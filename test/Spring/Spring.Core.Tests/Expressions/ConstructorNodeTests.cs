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

using NUnit.Framework;

#endregion

namespace Spring.Expressions
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Erich Eichinger</author>
    [TestFixture]
    public class ConstructorNodeTests
    {
        #region Test Classes

        public class PublicTestClass
        {
            public string _s;
            public int _i;

            public PublicTestClass(string s)
                :this(s, -1)
            {}

            protected PublicTestClass(string s, int i)
            {
                _s = s; _i=i;
            }
        }

        private class PrivateTestClass : PublicTestClass
        {
            private PrivateTestClass(string s, int i)
                :base(s,i)
            {
            }
        }

        #endregion Test Classes

        [Test]
        public void CanCreatePublicInstance()
        {
            ConstructorNode ctorNode = new ConstructorNode(typeof(PublicTestClass));
            StringLiteralNode sNode = new StringLiteralNode("theValue");
            ctorNode.AddArgument(sNode);

            PublicTestClass instance = (PublicTestClass) ((IExpression)ctorNode).GetValue();
            Assert.AreEqual( sNode.Text, instance._s );
            Assert.AreEqual( -1, instance._i );
        }

        [Test]
        public void CanCreatePublicInstanceWithNonPublicConstructor()
        {
            ConstructorNode ctorNode = new ConstructorNode();
            ctorNode.Text=typeof(PublicTestClass).FullName;
            StringLiteralNode sNode = new StringLiteralNode();
            sNode.Text = "theValue2";
            ctorNode.addChild(sNode);
            IntLiteralNode iNode = new IntLiteralNode();
            iNode.Text="2";
            ctorNode.addChild(iNode);

            PublicTestClass instance = (PublicTestClass) ((IExpression)ctorNode).GetValue();
            Assert.AreEqual( sNode.Text, instance._s );
            Assert.AreEqual( 2, instance._i );
        }

        [Test]
        public void CanCreateNonPublicInstanceWithNonPublicConstructor()
        {
            ConstructorNode ctorNode = new ConstructorNode();
            ctorNode.Text=typeof(PrivateTestClass).FullName;
            StringLiteralNode sNode = new StringLiteralNode();
            sNode.Text = "theValue3";
            ctorNode.addChild(sNode);
            IntLiteralNode iNode = new IntLiteralNode();
            iNode.Text="3";
            ctorNode.addChild(iNode);

            PublicTestClass instance = (PublicTestClass) ((IExpression)ctorNode).GetValue();
            Assert.AreEqual( sNode.Text, instance._s );
            Assert.AreEqual( 3, instance._i );
        }
    }
}