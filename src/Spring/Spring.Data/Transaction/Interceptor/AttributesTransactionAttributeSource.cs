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

using System.Reflection;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// Implementation of <see cref="ITransactionAttributeSource"/> that uses
	/// <see cref="System.Attribute"/>s.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	/// <author>Mark Pollack (.NET)</author>
	public class AttributesTransactionAttributeSource : AbstractFallbackTransactionAttributeSource
	{

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributesTransactionAttributeSource"/> class.
        /// </summary>
		public AttributesTransactionAttributeSource( )
		{
		}

		/// <summary>
		/// <see cref="Spring.Transaction.Interceptor.AbstractFallbackTransactionAttributeSource.FindAllAttributes(MethodInfo)"/>
		/// implementation.
		/// </summary>
		/// <remarks>
		/// We need all because of the need to analyze rollback rules.
		/// </remarks>
		/// <param name="method">The method to retrieve attributes for.</param>
		/// <returns>The transactional attributes associated with the method.</returns>
		protected override Attribute[] FindAllAttributes( MethodInfo method )
		{
			return Attribute.GetCustomAttributes(  method );
		}

		/// <summary>
		/// <see cref="Spring.Transaction.Interceptor.AbstractFallbackTransactionAttributeSource.FindAllAttributes(Type)"/>
		/// implementation.
		/// </summary>
		/// <param name="targetType">
		/// The <see cref="System.Type"/> to retrieve attributes for.
		/// </param>
		/// <returns>
		/// All attributes associated with the supplied <paramref name="targetType"/>.
		/// </returns>
		protected override Attribute[] FindAllAttributes( Type targetType )
		{
			return Attribute.GetCustomAttributes( targetType );
		}

        protected override ITransactionAttribute FindTransactionAttribute(Attribute[] attributes)
        {
            if (attributes == null)
            {
                return null;
            }
            foreach (Attribute attr in attributes)
            {
                if (attr is TransactionAttribute)
                {
                    TransactionAttribute ta = (TransactionAttribute)attr;
                    RuleBasedTransactionAttribute rbta =
                        new RuleBasedTransactionAttribute();

                    //TODO another reminder to sync property names
                    rbta.PropagationBehavior = ta.TransactionPropagation;
                    rbta.TransactionIsolationLevel = ta.IsolationLevel;
                    rbta.ReadOnly = ta.ReadOnly;
                    rbta.TransactionTimeout = ta.Timeout;
                    rbta.AsyncFlowOption = ta.AsyncFlowOption;

                    Type[] rbf = ta.RollbackFor;

                    var rollBackRules = new List<RollbackRuleAttribute>();

                    if (rbf != null)
                    {
                        foreach (Type t in rbf)
                        {
                            RollbackRuleAttribute rule = new RollbackRuleAttribute(t);
                            rollBackRules.Add(rule);
                        }
                    }


                    Type[] nrbfc = ta.NoRollbackFor;

                    if (nrbfc != null)
                    {
                        foreach (Type t in nrbfc)
                        {
                            NoRollbackRuleAttribute rule = new NoRollbackRuleAttribute(t);
                            rollBackRules.Add(rule);
                        }
                    }

                    rbta.RollbackRules = rollBackRules;

                    return rbta;

                }
            }
            return null;
        }

	}
}
