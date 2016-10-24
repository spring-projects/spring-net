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

namespace Spring.Transaction.Support
{
	/// <summary>
	/// Interface to be implemented by transaction objects that are able to
	/// return an internal rollback-only marker, typically from a another
	/// transaction that has participated and marked it as rollback-only.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Autodetected by <see cref="Spring.Transaction.Support.DefaultTransactionStatus"/>,
	/// to always return a current rollbackOnly flag even if not resulting from the current
	/// <see cref="Spring.Transaction.ITransactionStatus"/>.
	/// </p>
	/// </remarks>
	/// <author>Juergen Hoeller</author>
    /// <author>Griffin Caprio (.NET)</author>
	public interface ISmartTransactionObject
	{
	    /// <summary>
	    /// Return whether the transaction is internally marked as rollback-only.
	    /// </summary>
	    /// <returns>True of the transaction is marked as rollback-only.</returns>
        bool RollbackOnly
        { 
            get;
        }
	}
}
