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

using NUnit.Framework;
using Spring.Aop.Framework;

#endregion

namespace Spring.Transaction.Interceptor
{
    /// <summary>
    /// This class contains mock objects tests the TransactionInterceptor 
    /// </summary>
    /// <author>Mark Pollack</author>
    [TestFixture]
    public class TransactionInterceptorTests : AbstractTransactionAspectTests
    {
       

        protected override object Advised(object target, IPlatformTransactionManager ptm,
                                          ITransactionAttributeSource tas)
        {
            TransactionInterceptor ti = new TransactionInterceptor();
            ti.TransactionManager = ptm;
            Assert.AreEqual(ptm, ti.TransactionManager);
            ti.TransactionAttributeSource = tas;
            Assert.AreEqual(tas, ti.TransactionAttributeSource);
            ProxyFactory pf = new ProxyFactory(target);
            pf.AddAdvice(0, ti);
            return pf.GetProxy();
        }
    }
}