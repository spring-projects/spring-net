#region License

/*
 * Copyright © 2002-2010 the original author or authors.
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

using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;

#endregion

namespace Spring.Transaction.Interceptor
{
    /// <summary>
    /// This class contains tests for the various
    /// <see cref="ITransactionAttributeSource"/> implementations.
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class TransactionAttributeSourceTests
    {
        [Test]
        public void MethodMapTransactionAttributeSource()
        {
            MethodMapTransactionAttributeSource tas = new MethodMapTransactionAttributeSource();
            IDictionary methodMap = new Hashtable();
            methodMap.Add("System.Object.GetHashCode, mscorlib", "PROPAGATION_REQUIRED");
            methodMap.Add("System.Object.ToString, mscorlib",
                          new DefaultTransactionAttribute(TransactionPropagation.Supports));
            tas.MethodMap = methodMap;

            ITransactionAttribute ta = tas.ReturnTransactionAttribute(typeof(object).GetMethod("GetHashCode"), null);
            Assert.IsNotNull(ta);
            Assert.AreEqual(TransactionPropagation.Required, ta.PropagationBehavior);

            ta = tas.ReturnTransactionAttribute(typeof(object).GetMethod("ToString"), null);
            Assert.IsNotNull(ta);
            Assert.AreEqual(TransactionPropagation.Supports, ta.PropagationBehavior);
        }


        /// <summary>
        /// Test that configuration of the IDictionary baesd map supports transaction attribute information
        /// set via object (i.e. DefaultTransactionAttribute) as well as strings.
        /// </summary>
        [Test]
        public void NameMatchTransactionAttributeSource()
        {
            NameMatchTransactionAttributeSource tas = new NameMatchTransactionAttributeSource();
            IDictionary methodMap = new Hashtable();                     
            methodMap.Add("GetHashCode", "PROPAGATION_REQUIRED");
            methodMap.Add("ToString", new DefaultTransactionAttribute(TransactionPropagation.Supports));
            tas.NameMap = methodMap;            
            ITransactionAttribute ta = tas.ReturnTransactionAttribute(typeof (object).GetMethod("GetHashCode"), null);
            Assert.IsNotNull(ta);
            Assert.AreEqual(TransactionPropagation.Required, ta.PropagationBehavior);
            ta = tas.ReturnTransactionAttribute(typeof (object).GetMethod("ToString"), null);
            Assert.IsNotNull(ta);
            Assert.AreEqual(TransactionPropagation.Supports, ta.PropagationBehavior);
        }

        [Test]
        public void NameMatchTransactionAttributeSourceWithStarAtStartOfMethodName()
        {
            NameMatchTransactionAttributeSource tas = new NameMatchTransactionAttributeSource();
            NameValueCollection attributes = new NameValueCollection();
            attributes.Add("*ashCode", "PROPAGATION_REQUIRED");
            tas.NameProperties = attributes;
            ITransactionAttribute ta = tas.ReturnTransactionAttribute(typeof (object).GetMethod("GetHashCode"), null);
            Assert.IsNotNull(ta);
            Assert.AreEqual(TransactionPropagation.Required, ta.PropagationBehavior);
        }

        [Test]
        public void NameMatchTransactionAttributeSourceWithStarAtEndOfMethodName()
        {
            NameMatchTransactionAttributeSource tas = new NameMatchTransactionAttributeSource();
            NameValueCollection attributes = new NameValueCollection();
            attributes.Add("GetHashCod*", "PROPAGATION_REQUIRED");
            tas.NameProperties = attributes;
            ITransactionAttribute ta = tas.ReturnTransactionAttribute(typeof(object).GetMethod("GetHashCode"), null);
            Assert.IsNotNull(ta);
            Assert.AreEqual(TransactionPropagation.Required, ta.PropagationBehavior);
        }

        [Test]
        public void NameMatchTransactionAttributeSourceMostSpecificMethodNameIsDefinitelyMatched()
        {
            NameMatchTransactionAttributeSource tas = new NameMatchTransactionAttributeSource();
            NameValueCollection attributes = new NameValueCollection();
            attributes.Add("*", "PROPAGATION_REQUIRED");
            attributes.Add("GetHashCode", "PROPAGATION_MANDATORY");
            tas.NameProperties = attributes;
            ITransactionAttribute ta = tas.ReturnTransactionAttribute(typeof(object).GetMethod("GetHashCode"), null);
            Assert.IsNotNull(ta);
            Assert.AreEqual(TransactionPropagation.Mandatory, ta.PropagationBehavior);
        }

        [Test]
        public void NameMatchTransactionAttributeSourceWithEmptyMethodName()
        {
            NameMatchTransactionAttributeSource tas = new NameMatchTransactionAttributeSource();
            NameValueCollection attributes = new NameValueCollection();
            attributes.Add("", "PROPAGATION_MANDATORY");
            tas.NameProperties = attributes;
            ITransactionAttribute ta = tas.ReturnTransactionAttribute(typeof(object).GetMethod("GetHashCode"), null);
            Assert.IsNull(ta);            
        }
    }
}