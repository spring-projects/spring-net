#region License

/*
 * Copyright 2002-2010 the original author or authors.
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
using System.Data;
using System.Text;
using NUnit.Framework;

namespace Spring.Transaction.Interceptor
{
    [TestFixture]
    public class RollbackRuleAttributeTests
    {
        [Test]
        public void FoundImmediatelyWithString()
        {
            RollbackRuleAttribute rr = new RollbackRuleAttribute("System.Exception");
            Assert.IsTrue(rr.GetDepth(typeof (Exception)) == 0);
        }

        [Test]
        public void FoundImmediatelyWithClass()
        {
            RollbackRuleAttribute rr = new RollbackRuleAttribute(typeof (Exception));
            Assert.IsTrue(rr.GetDepth(new Exception()) == 0);
        }

        [Test]
        public void FoundImmediatelyWithType()
        {
            RollbackRuleAttribute rr = new RollbackRuleAttribute(typeof (Exception));
            Assert.IsTrue(rr.GetDepth(typeof (Exception)) == 0);
        }

        [Test]
        public void NotFound()
        {
            RollbackRuleAttribute rr = new RollbackRuleAttribute("System.Data.DataException");
            Assert.IsTrue(rr.GetDepth(typeof (ApplicationException)) == -1);
        }

        [Test]
        public void Ancestry()
        {
            RollbackRuleAttribute rr = new RollbackRuleAttribute("System.Exception");
            Assert.IsTrue(rr.GetDepth(typeof (DataException)) == 2);
        }

        [Test]
        public void AlwaysTrue()
        {
            RollbackRuleAttribute rr = new RollbackRuleAttribute("System.Exception");
            Assert.IsTrue(rr.GetDepth(typeof (SystemException)) > 0);
            Assert.IsTrue(rr.GetDepth(typeof (ApplicationException)) > 0);
            Assert.IsTrue(rr.GetDepth(typeof (DataException)) > 0);
            Assert.IsTrue(rr.GetDepth(typeof (TransactionSystemException)) > 0);
        }

        [Test]
        public void ConstructorArgMustBeAExceptionClass()
        {
            Assert.Throws<ArgumentException>(() => new RollbackRuleAttribute(typeof(StringBuilder)));
        }

        [Test]
        public void ConstructorArgMustBeAExceptionClassWithNullThrowableType()
        {
            Assert.Throws<ArgumentNullException>(() => new RollbackRuleAttribute((Type) null));
        }

        [Test]
        public void ConstructorArgExceptionStringNameVersionWithNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RollbackRuleAttribute((String) null));
        }
    }
}