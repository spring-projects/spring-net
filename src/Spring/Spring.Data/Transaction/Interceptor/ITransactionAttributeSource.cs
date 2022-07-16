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
	/// Interface used by <see cref="Spring.Transaction.Interceptor.TransactionInterceptor"/>
	/// to source transaction attributes.
	/// </summary>
	/// <remarks>
	/// <p>
	/// Implementations know how to source transaction attributes, whether from configuration,
	/// metadata attributes at source level, or anywhere else.
	/// </p>
	/// </remarks>
	/// <author>Rod Johnson</author>
	/// <author>Griffin Caprio (.NET)</author>
	public interface ITransactionAttributeSource
	{
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
		ITransactionAttribute ReturnTransactionAttribute( MethodInfo method, Type targetType );
	}
}
