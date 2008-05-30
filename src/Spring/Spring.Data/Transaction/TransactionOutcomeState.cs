#region License

#endregion

#region Imports

using System;

#endregion

namespace Spring.Transaction
{
	/// <summary>
	/// Represents a transaction's current state.
	/// </summary>
	/// <author>Griffin Caprio (.NET)</author>
	/// <version>$Id: TransactionOutcomeState.cs,v 1.4 2006/05/18 21:37:51 markpollack Exp $</version>
	public enum TransactionOutcomeState
	{
		/// <summary>
		/// The transaction state is unknown.
		/// </summary>
		Unknown,
		/// <summary>
		/// The transaction has been committed.
		/// </summary>
		Committed,
		/// <summary>
		/// The transaction has been rolled back.
		/// </summary>
		Rolledback,
		/// <summary>
		/// The transaction is in an unknown, mixed state.
		/// </summary>
		Mixed
	}
}
