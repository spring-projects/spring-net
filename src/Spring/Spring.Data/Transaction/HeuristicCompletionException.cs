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

using System.Runtime.Serialization;

namespace Spring.Transaction
{
	/// <summary>
	/// Exception that represents a transaction failure caused by heuristics.
	/// </summary>
	/// <author>Rod Johnson</author>
	/// <author>Juergen Hoeller</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class HeuristicCompletionException : TransactionException, ISerializable
	{
		/// <summary>
		/// The outcome state of the transaction: have some or all resources been committed?
		/// </summary>
		private TransactionOutcomeState _outcomeState = TransactionOutcomeState.Unknown;

		/// <summary>
		/// Returns the transaction's outcome state.
		/// </summary>
		public TransactionOutcomeState OutcomeState
		{
			get { return _outcomeState; }
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> representation of the supplied
		/// <see cref="Spring.Transaction.TransactionOutcomeState"/>.
		/// </summary>
		/// <param name="outcomeState">
		/// The <see cref="Spring.Transaction.TransactionOutcomeState"/> value that
		/// is to be stringified.
		/// </param>
		/// <returns>A <see cref="System.String"/> representation.</returns>
		public static string GetStateString( TransactionOutcomeState outcomeState )
		{
			return Enum.GetName (typeof (TransactionOutcomeState), outcomeState);
		}
		
		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.CannotCreateTransactionException"/> class.
		/// </summary>
		public HeuristicCompletionException( ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.CannotCreateTransactionException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		public HeuristicCompletionException( string message ) : base( message ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.CannotCreateTransactionException"/> class.
		/// </summary>
		/// <param name="message">
		/// A message about the exception.
		/// </param>
		/// <param name="rootCause">
		/// The root exception that is being wrapped.
		/// </param>
		public HeuristicCompletionException( string message, Exception rootCause )
			: base( message, rootCause ) {}

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.CannotCreateTransactionException"/> class
		/// with the specified <see cref="Spring.Transaction.TransactionOutcomeState"/>
		/// and inner exception.
		/// </summary>
		/// <param name="outcomeState">
		/// The <see cref="Spring.Transaction.TransactionOutcomeState"/>
		/// for the transaction.
		/// </param>
		/// <param name="innerException">
		/// The inner exception that is being wrapped.
		/// </param>
		public HeuristicCompletionException(
			TransactionOutcomeState outcomeState, Exception innerException)
			: base( "Heuristic completion: outcome state is " + GetStateString(outcomeState), innerException )
		{
			_outcomeState = outcomeState;
		}

		/// <inheritdoc />
		protected HeuristicCompletionException( SerializationInfo info, StreamingContext context )
			: base( info, context ) 
		{
			_outcomeState = ( TransactionOutcomeState ) info.GetInt32( "outcomeState" );
		}

		/// <inheritdoc />
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue( "outcomeState", _outcomeState );
			base.GetObjectData (info, context);
		}
	}
}
