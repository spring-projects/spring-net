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

using Spring.Aop;
using Spring.Aop.Support;

namespace Spring.Transaction.Interceptor
{
    public class ObjectFactoryTransactionAttributeSourceAdvisor : AbstractObjectFactoryPointcutAdvisor
    {

        private ITransactionAttributeSource _transactionAttributeSource;
        private IPointcut _pointcut;

        public ObjectFactoryTransactionAttributeSourceAdvisor()
        {
            _pointcut = new TransactonAttributeSourcePointcut(this);
        }


        private class TransactonAttributeSourcePointcut : AbstractTransactionAttributeSourcePointcut
        {
            private ObjectFactoryTransactionAttributeSourceAdvisor outer;
            public TransactonAttributeSourcePointcut(ObjectFactoryTransactionAttributeSourceAdvisor outer)
            {
                this.outer = outer;
            }

            #region Overrides of AbstractTransactionAttributeSourcePointcut

            protected override ITransactionAttributeSource TransactionAttributeSource
            {
                get { return outer._transactionAttributeSource; }
            }

            #endregion
        }

        public ITransactionAttributeSource TransactionAttributeSource
        {
            set { _transactionAttributeSource = value; }
        }

        #region Overrides of AbstractPointcutAdvisor

        /// <summary>
        /// The <see cref="Spring.Aop.IPointcut"/> that drives this advisor.
        /// </summary>
        public override IPointcut Pointcut
        {
            get { return _pointcut; }
            set { _pointcut = value; }
        }

        #endregion
    }
}