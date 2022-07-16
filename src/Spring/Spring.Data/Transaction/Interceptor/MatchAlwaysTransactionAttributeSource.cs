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
	/// Very simple implementation of <see cref="Spring.Transaction.Interceptor.ITransactionAttributeSource"/>
	/// which will always return the same <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/>
	/// for all methods fed to it.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/>
	/// may be specified, but will otherwise default to PROPAGATION_REQUIRED. This may be
	/// used in the cases where you want to use the same transaction attribute with all
	/// methods being handled by a transaction interceptor.
	/// </p>
	/// </remarks>
	/// <author>Colin Sampaleanu</author>
	/// <author>Griffin Caprio (.NET)</author>
	[Serializable]
	public class MatchAlwaysTransactionAttributeSource : ITransactionAttributeSource
	{
		private ITransactionAttribute _transactionAttribute;

		/// <summary>
		/// Creates a new instance of the
		/// <see cref="Spring.Transaction.Interceptor.MatchAlwaysTransactionAttributeSource"/>
		/// class.
		/// </summary>
		public MatchAlwaysTransactionAttributeSource()
		{
			_transactionAttribute = new DefaultTransactionAttribute();
		}

		/// <summary>
		/// Allows a transaction attribute to be specified, using the <see cref="System.String"/>
		/// form, for example, "PROPAGATION_REQUIRED".
		/// </summary>
		public ITransactionAttribute TransactionAttribute
		{
			set { _transactionAttribute = value; }
		}

		#region ITransactionAttributeSource Members
		/// <summary>
		/// Return the <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/> for this
		/// method.
		/// </summary>
		/// <param name="method">The method to check.</param>
		/// <param name="targetType">
		/// The target <see cref="System.Type"/>. May be null, in which case the declaring
		/// class of the supplied <paramref name="method"/> must be used.
		/// </param>
		/// <returns>
		/// A <see cref="Spring.Transaction.Interceptor.ITransactionAttribute"/> or
		/// null if the method is non-transactional.
		/// </returns>
		public ITransactionAttribute ReturnTransactionAttribute(MethodInfo method, Type targetType)
		{
			return _transactionAttribute;
		}
		#endregion

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
        /// </returns>
	    public override bool Equals(object obj)
	    {
	        if (this == obj) return true;
	        MatchAlwaysTransactionAttributeSource matchAlwaysTransactionAttributeSource = obj as MatchAlwaysTransactionAttributeSource;
	        if (matchAlwaysTransactionAttributeSource == null) return false;
	        return Equals(_transactionAttribute, matchAlwaysTransactionAttributeSource._transactionAttribute);
	    }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"></see>.
        /// </returns>
	    public override int GetHashCode()
	    {
	        return _transactionAttribute != null ? _transactionAttribute.GetHashCode() : 0;
	    }


	    ///<summary>
	    ///Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
	    ///</summary>
	    ///
	    ///<returns>
	    ///A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
	    ///</returns>
	    ///<filterpriority>2</filterpriority>
	    public override string ToString()
	    {
	        return GetType().Name + ": " + _transactionAttribute;
	    }
	}
}
