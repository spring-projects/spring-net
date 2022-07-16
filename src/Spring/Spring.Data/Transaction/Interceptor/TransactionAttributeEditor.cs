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

using System.Data;
using Spring.Util;

namespace Spring.Transaction.Interceptor
{
	/// <summary>
	/// Type converter for <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/>
	/// objects.
	/// </summary>
	/// <remarks>
	/// Takes <see cref="System.String"/>s of the form
	/// <p><code>PROPAGATION_NAME,ISOLATION_NAME,readOnly,timeout_NNNN,+Exception1,-Exception2</code></p>
	/// <p>where only propagation code is required. For example:</p>
	/// <p><code>PROPAGATION_MANDATORY,ISOLATION_DEFAULT</code></p>
	/// <p>
	/// The tokens can be in <strong>any</strong> order. Propagation and isolation codes
	/// must use the names of the values in the <see cref="Spring.Transaction.TransactionPropagation"/>
	/// enumeration. Timeout values are in seconds. If no timeout is specified, the transaction
	/// manager will apply a default timeout specific to the particular transaction manager.
	/// </p>
	/// <p>
	/// A "+" before an exception name substring indicates that transactions should commit even
	/// if this exception is thrown; a "-" that they should roll back.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	public class TransactionAttributeEditor
	{
		private RuleBasedTransactionAttribute _attribute;

		/// <summary>
		/// Parses the input properties string into a valid <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/>
		/// instance
		/// </summary>
		/// <param name="transactionProperties">
		/// The string defining the transactional properties.
		/// </param>
		public void SetAsText( string transactionProperties )
		{
			if (!StringUtils.HasText(transactionProperties))
			{
				_attribute = null;
			}
			else
			{
				string[] tokens = StringUtils.CommaDelimitedListToStringArray( transactionProperties );
				RuleBasedTransactionAttribute attribute = new RuleBasedTransactionAttribute();
				for ( int i = 0; i < tokens.Length; i++ )
				{
					string token = tokens[i].Trim();
					if ( token.StartsWith(DefaultTransactionAttribute.PROPAGATION_CONSTANT_PREFIX))
					{
						attribute.PropagationBehavior = convertPropagationValue( token );
					} else if ( token.StartsWith(DefaultTransactionAttribute.ISOLATION_CONSTANT_PREFIX ) )
					{
						attribute.TransactionIsolationLevel = convertIsolationValue( token );
					} else if ( token.StartsWith(DefaultTransactionAttribute.TIMEOUT_PREFIX ) )
					{
						string value = token.Substring(DefaultTransactionAttribute.TIMEOUT_PREFIX.Length);
						attribute.TransactionTimeout = Convert.ToInt32( value );
					} else if ( token.StartsWith( DefaultTransactionAttribute.READ_ONLY_MARKER ) )
					{
						attribute.ReadOnly = true;
					} else if ( token.StartsWith( DefaultTransactionAttribute.COMMIT_RULE_PREFIX ) )
					{
						attribute.AddRollbackRule( new NoRollbackRuleAttribute(token.Substring( 1 ) ) );
					} else if ( token.StartsWith( DefaultTransactionAttribute.ROLLBACK_RULE_PREFIX ) )
					{
						attribute.AddRollbackRule( new RollbackRuleAttribute( token.Substring( 1 ) ) );
					} else
					{
						throw new ArgumentException("Illegal transaction attribute token: [" + token + "]");
					}
				}
				_attribute = attribute;
			}
		}

		/// <summary>
		/// Gets the <see cref="Spring.Transaction.Interceptor.RuleBasedTransactionAttribute"/>
		/// from this editor.
		/// </summary>
		public ITransactionAttribute Value
		{
			get { return _attribute; }
		}

		private TransactionPropagation convertPropagationValue( string transactionPropagation )
		{
			switch ( transactionPropagation.ToUpper() )
			{
				case "PROPAGATION_REQUIRED":
					return TransactionPropagation.Required;
				case "PROPAGATION_SUPPORTS":
					return TransactionPropagation.Supports;
				case "PROPAGATION_MANDATORY":
					return TransactionPropagation.Mandatory;
				case "PROPAGATION_REQUIRES_NEW":
					return TransactionPropagation.RequiresNew;
				case "PROPAGATION_NOT_SUPPORTED":
					return TransactionPropagation.NotSupported;
				case "PROPAGATION_NEVER":
					return TransactionPropagation.Never;
				case "PROPAGATION_NESTED":
					return TransactionPropagation.Nested;
				default:
					throw new ArgumentException("Illegal transaction propagation token: [" + transactionPropagation + "]");
			}
		}

		private IsolationLevel convertIsolationValue( string isolationLevel )
		{
			switch ( isolationLevel.ToUpper() )
			{
				case "ISOLATION_DEFAULT":
                    return IsolationLevel.ReadCommitted;
				case "ISOLATION_READUNCOMMITTED":
					return IsolationLevel.ReadUncommitted;
				case "ISOLATION_READCOMMITTED":
					return IsolationLevel.ReadCommitted;
				case "ISOLATION_REPEATABLEREAD":
					return IsolationLevel.RepeatableRead;
				case "ISOLATION_SERIALIZABLE":
					return IsolationLevel.Serializable;
				default:
                    return IsolationLevel.ReadCommitted;
			}
		}
	}
}
