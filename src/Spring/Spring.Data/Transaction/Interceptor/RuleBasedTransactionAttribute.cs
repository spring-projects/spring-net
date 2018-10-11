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
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Spring.Collections;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/> implementation
	/// that works out whether a given exception should cause transaction rollback by applying
	/// a number of rollback rules, both positive and negative.
	/// </summary>
	/// <remarks>
	/// If no rules are relevant to the exception, it behaves like the 
	/// <see cref="Spring.Transaction.Interceptor.DefaultTransactionAttribute"/> class
	/// (rolling back on runtime exceptions)..
	/// <p>
	/// The <see cref="Spring.Transaction.Interceptor.TransactionAttributeEditor"/> 
	/// creates objects of this class.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	public class RuleBasedTransactionAttribute : DefaultTransactionAttribute
	{
		private IList<RollbackRuleAttribute> _rollbackRules;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.RuleBasedTransactionAttribute"/>
		/// class.
		/// </summary>
		/// <param name="transactionPropagation">
		/// The desired transaction propagation behaviour.
		/// </param>
		/// <param name="ruleList">
		/// The rollback rules list for this transaction attribute.
		/// </param>
		public RuleBasedTransactionAttribute(
			TransactionPropagation transactionPropagation,
			IList<RollbackRuleAttribute> ruleList )
			: base(transactionPropagation)
		{
			_rollbackRules = ruleList;
		}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.RuleBasedTransactionAttribute"/>
		/// class.
		/// </summary>
		public RuleBasedTransactionAttribute( )
		{
			_rollbackRules = new List<RollbackRuleAttribute>();
		}

		/// <summary>
		/// Sets the rollback rules list for this transaction attribute.
		/// </summary>
		public IList<RollbackRuleAttribute> RollbackRules
		{
			set => _rollbackRules = value;
		}

		/// <summary>
		/// Will a transaction be rolled back if the supplied <parameref name="exception"/>
		/// is thrown during the lifecycle of a transaction to which this attribute is applied?
		/// </summary>
		/// <param name="exception">The offending <see cref="System.Exception"/>.</param>
		/// <returns>True if the exception should cause a rollback, false otherwise.</returns>
		public override bool RollbackOn(Exception exception)
		{
			RollbackRuleAttribute finalAttribute = null;
			int deepest = Int32.MaxValue;

			if ( _rollbackRules != null )
			{
				foreach ( RollbackRuleAttribute rule in _rollbackRules )
				{
					int depth = rule.GetDepth( exception );
					if ( ( depth >= 0 ) && ( depth < deepest ) )
					{
						deepest = depth;
						finalAttribute = rule;
					}
				}
			}
			if ( null == finalAttribute )
			{
				return base.RollbackOn( exception );
			}
			return !( finalAttribute is NoRollbackRuleAttribute );
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> representation of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/> representation of this instance.
		/// </returns>
		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			result.Append(DefinitionDescription);
			SortedSet rules = new SortedSet();
			foreach ( RollbackRuleAttribute rule in _rollbackRules )
			{
				string sign = ( rule is NoRollbackRuleAttribute ) ? COMMIT_RULE_PREFIX : ROLLBACK_RULE_PREFIX;
				rules.Add( sign + rule.ExceptionName );
			}
			foreach ( string rule in rules )
			{
				result.Append(',');
				result.Append(rule);
			}
			return result.ToString( );
		}
		/// <summary>
		/// Adds a <see cref="Spring.Transaction.Interceptor.RollbackRuleAttribute"/> to this
		/// attributes list of rollback rules.
		/// </summary>
		/// <param name="rule">
		/// The <see cref="Spring.Transaction.Interceptor.RollbackRuleAttribute"/> to add.
		/// </param>
		public void AddRollbackRule( RollbackRuleAttribute rule )
		{
			_rollbackRules.Add( rule );
		}

		/// <summary>
		/// Clears the rollback rules for this attribute.
		/// </summary>
		public void ClearRollbackRules()
		{
			_rollbackRules.Clear();
		}
	}
}
